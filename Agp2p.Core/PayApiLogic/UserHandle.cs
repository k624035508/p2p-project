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
            MessageBus.Main.Subscribe<UserRealNameAuthRespMsg>(UserRealNameAuth);
            MessageBus.Main.Subscribe<UserRegisterRespMsg>(UserRegister);
            MessageBus.Main.Subscribe<AutoBidSignRespMsg>(AutoBidSign);
            MessageBus.Main.Subscribe<AutoRepaySignRespMsg>(AutoRepaySign);
            MessageBus.Main.Subscribe<Transfer2UserRespMsg>(Transfer2User);
        }

        /// <summary>
        /// 实名验证处理
        /// </summary>
        /// <param name="msg"></param>
        private static void UserRealNameAuth(UserRealNameAuthRespMsg msg)
        {
            try
            {
                Agp2pDataContext context = new Agp2pDataContext();
                //实名验证只有同步返回故需要在这里保存响应日志
                var respLog = new li_pay_response_log()
                {
                    request_id = msg.RequestId,
                    result = msg.Result,
                    status = (int)Agp2pEnums.SumapayResponseEnum.Return,
                    response_time = DateTime.Now,
                    response_content = msg.ResponseContent
                };
                context.li_pay_response_log.InsertOnSubmit(respLog);

                var requestLog = context.li_pay_request_log.SingleOrDefault(r => r.id == msg.RequestId);
                if (requestLog != null)
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
                        //检查请求处理结果
                        if (msg.CheckResult())
                        {
                            //查找对应的平台账户，更新用户信息
                            var user = context.dt_users.SingleOrDefault(u => u.id == msg.UserIdIdentity);
                            if (user != null)
                            {
                                if (msg.Status.Equals("0"))
                                {
                                    //更新客户信息
                                    user.token = msg.Token;
                                    user.real_name = msg.UserName;
                                    user.id_card_number = msg.IdNumber;
                                    //更新响应日志
                                    respLog.user_id = msg.UserIdIdentity;
                                    respLog.status = (int) Agp2pEnums.SumapayResponseEnum.Complete;
                                    //更新请求日志
                                    requestLog.complete_time = DateTime.Now;
                                    requestLog.status = (int) Agp2pEnums.SumapayRequestEnum.Complete;
                                    
                                    msg.HasHandle = true;
                                }
                                else
                                {
                                    msg.Remarks = "身份证与姓名不一致";
                                }
                            }
                            else
                            {
                                msg.Remarks = "无法找到平台账户，用户ID：" + msg.UserIdIdentity;
                            }
                        }
                    }
                }
                else
                {
                    msg.Remarks = "无法找到对应的请求日志，日志id：" + msg.RequestId;
                }

                respLog.remarks = msg.Remarks;
                context.SubmitChanges();
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
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
                        var user = context.dt_users.SingleOrDefault(u => u.id == msg.UserIdIdentity);
                        if (user != null)
                        {
                            user.identity_id = msg.UserId;
                            context.SubmitChanges();
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台账户，UserId：" + msg.UserIdIdentity;
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
                        var user = context.dt_users.SingleOrDefault(u => u.id == msg.UserIdIdentity);
                        if (user != null)
                        {
                            user.protocolCode = !msg.Cancel ? msg.ProtocolCode : null;
                            context.SubmitChanges();
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台账户，UserId：" + msg.UserIdIdentity;
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
                        var user = context.dt_users.SingleOrDefault(u => u.id == msg.UserIdIdentity);
                        if (user != null)
                        {
                            user.autoRepay = msg.Cancel ? null : "true";
                            context.SubmitChanges();
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台账户，UserId：" + msg.UserIdIdentity;
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
        /// 单笔转账到个人账户
        /// </summary>
        /// <param name="msg"></param>
        private static void Transfer2User(Transfer2UserRespMsg msg)
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
                        var user = context.dt_users.SingleOrDefault(u => u.id == msg.UserIdIdentity);
                        if (user != null)
                        {
                            //TODO 付款
                            
                            msg.HasHandle = true;
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台账户，UserId：" + msg.UserIdIdentity;
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
