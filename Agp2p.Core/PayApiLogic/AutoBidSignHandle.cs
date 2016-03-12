﻿using System;
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
                Agp2pDataContext context = new Agp2pDataContext();
                var respLog =
                    context.li_pay_response_log.OrderBy(r => r.response_time)
                        .FirstOrDefault(r => r.request_id == msg.RequestId);
                if (respLog != null)
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
                        //检查请求处理结果
                        if (msg.CheckResult())
                        {
                            //查找对应的平台账户，更新用户信息
                            var user = context.dt_users.SingleOrDefault(u => u.id == msg.UserId);
                            if (user != null)
                            {
                                user.protocolCode = !msg.Cancel? msg.ProtocolCode : null;
                                //更新响应日志
                                respLog.result = msg.Result;
                                respLog.user_id = msg.UserId;
                                respLog.status = (int)Agp2pEnums.SumapayResponseEnum.Complete;
                                //更新请求日志
                                respLog.li_pay_request_log.complete_time = DateTime.Now;
                                respLog.li_pay_request_log.status = (int)Agp2pEnums.SumapayRequestEnum.Complete;
                                context.SubmitChanges();
                            }
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
