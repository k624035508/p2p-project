using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 个人自动还款开通/取消处理
    /// </summary>
    internal class AutoRepaySignHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<AutoRepaySignRespMsg>(AutoRepaySign);
        }

        private static void AutoRepaySign(AutoRepaySignRespMsg msg)
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
                            if (msg.Cancel)
                            {
                                user.autoRepay = null;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(msg.BankAccount))
                                {
                                    user.autoRepay = JsonHelper.ObjectToJSON(new
                                    {
                                        type = "Bank",
                                        msg.BankAccount,
                                        msg.BankName,
                                        msg.Name
                                    });
                                }
                                else
                                {
                                    user.autoRepay = JsonHelper.ObjectToJSON(new
                                    {
                                        type = "Account"
                                    });
                                }
                            }
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
