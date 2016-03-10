using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 第三方托管请求/响应消息处理
    /// </summary>
    internal class PayApiHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<FrontEndReqMsg>(DoFrontEndRequest);
            MessageBus.Main.Subscribe<BackEndReqMsg>(DoBackEndRequest);
            MessageBus.Main.Subscribe<StartRespMsg>(DoResponse);
        }

        /// <summary>
        /// 请求前台接口
        /// </summary>
        /// <param name="msg"></param>
        private static void DoFrontEndRequest(FrontEndReqMsg msg)
        {
            try
            {
                Agp2pDataContext context = new Agp2pDataContext();
                var requestLog = new li_pay_request_log
                {
                    id = msg.RequestId,
                    user_id = msg.UserId,
                    project_id = msg.ProjectCode,
                    api = msg.Api,
                    status = (int) Agp2pEnums.SumapayRequestEnum.Waiting,
                    request_time = DateTime.Now,
                    //生成发送报文
                    request_content = Utils.BuildFormHtml(msg.GetSubmitPara(), msg.ApiInterface, "post", "gbk")
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
        /// 请求后台接口
        /// </summary>
        /// <param name="msg"></param>
        private static void DoBackEndRequest(BackEndReqMsg msg)
        {
            try
            {
                Agp2pDataContext context = new Agp2pDataContext();
                var requestLog = new li_pay_request_log
                {
                    id = msg.RequestId,
                    user_id = msg.UserId,
                    project_id = msg.ProjectCode,
                    api = msg.Api,
                    status = (int)Agp2pEnums.SumapayRequestEnum.Waiting,
                    request_time = DateTime.Now,
                    //生成发送报文
                    request_content = msg.GetPostPara()
                };
                //保存日志
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                context.SubmitChanges();
                //执行回调请求接口
                msg.CallBack(requestLog.request_content, msg.ApiInterface);
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
        private static void DoResponse(StartRespMsg msg)
        {
            Agp2pDataContext context = new Agp2pDataContext();
            //根据响应的requestId报文生成处理消息，对应各种消息处理逻辑
            var requestLog =
                context.li_pay_request_log.OrderByDescending(r => r.request_time)
                    .FirstOrDefault(r => r.id == msg.RequestId);
            if (requestLog != null)
            {
                switch (requestLog.api)
                {
                    //用户开户/用户激活
                    case (int)Agp2pEnums.SumapayApiEnum.URegi:
                    case (int)Agp2pEnums.SumapayApiEnum.Activ:
                        //TODO 正式环境改为异步
                        MessageBus.Main.Publish(new UserRegisterRespMsg(msg.RequestId, msg.Result, msg.ResponseContent));
                        break;

                }
            }
        }
    }
}
