using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 个人投标普通/集合项目
    /// </summary>
    internal class ManualBidHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<ManualBidRespMsg>(ManualBid);
        }

        private static void ManualBid(ManualBidRespMsg msg)
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
    }
}
