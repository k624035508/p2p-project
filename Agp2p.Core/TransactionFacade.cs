using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Transactions;
using Agp2p.Common;
using Agp2p.Core.AutoLogic;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Agp2p.Core.ActivityLogic;
using Agp2p.Core.Message.UserPointMsg;

namespace Agp2p.Core
{
    /// <summary>
    /// 事务门面类；支持充值/提现，充值/提现成功确认，投资和收益功能
    /// 也支持查询项目的投资进度的功能
    /// </summary>
    public static class TransactionFacade
    {
        public const decimal StandGuardFeeRate = 0;//0.006m;
        public const decimal DefaultHandlingFee = 0;//1;
        public const int NormalProjectProfitingDay = 365;
        public const int TicketProjectProfitingDay = 360;
        public const int HuoqiProjectProfitingDay = 360;
        public static bool StaticClaimTransferToOneUser = true;

        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<UserInvestedMsg>(m => CheckFinancingComplete(m.ProjectTransactionId)); // 项目满标需要生成还款计划
            MessageBus.Main.Subscribe<ProjectRepaidMsg>(m =>
            {
                if (m.IsProjectNeedComplete)
                {
                    CompleteProject(m.RepaymentTaskId);
                }
            });
        }

        /// <summary>
        /// 充值（待确认），等待充值成功的通知
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="payApi"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static li_bank_transactions Charge(this Agp2pDataContext context, int userId, decimal money,
            Agp2pEnums.PayApiTypeEnum payApi, string remark = null, string noOrder = "")
        {
            // 创建交易记录（充值进行中）
            var tr = new li_bank_transactions
            {
                charger = userId,
                transact_time = null,
                type = (int)Agp2pEnums.BankTransactionTypeEnum.Charge,
                status = (int)Agp2pEnums.BankTransactionStatusEnum.Acting,
                value = money,
                handling_fee = 0,
                handling_fee_type = (byte)Agp2pEnums.BankTransactionHandlingFeeTypeEnum.NoHandlingFee,
                no_order = noOrder.Equals("")
                    ? Utils.GetOrderNumberLonger()
                    : noOrder,
                create_time = DateTime.Now,
                remarks = remark,
                pay_api = (byte)payApi
            };
            context.li_bank_transactions.InsertOnSubmit(tr);

            // 增加冻结资金
            var wallet = context.li_wallets.Single(b => b.user_id == userId);
            //wallet.locked_money += money;
            wallet.last_update_time = tr.create_time;

            // 创建交易历史
            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.Charging);
            his.li_bank_transactions = tr;
            context.li_wallet_histories.InsertOnSubmit(his);

            context.SubmitChanges();

            MessageBus.Main.Publish(new BankTransactionCreatedMsg(tr));
            return tr;
        }

        /// <summary>
        /// 提现（待确认），等待提现成功的通知
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bankAccountId"></param>
        /// <param name="withdrawMoney"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static li_bank_transactions Withdraw(this Agp2pDataContext context, int bankAccountId,
            decimal withdrawMoney, string remark = null, string noOrder = "")
        {
            // 提现 100 起步，50w 封顶
            if (withdrawMoney < 100)
                throw new InvalidOperationException("操作失败：提现金额最低 100 元");

            if (50 * 10000 < withdrawMoney)
                throw new InvalidOperationException("操作失败：提现金额最高 500000 元");

            // 查询可用余额，足够的话才能提现
            var account = context.li_bank_accounts.Single(b => b.id == bankAccountId);
            var user = account.dt_users;
            var wallet = user.li_wallets;
            if (wallet.idle_money < withdrawMoney)
                throw new InvalidOperationException("操作失败：用户 " + user.user_name + " 的账户余额小于需要提现的金额");

            // 判断提现次数，每人每日的提现次数不能超过 3 次
            var withdrawTimesToday = context.li_bank_transactions.Count(btr => btr.li_bank_accounts.owner == user.id
                && btr.type == (int)Agp2pEnums.BankTransactionTypeEnum.Withdraw
                && btr.status != (int)Agp2pEnums.BankTransactionStatusEnum.Cancel && btr.create_time.Date == DateTime.Today);

            if (3 <= withdrawTimesToday)
            {
                throw new InvalidOperationException("每人每日的提现次数不能超过 3 次");
            }

            // 计算出产生防套现手续费的部分 (空闲 - 未投资 = 回款，提现回款金额无需手续费)
            var unusedMoney = wallet.idle_money - wallet.unused_money <= withdrawMoney
                ? wallet.unused_money - (wallet.idle_money - withdrawMoney)
                : 0;

            // 申请提现后将可用余额冻结
            wallet.idle_money -= withdrawMoney;
            wallet.locked_money += withdrawMoney;
            wallet.unused_money -= unusedMoney;
            wallet.last_update_time = DateTime.Now;

            // 创建交易记录（提现进行中）
            var tr = new li_bank_transactions
            {
                withdraw_account = bankAccountId,
                transact_time = null,
                type = (int)Agp2pEnums.BankTransactionTypeEnum.Withdraw,
                status = (int)Agp2pEnums.BankTransactionStatusEnum.Acting,
                value = withdrawMoney,
                // 防套现手续费公式：未投资金额 * 0.6%；有防提现手续费时不能在数据库里面直接设置默认的手续费(1元)，因为提现取消的时候需要靠这个数来恢复未投资金额
                // handling_fee = unusedMoney == 0 ? DefaultHandlingFee : unusedMoney*StandGuardFeeRate,
                // 提现小于 100 元收取 DefaultHandlingFee 元手续费 TODO 暂时不使用
                //handling_fee = withdrawMoney < 100 ? DefaultHandlingFee : 0,
                handling_fee = 0,
                handling_fee_type =
                    (byte)
                        (unusedMoney == 0
                            ? Agp2pEnums.BankTransactionHandlingFeeTypeEnum.WithdrawHandlingFee
                            : Agp2pEnums.BankTransactionHandlingFeeTypeEnum.WithdrawUnusedMoneyHandlingFee),
                no_order = noOrder.Equals("") ? Utils.GetOrderNumberLonger() : noOrder,
                remarks = remark,
                create_time = wallet.last_update_time // 时间应该一致
            };
            context.li_bank_transactions.InsertOnSubmit(tr);

            // 创建交易历史
            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.Withdrawing);
            his.li_bank_transactions = tr;
            context.li_wallet_histories.InsertOnSubmit(his);

            context.SubmitChanges();

            MessageBus.Main.Publish(new BankTransactionCreatedMsg(tr));
            return tr;
        }

        /// <summary>
        /// 投资（待确认），等待提现成功的通知
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bankAccountId"></param>
        /// <param name="withdrawMoney"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static li_project_transactions InvestConfirm(this Agp2pDataContext context, int userId, int projectId, decimal investingMoney, string noOrder = "")
        {
            li_project_transactions tr;
            li_wallets wallet;
            var investTime = DateTime.Now;
            using (var ts = new TransactionScope())
            {
                var pr = context.li_projects.Single(p => p.id == projectId);

                if ((int)Agp2pEnums.ProjectStatusEnum.Financing != pr.status &&
                    (int)Agp2pEnums.ProjectStatusEnum.FinancingTimeout != pr.status)
                    throw new InvalidOperationException("项目不可投资！");
                // 判断投资金额的数额是否合理
                var canBeInvest = pr.financing_amount - pr.investment_amount;
                if (canBeInvest == 0)
                    throw new InvalidOperationException("项目已经投满");
                if (canBeInvest < investingMoney)
                    throw new InvalidOperationException("投资金额 " + investingMoney + " 超出项目可投资金额 " + canBeInvest);
                if (investingMoney < 100)
                    throw new InvalidOperationException("投资金额最低 100 元");
                if (canBeInvest != investingMoney && canBeInvest - investingMoney < 100)
                    throw new InvalidOperationException($"您投标 {investingMoney} 元后项目的可投金额（{canBeInvest - investingMoney}）低于 100 元，这样下一个人就不能投啦，所以请调整你的投标金额");


                // 修改钱包，将金额放到待收资金中，流标后再退回空闲资金
                wallet = context.li_wallets.Single(w => w.user_id == userId);

                if (wallet.idle_money < investingMoney)
                    throw new InvalidOperationException("余额不足，无法投资");

                if (pr.IsNewbieProject1())
                {
                    throw new InvalidOperationException("新手标第一期已结束。");
                }
                // 限制对新手标2期的投资，只能投资 100，只能投 10 万
                if (pr.IsNewbieProject2())
                {
                    if (investingMoney < 100)
                    {
                        throw new InvalidOperationException("新手标规定最低只能投 100 元。");
                    }
                    if (100000 < wallet.total_investment)
                    {
                        throw new InvalidOperationException("对不起，您的累计投资金额已经超过100000，不能再投资新手标！");
                    }
                    var newbieProjectInvested = wallet.dt_users.li_project_transactions.Where(tra =>
                        tra.li_projects.IsNewbieProject2() &&
                        tra.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                        tra.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest)
                        .Aggregate(0m, (sum, ptr) => sum + ptr.principal);
                    if (100000 < newbieProjectInvested + investingMoney)
                    {
                        throw new InvalidOperationException($"新手标累计投资不能超过 100000，您剩余可投 {100000 - newbieProjectInvested}元");
                    }
                }
                else if (pr.IsHuoqiProject()) // 限制对活期项目的投资，最大投 10 w
                {
                    var alreadyInvest = wallet.dt_users.li_claims.Where(c =>
                        c.profitingProjectId == projectId && c.status < (int)Agp2pEnums.ClaimStatusEnum.Completed &&
                        c.IsLeafClaim())
                        .Aggregate(0m, (sum, c) => sum + c.principal);
                    if (100000 < alreadyInvest + investingMoney)
                    {
                        throw new InvalidOperationException("对活期项目最多可投 ¥100,000，你目前已投 " + alreadyInvest.ToString("c"));
                    }
                }


                // 创建投资记录
                tr = new li_project_transactions
                {
                    dt_users = wallet.dt_users,
                    li_projects = pr,
                    type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.Invest,
                    principal = investingMoney,
                    status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Pending,
                    no_order = noOrder,
                    create_time = investTime // 时间应该一致
                };
                context.li_project_transactions.InsertOnSubmit(tr);

                wallet.last_update_time = tr.create_time;

                

                if (pr.IsHuoqiProject())
                {
                    if (wallet.dt_users.IsAgent())
                        throw new InvalidOperationException("中间人不能投资活期项目");

                    // 从中间人手上接手 可转让债权
                    try
                    {
                        BuyHuoqiClaims(context, tr.principal, tr);
                    }
                    catch (Exception ex)
                    {
                        // throw new Exception("没有足够的项目可投，超出：" + exceed);
                        ts.Dispose();
                        // 进行延迟投资
                        DelayInvestHuoqi(userId, projectId, investingMoney);
                        
                    }
                }
                else
                {
                    
                }

                context.SubmitChanges();
                ts.Complete();
            }
            //MessageBus.Main.Publish(new UserInvestedMsg(tr.id, wallet.last_update_time)); // 广播用户的投资消息
            return tr;
        }

        /// <summary>
        /// 计算站岗手续费（防套现手续费）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="withdrawMoney"></param>
        /// <returns></returns>
        public static decimal CalcStandGuardFee(this Agp2pDataContext context, int userId, decimal withdrawMoney)
        {
            var wallet = context.li_wallets.Single(w => w.user_id == userId);
            if (wallet.idle_money < withdrawMoney)
            {
                throw new Exception("提现金额超出用户余额");
            }
            // 计算出产生防套现手续费的部分 (空闲 - 未投资 = 回款，提现回款金额无需手续费)
            var unusedMoney = wallet.idle_money - wallet.unused_money <= withdrawMoney
                ? wallet.unused_money - (wallet.idle_money - withdrawMoney)
                : 0;
            return unusedMoney * StandGuardFeeRate;
        }

        /// <summary>
        /// 确认银行交易
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bankTransactionId"></param>
        /// <param name="approver"></param>
        /// <param name="saveChange"></param>
        /// <returns></returns>
        public static li_bank_transactions ConfirmBankTransaction(this Agp2pDataContext context, int bankTransactionId,
            int? approver, bool saveChange = true)
        {
            // 更新原事务（完成事务）
            var tr = context.li_bank_transactions.Single(t => t.id == bankTransactionId);
            if (tr.status != (int)Agp2pEnums.BankTransactionStatusEnum.Acting)
                throw new InvalidOperationException("该银行卡" +
                                                    Utils.GetAgp2pEnumDes((Agp2pEnums.BankTransactionTypeEnum)tr.type) +
                                                    "事务已经被确认或取消了");
            tr.status = (byte)Agp2pEnums.BankTransactionStatusEnum.Confirm;
            tr.transact_time = DateTime.Now;
            tr.approver = approver;

            if (tr.type == (int)Agp2pEnums.BankTransactionTypeEnum.Charge) // 充值确认
            {
                var wallet = tr.dt_users.li_wallets;
                // 修改钱包金额
                //wallet.locked_money -= tr.value;
                wallet.idle_money += tr.value;
                wallet.unused_money += (tr.pay_api == (byte)Agp2pEnums.PayApiTypeEnum.ManualAppend ? 0 : tr.value);
                // 手工充值 可能为活动返利，不计手续费
                wallet.total_charge += tr.value;
                wallet.last_update_time = tr.transact_time.Value; // 时间应该一致

                // 创建交易历史
                var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.ChargeConfirm);
                his.li_bank_transactions = tr;
                context.li_wallet_histories.InsertOnSubmit(his);

                //添加充值手续费 TODO 提现手续费？ 
                if (tr.pay_api != null && tr.pay_api >= (int)Agp2pEnums.PayApiTypeEnum.Ecpss)
                {
                    decimal feeFate = 0;
                    var feeConfig = ConfigLoader.loadCostConfig();
                    switch (tr.pay_api)
                    {
                        case (int)Agp2pEnums.PayApiTypeEnum.EcpssQ:
                            feeFate = tr.value * 0.005m;
                            break;
                        case (int)Agp2pEnums.PayApiTypeEnum.Ecpss:
                            feeFate = tr.value * feeConfig.recharge_fee_rate;
                            break;
                        case (int)Agp2pEnums.PayApiTypeEnum.Sumapay:
                            feeFate = tr.value * feeConfig.recharge_fee_rate;
                            if (feeFate < 3) feeFate = 3;
                            break;
                        case (int)Agp2pEnums.PayApiTypeEnum.SumapayQ:
                            feeFate = tr.value * feeConfig.recharge_fee_rate_quick;
                            if (feeFate < 3) feeFate = 3;
                            break;
                    }

                    var rechangerFee = new li_company_inoutcome()
                    {
                        create_time = DateTime.Now,
                        user_id = (int)tr.charger,
                        outcome = feeFate,
                        type = (int)Agp2pEnums.OfflineTransactionTypeEnum.ReChangeFee,
                        remark = Utils.GetAgp2pEnumDes((Agp2pEnums.PayApiTypeEnum)tr.pay_api) + "充值手续费"
                    };
                    context.li_company_inoutcome.InsertOnSubmit(rechangerFee);
                }
                context.SubmitChanges();
            }
            else if (tr.type == (int)Agp2pEnums.BankTransactionTypeEnum.Withdraw) // 提款确认
            {
                var wallet = tr.li_bank_accounts.dt_users.li_wallets;
                // 修改钱包金额
                wallet.locked_money -= tr.value;
                wallet.total_withdraw += tr.value;
                wallet.last_update_time = tr.transact_time.Value;

                // 创建交易历史
                var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.WithdrawConfirm);
                his.li_bank_transactions = tr;
                context.li_wallet_histories.InsertOnSubmit(his);
            }
            else throw new InvalidEnumArgumentException("未知银行交易类型");
            if (saveChange) // 注意：外部保存话需要自己发送通知
            {
                context.SubmitChanges();
                MessageBus.Main.Publish(new BankTransactionFinishedMsg(tr));
            }
            return tr;
        }

        /// <summary>
        /// 确认银行交易
        /// </summary>
        /// <param name="context"></param>
        /// <param name="projectTransactionId"></param>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        public static li_project_transactions ConfirmProjectTransaction(this Agp2pDataContext context, int projectTransactionId, int ticketId)
        {
            var tr = context.li_project_transactions.Single(p => p.id == projectTransactionId);
            var pr = context.li_projects.Single(r => r.id == tr.project);
            var investingMoney = tr.principal;
            var wallet = context.li_wallets.Single(w => w.user_id == tr.investor);

            if (tr.status != (int) Agp2pEnums.ProjectTransactionStatusEnum.Pending)
            {
                throw new InvalidOperationException("该交易已被确认");
            }
            tr.status = (int) Agp2pEnums.ProjectTransactionStatusEnum.Success;
            if (ticketId != 0)
            {
                var atr = context.li_activity_transactions.SingleOrDefault(a =>
                    a.user_id == tr.investor &&
                    a.id == ticketId);
                if (atr == null)
                    throw new InvalidOperationException("找不到该活动券");
                if (atr.status != (int)Agp2pEnums.ActivityTransactionStatusEnum.Acting)
                    throw new InvalidOperationException("该活动券不可用");
                if (atr.activity_type == (byte)Agp2pEnums.ActivityTransactionActivityTypeEnum.TrialTicket)
                {
                    var ticket = new TrialTicketActivity.TrialTicket(atr);
                    if (ticket.GetTicketValue() != tr.principal)
                        throw new InvalidOperationException("投资的金额应等于体验券的面值");
                    ticket.Use(context, tr.project);
                }
                else if (atr.activity_type == (byte)Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket)
                {
                    var ticket = new InterestRateTicketActivity.InterestRateTicket(atr);
                    ticket.Use(context, tr.project, tr.principal);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            // 修改项目已投资金额
            pr.investment_amount += investingMoney;

            // 满标时再计算待收益金额
            wallet.idle_money -= investingMoney;
            wallet.unused_money -= Math.Min(investingMoney, wallet.unused_money); // 投资的话优先使用未投资金额，再使用回款金额
            wallet.investing_money += investingMoney;
            wallet.total_investment += investingMoney;
            wallet.last_update_time = DateTime.Now;

            // 创建债权
            var liClaims = new li_claims
            {
                li_project_transactions_invest = tr,
                createTime = wallet.last_update_time,
                projectId = tr.project,
                principal = investingMoney,
                status = (byte)Agp2pEnums.ClaimStatusEnum.Nontransferable,
                userId = wallet.user_id,
                profitingProjectId = tr.project,
                number = Utils.HiResNowString
            };
            context.li_claims.InsertOnSubmit(liClaims);

            // 修改钱包历史
            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.Invest);
            his.li_project_transactions = tr;
            context.li_wallet_histories.InsertOnSubmit(his);

            //如果首次投资，创建首次投资积分记录
            if (!context.li_project_transactions.Any(p => p.investor == tr.investor))
            {
                MessageBus.Main.Publish(new UserPointMsg(tr.investor, wallet.dt_users.user_name, (int)Agp2pEnums.PointEnum.FirstInvest));
            }
            context.SubmitChanges();
            MessageBus.Main.Publish(new UserInvestedMsg(tr.id, wallet.last_update_time)); // 广播用户的投资消息

            MessageBus.Main.Publish(new UserPointMsg(tr.investor, wallet.dt_users.user_name, (int)Agp2pEnums.PointEnum.Invest, (int)investingMoney * 28 / 100));  //投资送积分
            return tr;
        }


        /// <summary>
        /// 取消银行交易
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bankTransactionId"></param>
        /// <param name="approver"></param>
        /// <param name="saveChange"></param>
        /// <returns></returns>
        public static li_bank_transactions CancelBankTransaction(this Agp2pDataContext context, int bankTransactionId,
            int approver, bool saveChange = true)
        {
            // 更新原事务（完成事务）
            var tr = context.li_bank_transactions.Single(t => t.id == bankTransactionId);
            if (tr.status != (int)Agp2pEnums.BankTransactionStatusEnum.Acting)
                throw new InvalidOperationException("该银行卡" +
                                                    Utils.GetAgp2pEnumDes((Agp2pEnums.BankTransactionTypeEnum)tr.type) +
                                                    "事务已经被确认或取消了");
            tr.status = (byte)Agp2pEnums.BankTransactionStatusEnum.Cancel;
            tr.transact_time = DateTime.Now;
            tr.approver = approver;

            if (tr.type == (int)Agp2pEnums.BankTransactionTypeEnum.Charge) // 充值取消
            {
                var wallet = tr.dt_users.li_wallets;
                // 修改钱包金额
                //wallet.locked_money -= tr.value;
                wallet.last_update_time = tr.transact_time.Value; // 时间应该一致

                // 创建交易历史
                var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.ChargeCancel);
                his.li_bank_transactions = tr;
                context.li_wallet_histories.InsertOnSubmit(his);
            }
            else if (tr.type == (int)Agp2pEnums.BankTransactionTypeEnum.Withdraw) // 提款取消
            {
                var wallet = tr.li_bank_accounts.dt_users.li_wallets;
                // 修改钱包金额
                wallet.locked_money -= tr.value;
                wallet.idle_money += tr.value;
                if (tr.handling_fee_type ==
                    (int)Agp2pEnums.BankTransactionHandlingFeeTypeEnum.WithdrawUnusedMoneyHandlingFee)
                {
                    wallet.unused_money += StandGuardFeeRate == 0 ? 0 : tr.handling_fee / StandGuardFeeRate; // 恢复防套现手续费的部分
                }
                wallet.last_update_time = tr.transact_time.Value;

                // 创建交易历史
                var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.WithdrawCancel);
                his.li_bank_transactions = tr;
                context.li_wallet_histories.InsertOnSubmit(his);
            }
            if (saveChange) // 注意：外部保存话需要自己发送通知
            {
                context.SubmitChanges();
                MessageBus.Main.Publish(new BankTransactionFinishedMsg(tr));
            }
            return tr;
        }

        /// <summary>
        /// 根据钱包数据生成钱包历史
        /// </summary>
        /// <param name="wallet"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static li_wallet_histories CloneFromWallet(li_wallets wallet, Agp2pEnums.WalletHistoryTypeEnum actionType)
        {
            return new li_wallet_histories
            {
                user_id = wallet.user_id,
                action_type = (byte)actionType,
                idle_money = wallet.idle_money,
                locked_money = wallet.locked_money,
                investing_money = wallet.investing_money,
                profiting_money = wallet.profiting_money,
                total_investment = wallet.total_investment,
                total_profit = wallet.total_profit,
                create_time = wallet.last_update_time
            };
        }

        /// <summary>
        /// 因为活期债权不一定足够，需要延期投资。投资的时候会冻结用户资金，发短信提醒中间人，活期债权增加的时候再自动买入
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <param name="investingMoney"></param>
        public static void DelayInvestHuoqi(int userId, int projectId, decimal investingMoney)
        {
            var context = new Agp2pDataContext();
            var user = context.dt_users.Single(u => u.id == userId);
            var pr = context.li_projects.Single(p => p.id == projectId);
            if (!pr.IsHuoqiProject())
                throw new InvalidOperationException("延期投资只能用于活期项目");

            var wallet = user.li_wallets;

            if (wallet.idle_money < investingMoney)
                throw new InvalidOperationException("余额不足，无法投资");

            // 修改项目已投资金额
            pr.investment_amount += investingMoney;

            var now = DateTime.Now;

            wallet.idle_money -= investingMoney;
            wallet.locked_money += investingMoney;
            wallet.last_update_time = now;

            var tr = new li_project_transactions
            {
                dt_users = wallet.dt_users,
                li_projects = pr,
                type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.Invest,
                principal = investingMoney,
                status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Pending,
                create_time = wallet.last_update_time // 时间应该一致
            };
            context.li_project_transactions.InsertOnSubmit(tr);

            // 修改钱包历史
            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.Invest);
            his.li_project_transactions = tr;
            context.li_wallet_histories.InsertOnSubmit(his);

            context.SubmitChanges();

            // 广播用户的投资消息，并且发送通知中间人的短信
            MessageBus.Main.Publish(new UserInvestedMsg(tr.id, wallet.last_update_time, true));
        }

        public static void DelayInvestSuccess(int projectTransactionId)
        {
            var context = new Agp2pDataContext();

            var ptr = context.li_project_transactions.Single(ptr0 => ptr0.id == projectTransactionId);
            if (ptr.type != (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest && ptr.status != (int)Agp2pEnums.ProjectTransactionStatusEnum.Pending)
                throw new InvalidOperationException("交易记录类型不正确");

            var now = DateTime.Now;

            ptr.status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success;

            var wallet = ptr.dt_users.li_wallets;
            wallet.locked_money -= ptr.principal;
            wallet.investing_money += ptr.principal;
            wallet.total_investment += ptr.principal;
            wallet.last_update_time = now;

            // 修改钱包历史
            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.DelayInvestSuccess);
            his.li_project_transactions = ptr;
            context.li_wallet_histories.InsertOnSubmit(his);

            BuyHuoqiClaims(context, ptr.principal, ptr);

            context.SubmitChanges();
        }

        public static void DelayInvestFailure(int projectTransactionId)
        {
            var context = new Agp2pDataContext();
            var ptr = context.li_project_transactions.Single(tr0 => tr0.id == projectTransactionId);
            if (ptr.type != (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest && ptr.status != (int)Agp2pEnums.ProjectTransactionStatusEnum.Pending)
                throw new InvalidOperationException("交易记录类型不正确");

            // 修改项目已投资金额
            ptr.li_projects.investment_amount -= ptr.principal;

            ptr.status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Rollback;

            var now = DateTime.Now;
            var wallet = ptr.dt_users.li_wallets;

            wallet.idle_money += ptr.principal;
            wallet.locked_money -= ptr.principal;
            wallet.last_update_time = now;

            // 修改钱包历史
            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.InvestorRefund);
            his.li_project_transactions = ptr;
            context.li_wallet_histories.InsertOnSubmit(his);

            context.SubmitChanges();
        }

        /// <summary>
        /// 投资
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <param name="investingMoney"></param>
        public static void Invest(int userId, int projectId, decimal investingMoney, string noOrder = "", int ticketId = 0)
        {
            li_project_transactions tr;
            li_wallets wallet;
            var investTime = DateTime.Now;
            using (var ts = new TransactionScope())
            {
                var context = new Agp2pDataContext();
                var pr = context.li_projects.Single(p => p.id == projectId);

                if ((int)Agp2pEnums.ProjectStatusEnum.Financing != pr.status &&
                    (int)Agp2pEnums.ProjectStatusEnum.FinancingTimeout != pr.status)
                    throw new InvalidOperationException("项目不可投资！");
                // 判断投资金额的数额是否合理
                var canBeInvest = pr.financing_amount - pr.investment_amount;
                if (canBeInvest == 0)
                    throw new InvalidOperationException("项目已经投满");
                if (canBeInvest < investingMoney)
                    throw new InvalidOperationException("投资金额 " + investingMoney + " 超出项目可投资金额 " + canBeInvest);
                if (investingMoney < 100)
                    throw new InvalidOperationException("投资金额最低 100 元");
                if (canBeInvest != investingMoney && canBeInvest - investingMoney < 100)
                    throw new InvalidOperationException($"您投标 {investingMoney} 元后项目的可投金额（{canBeInvest - investingMoney}）低于 100 元，这样下一个人就不能投啦，所以请调整你的投标金额");

                if (ticketId != 0)
                {
                    var atr = context.li_activity_transactions.SingleOrDefault(a =>
                        a.user_id == userId &&
                        a.id == ticketId);
                    if (atr == null)
                        throw new InvalidOperationException("找不到该活动券");
                    if (atr.status != (int)Agp2pEnums.ActivityTransactionStatusEnum.Acting)
                        throw new InvalidOperationException("该活动券不可用");
                    if (atr.activity_type == (byte) Agp2pEnums.ActivityTransactionActivityTypeEnum.TrialTicket)
                    {
                        var ticket = new TrialTicketActivity.TrialTicket(atr);
                        if (ticket.GetTicketValue() != investingMoney)
                            throw new InvalidOperationException("投资的金额应等于体验券的面值");
                        ticket.Use(context, projectId);
                        ts.Complete();
                        return;
                    }
                    else if (atr.activity_type == (byte) Agp2pEnums.ActivityTransactionActivityTypeEnum.InterestRateTicket)
                    {
                        var ticket = new InterestRateTicketActivity.InterestRateTicket(atr);
                        ticket.Use(context, projectId, investingMoney);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                // 修改钱包，将金额放到待收资金中，流标后再退回空闲资金
                wallet = context.li_wallets.Single(w => w.user_id == userId);

                if (wallet.idle_money < investingMoney)
                    throw new InvalidOperationException("余额不足，无法投资");

                if (pr.IsNewbieProject1())
                {
                    throw new InvalidOperationException("新手标第一期已结束。");
                }
                // 限制对新手标2期的投资，只能投资 100，只能投 10 万
                if (pr.IsNewbieProject2())
                {
                    if (investingMoney < 100)
                    {
                        throw new InvalidOperationException("新手标规定最低只能投 100 元。");
                    }
                    if (100000 < wallet.total_investment)
                    {
                        throw new InvalidOperationException("对不起，您的累计投资金额已经超过100000，不能再投资新手标！");
                    }
                    var newbieProjectInvested = wallet.dt_users.li_project_transactions.Where(tra =>
                        tra.li_projects.IsNewbieProject2() &&
                        tra.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                        tra.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest)
                        .Aggregate(0m, (sum, ptr) => sum + ptr.principal);
                    if (100000 < newbieProjectInvested + investingMoney)
                    {
                        throw new InvalidOperationException($"新手标累计投资不能超过 100000，您剩余可投 {100000 - newbieProjectInvested}元");
                    }
                }
                else if (pr.IsHuoqiProject()) // 限制对活期项目的投资，最大投 10 w
                {
                    var alreadyInvest = wallet.dt_users.li_claims.Where(c =>
                        c.profitingProjectId == projectId && c.status < (int) Agp2pEnums.ClaimStatusEnum.Completed &&
                        c.IsLeafClaim())
                        .Aggregate(0m, (sum, c) => sum + c.principal);
                    if (100000 < alreadyInvest + investingMoney)
                    {
                        throw new InvalidOperationException("对活期项目最多可投 ¥100,000，你目前已投 " + alreadyInvest.ToString("c"));
                    }
                }

                // 修改项目已投资金额
                pr.investment_amount += investingMoney;

                // 满标时再计算待收益金额
                wallet.idle_money -= investingMoney;
                wallet.unused_money -= Math.Min(investingMoney, wallet.unused_money); // 投资的话优先使用未投资金额，再使用回款金额
                wallet.investing_money += investingMoney;
                wallet.total_investment += investingMoney;
                wallet.last_update_time = investTime;

                //如果首次投资，创建首次投资积分记录
                if (!context.li_project_transactions.Any(p => p.investor == userId))
                {
                    MessageBus.Main.Publish(new UserPointMsg(userId, wallet.dt_users.user_name, (int)Agp2pEnums.PointEnum.FirstInvest));
                }
                // 创建投资记录
                tr = new li_project_transactions
                {
                    dt_users = wallet.dt_users,
                    li_projects = pr,
                    type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.Invest,
                    principal = investingMoney,
                    status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success,
                    no_order = noOrder,
                    create_time = wallet.last_update_time // 时间应该一致
                };
                context.li_project_transactions.InsertOnSubmit(tr);
                
                // 修改钱包历史
                var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.Invest);
                his.li_project_transactions = tr;
                context.li_wallet_histories.InsertOnSubmit(his);

                if (pr.IsHuoqiProject())
                {
                    if (wallet.dt_users.IsAgent())
                        throw new InvalidOperationException("中间人不能投资活期项目");

                    // 从中间人手上接手 可转让债权
                    try
                    {
                        BuyHuoqiClaims(context, tr.principal, tr);
                    }
                    catch (Exception ex)
                    {
                        // throw new Exception("没有足够的项目可投，超出：" + exceed);
                        ts.Dispose();
                        // 进行延迟投资
                        DelayInvestHuoqi(userId, projectId, investingMoney);
                        return;
                    }
                }
                else
                {
                    // 创建债权
                    var liClaims = new li_claims
                    {
                        li_project_transactions_invest = tr,
                        createTime = wallet.last_update_time,
                        projectId = projectId,
                        principal = investingMoney,
                        status = (byte)Agp2pEnums.ClaimStatusEnum.Nontransferable,
                        userId = wallet.user_id,
                        profitingProjectId = projectId,
                        number = Utils.HiResNowString
                    };
                    context.li_claims.InsertOnSubmit(liClaims);
                }

                context.SubmitChanges();
                ts.Complete();
            }
            MessageBus.Main.Publish(new UserInvestedMsg(tr.id, wallet.last_update_time)); // 广播用户的投资消息
            MessageBus.Main.Publish(new UserPointMsg(userId, wallet.dt_users.user_name, (int)Agp2pEnums.PointEnum.Invest));  //投资送积分
        }

        /// <summary>
        /// 收回活期债权
        /// </summary>
        /// <param name="context"></param>
        /// <param name="huoqiClaims"></param>
        /// <param name="value"></param>
        /// <returns>收回的债权</returns>
        internal static List<li_claims> RecaptureHuoqiClaim(this Agp2pDataContext context, List<li_claims> huoqiClaims, DateTime moment)
        {
            var recapturedClaims = huoqiClaims.GroupBy(c => c.li_projects).SelectMany(claimGroupByProject =>
            {
                // 项目应该处于还款中/已完成 的状态
                var staticProject = claimGroupByProject.Key;
                Debug.Assert((int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying <= staticProject.status);
                Debug.Assert(!staticProject.IsHuoqiProject());

                return claimGroupByProject.GroupBy(c => c.dt_users_agent).Select(claimGroupByAgent =>
                 {
                     var agent = claimGroupByAgent.Key;
                     var investingMoney = claimGroupByAgent.Sum(c => c.principal);

                     // 修改钱包，将金额放到待收资金中，流标后再退回空闲资金
                     var agentWallet = agent.li_wallets;

                     // 判断投资金额的数额是否合理
                     if (agentWallet.idle_money < investingMoney)
                         throw new InvalidOperationException("余额不足，无法转让债权");

                     // 修改钱包金额
                     agentWallet.idle_money -= investingMoney;
                     // 收回普通用户的活期项目不会导致中间人的在投金额增加
                     // wallet.investing_money += investingMoney;
                     agentWallet.last_update_time = moment;

                     // 创建投资记录
                     var tr = new li_project_transactions
                     {
                         dt_users = agent,
                         li_projects = staticProject,
                         project = staticProject.id,
                         type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.AgentRecaptureHuoqiClaims,
                         principal = investingMoney,
                         status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success,
                         create_time = agentWallet.last_update_time // 时间应该一致
                     };
                     context.li_project_transactions.InsertOnSubmit(tr);

                     // 修改钱包历史
                     var his = CloneFromWallet(agentWallet, Agp2pEnums.WalletHistoryTypeEnum.AgentRecaptureHuoqiClaims);
                     his.li_project_transactions = tr;
                     context.li_wallet_histories.InsertOnSubmit(his);

                     // 进行债权转让
                     claimGroupByAgent.ForEach(originalClaim =>
                     {
                         // 这里的逻辑与 ClaimTransfer 大致相同，稍有不同
                         var transactTime = agentWallet.last_update_time;
                         var amount = originalClaim.principal;

                         var takeoverPart = originalClaim.TransferedChild(transactTime, Agp2pEnums.ClaimStatusEnum.Transferable, amount, tr);
                         takeoverPart.agent = null;
                         context.li_claims.InsertOnSubmit(takeoverPart);

                         // 转让了提现中的债权，由于提现 T + 1 的缘故，债权需要标记为 Unpaid
                         if (originalClaim.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer)
                         {
                             var transferedChild = originalClaim.NewPrincipalAndStatusChild(transactTime, Agp2pEnums.ClaimStatusEnum.TransferredUnpaid, amount);
                             context.li_claims.InsertOnSubmit(transferedChild);
                         }
                         else
                         {
                             var transferedChild = originalClaim.NewPrincipalAndStatusChild(transactTime, Agp2pEnums.ClaimStatusEnum.Transferred, amount);
                             context.li_claims.InsertOnSubmit(transferedChild);

                             // 处理债权转让的本金交易
                             var claimTransferPtr = new li_project_transactions
                             {
                                 principal = amount,
                                 project = originalClaim.projectId,
                                 create_time = transactTime,
                                 investor = originalClaim.userId,
                                 type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredOut,
                                 status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success,
                                 gainFromClaim = originalClaim.id,
                             };
                             context.li_project_transactions.InsertOnSubmit(claimTransferPtr);

                             var wallet = originalClaim.dt_users.li_wallets;
                             wallet.idle_money += amount;
                             // 与 ClaimTransfer 不同之处：这里需要减去已投金额
                             wallet.investing_money -= amount;
                             wallet.last_update_time = transactTime;

                             // 修改钱包历史
                             var outhis = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredOut);
                             outhis.li_project_transactions = claimTransferPtr;
                             context.li_wallet_histories.InsertOnSubmit(outhis);
                         }
                     });
                     return tr;
                 });
            }).SelectMany(ptr => ptr.li_claims_invested).ToList();

            context.SubmitChanges();

            return recapturedClaims;
        }

        public static void StaticProjectWithdraw(Agp2pDataContext context, int claimId, decimal keepInterestPercent)
        {
            // 定期债权转让申请：将债权设置为 NeedTransfer，48 小时后再还原为 Nontransferable
            /*  1）逾期债权均不得转让；TODO 托管实现之后再判断
                2）持有（指投资项目成功或购买转让项目）超过15天；
                3）债权在转让过程中变为逾期状态，剩余未转让债权将停止转让；
                4）转让申请日至少在下一个还款日/结息日的1天之前。*/

            if (keepInterestPercent < 0 || 1 < keepInterestPercent)
                throw new InvalidOperationException("无效的应收利息值，应为：0~1，代表完全转让利息/不转让利息");

            var claim = context.li_claims.Single(c => c.id == claimId);
            if (claim.dt_users.IsAgent())
                throw new InvalidOperationException("中间人不能转出债权");

            var timeSpan = DateTime.Today - claim.GetSourceClaim().createTime.Date;
            if (timeSpan.TotalDays < 15 && !claim.dt_users.IsCompanyAccount())
                throw new InvalidOperationException("你必须持有该债权满 15 日才能进行转让");

            var nextRepayTime = claim.li_projects.li_repayment_tasks.FirstOrDefault(ta => ta.IsUnpaid())?.should_repay_time;
            if (nextRepayTime == null)
                throw new InvalidOperationException("项目已经完成还款，不能进行债权转让申请");
            if (nextRepayTime.Value.Date <= DateTime.Today.AddDays(1))
                throw new InvalidOperationException("不能在项目还款日的 1 天前申请债权转让");

            using (var ts = new TransactionScope())
            {
                var needTransferClaim = claim.NewStatusChild(DateTime.Now, Agp2pEnums.ClaimStatusEnum.NeedTransfer);
                context.li_claims.InsertOnSubmit(needTransferClaim);
                context.SubmitChanges();

                if (keepInterestPercent != 0)
                {
                    // 将利息留给受让人，以便更快转出债权
                    var withdrawerProfitted = QueryWithdrawClaimFinalInterest(needTransferClaim);
                    needTransferClaim.keepInterest = keepInterestPercent * withdrawerProfitted;
                }

                context.SubmitChanges();
                ts.Complete();
            }
        }

        public static decimal QueryWithdrawClaimFinalInterest(li_claims needTransferClaim)
        {
            var currentRepaymentTask = needTransferClaim.li_projects.li_repayment_tasks.First(t => t.IsUnpaid());

            var agentPaidInterest = currentRepaymentTask.li_projects.GetClaimRatio(
                    new[] { needTransferClaim.Parent.createTime, currentRepaymentTask.GetStartProfitingTime() }.Max())
                .ReplaceKey(needTransferClaim.Parent, needTransferClaim)
                .GenerateRepayTransactions(currentRepaymentTask, currentRepaymentTask.should_repay_time).Single(ptr =>
                {
                    if (ptr.gainFromClaim == needTransferClaim.id)
                        return true;
                    return needTransferClaim.li_projects.li_claims.Single(c => c.id == ptr.gainFromClaim.Value).IsParentOf(needTransferClaim);
                }).interest.GetValueOrDefault();
            return agentPaidInterest;
        }

        public static void BuyClaim(Agp2pDataContext context, int claimId, int buyerId, decimal amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("请输入合适的债权购买金额");

            // 购买债权，创建 项目交易记录，类别为债权买入，以便添加钱包历史
            var needTransferClaim = context.li_claims.Single(c => c.id == claimId);

            var buyedTrs = needTransferClaim.li_project_transactions_profiting.Where(
                ptr =>
                    ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn &&
                    ptr.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Pending).ToList();

            var financingAmount = needTransferClaim.principal + needTransferClaim.keepInterest.GetValueOrDefault();

            var remainBuyable = financingAmount - buyedTrs.Aggregate(0m, (sum, tr) => sum + tr.principal);

            if (StaticClaimTransferToOneUser)
            {
                if (amount < remainBuyable)
                    throw new InvalidOperationException("您必须完全买入此债权");
                if (remainBuyable < amount)
                    throw new InvalidOperationException("请输入合适的债权购买金额");
            }
            else
            {
                if (remainBuyable < amount)
                    throw new InvalidOperationException("买入的金额大于债权剩余可买入的金额");
                if (100 <= remainBuyable && amount < 100)
                    throw new InvalidOperationException("买入债权的金额最低为 100 元");
                if (remainBuyable < 100 && amount != remainBuyable)
                    throw new InvalidOperationException("由于购买债权的最低金额为 100 元，但鉴于债权的可买入金额已低于 100 元，所以您必须全额买入该债权");
                if (remainBuyable != amount && remainBuyable - amount < 100)
                    throw new InvalidOperationException("您买入债权后债权的可投金额将低于 100 元，这样下一个人就不能买啦，所以请调整你的投标金额");
            }

            var buyer = context.dt_users.Single(u => u.id == buyerId);
            var buyInPtr = new li_project_transactions
            {
                status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Pending,
                type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn,
                principal = amount,
                create_time = DateTime.Now,
                dt_users = buyer,
                project = needTransferClaim.projectId,
                gainFromClaim = needTransferClaim.id
            };
            context.li_project_transactions.InsertOnSubmit(buyInPtr);

            // 修改钱包历史
            var wallet = buyer.li_wallets;
            if (wallet.idle_money < amount)
                throw new InvalidOperationException("余额不足以买入债权");

            wallet.idle_money -= amount;
            wallet.last_update_time = buyInPtr.create_time;

            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredIn);
            his.li_project_transactions = buyInPtr;
            context.li_wallet_histories.InsertOnSubmit(his);

            // 如果债权完全买入，则进行债权转让
            if (remainBuyable == amount)
            {
                StaticClaimTransferComplete(context, needTransferClaim, buyedTrs.Concat(new[] { buyInPtr }).ToList());
            }
            context.SubmitChanges();

            if (remainBuyable == amount)
                MessageBus.Main.Publish(new StaticClaimTransferSuccessMsg(needTransferClaim.id));
        }

        /// <summary>
        /// 定期项目债权转让取消
        /// </summary>
        /// <param name="context"></param>
        /// <param name="claimId"></param>
        public static void StaticClaimWithdrawCancel(Agp2pDataContext context, int claimId, bool save = true)
        {
            // 还原债权状态、归还买入者的买入金额
            var withdrawingClaim = context.li_claims.Single(c => c.id == claimId);
            Debug.Assert(withdrawingClaim.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer && withdrawingClaim.projectId == withdrawingClaim.profitingProjectId);

            var now = DateTime.Now;

            // 注意：创建时间是提现前的债权创建时间
            var newStatusChild = withdrawingClaim.NewStatusChild(withdrawingClaim.Parent.createTime, Agp2pEnums.ClaimStatusEnum.Nontransferable);
            context.li_claims.InsertOnSubmit(newStatusChild);

            withdrawingClaim.li_project_transactions_profiting.Where(
                ptr =>
                    ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn &&
                    ptr.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Pending).ForEach(buyInPtr =>
                   {
                       buyInPtr.status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Rollback;

                       var buyerWallet = buyInPtr.dt_users.li_wallets;
                       buyerWallet.idle_money += buyInPtr.principal;
                       buyerWallet.last_update_time = now;

                       var his = CloneFromWallet(buyerWallet, Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredInFail);
                       his.li_project_transactions = buyInPtr;
                       context.li_wallet_histories.InsertOnSubmit(his);
                   });

            if (save)
            {
                context.SubmitChanges();
            }
        }

        public static decimal QueryOriginalClaimFinalInterest(li_claims needTransferClaim)
        {
            var tasks = needTransferClaim.li_projects.li_repayment_tasks.Where(t => needTransferClaim.createTime < t.should_repay_time).ToList();
            var currentRepaymentTask = tasks.First();
            var claimRatio = needTransferClaim.li_projects.GetClaimRatio(new[] { needTransferClaim.Parent.createTime, currentRepaymentTask.GetStartProfitingTime() }.Max());
            return tasks.Select(task =>
            {
                return claimRatio
                    .GenerateRepayTransactions(task, task.should_repay_time)
                    .Single(ptr =>
                    {
                        if (ptr.gainFromClaim == needTransferClaim.id)
                            return true;
                        return
                            needTransferClaim.li_projects.li_claims.Single(c => c.id == ptr.gainFromClaim.Value).IsParentOf(needTransferClaim);
                    }).interest.GetValueOrDefault();
            }).Sum();
        }

        private static void StaticClaimTransferComplete(Agp2pDataContext context, li_claims needTransferClaim, List<li_project_transactions> buyedTrs)
        {
            var now = DateTime.Now;

            // 提现债权完成，创建“已转让”债权
            var transferredClaim = needTransferClaim.NewStatusChild(now, Agp2pEnums.ClaimStatusEnum.Transferred);
            context.li_claims.InsertOnSubmit(transferredClaim);

            // 原债权人减少原本债权产生的待收利息，并收回本金、利息（减去了手续费后）
            var unpaidTasks = needTransferClaim.li_projects.li_repayment_tasks.Where(t => t.IsUnpaid()).ToList();
            var currentRepaymentTask = unpaidTasks.First();
            // 取得原债权本金对应的应收利润
            var originalClaimFinalInterest = QueryOriginalClaimFinalInterest(needTransferClaim);

            // 根据债权计息时长来取得应收利息，这部分利息是受让人支付的
            // 出让人保留的利息
            var buyerPaidInterest = needTransferClaim.keepInterest.GetValueOrDefault();

            // 创建提现人收益记录，扣取手续费
            var staticWithdrawCostPercent = ConfigLoader.loadCostConfig().static_withdraw;
            var finalCost = Math.Round((needTransferClaim.principal + buyerPaidInterest) * staticWithdrawCostPercent, 2);

            if (0 < finalCost)
            {
                context.li_company_inoutcome.InsertOnSubmit(new li_company_inoutcome
                {
                    user_id = needTransferClaim.userId,
                    income = finalCost,
                    project_id = needTransferClaim.projectId,
                    type = (int)Agp2pEnums.OfflineTransactionTypeEnum.StaticClaimTransfer,
                    create_time = now,
                    remark = $"债权'{needTransferClaim.Parent.number}'转让成功，收取债权转让管理费",
                });
            }

            // 提现时债权所产生的实际收益
            var withdrawerProfitted = QueryWithdrawClaimFinalInterest(needTransferClaim);
            var claimTransferredOutPtr = new li_project_transactions
            {
                investor = needTransferClaim.userId,
                principal = needTransferClaim.principal - finalCost,
                type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredOut,
                status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success,
                interest = buyerPaidInterest,
                create_time = now,
                li_claims_from = transferredClaim,
                project = needTransferClaim.projectId,
                remark = $"债权 {needTransferClaim.principal.ToString("n")} 转让成功，折让 {(withdrawerProfitted - buyerPaidInterest).ToString("n")}，" +
                         needTransferClaim.GetProfitingSectionDays(currentRepaymentTask,
                             (d0, dProfiting, dAfter) =>
                                 $"原收益天数：{d0 + dProfiting + dAfter}，实际收益天数：{dProfiting}") + "，手续费：" + finalCost.ToString("n")
            };
            context.li_project_transactions.InsertOnSubmit(claimTransferredOutPtr);

            // 修改钱包以及创建钱包历史
            var ownerWallet = needTransferClaim.dt_users.li_wallets;
            ownerWallet.idle_money += claimTransferredOutPtr.principal + claimTransferredOutPtr.interest.GetValueOrDefault();
            ownerWallet.investing_money -= needTransferClaim.principal;
            // 再次转让的话上一个转让人的利息不归再次出让人
            ownerWallet.profiting_money -= originalClaimFinalInterest;
            ownerWallet.total_profit += claimTransferredOutPtr.interest.GetValueOrDefault();
            ownerWallet.last_update_time = claimTransferredOutPtr.create_time;

            var transferSuccessHistory = CloneFromWallet(ownerWallet, Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredOut);
            transferSuccessHistory.li_project_transactions = claimTransferredOutPtr;
            context.li_wallet_histories.InsertOnSubmit(transferSuccessHistory);


            // 为 债权受让人 生成债权、计算代收利息、创建钱包历史
            var remainProfitingOfClaim = originalClaimFinalInterest;
            var financingAmount = needTransferClaim.principal + buyerPaidInterest;
            var buyerProfitingsBeforeRounding = buyedTrs.Select(ptr => remainProfitingOfClaim * ptr.principal / financingAmount).ToList();
            var buyerProfitings = Utils.GetPerfectRounding(buyerProfitingsBeforeRounding, remainProfitingOfClaim, 2);

            // 由于买入债权时有一部分可能是利息，这部分和买入债权的本金部分要按各用户的买入比例分开
            var interestRatio = buyerPaidInterest / (needTransferClaim.principal + buyerPaidInterest);
            var buyerInterestPartsNotRound = buyedTrs.Select(tr => tr.principal * interestRatio).ToList();
            var buyerInterestParts = Utils.GetPerfectRounding(buyerInterestPartsNotRound, buyerPaidInterest, 2);

            var zip = buyerProfitings.Zip(buyerInterestParts, (profiting, buyerPaidInterestPart) => new { profiting, buyerPaidInterestPart });
            buyedTrs.ZipEach(zip, (buyTr, pi) =>
            {
                // 由中间户买入的债权为可转让债权，可以用作活期债权
                // 按提现时候开始计息，所以债权的创建时间为提现的时间
                var transferedChild = needTransferClaim.TransferedChild(needTransferClaim.createTime,
                    buyTr.dt_users.IsAgent()
                        ? Agp2pEnums.ClaimStatusEnum.Transferable
                        : Agp2pEnums.ClaimStatusEnum.Nontransferable,
                    buyTr.principal - pi.buyerPaidInterestPart, buyTr);
                context.li_claims.InsertOnSubmit(transferedChild);

                buyTr.status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success;

                var wallet = buyTr.dt_users.li_wallets;
                wallet.profiting_money += pi.profiting;
                wallet.investing_money += transferedChild.principal;
                wallet.total_investment += transferedChild.principal;
                wallet.last_update_time = now;

                var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredInSuccess);
                his.li_project_transactions = buyTr;
                context.li_wallet_histories.InsertOnSubmit(his);
            });
        }

        public static void HuoqiProjectWithdraw(this Agp2pDataContext context, int userId, int huoqiProjectId, decimal withdrawMoney)
        {
            // 将活期项目的债权设置为 需要转让
            var user = context.dt_users.Single(u => u.id == userId);

            // 最少提现 100 （尽量避免公司账号续投时续投金额低于 100）
            if (withdrawMoney < 100)
                throw new InvalidOperationException("每次提现不能少于 100 元");
            // 最多提现 50000
            var todayWithdrawClaims = user.li_claims.Where(c =>
                    c.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                    c.Parent.status == (int)Agp2pEnums.ClaimStatusEnum.Nontransferable &&
                    c.createTime.Date == DateTime.Today)
                .ToList();
            var todayWithdraw = todayWithdrawClaims.Aggregate(0m, (sum, c) => sum + c.principal);
            if (50000 < todayWithdraw + withdrawMoney)
                throw new InvalidOperationException("每日最多提现 50000 元，你已提现：" + todayWithdraw);

            var huoqiClaims =
                user.li_claims.Where(
                    c =>
                        c.profitingProjectId == huoqiProjectId &&
                        c.status == (int)Agp2pEnums.ClaimStatusEnum.Nontransferable && c.IsLeafClaim()).ToList();
            if (!huoqiClaims.Any())
                throw new InvalidOperationException("您目前没有投资此活期项目，无法提现");

            var project = context.li_projects.Single(p => p.id == huoqiProjectId);

            var withdrawTime = DateTime.Now;
            var sumOfPrincipal = huoqiClaims.Sum(c => c.principal);
            if (sumOfPrincipal < withdrawMoney)
            {
                throw new InvalidOperationException("您提现的金额不能超出您投资的本金：" + sumOfPrincipal.ToString("c"));
            }

            if (sumOfPrincipal == withdrawMoney)
            {
                // 全部提现
                var results = huoqiClaims.Select(c => c.NewStatusChild(withdrawTime, Agp2pEnums.ClaimStatusEnum.NeedTransfer)).ToList();
                context.li_claims.InsertAllOnSubmit(results);
            }
            else
            {
                // 部分提现，优先提现接近完成的项目
                var sortedClaims = huoqiClaims.OrderBy(
                    c => c.li_projects.li_repayment_tasks.LastOrDefault(t =>
                        t.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid ||
                        t.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime)?.should_repay_time)
                    .ThenBy(c => c.principal)
                    .ToList();
                HuoqiClaimsPartialWithdraw(context, sortedClaims, withdrawMoney, withdrawTime);
            }
            // 提现后减去活期项目的已投金额
            project.investment_amount -= withdrawMoney;

            context.SubmitChanges();

            // 发送中间人金额提醒
            MessageBus.Main.PublishAsync(new HuoqiWithdrawMsg(userId, huoqiProjectId, withdrawMoney));
        }

        private static void HuoqiClaimsPartialWithdraw(Agp2pDataContext context, IEnumerable<li_claims> claims, decimal withdrawMoney, DateTime withdrawTime)
        {
            if (!claims.Any() || withdrawMoney == 0) return;
            Debug.Assert(0 < withdrawMoney, "提现的金额不能是负数");

            var headClaim = claims.First();
            if (headClaim.principal <= withdrawMoney)
            {
                var child = headClaim.NewStatusChild(withdrawTime, Agp2pEnums.ClaimStatusEnum.NeedTransfer);
                context.li_claims.InsertOnSubmit(child);
                HuoqiClaimsPartialWithdraw(context, claims.Skip(1), withdrawMoney - headClaim.principal, withdrawTime);
            }
            else
            {
                // 提现了某个债权的一部分，需要进行拆分

                // 债权未完全转让，注意：创建时间与父债权相等
                var remain = headClaim.NewPrincipalChild(headClaim.createTime, headClaim.principal - withdrawMoney);
                context.li_claims.InsertOnSubmit(remain);

                var splited = headClaim.NewPrincipalAndStatusChild(withdrawTime, Agp2pEnums.ClaimStatusEnum.NeedTransfer, withdrawMoney);
                context.li_claims.InsertOnSubmit(splited);
            }
        }

        /* 活期项目：
            1、固定资金100万，100元起投，每个客户最大投资5万
            2、收益T+0，固定利率3.3%，次日开始返息
            3、购买活期项目后即设置为自动投标：
            第一优先匹配“活期项目的债权转让”，
            第二是“公司内部账号购买的标的债权转让”，
            最后是“正在发标中的项目”
            4、提现T+1，申请提现即转变为“活期项目的债权转让”项目，等待接手（下一个买入活期项目的客户），
                次日15:00回款前仍未有人接手，则使用公司内部账号自动购买此债权，15:00点返回客户提现资金到平台账户。*/

        public static void BuyHuoqiClaims(Agp2pDataContext context, decimal apportionAmount, li_project_transactions byPtr, DateTime? invesTime = null)
        {
            var needTransferHuoqiClaims = context.li_claims.Where(
                c =>
                    c.profitingProjectId == byPtr.project &&
                    c.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                    c.userId != byPtr.investor &&
                    !c.Children.Any())
                .ToList();

            apportionAmount = ApportionToClaims(context, needTransferHuoqiClaims, apportionAmount, byPtr, invesTime);

            if (0 < apportionAmount)
            {
                var agentId = byPtr.li_projects.li_risks.li_loaners.user_id;
                var huoqiBuyableClaims = context.li_claims.Where(
                    c => c.userId == agentId &&
                         c.status == (int)Agp2pEnums.ClaimStatusEnum.Transferable &&
                         c.userId != byPtr.investor &&
                         !c.Children.Any())
                    .ToList();
                apportionAmount = ApportionToClaims(context, huoqiBuyableClaims, apportionAmount, byPtr, invesTime);
            }

            if (apportionAmount != 0)
                throw new InvalidOperationException("活期债权不足，无法完成这个延期投资；超出：" + apportionAmount);
        }

        public static decimal QueryHuoqiBuyableClaimsAmount(Agp2pDataContext context, li_projects huoqiProject, int userId)
        {
            Debug.Assert(huoqiProject.IsHuoqiProject());
            var needTransferHuoqiClaims = context.li_claims.Where(
                c =>
                    c.profitingProjectId == huoqiProject.id &&
                    c.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                    c.userId != userId &&
                    !c.Children.Any())
                .ToList();

            var agentId = huoqiProject.li_risks.li_loaners.user_id;
            var huoqiBuyableClaims = context.li_claims.Where(
                c =>
                    c.userId == agentId &&
                    c.status == (int)Agp2pEnums.ClaimStatusEnum.Transferable &&
                    c.userId != userId &&
                    !c.Children.Any())
                .ToList();

            return needTransferHuoqiClaims.Concat(huoqiBuyableClaims).Aggregate(0m, (sum, c) => sum + c.principal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="investableProjects"></param>
        /// <param name="investingMoney"></param>
        /// <param name="byPtr"></param>
        /// <param name="investTime"></param>
        /// <returns>剩余可投金额</returns>
        private static decimal ApportionToProjects(Agp2pDataContext context, List<li_projects> investableProjects,
            decimal investingMoney, li_project_transactions byPtr, DateTime? investTime = null)
        {
            if (!investableProjects.Any() || investingMoney == 0)
                return investingMoney;
            Debug.Assert(0 < investingMoney);

            if (investableProjects.Sum(p => p.financing_amount - p.investment_amount) < investingMoney)
            {
                // 全部投资
                return investingMoney -
                       investableProjects.Select(
                           p => ClaimCreate(context, p, p.financing_amount - p.investment_amount, byPtr, investTime)).Sum();
            }

            var averageInvestment = investingMoney / investableProjects.Count;
            var priorityProjects = investableProjects.Where(p => p.financing_amount - p.investment_amount <= averageInvestment).ToList();
            if (priorityProjects.Any())
            {
                // 可投资金额低于平均值的项目，全部投资，其余的递归处理
                var notPriorityProjects = investableProjects.Where(p => averageInvestment < p.financing_amount - p.investment_amount).ToList();

                var consumed = priorityProjects.Select(p => ClaimCreate(context, p, p.financing_amount - p.investment_amount, byPtr, investTime)).Sum();
                return ApportionToProjects(context, notPriorityProjects, investingMoney - consumed, byPtr, investTime);
            }
            else
            {
                // 如果用户投资金额低于 1000，则优先匹配单个项目
                if (investingMoney <= 1000 && investableProjects.Any(p => investingMoney <= p.financing_amount - p.investment_amount))
                {
                    var suitProject = investableProjects.OrderBy(p => p.financing_amount - p.investment_amount).First(p => investingMoney <= p.financing_amount - p.investment_amount);
                    ClaimCreate(context, suitProject, investingMoney, byPtr, investTime);
                    return 0;
                }
                // 部分投资
                var perfectRounding = Utils.GetPerfectRounding(investingMoney.GetPerfectSplitStream(investableProjects.Count).ToList(), investingMoney, 0);
                investableProjects.ZipEach(perfectRounding, (p, investAmount) =>
                {
                    if (0 < investAmount)
                    {
                        ClaimCreate(context, p, investAmount, byPtr, investTime);
                    }
                });
                return 0;
            }
        }

        private static decimal ClaimCreate(Agp2pDataContext context, li_projects project, decimal investment, li_project_transactions byPtr, DateTime? investTime = null)
        {
            Debug.Assert(investment != 0);

            // 自动投标时修改定期项目的已投金额
            project.investment_amount += investment;
            var liClaims = new li_claims
            {
                li_project_transactions_invest = byPtr,
                createTime = investTime.GetValueOrDefault(byPtr.create_time),
                projectId = project.id,
                profitingProjectId = byPtr.li_projects.id,
                userId = byPtr.dt_users.id,
                status = (byte)Agp2pEnums.ClaimStatusEnum.Nontransferable,
                principal = investment,
                number = Utils.HiResNowString
            };
            context.li_claims.InsertOnSubmit(liClaims);
            return investment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="needTransferClaims"></param>
        /// <param name="investingMoney"></param>
        /// <param name="byPtr"></param>
        /// <param name="investTime"></param>
        /// <returns>返回没有匹配到债权的金额</returns>
        private static decimal ApportionToClaims(Agp2pDataContext context, List<li_claims> needTransferClaims,
            decimal investingMoney, li_project_transactions byPtr, DateTime? investTime = null)
        {
            if (!needTransferClaims.Any() || investingMoney == 0)
                return investingMoney;
            Debug.Assert(0 < investingMoney);

            if (needTransferClaims.Sum(c => c.principal) <= investingMoney)
            {
                // 全部接手
                needTransferClaims.ForEach(c => ClaimTransfer(context, c, c.principal, byPtr, investTime));
                return investingMoney - needTransferClaims.Sum(c => c.principal);
            }

            var averageTransfer = investingMoney / needTransferClaims.Count;
            var priorityClaimses = needTransferClaims.Where(c => c.principal <= averageTransfer).ToList();
            if (priorityClaimses.Any())
            {
                // 本金低于平均值的债权，全部接手，其余的递归处理
                var notPriorityClaimses = needTransferClaims.Where(c => averageTransfer < c.principal).ToList();
                priorityClaimses.ForEach(c => ClaimTransfer(context, c, c.principal, byPtr, investTime));
                return ApportionToClaims(context, notPriorityClaimses, investingMoney - priorityClaimses.Sum(c => c.principal), byPtr, investTime);
            }
            else
            {
                // 如果用户投资金额低于 1000，则优先匹配单个项目
                if (investingMoney <= 1000 && needTransferClaims.Any(c => investingMoney <= c.principal))
                {
                    var suitClaim = needTransferClaims.OrderBy(c => c.principal).First(c => investingMoney <= c.principal);
                    ClaimTransfer(context, suitClaim, investingMoney, byPtr, investTime);
                    return 0;
                }
                // 全部债权的金额都高于平均转让金额，全部进行转让。注意：债权的本金不能为小数
                var perfectRounding = Utils.GetPerfectRounding(investingMoney.GetPerfectSplitStream(needTransferClaims.Count).ToList(), investingMoney, 0);
                needTransferClaims.ZipEach(perfectRounding, (c, transferAmount) =>
                {
                    if (0 < transferAmount)
                    {
                        ClaimTransfer(context, c, transferAmount, byPtr, investTime);
                    }
                });
                return 0;
            }
        }

        /// <summary>
        /// 图例：债权表达：用户-债权状态；债权转让 -->；父子关系：父 { 子 + 子 + 子... }
        /// 规则：
        ///   可转让的债权：
        ///     完全转让：       A-Transferable --> A-Transferable { A-Transfered + B-Nontransferable }
        ///     不完全转让：     A-Transferable --> A-Transferable { A-Transfered + A-Transferable + B-Nontransferable }
        ///   需要转让的债权：
        ///     完全转让：       A-NeedTransfer --> A-NeedTransfer { A-TransferedUnpaid + B-Nontransferable }
        ///     不完全转让：     A-NeedTransfer --> A-NeedTransfer { A-TransferedUnpaid + A-NeedTransfer + B-Nontransferable }
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="originalClaim"></param>
        /// <param name="amount"></param>
        /// <param name="byPtr"></param>
        /// <param name="investTime"></param>
        /// <returns></returns>
        private static li_claims ClaimTransfer(Agp2pDataContext context, li_claims originalClaim, decimal amount, li_project_transactions byPtr, DateTime? investTime = null)
        {
            if (originalClaim.status != (int)Agp2pEnums.ClaimStatusEnum.Transferable && originalClaim.status != (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer)
                throw new InvalidOperationException("该债权不可转让");
            if (amount <= 0)
                throw new InvalidOperationException("债权转让金额不能小于0");
            if (originalClaim.principal < amount)
                throw new InvalidOperationException("债权转让金额不能超出债权的本金");
            if (originalClaim.userId == byPtr.dt_users.id)
                throw new InvalidOperationException("不能将债权转让给自己");

            var transactTime = investTime.GetValueOrDefault(byPtr.create_time);

            if (amount < originalClaim.principal)
            {
                // 债权未完全转让，注意：创建时间与父债权相等
                var remainClaim = originalClaim.NewPrincipalChild(originalClaim.createTime, originalClaim.principal - amount);
                context.li_claims.InsertOnSubmit(remainClaim);
            }

            // 如果是给中间户接手，则设为可转让债权
            var transferedStatus = byPtr.dt_users.IsAgent()
                ? Agp2pEnums.ClaimStatusEnum.Transferable
                : Agp2pEnums.ClaimStatusEnum.Nontransferable;
            var takeoverPart = originalClaim.TransferedChild(transactTime, transferedStatus, amount, byPtr);
            // 如果原始债权的所有者是中间人，则转让后的债权需要设置中间人，以便生成垫付交易记录
            if (originalClaim.dt_users.IsAgent())
            {
                takeoverPart.agent = originalClaim.userId;
            }
            context.li_claims.InsertOnSubmit(takeoverPart);


            // 转让了提现中的债权，由于提现 T + 1 的缘故，债权需要标记为 Unpaid
            if (originalClaim.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer)
            {
                var transferedChild = originalClaim.NewPrincipalAndStatusChild(transactTime, Agp2pEnums.ClaimStatusEnum.TransferredUnpaid, amount);
                context.li_claims.InsertOnSubmit(transferedChild);
            }
            else if (originalClaim.status == (int)Agp2pEnums.ClaimStatusEnum.Transferable)
            {
                var transferedChild = originalClaim.NewPrincipalAndStatusChild(transactTime, Agp2pEnums.ClaimStatusEnum.Transferred, amount);
                context.li_claims.InsertOnSubmit(transferedChild);

                // 公司账号回款需要 恢复活期项目的可投资金额
                // originalClaim.li_projects1.investment_amount -= amount;

                // 处理债权转让的本金交易
                var claimTransferPtr = new li_project_transactions
                {
                    principal = amount,
                    project = originalClaim.projectId,
                    create_time = transactTime,
                    investor = originalClaim.userId,
                    type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredOut,
                    status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success,
                    gainFromClaim = originalClaim.id,
                    //remark = $"项目 {originalClaim.li_projects.title} 的可转让债权转让成功： {amount}/{originalClaim.principal}"
                };
                context.li_project_transactions.InsertOnSubmit(claimTransferPtr);

                var wallet = originalClaim.dt_users.li_wallets;
                wallet.idle_money += amount;
                // 债权转让不会更改在投金额
                // wallet.investing_money -= amount;
                wallet.last_update_time = transactTime;

                // 修改钱包历史
                var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredOut);
                his.li_project_transactions = claimTransferPtr;
                context.li_wallet_histories.InsertOnSubmit(his);
            }
            else
            {
                throw new InvalidOperationException("异常的原始债权状态");
            }
            return takeoverPart;
        }

        /// <summary>
        /// 用户投资过后，如果项目可投金额为 0，则将状态设置为满标
        /// </summary>
        /// <param name="projectTransactionId"></param>
        private static void CheckFinancingComplete(int projectTransactionId)
        {
            var context = new Agp2pDataContext();
            var ptr = context.li_project_transactions.Single(tr => tr.id == projectTransactionId);
            var project = ptr.li_projects;
            if (project.IsHuoqiProject())
            {
                // 判断自动投标的项目是否满标
                var financingCompletedProject = ptr.li_claims_invested.Select(c => c.li_projects).Distinct().Where(
                    p =>
                        p.status == (int)Agp2pEnums.ProjectStatusEnum.Financing &&
                        p.financing_amount == p.investment_amount)
                    .ToList();
                financingCompletedProject.ForEach(p => FinishInvestment(context, p.id));
                return; // 活期项目不会满标
            }

            var canBeInvest = project.financing_amount - project.investment_amount;
            if (0 < canBeInvest) return; // 未满标
            if (project.IsNewbieProject1()) return; // 新手标项目不会满标
            FinishInvestment(context, project.id);
        }

        /// <summary>
        /// 投资结束
        /// </summary>
        /// <param name="context"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public static li_projects FinishInvestment(this Agp2pDataContext context, int projectId)
        {
            var project = context.li_projects.Single(p => p.id == projectId);
            if (project.status != (int)Agp2pEnums.ProjectStatusEnum.Financing)
                throw new InvalidOperationException("项目 " + project.title + " 不是发标状态，不能设置为满标");
            if (project.IsHuoqiProject())
                throw new InvalidOperationException("活期项目不会满标");

            project.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingSuccess;

            // 项目投资完成时间应该等于最后一个债权的创建时间
            var lastClaim =
                project.li_claims.Where(
                    c => c.status < (int)Agp2pEnums.ClaimStatusEnum.Completed && c.IsLeafClaim())
                    .OrderByDescending(c => c.createTime)
                    .FirstOrDefault();
            project.invest_complete_time = lastClaim?.createTime ?? DateTime.Now;

            context.SubmitChanges();

            MessageBus.Main.Publish(new ProjectFinancingCompletedMsg(projectId)); // 广播项目满标的消息
            return project;
        }

        /// <summary>
        /// 投资超时，截标
        /// </summary>
        /// <param name="context"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public static li_projects FinishInvestmentEvenTimeout(this Agp2pDataContext context, int projectId)
        {
            var project = context.li_projects.Single(p => p.id == projectId);
            if (project.status != (int)Agp2pEnums.ProjectStatusEnum.FinancingTimeout)
                throw new InvalidOperationException("项目不是投资超时状态，不能设置为截标");
            project.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingSuccess;

            // 截标时间就是当前时间
            project.invest_complete_time = DateTime.Now;

            context.SubmitChanges();

            MessageBus.Main.Publish(new ProjectFinancingCompleteEvenTimeoutMsg(projectId)); // 广播项目截标的消息
            return project;
        }

        /// <summary>
        /// 开始还款
        /// </summary>
        /// <param name="context"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public static li_projects StartRepayment(this Agp2pDataContext context, int projectId)
        {
            var project = context.li_projects.Single(p => p.id == projectId);
            if (project.status != (int)Agp2pEnums.ProjectStatusEnum.FinancingSuccess)
                throw new InvalidOperationException("项目不是满标状态，不能设置为正在还款状态");

            // 修改项目状态为满标/截标
            project.status = (int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying;
            project.make_loan_time = DateTime.Now; // 放款时间

            // 放款给借款人
            var loaner = project.li_risks.li_loaners;
            if (!project.IsNewbieProject1() && !project.IsHuoqiProject() && loaner != null)
            {
                // 如果已经进行过放款，则报错
                if (loaner.dt_users.li_bank_transactions.Any(btr =>
                        btr.type == (int)Agp2pEnums.BankTransactionTypeEnum.LoanerMakeLoan &&
                        btr.status == (int)Agp2pEnums.BankTransactionStatusEnum.Confirm &&
                        btr.remarks == projectId.ToString()))
                {
                    throw new InvalidOperationException("这个项目已经进行过放款");
                }
                MakeLoan(context, project.make_loan_time.Value, project, loaner.user_id, false);
            }

            var repaymentType = (Agp2pEnums.ProjectRepaymentTypeEnum)project.repayment_type; // 还款类型

            // 满标时计算真实总利率
            project.profit_rate = project.GetFinalProfitRate();
            var termCount = project.CalcRealTermCount(); // 实际期数

            var repayPrincipal = project.investment_amount; // 本金投资总额
            var interestAmount = Math.Round(project.profit_rate * repayPrincipal, 2); // 利息总额

            List<li_repayment_tasks> repaymentTasks;
            if (repaymentType == Agp2pEnums.ProjectRepaymentTypeEnum.DengEr) // 等额本息
            {
                repaymentTasks = Enumerable.Range(1, termCount)
                    .Zip(interestAmount.GetPerfectSplitStream(termCount),
                        (termNumber, repayInterestEachTerm) => new { termNumber, repayInterestEachTerm })
                    .Zip(repayPrincipal.GetPerfectSplitStream(termCount),
                        (a, repayPrincipalEachTerm) =>
                            new { a.termNumber, a.repayInterestEachTerm, repayPrincipalEachTerm })
                    .Select(term => new li_repayment_tasks
                    {
                        li_projects = project,
                        repay_interest = term.repayInterestEachTerm,
                        repay_principal = term.repayPrincipalEachTerm,
                        status = (byte)Agp2pEnums.RepaymentStatusEnum.Unpaid,
                        term = (short)term.termNumber,
                        should_repay_time = project.CalcRepayTimeByTerm(term.termNumber)
                    }).ToList();
            }
            else if (repaymentType == Agp2pEnums.ProjectRepaymentTypeEnum.XianXi) // 先息后本
            {
                repaymentTasks = Enumerable.Range(1, termCount)
                    .Zip(interestAmount.GetPerfectSplitStream(termCount),
                        (termNumber, repayInterestEachTerm) => new { termNumber, repayInterestEachTerm })
                    .Select(term => new li_repayment_tasks
                    {
                        li_projects = project,
                        repay_interest = term.repayInterestEachTerm, // 只付利息
                        repay_principal = 0,
                        status = (byte)Agp2pEnums.RepaymentStatusEnum.Unpaid,
                        term = (short)term.termNumber,
                        should_repay_time = project.CalcRepayTimeByTerm(term.termNumber)
                    }).ToList();
                repaymentTasks.Last().repay_principal = repayPrincipal; // 最后额外添加一期返还全部本金
            }
            else if (repaymentType == Agp2pEnums.ProjectRepaymentTypeEnum.DaoQi) // 到期还款付息
            {
                if (termCount != 1)
                    throw new Exception("到期还款付息 只能有一期");
                repaymentTasks = Enumerable.Repeat(new li_repayment_tasks
                {
                    li_projects = project,
                    repay_interest = interestAmount,
                    repay_principal = repayPrincipal,
                    status = (byte)Agp2pEnums.RepaymentStatusEnum.Unpaid,
                    term = 1,
                    should_repay_time = project.CalcRepayTimeByTerm(1)
                }, 1).ToList();
            }
            else throw new InvalidEnumArgumentException("项目的还款类型值异常");
            context.li_repayment_tasks.InsertAllOnSubmit(repaymentTasks);

            // 计算每个投资人的待收益金额，因为不一定是投资当日满标，所以不能投资时就知道收益（不同时间满标/截标会对导致不同的回款时间间隔，从而导致利率不同）
            context.CalcProfitingMoneyAfterRepaymentTasksCreated(project, repaymentTasks);
            context.SubmitChanges();

            MessageBus.Main.PublishAsync(new ProjectStartRepaymentMsg(projectId, project.make_loan_time.Value)); // 广播项目开始还款的消息
            return project;
        }

        /// <summary>
        /// 根据用户从某个项目中能获得的实际收益来计算“待收金额”，完全避免精度问题
        /// </summary>
        /// <param name="context"></param>
        /// <param name="project"></param>
        /// <param name="tasks"></param>
        private static void CalcProfitingMoneyAfterRepaymentTasksCreated(this Agp2pDataContext context, li_projects project, List<li_repayment_tasks> tasks)
        {
            // 查询每个用户的债权记录（一个用户可能投资多次）
            var userClaims = project.li_claims_profiting.Where(c => c.status < (int)Agp2pEnums.ClaimStatusEnum.Completed && c.IsLeafClaim())
                    .ToLookup(c => c.dt_users);

            var wallets = userClaims.Select(ir => ir.Key.li_wallets).ToList();
            var walletDict = wallets.ToDictionary(w => w.user_id);

            // 重新计算代收本金前，先减去原来的投资金额
            userClaims.ForEach(uc =>
            {
                uc.Key.li_wallets.investing_money -= uc.Sum(c => c.principal);
            });

            // 修改钱包的值（待收益金额和时间）
            foreach (var task in tasks)
            {
                // 累加利润：避免直接计算总利润（利率 * 总投资额），这样可以避免精度问题
                var predicts = GenerateRepayTransactions(task, task.should_repay_time);
                predicts.ForEach(ptr =>
                {
                    var repayTo = walletDict[ptr.investor];
                    repayTo.profiting_money += ptr.interest.GetValueOrDefault();
                    repayTo.investing_money += ptr.principal;
                });
            }

            var projectInvestCompleteTime = project.make_loan_time.Value;
            wallets.ForEach(w => w.last_update_time = projectInvestCompleteTime); // 时间应该一致

            // 创建钱包历史
            var histories = wallets.Select(w =>
            {
                var his = CloneFromWallet(w, Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess);
                his.li_project_transactions = userClaims[w.dt_users].Last().li_project_transactions_invest;
                return his;
            });
            context.li_wallet_histories.InsertAllOnSubmit(histories);
        }

        /// <summary>
        /// 计算每期的还款时间
        /// </summary>
        /// <param name="baseTime"></param>
        /// <param name="termSpan">还款期限跨度</param>
        /// <param name="termNumber">第几期</param>
        /// <param name="termUnitCount"></param>
        /// <returns></returns>
        public static DateTime CalcRepayTimeByTerm(this li_projects proj, int termNumber, DateTime? makeLoanTime = null)
        {
            var baseTime = proj.make_loan_time ?? makeLoanTime.Value;
            switch ((Agp2pEnums.ProjectRepaymentTermSpanEnum)proj.repayment_term_span)
            {
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Year:
                    return baseTime.AddYears(termNumber);
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Month:
                    return baseTime.AddMonthsPreferLastDay(termNumber); // 月尾满标的话总是在月尾付息
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Day:
                    Debug.Assert(termNumber == 1, "日标只还一期");
                    return baseTime.AddDays(proj.repayment_term_span_count);
                default:
                    throw new InvalidEnumArgumentException("异常的项目还款跨度值");
            }
        }

        /// <summary>
        /// 添加月份，匹配月尾（2月28日 => 3月31日)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="months"></param>
        /// <returns></returns>
        private static DateTime AddMonthsPreferLastDay(this DateTime dateTime, int months)
        {
            var addedMonth = dateTime.AddMonths(months);
            if (dateTime.Day == DateTime.DaysInMonth(dateTime.Year, dateTime.Month)) // 是否月尾
            {
                var daysInMonth = DateTime.DaysInMonth(addedMonth.Year, addedMonth.Month);
                return addedMonth.AddDays(daysInMonth - addedMonth.Day); // 去到月尾
            }
            return addedMonth;
        }

        /// <summary>
        /// 计算真实还款期数
        /// </summary>
        /// <param name="termSpan"></param>
        /// <param name="termSpanCount"></param>
        /// <returns></returns>
        public static int CalcRealTermCount(this li_projects proj)
        {
            switch ((Agp2pEnums.ProjectRepaymentTermSpanEnum)proj.repayment_term_span)
            {
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Year:
                    return proj.repayment_term_span_count;
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Month:
                    return proj.repayment_term_span_count;
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Day: // 日的话只在最后一日还
                    return 1;
                default:
                    throw new InvalidEnumArgumentException("异常的项目还款跨度值");
            }
        }

        public static decimal GetFinalProfitRate(this li_projects proj, DateTime? makeLoanTime = null)
        {
            if (proj.IsHuoqiProject())
                return proj.profit_rate_year / 100 / HuoqiProjectProfitingDay;

            if (0 < proj.profit_rate)
                return proj.profit_rate;

            if (proj.dt_article_category.call_index == "ypb" || proj.dt_article_category.call_index == "ypl" || proj.IsNewbieProject2())
            {
                var projectRepaymentTermSpanEnum = (Agp2pEnums.ProjectRepaymentTermSpanEnum)proj.repayment_term_span;
                if (projectRepaymentTermSpanEnum != Agp2pEnums.ProjectRepaymentTermSpanEnum.Day)
                {
                    throw new Exception("银票宝的期数只能是按日算");
                }
                return proj.profit_rate_year / 100 / TicketProjectProfitingDay * proj.repayment_term_span_count;
            }
            return CalcFinalProfitRate(proj, makeLoanTime);
        }

        /// <summary>
        /// 计算最终的项目利润率（始终按天数算最终利润率）
        /// </summary>
        /// <param name="baseTime"></param>
        /// <param name="profitRateYear"></param>
        /// <param name="termSpanEnum"></param>
        /// <param name="termSpanCount"></param>
        /// <returns></returns>
        private static decimal CalcFinalProfitRate(li_projects proj, DateTime? makeLoanTime = null)
        {
            var baseTime = proj.make_loan_time ?? makeLoanTime.Value;
            var profitRateYear = proj.profit_rate_year / 100; // 年化利率未除以 100
            var termSpanCount = proj.repayment_term_span_count;

            switch ((Agp2pEnums.ProjectRepaymentTermSpanEnum)proj.repayment_term_span) // 公式：年利率 * 总天数 / 365
            {
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Year:
                    return profitRateYear * termSpanCount;
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Month:
                    // 最后那期还款的日期 - 满标的日期 = 总天数
                    var lastRepayDate = proj.CalcRepayTimeByTerm(termSpanCount, makeLoanTime).Date;
                    var days = lastRepayDate.Subtract(baseTime.Date).Days;
                    return profitRateYear * days / NormalProjectProfitingDay;
                case Agp2pEnums.ProjectRepaymentTermSpanEnum.Day:
                    return profitRateYear * termSpanCount / NormalProjectProfitingDay;
                default:
                    throw new InvalidEnumArgumentException("异常的项目还款跨度值");
            }
        }

        public static Agp2pEnums.WalletHistoryTypeEnum GetWalletHistoryTypeByProjectTransaction(li_project_transactions ptr)
        {
            if (ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.RepayToInvestor)
            {
                if (ptr.principal != 0 && ptr.interest != 0)
                    return Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest;
                else if (ptr.principal == 0)
                    return Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest;
                else if (ptr.interest == 0)
                    return Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipal;
            }
            else if (ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.RepayOverdueFine)
            {
                return Agp2pEnums.WalletHistoryTypeEnum.RepaidOverdueFine;
            }
            else if (ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredOut)
            {
                return Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredOut;
            }
            else if (ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn)
            {
                return ptr.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Pending
                    ? Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredIn
                    : Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredInSuccess;
            }
            else if (ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.HuoqiProjectWithdraw)
            {
                return Agp2pEnums.WalletHistoryTypeEnum.HuoqiProjectWithdrawSuccess;
            }
            throw new Exception("项目交易状态异常");
        }

        public static bool IsUnpaid(this li_repayment_tasks task)
        {
            return task.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid ||
                   task.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime;
        }

        /// <summary>
        /// 放款
        /// </summary>
        /// <param name="context"></param>
        /// <param name="makeLoanAt"></param>
        /// <param name="project"></param>
        /// <param name="loanerUserId"></param>
        /// <param name="save"></param>
        public static void MakeLoan(Agp2pDataContext context, DateTime makeLoanAt, li_projects project, int loanerUserId, bool save = true)
        {
            decimal loanFeeSum = 0;
            decimal bondFeeSum = 0;
            // 计算平台服务费
            if (project.loan_fee_rate != null && project.loan_fee_rate > 0)
            {
                loanFeeSum = decimal.Round(project.investment_amount * project.loan_fee_rate.GetValueOrDefault(0), 2, MidpointRounding.AwayFromZero);
                context.li_company_inoutcome.InsertOnSubmit(new li_company_inoutcome
                {
                    user_id = loanerUserId,
                    income = loanFeeSum,
                    project_id = project.id,
                    type = (int)Agp2pEnums.OfflineTransactionTypeEnum.ManagementFeeOfLoanning,
                    create_time = DateTime.Now,
                    remark = $"借款项目'{project.title}'收取平台服务费"
                });
            }
            //计算风险保证金
            if (project.bond_fee_rate != null && project.bond_fee_rate > 0)
            {
                bondFeeSum = decimal.Round(project.investment_amount * project.bond_fee_rate.GetValueOrDefault(0), 2, MidpointRounding.AwayFromZero);
                context.li_company_inoutcome.InsertOnSubmit(new li_company_inoutcome
                {
                    user_id = loanerUserId,
                    income = bondFeeSum,
                    project_id = project.id,
                    type = (int)Agp2pEnums.OfflineTransactionTypeEnum.BondFee,
                    create_time = DateTime.Now,
                    remark = $"借款项目'{project.title}'收取风险保证金"
                });
            }

            decimal amount = decimal.Round(project.investment_amount - loanFeeSum - bondFeeSum, 2, MidpointRounding.AwayFromZero);
            var btr = new li_bank_transactions
            {
                handling_fee_type = (byte)Agp2pEnums.BankTransactionHandlingFeeTypeEnum.NoHandlingFee,
                type = (byte)Agp2pEnums.BankTransactionTypeEnum.LoanerMakeLoan,
                status = (byte)Agp2pEnums.BankTransactionStatusEnum.Confirm,
                create_time = makeLoanAt,
                transact_time = makeLoanAt,
                charger = loanerUserId,
                value = amount,
                no_order = "",
                remarks = project.id.ToString()
            };
            // 创建钱包历史
            var wallet = context.li_wallets.Single(w => w.user_id == loanerUserId);
            wallet.idle_money += amount;
            wallet.last_update_time = makeLoanAt;

            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.LoanerMakeLoanSuccess);
            his.li_bank_transactions = btr;
            context.li_wallet_histories.InsertOnSubmit(his);

            if (save)
            {
                context.SubmitChanges();
            }
        }

        /// <summary>
        /// 收取借款人的还款
        /// </summary>
        /// <param name="loanerUserId"></param>
        /// <param name="bankAccountId"></param>
        /// <param name="amount"></param>
        public static void GainLoanerRepayment(this Agp2pDataContext context, DateTime gainAt, int repaymentTaskId, int loanerUserId, decimal amount, bool save = true)
        {
            //查找是否已经生成还款记录
            if(context.li_bank_transactions.Any(t => t.type == (int)Agp2pEnums.BankTransactionTypeEnum.GainLoanerRepay 
            && t.status == (int)Agp2pEnums.BankTransactionStatusEnum.Confirm
            && t.remarks == repaymentTaskId.ToString()))
                throw new InvalidOperationException("借款人已经还款");


            var wallet = context.li_wallets.Single(w => w.user_id == loanerUserId);
            if (wallet.idle_money < amount)
                throw new InvalidOperationException("借款人的余额不足");

            var btr = new li_bank_transactions
            {
                handling_fee_type = (byte)Agp2pEnums.BankTransactionHandlingFeeTypeEnum.NoHandlingFee,
                type = (byte)Agp2pEnums.BankTransactionTypeEnum.GainLoanerRepay,
                status = (byte)Agp2pEnums.BankTransactionStatusEnum.Confirm,
                create_time = gainAt,
                transact_time = gainAt,
                charger = loanerUserId,
                value = amount,
                no_order = "",
                remarks = repaymentTaskId.ToString()
            };
            // 创建钱包历史
            wallet.idle_money -= amount;
            wallet.last_update_time = gainAt;

            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.LoanerRepaySuccess);
            his.li_bank_transactions = btr;
            context.li_wallet_histories.InsertOnSubmit(his);

            if (save)
            {
                //using (TransactionScope txScope = new TransactionScope())
                //{
                //    try
                //    {
                        //context.SubmitChanges(ConflictMode.ContinueOnConflict);
                        context.SubmitChanges();
                    //}
                    //catch (ChangeConflictException)
                    //{
                        //context.ChangeConflicts.ResolveAll(RefreshMode.KeepCurrentValues);
                        //冲突发生时获取最新的数据，并更新实体类对象的原始值，保留实体类对象的当前值。
                        //context.SubmitChanges();
                //    }
                //    txScope.Complete();
                //}
                MessageBus.Main.PublishAsync(new GainLoanerRepaymentMsg(gainAt, repaymentTaskId, loanerUserId,
                    amount));
            }
        }

        /// <summary>
        /// 马上执行还款计划（不能异步执行）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="repaymentId"></param>
        /// <param name="autoPaid"></param>
        public static li_repayment_tasks ExecuteRepaymentTask(this Agp2pDataContext context, int repaymentId,
            Agp2pEnums.RepaymentStatusEnum statusAfterPay = Agp2pEnums.RepaymentStatusEnum.AutoPaid)
        {
            var repaymentTask = context.li_repayment_tasks.Single(r => r.id == repaymentId);
            var proj = repaymentTask.li_projects;
            if (repaymentTask.status != (int)Agp2pEnums.RepaymentStatusEnum.Unpaid &&
                repaymentTask.status != (int)Agp2pEnums.RepaymentStatusEnum.OverTime)
                throw new InvalidOperationException("这个还款计划已经执行过了");
            if (statusAfterPay < Agp2pEnums.RepaymentStatusEnum.ManualPaid)
                throw new InvalidOperationException("还款计划的执行状态不正确");

            // 执行还款
            repaymentTask.status = (byte)statusAfterPay;
            repaymentTask.repay_at = DateTime.Now;

            var ptrs = GenerateRepayTransactions(repaymentTask, repaymentTask.repay_at.Value, true); //变更时间应该等于还款计划的还款时间
            context.li_project_transactions.InsertAllOnSubmit(ptrs);

            Dictionary<int, li_project_transactions> ptrAddedCost = null;
            if (repaymentTask.cost.GetValueOrDefault() != 0)
            {
                // 提前还款/逾期还款 需要减去原来的代收，所以需要先求出原来的代收
                ptrAddedCost = GenerateRepayTransactions(repaymentTask, repaymentTask.repay_at.Value, false, true).ToDictionary(tr => tr.investor);
            }

            //if (proj.IsHuoqiProject())
            //{
            //    // 由于活期的收益是由中间人垫付的，所以需要生成中间人的垫付交易记录
            //    InsertAgentPrepayPtr(context, ptrs);
            //}

            foreach (var ptr in ptrs)
            {
                // 增加钱包空闲金额与减去待收本金和待收利润
                var wallet = ptr.dt_users.li_wallets;

                wallet.idle_money += ptr.interest.GetValueOrDefault() + ptr.principal;
                wallet.investing_money -= ptr.principal;
                // 活期项目不减代收
                if (!proj.IsHuoqiProject())
                {
                    // 由于 提前还款/逾期还款 的缘故，需要修正待收益
                    wallet.profiting_money -= ptrAddedCost == null
                        ? ptr.interest.GetValueOrDefault()
                        : ptrAddedCost[ptr.investor].interest.GetValueOrDefault();
                }
                wallet.total_profit += ptr.interest.GetValueOrDefault();
                wallet.last_update_time = ptr.create_time;

                // 添加钱包历史
                var his = CloneFromWallet(wallet, GetWalletHistoryTypeByProjectTransaction(ptr));
                his.li_project_transactions = ptr;
                context.li_wallet_histories.InsertOnSubmit(his);
            }

            context.SubmitChanges();

            var projectNeedComplete = !proj.IsHuoqiProject() && !proj.IsNewbieProject1() && !proj.li_repayment_tasks.Any(
                ta =>
                    ta.id != repaymentId &&
                    (ta.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid ||
                     ta.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime));
            MessageBus.Main.Publish(new ProjectRepaidMsg(repaymentId, projectNeedComplete)); // 广播项目还款的消息，不能使用异步消息，否则续投活期债权会出现多线程竞争风险

            return repaymentTask;
        }

        private static void InsertAgentPrepayPtr(Agp2pDataContext context, List<li_project_transactions> ptrs)
        {
            // 根据活期收益，生成中间人垫付记录
            ptrs.ForEach(ptr =>
            {
                var agent = ptr.li_claims_from.dt_users_agent;
                var agentPaidPtr = new li_project_transactions
                {
                    investor = agent.id,
                    principal = 0,
                    interest = ptr.interest,
                    type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.AgentPaidInterest,
                    status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success, /* 由定期项目直接还款给中间人 */
                    create_time = ptr.create_time,
                    li_claims_from = ptr.li_claims_from,
                    project = ptr.project
                };
                context.li_project_transactions.InsertOnSubmit(agentPaidPtr);

                var agentWallet = agent.li_wallets;
                agentWallet.idle_money -= agentPaidPtr.interest.GetValueOrDefault();
                agentWallet.last_update_time = ptr.create_time;

                var agentPaidHis = CloneFromWallet(agentWallet, Agp2pEnums.WalletHistoryTypeEnum.AgentPaidInterest);
                agentPaidHis.li_project_transactions = agentPaidPtr;
                context.li_wallet_histories.InsertOnSubmit(agentPaidHis);
            });
        }

        private static void CompleteProject(int repaymentTaskId)
        {
            // 如果所有还款计划均已执行，将项目标记为完成
            var context = new Agp2pDataContext(); // 旧的 context 有缓存，查询的结果不正确
            var repaymentTask = context.li_repayment_tasks.Single(ta => ta.id == repaymentTaskId);
            var pro = repaymentTask.li_projects;
            if (!pro.IsNewbieProject1() && !pro.IsHuoqiProject()
                && !pro.li_repayment_tasks.Any(r => r.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid || r.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime))
            {
                pro.status = (int)Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime;
                pro.complete_time = repaymentTask.repay_at;
                context.SubmitChanges();

                ContinueInvestAfterProjectCompleted(pro.id);

                // 广播项目完成的消息
                MessageBus.Main.PublishAsync(new ProjectRepayCompletedMsg(pro.id, repaymentTask.repay_at.Value));
            }
        }

        private static void ContinueInvestAfterProjectCompleted(int projectId)
        {
            using (var ts = new TransactionScope())
            {
                var newContext = new Agp2pDataContext();
                var pro = newContext.li_projects.Single(p => p.id == projectId);
                // 将定期债权设置为完成，活期收益的债权需要继续自动投标：中间人先收回活期债权，完成掉。然后用户再进行自动投标，如果自动投标失败，则失败的部分退款
                var needComplete = pro.li_claims.Where(c => c.status < (int)Agp2pEnums.ClaimStatusEnum.Completed && c.IsLeafClaim()).ToList();

                // 定期债权全部完成
                var staticClaims = needComplete.Where(c => c.profitingProjectId == c.projectId)
                    .Select(c => c.NewStatusChild(pro.complete_time.Value, Agp2pEnums.ClaimStatusEnum.Completed))
                    .ToList();
                newContext.li_claims.InsertAllOnSubmit(staticClaims);

                // 活期债权（可能是多个活期项目自动投资而产生），需要被原中间人收回
                var huoqiProfitingClaims = needComplete.Where(c => c.profitingProjectId != c.projectId).ToList(); // 自动投标的项目

                // 减去活期项目的已投金额
                huoqiProfitingClaims.GroupBy(c => c.li_projects_profiting).ToDictionary(g => g.Key, g => g.Sum(c => c.principal)).ForEach(
                    pair =>
                    {
                        pair.Key.investment_amount -= pair.Value;
                    });

                // 中间人通过再次投资原定期项目以收回活期债权
                var recapturedHuoqiClaim = newContext.RecaptureHuoqiClaim(huoqiProfitingClaims, pro.complete_time.Value);

                // 收回后将债权设置为完成
                var completedClaims = recapturedHuoqiClaim.Select(
                        c => c.NewStatusChild(pro.complete_time.Value, Agp2pEnums.ClaimStatusEnum.Completed)).ToList();
                newContext.li_claims.InsertAllOnSubmit(completedClaims);

                newContext.SubmitChanges();

                // 自动续投
                var needContinueInvest = huoqiProfitingClaims.Where(c => c.status < (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer)
                    .GroupBy(c => c.dt_users)
                    .ToDictionary(g => g.Key, claims => claims.Sum(c => c.principal));

                ContinueInvest(newContext, needContinueInvest.AsEnumerable(), pro.complete_time.Value);

                ts.Complete();
            }
        }

        private static void ContinueInvest(Agp2pDataContext context, IEnumerable<KeyValuePair<dt_users, decimal>> needContinueInvest, DateTime moment)
        {
            if (!needContinueInvest.Any()) return;

            var user = needContinueInvest.First().Key;
            var investingMoney = needContinueInvest.First().Value;

            var huoqiProject = context.li_projects.SingleOrDefault(p =>
                        p.dt_article_category.call_index == "huoqi" &&
                        p.status == (int)Agp2pEnums.ProjectStatusEnum.Financing);

            var huoqiBuyableClaimsAmount = QueryHuoqiBuyableClaimsAmount(context, huoqiProject, user.id);

            var maxInvestingAmount = Math.Min(huoqiBuyableClaimsAmount, investingMoney);

            if (huoqiProject == null)
            {
                // 完全没有续投，告诉用户
                var dtUserMessage = new dt_user_message
                {
                    type = 1,
                    accept_user_name = user.user_name,
                    title = "续投活期项目失败",
                    content = $"由于活期项目已结束，没能帮您进行活期投资续投，本金 {investingMoney.ToString("c")} 已经退回到您的账号，请查收。",
                    post_user_name = "",
                    post_time = moment,
                    receiver = user.id
                };
                context.dt_user_message.InsertOnSubmit(dtUserMessage);
                context.AppendAdminLog("Huoqi", "没有发布活期项目，无法进行自动续投：" + maxInvestingAmount.ToString("c"));
            }
            else if (0 < maxInvestingAmount)
            {
                if (maxInvestingAmount != investingMoney)
                {
                    // 没有完全续投，告诉用户
                    var dtUserMessage = new dt_user_message
                    {
                        type = 1,
                        accept_user_name = user.user_name,
                        title = "续投活期项目失败",
                        content =
                            $"由于活期债权不足，只能帮您进行了本金为 {maxInvestingAmount.ToString("c")} 的活期项目续投，其余未能续投的部分本金 {(investingMoney - maxInvestingAmount).ToString("c")} 已经退回到您的账号，请查收。",
                        post_user_name = "",
                        post_time = moment,
                        receiver = user.id
                    };
                    context.dt_user_message.InsertOnSubmit(dtUserMessage);
                }

                // 修改钱包，将金额放到待收资金中，流标后再退回空闲资金
                var wallet = user.li_wallets;

                // 判断投资金额的数额是否合理
                if (wallet.idle_money < maxInvestingAmount)
                    throw new InvalidOperationException("余额不足，无法转让债权");

                // 修改钱包金额
                wallet.idle_money -= maxInvestingAmount;
                wallet.investing_money += maxInvestingAmount;
                wallet.total_investment += maxInvestingAmount;
                wallet.last_update_time = moment;

                // 创建投资记录
                var tr = new li_project_transactions
                {
                    dt_users = wallet.dt_users,
                    li_projects = huoqiProject,
                    type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.AutoInvest,
                    principal = maxInvestingAmount,
                    status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success,
                    create_time = wallet.last_update_time, // 时间应该一致
                    remark = maxInvestingAmount == investingMoney
                            ? "完全自动续投"
                            : $"部分自动续投，退回部分：{(investingMoney - maxInvestingAmount).ToString("c")}"
                };
                context.li_project_transactions.InsertOnSubmit(tr);

                // 修改钱包历史
                var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.AutoInvest);
                his.li_project_transactions = tr;
                context.li_wallet_histories.InsertOnSubmit(his);

                huoqiProject.investment_amount += maxInvestingAmount;

                BuyHuoqiClaims(context, maxInvestingAmount, tr);
            }
            else
            {
                // 完全没有续投，告诉用户
                var dtUserMessage = new dt_user_message
                {
                    type = 1,
                    accept_user_name = user.user_name,
                    title = "续投活期项目失败",
                    content = $"由于活期债权不足，没能帮您进行活期投资续投，本金 {investingMoney.ToString("c")} 已经退回到您的账号，请查收。",
                    post_user_name = "",
                    post_time = moment,
                    receiver = user.id
                };
                context.dt_user_message.InsertOnSubmit(dtUserMessage);
            }

            context.SubmitChanges();

            ContinueInvest(context, needContinueInvest.Skip(1), moment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="queryTime">查询时间，会根据它查询出不同的债权状态，默认是当前时间。区间应该位于还款计划进行期间</param>
        /// <returns></returns>
        public static Dictionary<li_claims, decimal> GetClaimRatio(this li_projects proj, DateTime? queryTime = null)
        {
            if (proj.IsHuoqiProject())
            {
                var claims = proj.li_claims_profiting.Where(c => c.IsProfiting(queryTime)).ToList();
                var huoqiProjectInvestmentAmount = claims.Aggregate(0m, (sum, c) => sum + c.principal);
                return claims.ToDictionary(c => c, c => c.principal / huoqiProjectInvestmentAmount);
            }

            var profitingClaims = queryTime == null
                    ? proj.li_claims.Where(c => c.IsProfiting()).ToList()
                    : proj.li_claims.Where(c => c.Parent == null).SelectMany(c => c.QueryLeafClaimsAtMoment(queryTime)).ToList();

            // 仅针对单个用户的还款
            if (proj.IsNewbieProject1())
            {
                return profitingClaims.ToDictionary(c => c, c => 1m);
            }

            // 计算出每个债权的本金占比，公式：债权金额 / 项目投资总额
            return profitingClaims.ToDictionary(c => c, c => c.principal / proj.investment_amount);
        }

        public static List<li_project_transactions> GenerateRepayTransactions(li_repayment_tasks repaymentTask,
            DateTime transactTime, bool unsafeCreateEntities = false, bool applyCostIntoInterest = false)
        {
            return repaymentTask.li_projects.GetClaimRatio()
                .GenerateRepayTransactions(repaymentTask, transactTime, unsafeCreateEntities, applyCostIntoInterest);
        }

        /// <summary>
        /// 生成还款交易记录（未保存）
        /// </summary>
        /// <param name="claimRatio"></param>
        /// <param name="repaymentTask"></param>
        /// <param name="transactTime">交易时间，一般传 task.shouldRepayTime </param>
        /// <param name="unsafeCreateEntities">如果设置了实体类到 project_transaction 然后 SubmitChanges 的话，ptr 会被插入到数据库 </param>
        /// <param name="applyCostIntoInterest">如果为 true ，则 interest 实际上为计算了 cost 的收益</param>
        /// <returns></returns>
        public static List<li_project_transactions> GenerateRepayTransactions(this Dictionary<li_claims, decimal> claimRatio,
            li_repayment_tasks repaymentTask, DateTime transactTime, bool unsafeCreateEntities = false, bool applyCostIntoInterest = false)
        {
            if (!claimRatio.Any())
                return Enumerable.Empty<li_project_transactions>().ToList();

            var shouldRepayInterest = repaymentTask.repay_interest;

            var interestSkipped = 0m;

            if (!repaymentTask.li_projects.IsNewbieProject1())
                Debug.Assert(Math.Round(claimRatio.Aggregate(0m, (sum, pair) => sum + pair.Value), 6) == 1, "债权比率之和不等于 1，会造成四舍五入结果异常");

            var rounded = claimRatio
                .Where(pair => repaymentTask.only_repay_to == null || pair.Key.userId == repaymentTask.only_repay_to) // 只对某投资者回款（新手标）
                .Select(pair =>
                {
                    var totalInterest = pair.Value * shouldRepayInterest;
                    var claim = pair.Key;

                    var agent = repaymentTask.li_projects.IsHuoqiProject() ? null : claim.dt_users_agent ?? claim.Parent?.dt_users_agent;

                    // 如果是活期债权，则以中间人的债权计息
                    var profitingDayLengthBaseClaim = agent != null ? claim.GetFirstHistoryClaimByOwner(agent.id) : claim;

                    // 根据买入时间计算利息
                    var realityInterest = Math.Round(totalInterest, 2);

                    // 为提现的债权计算应收利息
                    if (!repaymentTask.li_projects.IsHuoqiProject() && profitingDayLengthBaseClaim.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer)
                    {
                        var tmp = profitingDayLengthBaseClaim.GetProfitingSectionDays(repaymentTask,
                                (claimBeforeProfitingDays, claimProfitingDays, claimInvalidDays) =>
                                {
                                    var splitted = totalInterest.GetPerfectSplitStream(claimBeforeProfitingDays + claimProfitingDays + claimInvalidDays, 3).ToList();
                                    splitted = Utils.GetPerfectRounding(splitted, Math.Round(totalInterest, 2), 2);
                                    return new
                                    {
                                        skippedBefore = splitted.Take(claimBeforeProfitingDays).Aggregate(0m, (sum, p) => sum + p),
                                        interest = splitted
                                            .Skip(claimBeforeProfitingDays)
                                            .Take(claimProfitingDays)
                                            .Aggregate(0m, (sum, num) => sum + num),
                                        skippedAfter = splitted
                                            .Skip(claimBeforeProfitingDays + claimProfitingDays)
                                            .Take(claimInvalidDays)
                                            .Aggregate(0m, (sum, p) => sum + p)
                                    };
                                });
                        realityInterest = tmp.interest;
                        interestSkipped += tmp.skippedBefore + tmp.skippedAfter;
                    }

                    string remark = null;
                    if (repaymentTask.status == (int)Agp2pEnums.RepaymentStatusEnum.EarlierPaid && 0 < repaymentTask.cost)
                    {
                        var originalInterest = realityInterest + pair.Value * repaymentTask.cost.GetValueOrDefault();
                        remark = $"提前还款：此债权本期原来的待收益 {originalInterest:f2}，实际收益 {realityInterest:f2}";
                        if (applyCostIntoInterest)
                        {
                            realityInterest = originalInterest;
                        }
                    }
                    else if (repaymentTask.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTimePaid)
                    {
                        var originalInterest = realityInterest + pair.Value * repaymentTask.cost.GetValueOrDefault();
                        remark = $"逾期还款：此债权本期原来的待收益 {originalInterest:f2}，实际收益 {realityInterest:f2}";
                        if (applyCostIntoInterest)
                        {
                            realityInterest = originalInterest;
                        }
                    }

                    // 如果是定期还款并且存在 agent，则回款给 agent
                    var gainer = agent ?? claim.dt_users;

                    // 不能直接复制实体类，否则 context 保存后会添加记录
                    var ptr = new li_project_transactions
                    {
                        create_time = transactTime, // 变更时间应该等于还款计划的还款时间
                        type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.RepayToInvestor,
                        project = repaymentTask.project,
                        investor = gainer.id,
                        status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Success,
                        principal = pair.Value * repaymentTask.repay_principal,
                        interest = realityInterest,
                        remark = remark,
                        gainFromClaim = claim.id
                    };
                    if (unsafeCreateEntities)
                    {
                        // 如果设置了实体类到 project_transaction 然后 SubmitChanges 的话，ptr 会被插入到数据库
                        ptr.dt_users = gainer;
                        ptr.li_claims_from = claim;
                    }
                    return ptr;
                }).OrderBy(ptr => ptr.investor).ToList();

            if (!rounded.Any())
                return Enumerable.Empty<li_project_transactions>().ToList();

            var preRoundedInterest = rounded.Select(ptr => ptr.interest.GetValueOrDefault()).ToList();
            var forceSum = applyCostIntoInterest
                ? repaymentTask.cost.GetValueOrDefault() + shouldRepayInterest
                : shouldRepayInterest;
            var perfectRoundedInterest = Utils.GetPerfectRounding(preRoundedInterest, forceSum - interestSkipped, 2);
            rounded.ZipEach(perfectRoundedInterest, (ptr, newInterest) => ptr.interest = newInterest);

            var preRoundedPrincipal = rounded.Select(ptr => ptr.principal).ToList();
            var perfectRoundedPrincipal = Utils.GetPerfectRounding(preRoundedPrincipal, repaymentTask.repay_principal, 2);
            rounded.ZipEach(perfectRoundedPrincipal, (ptr, newPrincipal) => ptr.principal = newPrincipal);

            return rounded;
        }

        /// <summary>
        /// 提前还款，之后未还的还款计划作废，全部投资者减去该项目剩余的待收益金额，执行还款转账操作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="projectId"></param>
        /// <param name="remainTermPrincipalRatePercent">剩余期数的本息百分比率（不包括当前这期）</param>
        public static li_projects EarlierRepayAll(this Agp2pDataContext context, int projectId,
            decimal remainTermPrincipalRatePercent)
        {
            var project = context.li_projects.Single(p => p.id == projectId);
            var unpaidTasks =
                project.li_repayment_tasks.Where(t => t.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid).ToList();
            if (!unpaidTasks.Any()) throw new Exception("全部还款计划均已执行，不能进行提前还款");
            if (remainTermPrincipalRatePercent < 0 || 100 < remainTermPrincipalRatePercent)
                throw new Exception("剩余利息百分比率不正常");

            var now = DateTime.Now;

            var remainTermPrincipalRate = remainTermPrincipalRatePercent;

            var currentTask = unpaidTasks.First();

            var willInvalidTasks = unpaidTasks.Skip(1).ToList();
            if (!willInvalidTasks.Any())
            {
                // 向借款人收取还款
                GainLoanerRepayment(context, now, currentTask.id,
                    currentTask.li_projects.li_risks.li_loaners.user_id,
                    currentTask.repay_principal + currentTask.repay_interest);
                context.ExecuteRepaymentTask(currentTask.id, Agp2pEnums.RepaymentStatusEnum.EarlierPaid);
                return project;
            }

            willInvalidTasks.ForEach(t => t.status = (byte)Agp2pEnums.RepaymentStatusEnum.Invalid); // 原计划作废

            var remainPrincipal = willInvalidTasks.Sum(t => t.repay_principal);
            var remainInterest = willInvalidTasks.Sum(t => t.repay_interest);

            var willPayInterest = Math.Round(remainPrincipal * remainTermPrincipalRate, 2); // 未还本金 * 比率

            // 生成新的计划
            var earlierRepayTask = new li_repayment_tasks
            {
                li_projects = project,
                cost = remainInterest - willPayInterest,
                repay_interest = willPayInterest,
                repay_principal = remainPrincipal,
                should_repay_time = willInvalidTasks.Last().should_repay_time,
                term = willInvalidTasks.First().term,
                status = (byte)Agp2pEnums.RepaymentStatusEnum.Unpaid,
            };
            if (earlierRepayTask.cost < 0)
            {
                throw new Exception("数值异常：提前还款后投资者收到利息反而更高");
            }
            context.li_repayment_tasks.InsertOnSubmit(earlierRepayTask);

            context.SubmitChanges();

            GainLoanerRepayment(context, now, currentTask.id,
                currentTask.li_projects.li_risks.li_loaners.user_id,
                currentTask.repay_principal + currentTask.repay_interest);
            context.ExecuteRepaymentTask(currentTask.id, Agp2pEnums.RepaymentStatusEnum.EarlierPaid);

            GainLoanerRepayment(context, now, earlierRepayTask.id,
                currentTask.li_projects.li_risks.li_loaners.user_id,
                earlierRepayTask.repay_principal + earlierRepayTask.repay_interest);
            context.ExecuteRepaymentTask(earlierRepayTask.id, Agp2pEnums.RepaymentStatusEnum.EarlierPaid);

            return project;
        }

        /// <summary>
        /// 逾期还款
        /// </summary>
        /// <param name="context"></param>
        /// <param name="repayTaskId"></param>
        /// <param name="overTimePayRate"></param>
        public static void OverTimeRepay(this Agp2pDataContext context, int repayTaskId, Model.costconfig costconfig)
        {
            var repaymentTask = context.li_repayment_tasks.Single(r => r.id == repayTaskId);
            if (repaymentTask.status != (int)Agp2pEnums.RepaymentStatusEnum.OverTime)
                throw new InvalidOperationException("当前还款不是逾期还款！");

            //逾期罚息
            var overTimePayInterest = repaymentTask.repay_interest * costconfig.overtime_pay;
            repaymentTask.cost = repaymentTask.repay_interest - overTimePayInterest;
            repaymentTask.repay_interest = overTimePayInterest;
            //计算逾期管理费
            var projectTransaction = new li_company_inoutcome()
            {
                user_id = (int)repaymentTask.li_projects.li_risks.li_loaners.user_id,
                project_id = repaymentTask.project,
                type = (int)Agp2pEnums.OfflineTransactionTypeEnum.ManagementFeeOfOverTime,
                create_time = DateTime.Now,
                remark = $"收取'{repaymentTask.li_projects.title}'第{repaymentTask.term}期的逾期管理费"
            };

            var overDays = DateTime.Now.Subtract(repaymentTask.should_repay_time).Days;

            if (repaymentTask.li_projects.dt_article_category.call_index.ToUpper().Contains("YPB"))
            {
                //票据业务
                projectTransaction.income = repaymentTask.li_projects.financing_amount * overDays *
                                               costconfig.overtime_cost_bank;
            }
            else
            {
                //非票据业务
                if (overDays <= 30)
                    projectTransaction.income = repaymentTask.li_projects.financing_amount * overDays *
                                                   costconfig.overtime_cost;
                else
                    projectTransaction.income = repaymentTask.li_projects.financing_amount * overDays *
                                                   costconfig.overtime_cost2;
            }
            context.li_company_inoutcome.InsertOnSubmit(projectTransaction);

            context.SubmitChanges();
            context.ExecuteRepaymentTask(repayTaskId, Agp2pEnums.RepaymentStatusEnum.OverTimePaid);
        }

        /// <summary>
        /// 撤销投资（功能只在后台）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="projectTransactionId"></param>
        /// <param name="refundTime"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        public static li_project_transactions Refund(this Agp2pDataContext context, int projectTransactionId, DateTime refundTime, bool save = true)
        {
            // 判断项目状态
            var tr = context.li_project_transactions.Single(t => t.id == projectTransactionId);
            if ((int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying <= tr.li_projects.status)
                throw new InvalidOperationException("项目所在的状态不能退款");

            // 更改交易状态
            tr.status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Rollback;

            // 债权失效
            var invalidClaims = tr.li_claims_invested.Where(c => c.IsLeafClaim())
                .Select(c => c.NewStatusChild(refundTime, Agp2pEnums.ClaimStatusEnum.Invalid))
                .ToList();
            context.li_claims.InsertAllOnSubmit(invalidClaims);

            // 修改项目已投资金额
            tr.li_projects.investment_amount -= tr.principal;

            // 更改钱包金额
            var wallet = tr.dt_users.li_wallets;

            // 如果项目已经正在还款再撤销，就需要减掉 待收利润；满标前撤销的话待收利润未计算，所以不用减
            var investedMoney = tr.principal;
            wallet.idle_money += investedMoney;
            wallet.investing_money -= investedMoney;
            wallet.total_investment -= investedMoney;
            wallet.last_update_time = refundTime;

            // 添加钱包历史
            var his = CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.InvestorRefund);
            his.li_project_transactions = tr;
            context.li_wallet_histories.InsertOnSubmit(his);

            if (save)
            {
                context.SubmitChanges();
                MessageBus.Main.PublishAsync(new UserRefundMsg(projectTransactionId));
            }

            return tr;
        }

        /// <summary>
        /// 项目流标，将项目的全部投资退款
        /// </summary>
        /// <param name="context"></param>
        /// <param name="projectId"></param>
        public static void ProjectFinancingFail(this Agp2pDataContext context, int projectId)
        {
            // 判断项目状态
            var proj = context.li_projects.Single(t => t.id == projectId);
            if ((int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying <= proj.status)
                throw new InvalidOperationException("项目所在的状态不能退款");

            var refundTime = DateTime.Now;
            proj.li_project_transactions.Where(
                tr =>
                    tr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    tr.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .Select(tr => tr.id)
                .ForEach(trId => context.Refund(trId, refundTime, false));

            proj.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingFail;
            context.SubmitChanges();

            MessageBus.Main.PublishAsync(new ProjectFinancingFailMsg(projectId));
        }

        /// <summary>
        /// 取得投资进度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="projectId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static T GetInvestmentProgress<T>(this li_projects pro, Func<decimal, decimal, T> callback)
        {
            Debug.Assert(pro.investment_amount <= pro.financing_amount);
            return callback(pro.investment_amount, pro.financing_amount);
        }

        public static decimal GetInvestmentProgressPercent(this li_projects pro)
        {
            return pro.GetInvestmentProgress((a, b) => (a / b));
        }

        public static string GetInvestmentBalance(this li_projects pro)
        {
            return pro.GetInvestmentProgress((a, b) => (b - a).ToString("n0")) + "元";
        }

        /// <summary>
        /// 获取投资人数
        /// </summary>
        /// <param name="pro"></param>
        /// <returns></returns>
        public static int GetInvestedUserCount(this li_projects pro, Agp2pEnums.ProjectTransactionStatusEnum filter = Agp2pEnums.ProjectTransactionStatusEnum.Success)
        {
            return
                pro.li_project_transactions.Where(t => t.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                                                       t.status == (int)filter)
                    .GroupBy(t => t.investor)
                    .Count();
        }

        /// <summary>
        /// 累计赚取
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static decimal QueryTotalProfit(this Agp2pDataContext context)
        {
            return context.li_wallets.Select(w => w.total_profit).AsEnumerable().DefaultIfEmpty(0).Sum();
        }

        /// <summary>
        /// 累计投资
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static decimal QueryTotalInvested(this Agp2pDataContext context)
        {
            return context.li_wallets.Select(w => w.total_investment).AsEnumerable().DefaultIfEmpty(0).Sum();
        }

        /// <summary>
        /// 累计待收
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static decimal QueryTotalInvesting(this Agp2pDataContext context)
        {
            return context.li_wallets.Select(w => w.investing_money).AsEnumerable().DefaultIfEmpty(0).Sum();
        }

        /// <summary>
        /// N 日内的成交量
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inDaysEarlier"></param>
        /// <returns></returns>
        public static decimal QueryTradingVolume(this Agp2pDataContext context, int inDaysEarlier)
        {
            var now = DateTime.Now;
            return context.li_project_transactions.Where(
                r =>
                    r.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                    r.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    now.AddDays(-inDaysEarlier).Date <= r.create_time.Date && now.Date > r.create_time.Date)
                .Select(r => r.principal).AsEnumerable().DefaultIfEmpty(0).Sum();
        }

        /// <summary>
        /// 获取钱包历史中的收入金额（返回两个数，所以要用回调）
        /// </summary>
        /// <param name="his"></param>
        /// <returns></returns>
        public static T QueryTransactionIncome<T>(li_wallet_histories his, Func<decimal?, decimal?, T> callback)
        {
            if (his.li_bank_transactions != null)
            {
                var btrType = his.li_bank_transactions.type;
                var chargedValue = btrType == (int)Agp2pEnums.BankTransactionTypeEnum.Withdraw || btrType == (int)Agp2pEnums.BankTransactionTypeEnum.GainLoanerRepay
                    ? (decimal?)null
                    : his.li_bank_transactions.value;
                return callback(chargedValue, null);
            }
            if (his.li_project_transactions != null)
            {
                decimal? receivedPrincipal, profited;
                if (his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.Invest
                    || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess
                    || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.AgentPaidInterest
                    || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.AgentRecaptureHuoqiClaims
                    || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredIn
                    || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredInSuccess)
                {
                    receivedPrincipal = profited = null;
                }
                else
                {
                    receivedPrincipal = his.li_project_transactions.principal;
                    profited = his.li_project_transactions.interest;
                }
                return callback(receivedPrincipal, profited);
            }
            if (his.li_activity_transactions != null &&
                his.li_activity_transactions.type == (int)Agp2pEnums.ActivityTransactionTypeEnum.Lost)
            {
                return callback(null, null);
            }
            var gainValue = his.li_activity_transactions != null ? his.li_activity_transactions.value : (decimal?)null;
            return callback(gainValue, null);
        }

        public static string QueryTransactionIncome<T>(li_wallet_histories his)
        {
            if (typeof(T) == typeof(string)) // 返回羊角符号
            {
                return QueryTransactionIncome(his, (principal, profit) =>
                {
                    if (principal != null && profit != null)
                        return string.Format("{0:c} + {1:c}", principal, profit);
                    else if (principal == null && profit == null)
                        return "";
                    else if (principal == null)
                        return profit.Value.ToString("c");
                    else
                        return principal.Value.ToString("c");
                });
            }
            else if (typeof(T) == typeof(decimal?)) // 没有羊角符号
            {
                return QueryTransactionIncome(his, (principal, profit) =>
                {
                    if (principal != null && profit != null)
                        return string.Format("{0} + {1}", principal, profit);
                    else if (principal == null && profit == null)
                        return "";
                    else if (principal == null)
                        return profit.Value.ToString();
                    else
                        return principal.Value.ToString();
                });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 获取钱包历史中的支出金额
        /// </summary>
        /// <param name="his"></param>
        /// <returns></returns>
        public static decimal? QueryTransactionOutcome(this li_wallet_histories his)
        {
            if (his.li_bank_transactions != null)
            {
                var btrType = his.li_bank_transactions.type;
                if (his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.WithdrawConfirm
                    || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredInSuccess)
                {
                    return null;
                }
                return btrType == (int)Agp2pEnums.BankTransactionTypeEnum.Charge || btrType == (int)Agp2pEnums.BankTransactionTypeEnum.LoanerMakeLoan
                    ? (decimal?)null
                    : his.li_bank_transactions.value; // 提现
            }
            if (his.li_project_transactions != null)
            {
                if (his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess) // 项目满标不显示支出
                    return null;
                if (his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.AgentPaidInterest)
                    return his.li_project_transactions.interest.GetValueOrDefault();
                return his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.Invest
                        || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.AgentRecaptureHuoqiClaims
                        || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredIn
                        || his.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.ClaimTransferredInSuccess
                    ? his.li_project_transactions.principal // 投资
                    : (decimal?)null;
            }
            // 活动扣除
            if (his.li_activity_transactions != null &&
                his.li_activity_transactions.type == (int)Agp2pEnums.ActivityTransactionTypeEnum.Gain) return null;
            return his.li_activity_transactions?.value;
        }

        public static string GetProjectTermSpanEnumDesc(this li_projects proj)
        {
            var desc = Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTermSpanEnum)proj.repayment_term_span);
            /*if ((Agp2pEnums.ProjectRepaymentTermSpanEnum) proj.repayment_term_span == Agp2pEnums.ProjectRepaymentTermSpanEnum.Month)
                return "个" + desc;*/
            return desc;
        }

        public static string GetProjectStatusDesc(this li_projects proj)
        {
            return Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectStatusEnum)proj.status);
        }

        public static string GetProjectRepaymentTypeDesc(this li_projects proj)
        {
            return Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTypeEnum)proj.repayment_type);
        }

        public static string GetProfitRateYearly(this li_projects proj)
        {
            return (proj.profit_rate_year / 100).ToString("p2");
        }

        public static string GetRepaymentTaskProgress(this li_repayment_tasks task)
        {
            var count = task.li_projects.li_repayment_tasks.Count(t => t.status != (int)Agp2pEnums.RepaymentStatusEnum.Invalid);
            return $"{task.term}/{count}";
        }

        public static string GetInvestContractContext(this Agp2pDataContext context, li_project_transactions investment, string templatePath)
        {
            var project = investment.li_projects;
            //获得投资协议模板（暂时为票据，TODO 其他产品的投资协议）
            var a4Template = File.ReadAllText(templatePath);
            //替换模板内容
            var lastRepaymentTask = project.li_repayment_tasks.LastOrDefault(t => t.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid);
            return a4Template.Replace("{title}", project.title + " 票据质押借款协议")
                .Replace("{contract_no}", investment.agree_no)
                //甲方(借款人)信息
                .Replace("{company_name}",
                    project.li_risks.li_loaners.li_loaner_companies != null
                        ? project.li_risks.li_loaners.li_loaner_companies.name
                        : "")
                .Replace("{user_name_loaner}", project.li_risks.li_loaners.dt_users.real_name)
                .Replace("{business_license}", project.li_risks.li_loaners.li_loaner_companies?.business_license_no)
                .Replace("{organization_certificate}", project.li_risks.li_loaners.li_loaner_companies?.organization_no)

                //乙方(投资人)信息
                .Replace("{user_real_name_invester}", investment.dt_users.real_name)
                .Replace("{user_name_invester}", investment.dt_users.user_name)
                .Replace("{id_card_invester}", investment.dt_users.id_card_number)

                //借款明细
                .Replace("{loan_amount}", project.financing_amount.ToString("N0"))
                .Replace("{loan_amount_upper}", project.financing_amount.ToRmbUpper())
                .Replace("{invest_amount}", investment.principal.ToString("N0"))
                .Replace("{invest_amount_upper}", investment.principal.ToRmbUpper())
                .Replace("{profit_rate_year}", (project.profit_rate_year / 100).ToString("p2"))
                .Replace("{repayment_term_span}", project.repayment_term_span_count + "天")
                .Replace("{make_loan_date}", project.make_loan_time?.ToString("yyyy年MM月dd日"))
                .Replace("{complete_date}", lastRepaymentTask?.should_repay_time.ToString("yyyy年MM月dd日") ?? "")

                //质押汇票明细
                .Replace("{bill_no}", project.GetMortgageInfo("no"))
                .Replace("{bill_amount}", project.GetMortgageInfo("amount"))
                .Replace("{bill_end_date}", project.GetMortgageInfo("end_time"))
                .Replace("{bill_bank}", project.GetMortgageInfo("bank"));
        }

        public static string GetMortgageInfo(this li_projects proj, string propertyKey)
        {
            if (proj.dt_article_category.call_index == "ypb")
            {
                return proj.li_risks.li_risk_mortgage.Select(rm => rm.li_mortgages).Select(m =>
                {
                    var schemeObj = (JObject)JsonConvert.DeserializeObject(m.li_mortgage_types.scheme);
                    var kv = (JObject)JsonConvert.DeserializeObject(m.properties);
                    var bankName = schemeObj.Cast<KeyValuePair<string, JToken>>().Where(p => p.Key.ToString() == propertyKey)
                        .Select(p => kv[p.Key].ToString()).SingleOrDefault();
                    return bankName;
                }).FirstOrDefault() ?? "";
            }
            return "";
        }

        public static string GetLonerName(this Agp2pDataContext context, int projectId)
        {
            var project = context.li_projects.SingleOrDefault(p => p.id == projectId);
            if (project != null)
            {
                switch (project.type)
                {
                    case (int)Agp2pEnums.LoanTypeEnum.Company:
                        return project.li_risks.li_loaners?.li_loaner_companies?.name;
                    default:
                        if (project.li_risks.li_loaners == null)
                            return "";
                        var user = project.li_risks.li_loaners.dt_users;
                        return $"{user.real_name}({user.user_name})";
                }
            }
            return "";
        }
    }
}