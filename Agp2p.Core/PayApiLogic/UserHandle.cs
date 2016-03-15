using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 用户接口响应处理
    /// </summary>
    internal class UserHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<UserRegisterRespMsg>(UserRegister);
            MessageBus.Main.Subscribe<AutoBidSignRespMsg>(AutoBidSign);
            MessageBus.Main.Subscribe<AutoRepaySignRespMsg>(AutoRepaySign);
        }

        /// <summary>
        /// 开户处理
        /// </summary>
        /// <param name="msg"></param>
        private static void UserRegister(UserRegisterRespMsg msg)
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
                            user.identity_id = msg.IdentityId;
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
        /// 自动投标签约处理
        /// </summary>
        /// <param name="msg"></param>
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
                            user.protocolCode = !msg.Cancel ? msg.ProtocolCode : null;
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
        /// 自动还款签约处理
        /// </summary>
        /// <param name="msg"></param>
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
