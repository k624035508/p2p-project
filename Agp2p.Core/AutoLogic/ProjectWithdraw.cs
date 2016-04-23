using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Core.PayApiLogic;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.AutoLogic
{
    public class ProjectWithdraw
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => HuoqiClaimTransferToCompanyWhenNeeded(m.TimerType, m.OnTime)); // 活期项目提现后，由中间户接手
            MessageBus.Main.Subscribe<TimerMsg>(m => DoHuoqiProjectWithdraw(m.TimerType, m.OnTime, DateTime.Now)); // 活期项目提现的执行
        }

        public static void HuoqiClaimTransferToCompanyWhenNeeded(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.AutoRepayTimer) return;

            using (var ts = new TransactionScope())
            {
                // 将需要转让的债权由中间人购买，转手之后设置为 TransferredUnpaid
                var context = new Agp2pDataContext();
                var companyUsers = context.dt_users.Where(u => u.dt_user_groups.title == AutoRepay.AgentGroup).ToList();
                if (!companyUsers.Any())
                    throw new InvalidOperationException("请先往“中间户”的会员组添加会员");

                // 接手昨日/更早的提现
                var needTransferClaims = context.li_claims.Where(
                    c =>
                        c.projectId != c.profitingProjectId && c.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                        !c.Children.Any() && c.createTime.Date < DateTime.Today)
                    .ToList();

                if (!needTransferClaims.Any()) return;

                //根据活期项目发送还款请求
                needTransferClaims.ToLookup(c => c.li_projects_profiting).ForEach(groupByProj =>
                {
                    //找出活期项目对应的中间人
                    var huoqiProj = groupByProj.Key;
                    var loaner = huoqiProj.li_risks.li_loaners.dt_users;
                    var needTransferClaimsSum = groupByProj.Sum(c => c.principal);
                    if (loaner.li_wallets.idle_money < needTransferClaimsSum)
                        throw new InvalidOperationException("警告：中间人的余额不足以接手需要转让的债权");

                    //创建自动还款托管接口请求
                    var autoRepayReqMsg = new AutoRepayReqMsg(loaner.id, huoqiProj.id, needTransferClaimsSum.ToString("f"));
                    MessageBus.Main.PublishAsync(autoRepayReqMsg, ar =>
                    {
                        //处理请求同步返回结果
                        var repayRespMsg = BaseRespMsg.NewInstance<RepayRespMsg>(autoRepayReqMsg.SynResult);
                        repayRespMsg.HuoqiRepay = true;
                        MessageBus.Main.PublishAsync(repayRespMsg, result =>
                        {
                            if(repayRespMsg.HasHandle)
                                //托管还款完成后才接手转出的债权
                                context.RecaptureHuoqiClaim(groupByProj.ToList(), DateTime.Now);
                        });
                    });
                });
                
                
                ts.Complete();
            }
        }

        public static void DoHuoqiProjectWithdraw(TimerMsg.Type timerType, bool onTime, DateTime withdrawAt)
        {
            if (timerType != TimerMsg.Type.AutoRepayTimer) return;

            var context = new Agp2pDataContext();
            var repayTime = DateTime.Now;

            // 对 withdrawDay 的前一天的 Unpaid 债权进行回款（方便在脚本中使用）
            var checkDay = withdrawAt.Date.AddDays(-1);

            // 执行未回款债权的回款，减少项目的在投金额（必须要是今日之前的提现）
            var claims = context.li_claims.Where(
                c =>
                    (c.status == (int)Agp2pEnums.ClaimStatusEnum.CompletedUnpaid ||
                     c.status == (int)Agp2pEnums.ClaimStatusEnum.TransferredUnpaid) &&
                    c.Parent.createTime.Date == checkDay && !c.Children.Any()).ToList();
            if (!claims.Any()) return;

            // 查询出昨日的全部提现及其是第几次的提现
            var yesterdayWithdraws = context.li_claims.Where(c =>
                c.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                c.Parent.status == (int)Agp2pEnums.ClaimStatusEnum.Nontransferable &&
                c.createTime.Date == checkDay)
                .GroupBy(c => c.dt_users)
                .ToDictionary(g => g.Key, g =>
                        g.Zip(Utils.Infinite(), (claim, index) => new { claim, index })
                            .ToDictionary(e => e.claim, e => e.index));

            claims.ToLookup(c => c.li_projects_profiting).ForEach(pcs =>
            {
                var huoqiProject = pcs.Key;
                //发送托管本金到账请求成功后才执行转出逻辑
                // TODO 这里只返本金
                RequestApiHandle.SendReturnPrinInte(huoqiProject.id, (pcs.Sum(pcsc => pcsc.principal)).ToString("f"), 0, false, true,  
                    () => {
                        pcs.ToLookup(c => c.dt_users).ForEach(ucs =>
                        {
                            var investor = ucs.Key;
                            var wallet = investor.li_wallets;

                            ucs.ForEach(c =>
                            {
                                Agp2pEnums.ClaimStatusEnum newStatus;
                                if (c.status == (int)Agp2pEnums.ClaimStatusEnum.CompletedUnpaid)
                                    newStatus = Agp2pEnums.ClaimStatusEnum.Completed;
                                else if (c.status == (int)Agp2pEnums.ClaimStatusEnum.TransferredUnpaid)
                                    newStatus = Agp2pEnums.ClaimStatusEnum.Transferred;
                                else
                                    throw new InvalidOperationException("活期项目 T+1 提款出错：未知的债权状态");

                                var newStatusChild = c.NewStatusChild(repayTime, newStatus);
                                context.li_claims.InsertOnSubmit(newStatusChild);

                                // 提现大于 3 次后每次提现都扣除手续费 0.25%
                                var userYesterdayWithdraws = yesterdayWithdraws[c.dt_users];
                                var parentClaim = userYesterdayWithdraws.Keys.Single(c.IsChildOf);
                                var withdrawIndex = userYesterdayWithdraws[parentClaim];
                                //TODO 手续费率需要按照配置？
                                var handlingFee = withdrawIndex <= 2 ? 0 : c.principal * 0.25m / 100;

                                var withdrawTransact = new li_project_transactions
                                {
                                    principal = c.principal - handlingFee,
                                    project = huoqiProject.id,
                                    create_time = repayTime,
                                    investor = investor.id,
                                    type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.HuoqiProjectWithdraw,
                                    status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success,
                                    gainFromClaim = c.id,
                                    remark = $"活期项目【{huoqiProject.title}】债权赎回成功：债权金额 {c.principal.ToString("c")}，赎回费 {handlingFee.ToString("c")}"
                                };
                                context.li_project_transactions.InsertOnSubmit(withdrawTransact);

                                wallet.idle_money += withdrawTransact.principal;
                                wallet.investing_money -= c.principal;
                                wallet.last_update_time = repayTime;

                                var his = TransactionFacade.CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.HuoqiProjectWithdrawSuccess);
                                his.li_project_transactions = withdrawTransact;
                                context.li_wallet_histories.InsertOnSubmit(his);
                            });
                        });
                    });
            });
            context.AppendAdminLog("HuoqiWithdraw", "今日活期项目提现成功: " + claims.Sum(c => c.principal).ToString("c"));

            context.SubmitChanges();
        }
    }
}
