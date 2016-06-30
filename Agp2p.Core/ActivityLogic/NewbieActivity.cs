using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.ActivityLogic
{
    /// <summary>
    /// 新手标第一期 停用
    /// </summary>
    public class NewbieActivity
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<UserInvestedMsg>(m => CheckNewbieInvest(m.ProjectTransactionId)); // 用户投资消息，检测是不是投资了新手体验标，是的话执行相应逻辑
            MessageBus.Main.Subscribe<TimerMsg>(m => HandleTimerMsg(m.TimerType, m.OnTime)); // 到期则放款
        }

        /// <summary>
        /// 新手标第一期自动返10元（停用）
        /// </summary>
        /// <param name="timerName"></param>
        /// <param name="onTime"></param>
        private static void HandleTimerMsg(TimerMsg.Type timerName, bool onTime)
        {
            if (timerName != TimerMsg.Type.AutoRepayTimer) return;

            var context = new Agp2pDataContext();
            var shouldRepayTask = context.li_repayment_tasks.Where(
                t =>
                    t.only_repay_to != null &&
                    (t.status == (int) Agp2pEnums.RepaymentStatusEnum.Unpaid || t.status == (int) Agp2pEnums.RepaymentStatusEnum.OverTime) &&
                    t.should_repay_time.Date <= DateTime.Today).ToList();
            if (!shouldRepayTask.Any()) return;

            shouldRepayTask.ForEach(ta => context.ExecuteRepaymentTask(ta.id));
            context.AppendAdminLogAndSave("AutoRepay", "新手体验标自动还款：" + string.Join(", ", shouldRepayTask.Select(t => t.dt_users.user_name).ToArray()));
        }

        /// <summary>
        /// 新手标第一期逻辑（停用）
        /// </summary>
        /// <param name="projectTransactionId"></param>
        public static void CheckNewbieInvest(int projectTransactionId)
        {
            var context = new Agp2pDataContext();
            var ptr = context.li_project_transactions.Single(tr => tr.id == projectTransactionId);
            var project = ptr.li_projects;
            if (!project.IsNewbieProject1())
            {
                return;
            }

            if (project.repayment_type != (int) Agp2pEnums.ProjectRepaymentTypeEnum.DaoQi)
            {
                throw new InvalidOperationException("新手标只考虑了到期还款付息的情况");
            }

            if (project.li_repayment_tasks.Any(r => r.only_repay_to == ptr.investor))
            {
                throw new InvalidOperationException("已经创建过此投资者的回款计划");
            }

            // 创建针对单个用户的还款计划
            var finalProfitRate = 0.1m;// project.GetFinalProfitRate(ptr.create_time); // 暂时写死利率为 1/10，即投资 100 元有 10 元

            var ta = new li_repayment_tasks
            {
                project = project.id,
                repay_interest = Math.Round(finalProfitRate * ptr.principal, 2),
                repay_principal = ptr.principal,
                status = (byte) Agp2pEnums.RepaymentStatusEnum.Unpaid,
                term = 1,
                should_repay_time = project.CalcRepayTimeByTerm(1, ptr.create_time),
                only_repay_to = ptr.investor
            };
            context.li_repayment_tasks.InsertOnSubmit(ta);

            // 修改代收利息，添加钱包历史
            var wallet = ptr.dt_users.li_wallets;
            wallet.profiting_money += ta.repay_interest;
            wallet.last_update_time = ptr.create_time;

            var history = TransactionFacade.CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess);
            history.li_project_transactions = ptr;
            context.li_wallet_histories.InsertOnSubmit(history);

            // 如果体验标完成了，则设置为完成
            if (project.financing_amount == project.investment_amount)
            {
                project.status = (int)Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime;
                project.invest_complete_time = ptr.create_time;
                project.complete_time = ptr.create_time;
            }
            context.SubmitChanges();
        }
    }
}
