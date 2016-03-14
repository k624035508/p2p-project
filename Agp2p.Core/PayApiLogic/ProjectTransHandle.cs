using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 项目交易接口响应处理
    /// </summary>
    internal class ProjectTransHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<BidRespMsg>(Bid);
            MessageBus.Main.Subscribe<WithDrawalRespMsg>(Withdrawal);
            MessageBus.Main.Subscribe<RepealProjectRespMsg>(RepealProject);
        }

        /// <summary>
        /// 个人投标/自动投标 响应
        /// </summary>
        /// <param name="msg"></param>
        private static void Bid(BidRespMsg msg)
        {
            try
            {
                //检查签名
                if (msg.CheckSignature())
                {
                    //检查请求处理结果
                    if (msg.CheckResult())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的交易流水
                        var trans = context.li_project_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                        if (trans != null)
                        {
                            //更新流水信息

                                
                            //检查用户资金信息


                            context.SubmitChanges();
                            msg.HasHandle = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO 返回错误信息
                throw ex;
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
                //检查签名
                if (msg.CheckSignature())
                {
                    //检查请求处理结果
                    if (msg.CheckResult())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的原交易流水
                        var trans = context.li_project_transactions.SingleOrDefault(u => u.no_order == msg.BidRequestId);
                        if (trans != null)
                        {
                            //撤销投资


                            context.SubmitChanges();
                            msg.HasHandle = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO 返回错误信息
                throw ex;
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
                //检查签名
                if (msg.CheckSignature())
                {
                    //检查请求处理结果
                    if (msg.CheckResult())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的项目
                        var pro = context.li_projects.SingleOrDefault(p => p.id == Utils.StrToInt(msg.ProjectCode, 0));
                        if (pro != null)
                        {
                            //流标


                            context.SubmitChanges();
                            msg.HasHandle = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO 返回错误信息
                throw ex;
            }
        }
    }
}
