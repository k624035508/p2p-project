using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.AutoLogic
{
    public class CheckDelayInvestOverTime
    {
        internal static void DoSubscribe()
        {
            // 如果仍存在的昨日的延期投资，判断一下活期债权是否足够，是的话则投资成功，否的话投资失败
            MessageBus.Main.Subscribe<TimerMsg>(m => DoCheckDelayInvestOverTime(m.TimerType, m.OnTime));

            // 定期债权转让成功，尝试完成延期投资
            MessageBus.Main.Subscribe<StaticClaimTransferSuccessMsg>(m => TryCompleteDelayInvest(m.NeedTransferClaimId));
        }

        private static void TryCompleteDelayInvest(int needTransferClaimId)
        {
            var context = new Agp2pDataContext();
            var delayInvested = context.li_project_transactions.Where(
                ptr =>
                    ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    ptr.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Pending)
                .AsEnumerable()
                .Where(ptr => ptr.li_projects.IsHuoqiProject())
                .ToList();

            if (!delayInvested.Any()) return;

            // 有中间人买入过的债权才可以进行活期投资
            var agentClaimAppended = context.li_claims.Single(c => c.id == needTransferClaimId)
                    .Children.Any(c => c.status == (int) Agp2pEnums.ClaimStatusEnum.Transferable && c.dt_users.IsAgent());
            if (!agentClaimAppended) return;

            var huoqiInvestableClaimsAmount = context.GetHuoqiInvestableClaims()
                .Aggregate(0m, (sum, c) => sum + c.principal);

            if (huoqiInvestableClaimsAmount == 0) return;

            delayInvested.Where(ptr => ptr.principal <= huoqiInvestableClaimsAmount)
                .OrderBy(ptr => ptr.principal)
                .Aggregate(huoqiInvestableClaimsAmount, (total, ptr) =>
                {
                    if (total < ptr.principal) return total;

                    TransactionFacade.DelayInvestSuccess(ptr.id);
                    context.AppendAdminLog("Huoqi", $"活期延期投资成功，用户 {ptr.dt_users.GetFriendlyUserName()}，金额 {ptr.principal}");
                    return total - ptr.principal;
                });

            context.SubmitChanges();
        }

        public static void DoCheckDelayInvestOverTime(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.AutoRepayTimer) return;

            var context = new Agp2pDataContext();
            var delayInvested = context.li_project_transactions.Where(
                ptr =>
                    ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Pending &&
                    ptr.create_time < DateTime.Today)
                .AsEnumerable()
                .Where(ptr => ptr.li_projects.IsHuoqiProject())
                .ToList();

            if (!delayInvested.Any()) return;

            var huoqiInvestableClaimsAmount = context.GetHuoqiInvestableClaims()
                .Aggregate(0m, (sum, c) => sum + c.principal);
            delayInvested.Aggregate(huoqiInvestableClaimsAmount, (total, ptr) =>
                {
                    if (ptr.principal <= total)
                    {
                        TransactionFacade.DelayInvestSuccess(ptr.id);
                        context.AppendAdminLog("Huoqi", $"活期延期投资成功，用户 {ptr.dt_users.GetFriendlyUserName()}，金额 {ptr.principal}" );
                        return total - ptr.principal;
                    }
                    else
                    {
                        TransactionFacade.DelayInvestFailure(ptr.id);
                        context.AppendAdminLog("Huoqi", $"活期延期投资失败，用户 {ptr.dt_users.GetFriendlyUserName()}，金额 {ptr.principal}" );
                        return total;
                    }
                });

            context.SubmitChanges();
        }
    }
}
