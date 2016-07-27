using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Agp2p.Core.Message;

namespace Agp2p.Core.ActivityLogic
{
    public class HongBaoActivity
    {
        internal static void DoSubscribe()
        {
            // MessageBus.Main.Subscribe<NewUserCreatedMsg>(m => NewUser(m.UserId, m.RegTime)); // 发放新用户的红包
            MessageBus.Main.Subscribe<UserInvestedMsg>(m => HandleUserInvestedMsg(m.ProjectTransactionId, m.InvestTime)); // 投资激活红包
            //MessageBus.Main.Subscribe<UserLoginMsg>(m => HandleUserLoginMsg(m.UserId)); // 查出过期的奖券，将其设置为失效
        }

        private static void HandleUserLoginMsg(int userId)
        {
            var context = new Agp2pDataContext();
            var user = context.dt_users.Single(u => u.id == userId);
            var hongbaos = user.li_activity_transactions.Where(
                t =>
                    t.activity_type == (int)Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao &&
                    t.status == (int)Agp2pEnums.ActivityTransactionStatusEnum.Acting).AsEnumerable()
                    .Select(a => new HongBao(a)).ToList();
            hongbaos.ForEach(h =>
            {
                if (h.GetDeadline() < DateTime.Now.Date)
                {
                    // 奖券已过期，标记为失效
                    // var invested = h.GetInvested(); TODO 将投资金额转移到未过期的红包
                    h.SetInvested(null);
                    h.Cancel();
                }
            });
            context.SubmitChanges();
        }

        public class HongBao
        {
            public readonly li_activity_transactions atr;

            public HongBao(li_activity_transactions atr)
            {
                this.atr = atr;
            }

            public void SetInvested(decimal? invested)
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                if (invested == null)
                {
                    if (jsonObj.Value<decimal?>("Invested") != null)
                        jsonObj.Remove("Invested");
                }
                else
                    jsonObj["Invested"] = invested.Value;
                atr.details = jsonObj.ToString(Formatting.None);
            }

            public decimal GetInvested()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return jsonObj.Value<decimal?>("Invested") ?? 0;
            }

            public decimal GetInvestUntil()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return jsonObj.Value<decimal>("InvestUntil");
            }

            public void Activate(DateTime investTime)
            {
                SetInvested(null);
                atr.status = (byte)Agp2pEnums.ActivityTransactionStatusEnum.Confirm;
                atr.transact_time = investTime;
                atr.remarks = string.Format("投资 {0:c} 激活 {1:c} 红包", GetInvestUntil(), atr.value);
            }

            public DateTime GetDeadline()
            {
                var jsonObj = (JObject)JsonConvert.DeserializeObject(atr.details);
                return Convert.ToDateTime(jsonObj.Value<string>("Deadline"));
            }

            public void Cancel()
            {
                atr.status = (byte)Agp2pEnums.ActivityTransactionStatusEnum.Cancel;
                atr.transact_time = GetDeadline();
            }
        }

        private static void HandleUserInvestedMsg(int projectTransactionId, DateTime investTime)
        {
            var context = new Agp2pDataContext();

            var projectTransaction = context.li_project_transactions.Single(tr => tr.id == projectTransactionId);

            var unactived = context.li_activity_transactions.Where(a =>
                    a.user_id == projectTransaction.investor && a.status == (int)Agp2pEnums.ActivityTransactionStatusEnum.Acting &&
                    a.type == (int)Agp2pEnums.ActivityTransactionTypeEnum.Gain &&
                    a.activity_type == (int)Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao).ToList();
            if (!unactived.Any()) return;

            var wallet = context.li_wallets.Single(w => w.user_id == projectTransaction.investor);
            decimal investAmount = projectTransaction.principal;

            // 优先取得较大的红包，一样大的话优先满足快过期的红包
            var rps = unactived.Select(a => new HongBao(a))
                    .OrderByDescending(a => a.GetInvestUntil())
                    .ThenBy(a => a.GetDeadline())
                    .ToList();

            // 汇总未激活红包的投资金额
            investAmount += rps.Sum(rp => rp.GetInvested());
            rps.ForEach(rp => rp.SetInvested(null));

            foreach (var rp in rps)
            {
                if (rp.GetInvestUntil() <= investAmount) // 投资足够激活红包
                {
                    investAmount -= rp.GetInvestUntil();

                    // 红包激活，发放奖金，更改状态
                    rp.Activate(investTime);
                    var curr = rp.atr;

                    wallet.idle_money += curr.value;
                    wallet.last_update_time = investTime;

                    var his = TransactionFacade.CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.GainConfirm);
                    his.li_activity_transactions = curr;
                    context.li_wallet_histories.InsertOnSubmit(his);
                }
            }
            // 有剩余投资金额不够激活钱包的话将其记在第一个未被激活的红包
            var firstRp = rps.FirstOrDefault(rp => rp.atr.status == (int)Agp2pEnums.ActivityTransactionStatusEnum.Acting);
            if (0 < investAmount && firstRp != null)
            {
                firstRp.SetInvested(investAmount);
            }
            context.SubmitChanges();
        }

        private static readonly Dictionary<decimal, decimal> HongbaoActivateMoneyMap = new Dictionary<decimal, decimal>
        {
            {10m, 100m},
        };

        public static void GiveUser(int userId, int expireAfterDays = 365)
        {
            var context = new Agp2pDataContext();

            var useTime = DateTime.Now;
            var trs = HongbaoActivateMoneyMap.Select(m => new li_activity_transactions
            {
                user_id = userId,
                create_time = useTime,
                value = m.Key,
                details = JsonConvert.SerializeObject(new
                {
                    Deadline = useTime.AddDays(expireAfterDays).ToString("yyyy-MM-dd"),
                    InvestUntil = m.Value
                }),
                type = (byte)Agp2pEnums.ActivityTransactionTypeEnum.Gain,
                status = (byte)Agp2pEnums.ActivityTransactionStatusEnum.Acting,
                activity_type = (byte)Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao,
            });
            context.li_activity_transactions.InsertAllOnSubmit(trs);
            context.SubmitChanges();
        }

        public static List<HongBao> QueryHongBao(dt_users user)
        {
            return user.li_activity_transactions
                .Where(atr => atr.activity_type == (byte)Agp2pEnums.ActivityTransactionActivityTypeEnum.HongBao &&
                atr.status == (byte)Agp2pEnums.ActivityTransactionStatusEnum.Acting)
                .AsEnumerable().Select(atr => new HongBao(atr)).ToList();

        }
    }
}