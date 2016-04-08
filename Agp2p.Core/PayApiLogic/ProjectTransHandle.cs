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
                        TransactionFacade.Invest((int)msg.UserIdIdentity, Utils.StrToInt(msg.ProjectCode, 0), Utils.StrToDecimal(msg.Sum, 0), msg.RequestId);
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
                        var pro = context.li_projects.SingleOrDefault(p => p.id == Utils.StrToInt(msg.ProjectCode, 0));
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
                        var pro = context.li_projects.SingleOrDefault(p => p.id == Utils.StrToInt(msg.ProjectCode, 0));
                        if (pro != null)
                        {
                            //TODO 正式后改为异步返回才放款
                            if (msg.Sync)
                            {
                                //开始还款，包括向借款人放款
                                context.StartRepayment(Utils.StrToInt(msg.ProjectCode, 0));
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
                        var pro = context.li_projects.SingleOrDefault(p => p.id == Utils.StrToInt(msg.ProjectCode, 0));
                        if (pro != null)
                        {
                            var req = context.li_pay_request_log.SingleOrDefault(r => r.id == msg.RequestId);
                            if (req != null)
                            {
                                var dic = Utils.UrlParamToData(req.remarks);
                                int repayId = Utils.StrToInt(dic["repayTaskId"], 0);
                                var repayRask = context.li_repayment_tasks.SingleOrDefault(r => r.id == repayId);
                                if (repayRask != null)
                                {
                                    if (Utils.StrToBool(dic["isEarly"], false))
                                    {
                                        //TODO 提前还款
                                    }
                                    else
                                    {
                                        //生成还款记录
                                        context.GainLoanerRepayment(DateTime.Now, repayId, (int) msg.UserIdIdentity,
                                            Utils.StrToDecimal(msg.Sum, 0));
                                        //手动还款立刻发送本息到账请求
                                        if (!msg.AutoRepay)
                                        {
                                            //计算投资者本息明细
                                            var transList = TransactionFacade.GenerateRepayTransactions(repayRask, DateTime.Now);
                                            //创建本息到账请求并设置分账列表
                                            var returnPrinInteReqMsg = new ReturnPrinInteReqMsg(msg.ProjectCode, msg.Sum);
                                            returnPrinInteReqMsg.SetSubledgerList(transList);
                                            //发送请求
                                            MessageBus.Main.Publish(returnPrinInteReqMsg);
                                            //处理请求结果
                                            var returnPrinInteRespMsg = BaseRespMsg.NewInstance<ReturnPrinInteRespMsg>(returnPrinInteReqMsg.SynResult);
                                            returnPrinInteRespMsg.Sync = true;
                                            returnPrinInteRespMsg.RepayTaskId = repayId;
                                            MessageBus.Main.Publish(returnPrinInteRespMsg);
                                            if (!returnPrinInteRespMsg.HasHandle)
                                            {
                                                msg.Remarks = "本息到账失败：" + returnPrinInteRespMsg.Remarks;
                                            }
                                        }
                                    }
                                    msg.HasHandle = true;
                                }
                                else
                                    msg.Remarks = "没有找到对应的还款计划，还款计划编号为：" + repayId;
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
                        {
                            context.ExecuteRepaymentTask(msg.RepayTaskId);
                            msg.HasHandle = true;
                        }
                        else
                        {
                            //TODO 提前还款


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
                        //查找对应的债权
                        var pro = context.li_projects.SingleOrDefault(p => p.id == Utils.StrToInt(msg.ProjectCode, 0));
                        if (pro != null)
                        {
                            //TODO 债权转让

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
