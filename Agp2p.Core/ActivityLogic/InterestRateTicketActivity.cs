using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Agp2p.Linq2SQL;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.Message;
using System.Collections.Generic;

namespace Agp2p.Core.ActivityLogic
{
    public class InterestRateTicketActivity
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => HandleTimerMsg(m.TimerType, m.OnTime)); // 判断是否过期
            MessageBus.Main.Subscribe<ProjectStartRepaymentMsg>(m => DoCalcProfitingMoney(m.ProjectId, m.MakeLoanTime)); // 支付额外利息
            MessageBus.Main.Subscribe<ProjectRepayCompletedMsg>(m => DoPayBonusInterest(m.ProjectId, m.ProjectCompleteTime)); // 支付额外利息
        }

        public class InterestRateTicket
        {
            private readonly li_activity_transactions atr;
            private readonly JObject detailObj;

            public InterestRateTicket(li_activity_transactions atr)
            {
                Debug.Assert(atr.activity_type == (int)Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket);
                this.atr = atr;
                detailObj = (JObject)JsonConvert.DeserializeObject(atr.details);
            }

            public int GetUserId()
            {
                return atr.user_id;
            }

            public decimal GetMinInvestAmount()
            {
                return detailObj.Value<decimal>("MinInvestAmount");
            }

            public decimal GetInterestRateBonus()
            {
                return detailObj.Value<decimal>("InterestRateBonus");
            }

            public int GetMinProjectDayCount()
            {
                return detailObj.Value<int>("MinProjectDayCount");
            }

            public bool IsUsed()
            {
                return atr.status == (int)Agp2pEnums.ActivityTransactionStatusEnum.Confirm;
            }

            public DateTime GetDeadline()
            {
                var deadlineStr = detailObj.Value<string>("Deadline");
                if (string.IsNullOrWhiteSpace(deadlineStr))
                {
                    return atr.create_time.AddDays(30).Date;
                }
                else
                {
                    return Convert.ToDateTime(deadlineStr);
                }
            }

            public void SetCancelIfExpired()
            {
                if (IsExpired())
                {
                    atr.status = (byte)Agp2pEnums.ActivityTransactionStatusEnum.Cancel;
                }
            }

            public bool IsExpired()
            {
                return GetDeadline() < DateTime.Now.Date;
            }

            public void Use(Agp2pDataContext context, int projectId, decimal investAmount)
            {
                if (IsUsed()) throw new Exception("此加息券已经使用过了");
                if (IsExpired()) throw new Exception("此加息券已经过期了");

                var proj = context.li_projects.Single(p => p.id == projectId);
                if ((int)Agp2pEnums.ProjectStatusEnum.Financing != proj.status)
                    throw new InvalidOperationException("项目不是发标状态，不能投资");

                var minInvestAmount = GetMinInvestAmount();
                if (investAmount < minInvestAmount)
                {
                    throw new InvalidOperationException("投资金额低于加息券的规定金额");
                }

                var useTime = DateTime.Now;
                var finalRepayTime = proj.CalcRepayTimeByTerm(proj.CalcRealTermCount(), useTime);
                if (finalRepayTime < useTime.AddDays(GetMinProjectDayCount()))
                {
                    throw new InvalidOperationException("项目期限短于加息券规定的项目期限");
                }

                detailObj["ProjectId"] = projectId;

                // 按比例计算利息：项目年化:奖励年化 = 项目利率:奖励利率
                var rate = proj.GetFinalProfitRate(useTime) * GetInterestRateBonus() / proj.profit_rate_year;
                atr.value = Math.Round(investAmount * rate, 2);

                atr.remarks = string.Format("[加息券]已用于 {0} 项目，预计收益 {1}", proj.title, atr.value);
                atr.details = detailObj.ToString(Formatting.None);
                atr.status = (int)Agp2pEnums.ActivityTransactionStatusEnum.Confirm;

                // remember to save context
            }
        }

        private static void DoCalcProfitingMoney(int projectId, DateTime makeLoanTime)
        {
            var context = new Agp2pDataContext();

            // 找出未支付的奖励
            var projectAtrs = context.li_activity_transactions.Where(atr =>
                atr.activity_type == (byte)Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket &&
                atr.status == (byte)Agp2pEnums.ActivityTransactionStatusEnum.Confirm &&
                atr.type == (byte)Agp2pEnums.ActivityTransactionTypeEnum.Gain &&
                atr.transact_time == null &&
                (atr.details.Contains("\"ProjectId\":" + projectId + ",") || atr.details.Contains("\"ProjectId\":" + projectId + "}")))
                .ToLookup(atr => atr.dt_users);

            projectAtrs.ForEach(p =>
            {
                var wallet = p.Key.li_wallets;
                p.ForEach(atr =>
                {
                    // 满标时再计算待收益金额
                    wallet.profiting_money += atr.value;
                    wallet.last_update_time = makeLoanTime;

                    // 修改钱包历史
                    var his = TransactionFacade.CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.Gaining);
                    his.li_activity_transactions = atr;
                    context.li_wallet_histories.InsertOnSubmit(his);
                });
            });
            if (projectAtrs.Any())
                context.SubmitChanges();
        }

        private static void DoPayBonusInterest(int projectId, DateTime projectCompleteTime)
        {
            var context = new Agp2pDataContext();

            // 找出未支付的奖励
            var projectAtrs = context.li_activity_transactions.Where(atr =>
                atr.activity_type == (byte)Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket &&
                atr.status == (byte)Agp2pEnums.ActivityTransactionStatusEnum.Confirm &&
                atr.type == (byte)Agp2pEnums.ActivityTransactionTypeEnum.Gain &&
                atr.transact_time == null &&
                (atr.details.Contains("\"ProjectId\":" + projectId + ",") || atr.details.Contains("\"ProjectId\":" + projectId + "}")))
                .ToLookup(atr => atr.dt_users);

            // 支付奖励并减去代收金额
            projectAtrs.ForEach(userTickets =>
            {
                var wallet = userTickets.Key.li_wallets;
                userTickets.ForEach(atr =>
                {
                    atr.remarks = "加息券收益";
                    atr.transact_time = projectCompleteTime;

                    wallet.profiting_money -= atr.value;
                    wallet.idle_money += atr.value;
                    wallet.total_profit += atr.value;
                    wallet.last_update_time = projectCompleteTime;

                    var his = TransactionFacade.CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.GainConfirm);
                    his.li_activity_transactions = atr;
                    context.li_wallet_histories.InsertOnSubmit(his);
                });
            });

            if (projectAtrs.Any())
                context.AppendAdminLog(DTEnums.ActionEnum.Edit.ToString(), "加息券自动放款用户数：" + projectAtrs.Count);

            context.SubmitChanges();
        }

        public static void HandleTimerMsg(TimerMsg.Type timerName, bool startUp)
        {
            if (timerName != TimerMsg.Type.AutoRepayTimer) return;

            var repayTime = DateTime.Now;
            var context = new Agp2pDataContext();
            // 找出过期的券并标记为过期
            context.li_activity_transactions.Where(a =>
                a.activity_type == (int)Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket &&
                a.status == (int)Agp2pEnums.ActivityTransactionStatusEnum.Acting)
                .AsEnumerable()
                .ForEach(atr => new InterestRateTicket(atr).SetCancelIfExpired());

            context.SubmitChanges();
        }

        public static int GiveUser(int userId, decimal interestRateBonusPercent, decimal minInvestValue,
            int minProjectDayCount, int ticketExpireAfterDays = 30)
        {
            var context = new Agp2pDataContext();

            var trs = new li_activity_transactions
            {
                user_id = userId,
                create_time = DateTime.Now,
                value = 0, // 使用之后才有值
                details = JsonConvert.SerializeObject(new
                {
                    InterestRateBonus = interestRateBonusPercent,
                    minInvestValue = minInvestValue,
                    MinProjectDayCount = minProjectDayCount,
                    Deadline = DateTime.Today.AddDays(ticketExpireAfterDays).ToString("yyyy-MM-dd")
                }),
                type = (byte)Agp2pEnums.ActivityTransactionTypeEnum.Gain,
                status = (byte)Agp2pEnums.ActivityTransactionStatusEnum.Acting,
                activity_type = (byte)Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket,
            };
            context.li_activity_transactions.InsertOnSubmit(trs);
            context.SubmitChanges();

            return trs.id;
        }

        public static List<InterestRateTicket> QueryTicket(dt_users user)
        {
            return user.li_activity_transactions
                .Where(atr => atr.activity_type == (byte)Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket &&
                atr.status == (byte)Agp2pEnums.ActivityTransactionStatusEnum.Acting)
                .AsEnumerable().Select(atr => new InterestRateTicket(atr)).ToList();
        }
    }
}