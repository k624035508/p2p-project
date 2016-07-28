using System;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Transactions;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;
using Agp2p.Core;
using Newtonsoft.Json;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 项目交易接口响应处理
    /// </summary>
    internal class ProjectTransHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<BidRespMsg>(Bid); //投资
            MessageBus.Main.Subscribe<WithDrawalRespMsg>(Withdrawal); //撤销投标
            MessageBus.Main.Subscribe<RepealProjectRespMsg>(RepealProject); //流标
            MessageBus.Main.Subscribe<MakeLoanRespMsg>(MakeLoan); //放款
            MessageBus.Main.Subscribe<RepayRespMsg>(Repay); //还款
            MessageBus.Main.Subscribe<ReturnPrinInteRespMsg>(ReturnPrinInte); //本息到账
            MessageBus.Main.Subscribe<CreditAssignmentRespMsg>(CreditAssignment); //债权转让
            MessageBus.Main.Subscribe<QueryProjectRespMsg>(QueryProject); //查询项目
            MessageBus.Main.Subscribe<RemoveCardRespMsg>(RemoveCard); //解绑银行卡
        }

        /// <summary>
        /// 个人投标/自动投标 响应
        /// </summary>
        /// <param name="msg"></param>
        private static void Bid(BidRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
#if !DEBUG
                        //同步返回平台不做处理
                        if (msg.Result.Equals("00001")) return;
#endif

                        //查找对应的交易流水
                        //var trans = context.li_project_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                        //if (trans != null)
                        //{
                        //TODO 检查用户资金信息
                        TransactionFacade.Invest((int)msg.UserIdIdentity, msg.ProjectCode,
                            Utils.StrToDecimal(msg.Sum, 0), msg.RequestId);
                        msg.HasHandle = true;
                        //}
                        //else
                        //{
                        //    msg.Remarks = "没有找到平台交易流水记录，交易流水号为：" + msg.RequestId;
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 个人撤标响应
        /// </summary>
        /// <param name="msg"></param>
        private static void Withdrawal(WithDrawalRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
#if !DEBUG
                        //同步返回平台不做处理
                        if (msg.Result.Equals("00001")) return;
#endif

                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的原交易流水
                        var trans = context.li_project_transactions.SingleOrDefault(u => u.no_order == msg.BidRequestId);
                        if (trans != null)
                        {
                            context.Refund(trans.id, DateTime.Now);
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台交易流水记录，交易流水号为：" + msg.RequestId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 流标普通项目
        /// </summary>
        /// <param name="msg"></param>
        private static void RepealProject(RepealProjectRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
#if !DEBUG
                        //同步返回平台不做处理
                        if (msg.Result.Equals("00001")) return;
#endif

                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的项目
                        var pro = context.li_projects.SingleOrDefault(p => p.id == msg.ProjectCode);
                        if (pro != null)
                        {
                            context.ProjectFinancingFail(pro.id);
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台项目，项目编号为：" + msg.ProjectCode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 项目放款
        /// </summary>
        /// <param name="msg"></param>
        private static void MakeLoan(MakeLoanRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的项目
                        var pro = context.li_projects.SingleOrDefault(p => p.id == msg.ProjectCode);
                        if (pro != null)
                        {
                            //异步返回才放款,内网测试使用同步
#if DEBUG
                            if (msg.Sync)
                            {
#endif
#if !DEBUG
                            if (!msg.Sync)
                            {
#endif
                                //定期项目进入开始还款，活期项目直接向借款人放款
                                if (pro.IsHuoqiProject())
                                {
                                    TransactionFacade.MakeLoan(context, DateTime.Now, pro, pro.li_risks.li_loaners.user_id);
                                }
                                else
                                {
                                    context.StartRepayment(pro.id);
                                }
                            }
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台项目，项目编号为：" + msg.ProjectCode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 还款
        /// </summary>
        /// <param name="msg"></param>
        private static void Repay(RepayRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //异步返回才执行,内网测试使用同步
#if DEBUG
                        if (msg.Sync)
                        {
#endif
#if !DEBUG
                        if (!msg.Sync)
                        {
#endif
                            var req = context.li_pay_request_log.SingleOrDefault(r => r.id == msg.RequestId);
                            if (req != null)
                            {
                                //查找对应的项目
                                var pro = context.li_projects.SingleOrDefault(p => p.id == msg.ProjectCode);
                                if (pro != null)
                                {
                                    if (!string.IsNullOrEmpty(req.remarks))
                                    {
                                        //活期项目不需要生成还款记录
                                        if (!msg.HuoqiRepay)
                                        {
                                            var dic = Utils.UrlParamToData(req.remarks);
                                            int repayId = Utils.StrToInt(dic["repayTaskId"], 0);
                                            context.GainLoanerRepayment(DateTime.Now, repayId, (int) msg.UserIdIdentity,
                                                Utils.StrToDecimal(msg.Sum, 0));

                                            //如果是手动还款立刻发送本息到账请求 TODO 是否需要？
                                            if (!msg.AutoRepay)
                                            {
                                                RequestApiHandle.SendReturnPrinInte(msg.ProjectCode, msg.Sum, repayId,
                                                    Utils.StrToBool(dic["isEarly"], false), false);
                                            }
                                        }
                                        msg.HasHandle = true;
                                    }
                                    else
                                        msg.Remarks = "请求没有包含还款计划信息！";
                                }
                                else
                                    msg.Remarks = "没有找到平台项目，项目编号为：" + msg.ProjectCode;
                            }
                            else
                                msg.Remarks = "没有找到对应的还款请求，请求编号为：" + msg.RequestId;
                        }
                    }
                }
            }
            catch (ChangeConflictException)
            {
                MessageBus.Main.Publish(msg);
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 本息到账
        /// </summary>
        /// <param name="msg"></param>
        private static void ReturnPrinInte(ReturnPrinInteRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
                        //异步返回才执行,内网测试使用同步
#if DEBUG
                        if (msg.Sync)
                        {
#endif
#if !DEBUG
                        if (!msg.Sync)
                        {
#endif
                            Agp2pDataContext context = new Agp2pDataContext();
                            var req = context.li_pay_request_log.SingleOrDefault(r => r.id == msg.RequestId);
                            if (req != null)
                            {
                                var dic = Utils.UrlParamToData(req.remarks);
                                //活期项目不需要执行还款计划
                                if (!Utils.StrToBool(dic["isHuoqi"], false))
                                {
                                    if (!Utils.StrToBool(dic["isEarly"], false))
                                        context.ExecuteRepaymentTask(Utils.StrToInt(dic["repayTaskId"], 0));
                                    else
                                        context.EarlierRepayAll(msg.ProjectCode, ConfigLoader.loadCostConfig().earlier_pay);
                                }
                            }
                            else
                                msg.Remarks = "没有找到对应的还款请求，请求编号为：" + msg.RequestId;
                            msg.HasHandle = true;
                        }
                    }
                }
            }
            catch (ChangeConflictException)
            {
                MessageBus.Main.Publish(msg);
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 债权转让
        /// </summary>
        /// <param name="msg"></param>
        private static void CreditAssignment(CreditAssignmentRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
#if !DEBUG
                        //同步返回平台不做处理
                        if (msg.Result.Equals("00001")) return;
#endif

                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的债权交易流水
                        var trans =
                            context.li_project_transactions.SingleOrDefault(p => p.no_order == msg.OriginalRequestId);
                        if (trans != null)
                        {
                            TransactionFacade.BuyClaim(context,
                                trans.li_claims_invested.OrderByDescending(c => c.createTime)
                                    .First(c => c.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer)
                                    .id, (int)msg.UserIdIdentity,
                                Utils.StrToDecimal(msg.AssignmentSum, 0));
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台项目，项目编号为：" + msg.ProjectCode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 查询活期项目
        /// </summary>
        /// <param name="msg"></param>
        private static void QueryProject(QueryProjectRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
                        //根据放款余额，发送放款请求
                        if (Utils.StrToDecimal(msg.LoanAccountBalance, 0) > 0)
                        {
                            Agp2pDataContext context = new Agp2pDataContext();
                            var project = context.li_projects.SingleOrDefault(p => p.id == msg.ProjectCode);
                            if (project != null)
                            {
                                var makeLoanReqMsg = new MakeLoanReqMsg(project.li_risks.li_loaners.user_id, project.id, msg.LoanAccountBalance, true);
                                MessageBus.Main.PublishAsync(makeLoanReqMsg, ar =>
                                {
                                    var msgResp = BaseRespMsg.NewInstance<MakeLoanRespMsg>(makeLoanReqMsg.SynResult);
                                    msgResp.Sync = true;
                                    MessageBus.Main.PublishAsync(msgResp);
                                });
                            }
                            else
                            {
                                msg.Remarks = "没有找到对应的项目，项目id：" + msg.ProjectCode;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 银行卡解绑响应处理
        /// </summary>
        /// <param name="msg"></param>
        private static void RemoveCard(RemoveCardRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
#if !DEBUG
                        //同步平台不做处理
                        if(msg.Result.Equals("00001")) return;
#endif
                        Agp2pDataContext context = new Agp2pDataContext();
                        var user = context.dt_users.SingleOrDefault(u => u.id == msg.UserIdIdentity);
                        if (user.li_bank_accounts.Any())
                        {
                            var quickpay = user.li_bank_accounts.SingleOrDefault(a => a.type == (int)Agp2pEnums.BankAccountType.QuickPay);
                            quickpay.type = (int)Agp2pEnums.BankAccountType.WebBank;                           
                            context.SubmitChanges();
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到解绑信息，流水号为：" + msg.RequestId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }
    }
}
