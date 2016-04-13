using System;
using System.Linq;
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
            MessageBus.Main.Subscribe<BidRespMsg>(Bid);//投资
            MessageBus.Main.Subscribe<WithDrawalRespMsg>(Withdrawal);//撤销投标
            MessageBus.Main.Subscribe<RepealProjectRespMsg>(RepealProject);//流标
            MessageBus.Main.Subscribe<MakeLoanRespMsg>(MakeLoan);//放款
            MessageBus.Main.Subscribe<RepayRespMsg>(Repay);//还款
            MessageBus.Main.Subscribe<ReturnPrinInteRespMsg>(ReturnPrinInte);//本息到账
            MessageBus.Main.Subscribe<CreditAssignmentRespMsg>(CreditAssignment);//债权转让
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
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的交易流水
                        //var trans = context.li_project_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                        //if (trans != null)
                        //{
                        //TODO 检查用户资金信息
                        TransactionFacade.Invest((int)msg.UserIdIdentity, msg.ProjectCode, Utils.StrToDecimal(msg.Sum, 0), msg.RequestId);
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
                            //TODO 正式后改为异步返回才放款
                            if (msg.Sync)
                            {
                                //开始还款，包括向借款人放款
                                context.StartRepayment(msg.ProjectCode);
                                msg.HasHandle = true;
                            }
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
                        //查找对应的项目
                        var pro = context.li_projects.SingleOrDefault(p => p.id == msg.ProjectCode);
                        if (pro != null)
                        {
                            var req = context.li_pay_request_log.SingleOrDefault(r => r.id == msg.RequestId);
                            if (req != null)
                            {
                                if (!string.IsNullOrEmpty(req.remarks))
                                {
                                    var dic = Utils.UrlParamToData(req.remarks);
                                    int repayId = Utils.StrToInt(dic["repayTaskId"], 0);
                                    //生成还款记录
                                    context.GainLoanerRepayment(DateTime.Now, repayId, (int)msg.UserIdIdentity, Utils.StrToDecimal(msg.Sum, 0));
                                    //如果是手动还款立刻发送本息到账请求
                                    if (!msg.AutoRepay)
                                    {
                                        RequestApiHandle.SendReturnPrinInte(msg.ProjectCode, msg.Sum, repayId,
                                            Utils.StrToBool(dic["isEarly"], false));
                                    }
                                    msg.HasHandle = true;
                                }
                                else
                                    msg.Remarks = "请求没有包含还款计划信息！";

                            }
                            else
                                msg.Remarks = "没有找到对应的还款请求，请求编号为：" + msg.RequestId;
                        }
                        else
                            msg.Remarks = "没有找到平台项目，项目编号为：" + msg.ProjectCode;
                    }
                }
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
                        Agp2pDataContext context = new Agp2pDataContext();
                        if (!msg.IsEarlyPay)
                            context.ExecuteRepaymentTask(msg.RepayTaskId);
                        else
                            context.EarlierRepayAll(msg.ProjectCode, ConfigLoader.loadCostConfig().earlier_pay);
                        msg.HasHandle = true;
                    }
                }
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
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的债权交易流水
                        var trans = context.li_project_transactions.SingleOrDefault(p => p.no_order == msg.OriginalRequestId);
                        if (trans != null)
                        {
                            TransactionFacade.BuyClaim(context, trans.li_claims_invested.First(c => c.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer).id, (int) msg.UserIdIdentity,
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
    }
}
