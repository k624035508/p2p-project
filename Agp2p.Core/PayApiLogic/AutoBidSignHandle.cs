using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 个人自动投标续约/取消处理
    /// </summary>
    internal class AutoBidSignHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<AutoBidSignRespMsg>(AutoBidSign);
        }

        private static void AutoBidSign(AutoBidSignRespMsg msg)
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
                        //查找对应的平台账户，更新用户信息
                        var user = context.dt_users.SingleOrDefault(u => u.id == msg.UserId);
                        if (user != null)
                        {
                            user.protocolCode = !msg.Cancel? msg.ProtocolCode : null;
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
