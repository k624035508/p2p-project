﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 第三方托管请求消息处理
    /// </summary>
    internal class RequestApiHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<FrontEndReqMsg>(DoFrontEndRequest);
            MessageBus.Main.Subscribe<BackEndReqMsg>(DoBackEndRequest);
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
                    request_content = BuildFormHtml(msg.GetSubmitPara(), msg.ApiInterface)
                };
                //保存日志
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                context.SubmitChanges();
                msg.RequestContent = requestLog.request_content;
            }
            catch (Exception ex)
            {
                //TODO 返回错误信息
                throw ex;
            }
        }

        private static string BuildFormHtml(SortedDictionary<string, string> sParaTemp, string gateway)
        {
            StringBuilder sbHtml = new StringBuilder();
            sbHtml.Append("<form id='submit' name='submit' action='" + gateway + "' method='post'>");

            foreach (KeyValuePair<string, string> temp in sParaTemp)
            {
                sbHtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
            }

            sbHtml.Append("<input type='submit' value='ok' style='display:none;'></form>");
            sbHtml.Append("<script>document.forms['submit'].submit();</script>");

            return sbHtml.ToString();
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
                msg.SynResult = Utils.HttpPostGbk(msg.ApiInterface, requestLog.request_content);
            }
            catch (Exception ex)
            {
                //TODO 返回错误信息
                throw ex;
            }
        }
    }
}
