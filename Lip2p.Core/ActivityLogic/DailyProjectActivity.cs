using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lip2p.Common;
using Lip2p.Core.Message;
using Lip2p.Linq2SQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lip2p.Core.ActivityLogic
{
    public class DailyProjectActivity
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<UserInvestedMsg>(m => HandleUserInvestedMsg(m.ProjectTransactionId, m.InvestTime)); // 老用户投资送体验券
            MessageBus.Main.Subscribe<TimerMsg>(m => HandleTimerMsg(m.OnTime)); // 到期则放款
        }

        public class DailyProjectTicket
        {
            private readonly li_activity_transactions atr;

            public DailyProjectTicket(li_activity_transactions atr)
            {
                Debug.Assert(atr.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject);
                this.atr = atr;
            }

            public int GetUserId()
            {
                return atr.user_id;
            }

            public decimal GetTicketValue()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return jsonObj.Value<decimal>("Value");
            }

            public bool IsUsed()
            {
                return atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm;
            }

            public DateTime GetDeadline()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return Convert.ToDateTime(jsonObj.Value<string>("Deadline"));
            }

            public DateTime? GetRepayTime()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return jsonObj.Value<DateTime?>("RepayTime");
            }

            public void SetCancelIfExpired()
            {
                if (IsExpired())
                {
                    atr.status = (byte) Lip2pEnums.ActivityTransactionStatusEnum.Cancel;
                }
            }

            public bool IsExpired()
            {
                return GetDeadline() < DateTime.Now.Date;
            }

            public void Use(Lip2pDataContext context, int projectId)
            {
                if (IsUsed()) throw new Exception("此天标券已经使用过了");
                if (IsExpired()) throw new Exception("此天标券已经过期了");

                var proj = context.li_projects.Single(p => p.id == projectId);
                if ((int)Lip2pEnums.ProjectStatusEnum.Financing != proj.status)
                    throw new InvalidOperationException("项目不是发标状态，不能投资");

                var ticketValue = GetTicketValue();

                // 判断投资金额的数额是否合理
                var canBeInvest = proj.financing_amount - proj.investment_amount;
                if (canBeInvest == 0)
                    throw new InvalidOperationException("项目已经满标");
                if (canBeInvest < ticketValue)
                    throw new InvalidOperationException("天标券金额 " + ticketValue + " 超出项目可投资金额 " + canBeInvest);
                if (canBeInvest != ticketValue && canBeInvest - ticketValue < 100)
                    throw new InvalidOperationException("最后一次投标的最低金额为 " + canBeInvest + " 元");

                var useTime = DateTime.Now;
                // 计算利息
                var termSpan = (Lip2pEnums.ProjectRepaymentTermSpanEnum)proj.repayment_term_span;
                var rate = TransactionFacade.CalcFinalProfitRate(useTime, proj.profit_rate_year, termSpan, proj.repayment_term_span_count);
                atr.value = Math.Round(ticketValue * rate, 2);
                // 计算放款时间
                var repayTime = TransactionFacade.CalcRepayTime(useTime, termSpan,
                    termSpan == Lip2pEnums.ProjectRepaymentTermSpanEnum.Day ? 1 : proj.repayment_term_span_count,
                    proj.repayment_term_span_count);

                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                jsonObj["ProjectId"] = projectId;
                jsonObj["RepayTime"] = repayTime.ToString("yyyy-MM-dd HH:mm:ss");
                atr.remarks = string.Format("[天标]将于 {0:yyyy-MM-dd} 收益 {1:c}", repayTime, atr.value);
                atr.details = jsonObj.ToString(Formatting.None);
                atr.status = (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm;

                var profitingMoney = atr.value;

                // 修改项目已投资金额
                proj.investment_amount += ticketValue;
                if (proj.investment_amount == proj.financing_amount) // 如果项目满了，设置为满标
                {
                    proj.invest_complete_time = useTime;
                    proj.status = (int) Lip2pEnums.ProjectStatusEnum.FinancingSuccess; // FIXME 直接跳到 还款中 ？
                    proj.update_time = useTime;
                }

                // 修改钱包，添加待收金额
                var wallet = context.li_wallets.Single(w => w.user_id == atr.user_id);
                // 满标时再计算待收益金额
                wallet.profiting_money += profitingMoney;
                wallet.last_update_time = useTime;

                // 修改钱包历史
                var his = TransactionFacade.CloneFromWallet(wallet, Lip2pEnums.WalletHistoryTypeEnum.Gaining);
                his.li_activity_transactions = atr;
                context.li_wallet_histories.InsertOnSubmit(his);

                context.SubmitChanges();
            }
        }

        private static void HandleTimerMsg(bool startUp)
        {
            var repayTime = DateTime.Now;
            var today = repayTime.Date;

            var context = new Lip2pDataContext();
            // 找出过期的券并标记为过期
            context.li_activity_transactions.Where(a =>
                a.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject &&
                a.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting)
                .AsEnumerable()
                .ForEach(atr => new DailyProjectTicket(atr).SetCancelIfExpired());

            // 找出使用了但是未放款的券
            var unpaidTickets = context.li_activity_transactions.Where(
                a =>
                    a.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject &&
                    a.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm)
                .Where(a => a.transact_time == null)
                .AsEnumerable()
                .Where(a => new DailyProjectTicket(a).GetRepayTime().GetValueOrDefault().Date == today) // 确保今日是放款日
                .ToList();

            // 添加活动备注，减去钱包待收利息，创建钱包历史
            unpaidTickets.ForEach(atr =>
            {
                atr.remarks = "天标收益";
                atr.transact_time = repayTime;

                var wallet = atr.dt_users.li_wallets;
                wallet.idle_money += atr.value;
                wallet.profiting_money -= atr.value;
                wallet.total_profit += atr.value;
                wallet.last_update_time = repayTime;

                var his = TransactionFacade.CloneFromWallet(wallet, Lip2pEnums.WalletHistoryTypeEnum.GainConfirm);
                his.li_activity_transactions = atr;
                context.li_wallet_histories.InsertOnSubmit(his);
            });

            if (unpaidTickets.Any())
                context.AppendAdminLog(DTEnums.ActionEnum.Edit.ToString(), "天标券自动放款：" + unpaidTickets.Count);

            context.SubmitChanges();
        }

        private static readonly DateTime startTime = new DateTime(2015, 6, 18);
        private static readonly DateTime deadline = new DateTime(2015, 7, 12);
        private static void HandleUserInvestedMsg(int projectTransactionId, DateTime investTime)
        {
            if (Utils.IsDebugging() || startTime <= investTime.Date && investTime.Date <= deadline)
            {
                var context = new Lip2pDataContext();
                var investedTr = context.li_project_transactions.Single(t => t.id == projectTransactionId);

                // 查出自己的全部天标券，计算总额
                var dailyTickets =
                    investedTr.dt_users.li_activity_transactions.Where(
                        t => t.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject)
                        .AsEnumerable()
                        .Select(t => new DailyProjectTicket(t))
                        .ToList();

                var sumOfTicketValue = dailyTickets.Select(ti => ti.GetTicketValue()).DefaultIfEmpty(0).Sum();
                var remain = 10*10000 - sumOfTicketValue; // 还有多少抵达上限

                if (remain == 0)
                    return;

                var dailyTr = new li_activity_transactions
                {
                    user_id = investedTr.investor,
                    create_time = investTime,
                    value = 0, // 投资了项目后再设置
                    details = JsonConvert.SerializeObject(new
                    {
                        Value = Math.Min(remain, investedTr.value), // 限制上限 10w
                        Deadline = deadline.ToString("yyyy-MM-dd")
                    }),
                    type = (byte) Lip2pEnums.ActivityTransactionTypeEnum.Gain,
                    status = (byte) Lip2pEnums.ActivityTransactionStatusEnum.Acting,
                    activity_type = (byte) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject,
                };
                context.li_activity_transactions.InsertOnSubmit(dailyTr);
                context.SubmitChanges();
            }
        }
    }
}
