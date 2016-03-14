using System;
using System.Collections.Generic;
using System.IO;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Text;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Newtonsoft.Json;

namespace Agp2p.Web.api.payment.sumapay
{
    public partial class noticeUrl : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string reqStr = ReadReqStr();
            var sd = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(reqStr);
            if (!string.IsNullOrEmpty(sd["requestId"]))
            {
                Agp2pDataContext context = new Agp2pDataContext();
                //保存响应日志
                var responseLog = new li_pay_response_log()
                {
                    request_id = sd["requestId"],
                    result = sd["result"],
                    status = (int)Agp2pEnums.SumapayResponseEnum.Return,
                    response_time = DateTime.Now,
                    response_content = reqStr
                };
                context.li_pay_response_log.InsertOnSubmit(responseLog);
                //根据响应的requestId报文生成处理消息，对应各种消息处理逻辑
                var requestLog =
                    context.li_pay_request_log.SingleOrDefault(r => r.id == responseLog.request_id);
                if (requestLog != null)
                {
                    BaseRespMsg respMsg = null;
                    switch (requestLog.api)
                    {
                        //个人开户/激活
                        case (int)Agp2pEnums.SumapayApiEnum.URegi:
                        case (int)Agp2pEnums.SumapayApiEnum.Activ:
                            respMsg = new UserRegisterRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content);
                            break;
                        //个人自动投标续约
                        case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                            respMsg = new AutoBidSignRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content);
                            break;
                        //个人自动投标取消
                        case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                            respMsg = new AutoBidSignRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content, true);
                            break;
                        //个人自动账户/银行还款开通
                        case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                        case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                            respMsg = new AutoRepaySignRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content);
                            break;
                        //个人自动还款取消
                        case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                            respMsg = new AutoRepaySignRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content, true);
                            break;
                        //个人网银充值
                        case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                            respMsg = new RechargeRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content);
                            break;
                        //个人一键充值
                        case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                            respMsg = new RechargeRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content, true);
                            break;
                        //个人投标普通/集合项目
                        case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                        case (int)Agp2pEnums.SumapayApiEnum.CoBid:
                            respMsg = new ManualBidRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content);
                            break;
                        default:
                            respMsg = new BaseRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content);
                            break;
                    }
                    //发送响应消息异步处理
                    MessageBus.Main.PublishAsync(respMsg, s =>
                    {
                        //响应处理完后更新日志信息
                        if (respMsg.HasHandle)
                        {
                            //更新响应日志
                            responseLog.user_id = respMsg.UserId;
                            responseLog.project_id = respMsg.ProjectCode;
                            responseLog.status = (int) Agp2pEnums.SumapayResponseEnum.Complete;
                            //更新请求日志
                            requestLog.complete_time = DateTime.Now;
                            requestLog.status = (int) Agp2pEnums.SumapayRequestEnum.Complete;
                        }
                        else
                        {
                            //记录失败信息
                            responseLog.remarks += respMsg.Remarks + ";";
                        }
                        context.SubmitChanges();
                    });
                }
            }
            else
            {
                //TODO 记录错误信息
            }
        }

        /// <summary>
        /// 从request中读取流，组成字符串返回
        /// </summary>
        /// <returns></returns>
        public string ReadReqStr()
        {
            StringBuilder sb = new StringBuilder();
            //Stream inputStream = Request.GetBufferlessInputStream();
            Stream inputStream = Request.InputStream;
            StreamReader reader = new StreamReader(inputStream, System.Text.Encoding.UTF8);

            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                sb.Append(line);
            }
            reader.Close();
            return sb.ToString();

        }
    }
}
