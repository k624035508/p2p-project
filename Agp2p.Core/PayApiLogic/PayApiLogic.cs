using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 第三方托管请求消息处理
    /// </summary>
    internal class PayApiLogic
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<UserRegisterReqMsg>(DoRequest);
            MessageBus.Main.Subscribe<UserRegisterRespMsg>(DoResponse);
        }

        /// <summary>
        /// 请求接口
        /// </summary>
        /// <param name="msg"></param>
        private static void DoRequest(BaseReqMsg msg)
        {
            try
            {
                Agp2pDataContext context = new Agp2pDataContext();
                var requestLog = new li_pay_request_log
                {
                    id = msg.RequestId,
                    user_id = msg.UserIdIdentity,
                    project_id = msg.ProjectCode,
                    api = msg.Api,
                    status = (int) Agp2pEnums.SumapayRequestEnum.Waiting,
                    request_time = DateTime.Now,
                    //生成发送报文
                    request_content = Utils.BuildFormHtml(msg.GetSubmitPara(), msg.ApiUrl, "post", "gbk")
                };
                //保存日志
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                context.SubmitChanges();
                //执行回调请求接口
                msg.CallBack(requestLog.request_content);
            }
            catch (Exception ex)
            {
                //TODO 返回错误信息
                throw ex;
            }
        }

        /// <summary>
        /// 接口响应
        /// </summary>
        /// <param name="msg"></param>
        private static void DoResponse(BaseRespMsg msg)
        {
            Agp2pDataContext context = new Agp2pDataContext();
            //保存响应日志
            var responseLog = new li_pay_response_log()
            {
                request_id = msg.RequestId,
                result = msg.Result,
                user_id = msg.UserId,
                project_id = msg.ProjectCode,
                status = (int)Agp2pEnums.SumapayResponseEnum.Return,
                response_time = DateTime.Now,
                response_content = msg.ResponseContent
            };
            context.li_pay_response_log.InsertOnSubmit(responseLog);

            //检查签名
            if (msg.CheckSignature())
            {
                if (msg.CheckResult())
                {
                    //找出对应的请求日志
                    var requestLog = context.li_pay_request_log.SingleOrDefault(l => l.id == msg.RequestId);
                    if (requestLog != null)
                    {
                        requestLog.status = (int) Agp2pEnums.SumapayRequestEnum.Complete;
                    }
                }
            }
        }
    }
}
