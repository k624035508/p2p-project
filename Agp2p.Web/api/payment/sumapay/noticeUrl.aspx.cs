using System;
using System.Collections.Generic;
using System.IO;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Text;
using System.Web;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Model.DTO;
using Newtonsoft.Json;

namespace Agp2p.Web.api.payment.sumapay
{
    public partial class noticeUrl : System.Web.UI.Page
    {
        protected li_pay_request_log RequestLog;
        protected li_pay_response_log ResponseLog;

        protected void Page_Load(object sender, EventArgs e)
        {
            DoResponse();
            //通知托管平台已收到异步消息
            Response.Write("success");
        }

        /// <summary>
        /// 处理托管接口请求响应
        /// </summary>
        /// <param name="isSync">是否同步返回</param>
        protected void DoResponse(bool isSync = false)
        {
            Agp2pDataContext context = new Agp2pDataContext();

            try
            {
                string reqStr = ReadReqStr();
                //保存响应日志
                ResponseLog = new li_pay_response_log()
                {
                    request_id = string.IsNullOrEmpty(Request["requestId"]) ? null : Request["requestId"],
                    result = string.IsNullOrEmpty(Request["result"]) ? "" : Request["result"],
                    status = (int)Agp2pEnums.SumapayResponseEnum.Return,
                    response_time = DateTime.Now,
                    response_content = reqStr
                };
                if (!string.IsNullOrEmpty(ResponseLog.request_id))
                {
                    //根据响应报文找到对应的request，生成处理消息，对应各种消息处理逻辑
                    RequestLog =
                        context.li_pay_request_log.SingleOrDefault(r => r.id == ResponseLog.request_id);
                    if (RequestLog != null)
                    {
                        //检查请求是否已经处理过
                        if (RequestLog.status == (int)Agp2pEnums.SumapayRequestEnum.Waiting)
                        {
                            try
                            {
                                BaseRespMsg respMsg = null;
                                switch (RequestLog.api)
                                {
                                    //个人开户/激活
                                    case (int)Agp2pEnums.SumapayApiEnum.URegi:
                                    case (int)Agp2pEnums.SumapayApiEnum.URegM:
                                    case (int)Agp2pEnums.SumapayApiEnum.Activ:
                                        respMsg = new UserRegisterRespMsg(reqStr);
                                        break;
                                    //个人自动投标续约
                                    case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                                        respMsg = new AutoBidSignRespMsg(reqStr);
                                        break;
                                    //个人自动投标取消
                                    case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                                        respMsg = new AutoBidSignRespMsg(reqStr);
                                        ((AutoBidSignRespMsg)respMsg).Cancel = true;
                                        break;
                                    //个人自动账户/银行还款开通
                                    case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                                    case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                                        respMsg = new AutoRepaySignRespMsg(reqStr);
                                        break;
                                    //个人自动还款取消
                                    case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                                        respMsg = new AutoRepaySignRespMsg(reqStr);
                                        ((AutoRepaySignRespMsg)respMsg).Cancel = true;
                                        break;
                                    //个人网银/一键充值
                                    case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                                    case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                                    case (int)Agp2pEnums.SumapayApiEnum.WhReM:
                                        respMsg = new RechargeRespMsg(reqStr);
                                        break;
                                    //个人投标/自动投标 普通/集合项目
                                    case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                                    case (int)Agp2pEnums.SumapayApiEnum.McBid:
                                    case (int)Agp2pEnums.SumapayApiEnum.MaBiM:
                                    case (int)Agp2pEnums.SumapayApiEnum.McBiM:
                                    case (int)Agp2pEnums.SumapayApiEnum.AmBid:
                                    case (int)Agp2pEnums.SumapayApiEnum.AcBid:
                                        respMsg = new BidRespMsg(reqStr);
                                        break;
                                    //个人撤标
                                    case (int)Agp2pEnums.SumapayApiEnum.CaPro:
                                    case (int)Agp2pEnums.SumapayApiEnum.CoPro:
                                        respMsg = new WithDrawalRespMsg(reqStr);
                                        break;
                                    //流标普通项目
                                    case (int)Agp2pEnums.SumapayApiEnum.RePro:
                                        respMsg = new RepealProjectRespMsg(reqStr);
                                        break;
                                    //普通/集合项目放款
                                    case (int)Agp2pEnums.SumapayApiEnum.ALoan:
                                    case (int)Agp2pEnums.SumapayApiEnum.CLoan:
                                        respMsg = new MakeLoanRespMsg(reqStr);
                                        break;
                                    //个人提现
                                    case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                                    case (int)Agp2pEnums.SumapayApiEnum.WdraM:
                                        respMsg = new WithdrawRespMsg(reqStr, isSync);
                                        break;
                                    //个人存管账户还款普通/集合项目
                                    case (int)Agp2pEnums.SumapayApiEnum.MaRep:
                                    case (int)Agp2pEnums.SumapayApiEnum.McRep:
                                        respMsg = new RepayRespMsg(reqStr);
                                        break;
                                    //个人协议还款普通/集合项目
                                    case (int)Agp2pEnums.SumapayApiEnum.BaRep:
                                    case (int)Agp2pEnums.SumapayApiEnum.BcRep:
                                        respMsg = new RepayRespMsg(reqStr);
                                        ((RepayRespMsg)respMsg).BankRepay = true;
                                        break;
                                    //个人自动还款普通/集合项目
                                    case (int)Agp2pEnums.SumapayApiEnum.AcRep:
                                    case (int)Agp2pEnums.SumapayApiEnum.AbRep:
                                        respMsg = new RepayRespMsg(reqStr);
                                        ((RepayRespMsg)respMsg).AutoRepay = true;
                                        break;
                                    //普通/集合项目本息到账
                                    case (int)Agp2pEnums.SumapayApiEnum.RetPt:
                                    case (int)Agp2pEnums.SumapayApiEnum.RetCo:
                                        respMsg = new ReturnPrinInteRespMsg(reqStr);
                                        break;
                                    //债权转让
                                    case (int)Agp2pEnums.SumapayApiEnum.CreAs:
                                    case (int)Agp2pEnums.SumapayApiEnum.CreAM:
                                        respMsg = new CreditAssignmentRespMsg(reqStr);
                                        break;
                                    //查询项目
                                    case (int)Agp2pEnums.SumapayApiEnum.QuPro:
                                        respMsg = BaseRespMsg.NewInstance<QueryProjectRespMsg>(reqStr);
                                        break;
                                    default:
                                        respMsg = new BaseRespMsg();
                                        break;
                                }

                                //发送响应消息异步处理
                                MessageBus.Main.PublishAsync(respMsg, s =>
                                {
                                    ResponseLog.user_id = s.UserIdIdentity;
                                    ResponseLog.project_id = s.ProjectCode;
                                    RequestLog.complete_time = DateTime.Now;

                                    if (s.HasHandle)
                                    {
                                        ResponseLog.status = (int)Agp2pEnums.SumapayResponseEnum.Complete;
                                        RequestLog.status = (int)Agp2pEnums.SumapayRequestEnum.Complete;
                                    }
                                    else
                                    {
                                        ResponseLog.status = (int)Agp2pEnums.SumapayResponseEnum.Invalid;
                                        RequestLog.status = (int)Agp2pEnums.SumapayRequestEnum.Fail;
                                    //记录失败信息
                                    ResponseLog.remarks += s.Remarks + ";";
                                    }
                                    //TODO 会出现找不到行或已处理错误
                                    context.li_pay_response_log.InsertOnSubmit(ResponseLog);
                                    context.SubmitChanges();
                                });
                            }
                            catch (Exception ex)
                            {
                                ResponseLog.remarks = "内部错误:" + ex.Message;
                                context.li_pay_response_log.InsertOnSubmit(ResponseLog);
                                context.SubmitChanges();
                            }
                        }
                    }
                    else
                    {
                        ResponseLog.remarks = "没有找到对应的请求流水号，RequestID:" + ResponseLog.request_id;
                        context.li_pay_response_log.InsertOnSubmit(ResponseLog);
                        context.SubmitChanges();
                    }
                }
                else
                {
                    ResponseLog.remarks = "请求流水号为空！";
                    context.li_pay_response_log.InsertOnSubmit(ResponseLog);
                    context.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                ResponseLog.remarks = "内部错误：" + ex.Message;
                context.li_pay_response_log.InsertOnSubmit(ResponseLog);
                context.SubmitChanges();
            }
        }

        /// <summary>
        /// 从request中读取流，组成字符串返回
        /// </summary>
        /// <returns></returns>
        protected string ReadReqStr()
        {
            Stream inputStream = Request.InputStream;
            var destEncode = Encoding.GetEncoding("GBK");
            byte[] filecontent = new byte[inputStream.Length];
            inputStream.Read(filecontent, 0, filecontent.Length);
            string postquery = destEncode.GetString(filecontent);
            return HttpUtility.UrlDecode(postquery, destEncode);
        }
    }
}
