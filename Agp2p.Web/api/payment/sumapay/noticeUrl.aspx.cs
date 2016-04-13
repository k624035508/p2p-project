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
            string reqStr = ReadReqStr();
            Agp2pDataContext context = new Agp2pDataContext();
            //保存响应日志
            ResponseLog = new li_pay_response_log()
            {
                request_id = Request["requestId"],
                result = Request["result"],
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
                    if (RequestLog.status == (int) Agp2pEnums.SumapayRequestEnum.Waiting)
                    {
                        BaseRespMsg respMsg = null;
                        switch (RequestLog.api)
                        {
                            //个人开户/激活
                            case (int) Agp2pEnums.SumapayApiEnum.URegi:
                            case (int) Agp2pEnums.SumapayApiEnum.Activ:
                                respMsg = isSync ? new UserRegisterRespMsg(reqStr) : BaseRespMsg.NewInstance<UserRegisterRespMsg>(reqStr);
                                break;
                            //个人自动投标续约
                            case (int) Agp2pEnums.SumapayApiEnum.AtBid:
                                respMsg = isSync ? new AutoBidSignRespMsg(reqStr) : BaseRespMsg.NewInstance<AutoBidSignRespMsg>(reqStr);
                                break;
                            //个人自动投标取消
                            case (int) Agp2pEnums.SumapayApiEnum.ClBid:
                                respMsg = isSync ? new AutoBidSignRespMsg(reqStr) : BaseRespMsg.NewInstance<AutoBidSignRespMsg>(reqStr);
                                ((AutoBidSignRespMsg) respMsg).Cancel = true;
                                break;
                            //个人自动账户/银行还款开通
                            case (int) Agp2pEnums.SumapayApiEnum.AcReO:
                            case (int) Agp2pEnums.SumapayApiEnum.AbReO:
                                respMsg = isSync ? new AutoRepaySignRespMsg(reqStr) : BaseRespMsg.NewInstance<AutoRepaySignRespMsg>(reqStr) ;
                                break;
                            //个人自动还款取消
                            case (int) Agp2pEnums.SumapayApiEnum.ClRep:
                                respMsg = isSync ? new AutoRepaySignRespMsg(reqStr) : BaseRespMsg.NewInstance<AutoRepaySignRespMsg>(reqStr);
                                ((AutoRepaySignRespMsg) respMsg).Cancel = true;
                                break;
                            //个人网银/一键充值
                            case (int) Agp2pEnums.SumapayApiEnum.WeRec:
                            case (int) Agp2pEnums.SumapayApiEnum.WhRec:
                                respMsg = isSync ? new RechargeRespMsg(reqStr) : BaseRespMsg.NewInstance<RechargeRespMsg>(reqStr);
                                break;
                            //个人投标/自动投标 普通/集合项目
                            case (int) Agp2pEnums.SumapayApiEnum.MaBid:
                            case (int) Agp2pEnums.SumapayApiEnum.McBid:
                            case (int) Agp2pEnums.SumapayApiEnum.AmBid:
                            case (int) Agp2pEnums.SumapayApiEnum.AcBid:
                                respMsg = isSync ? new BidRespMsg(reqStr) : BaseRespMsg.NewInstance<BidRespMsg>(reqStr);
                                break;
                            //个人撤标
                            case (int) Agp2pEnums.SumapayApiEnum.CaPro:
                            case (int) Agp2pEnums.SumapayApiEnum.CoPro:
                                respMsg = BaseRespMsg.NewInstance<WithDrawalRespMsg>(reqStr);
                                break;
                            //流标普通项目
                            case (int) Agp2pEnums.SumapayApiEnum.RePro:
                                respMsg = BaseRespMsg.NewInstance<RepealProjectRespMsg>(reqStr);
                                break;
                            //普通/集合项目放款
                            case (int) Agp2pEnums.SumapayApiEnum.ALoan:
                            case (int) Agp2pEnums.SumapayApiEnum.CLoan:
                                respMsg = BaseRespMsg.NewInstance<MakeLoanRespMsg>(reqStr);
                                break;
                            //个人提现
                            case (int) Agp2pEnums.SumapayApiEnum.Wdraw:
                                respMsg = isSync ? new WithdrawRespMsg(reqStr, true) : BaseRespMsg.NewInstance<WithdrawRespMsg>(reqStr);
                                break;
                            //个人存管账户还款普通/集合项目
                            case (int) Agp2pEnums.SumapayApiEnum.MaRep:
                            case (int) Agp2pEnums.SumapayApiEnum.McRep:
                                respMsg = isSync ? new RepayRespMsg(reqStr) : BaseRespMsg.NewInstance<RepayRespMsg>(reqStr);
                                break;
                            //个人协议还款普通/集合项目
                            case (int) Agp2pEnums.SumapayApiEnum.BaRep:
                            case (int) Agp2pEnums.SumapayApiEnum.BcRep:
                                respMsg = isSync ? new RepayRespMsg(reqStr) : BaseRespMsg.NewInstance<RepayRespMsg>(reqStr);
                                ((RepayRespMsg) respMsg).BankRepay = true;
                                break;
                            //个人自动还款普通/集合项目
                            case (int) Agp2pEnums.SumapayApiEnum.AcRep:
                            case (int) Agp2pEnums.SumapayApiEnum.AbRep:
                                respMsg = isSync ? new RepayRespMsg(reqStr) : BaseRespMsg.NewInstance<RepayRespMsg>(reqStr);
                                ((RepayRespMsg) respMsg).AutoRepay = true;
                                break;
                            //普通/集合项目本息到账
                            case (int) Agp2pEnums.SumapayApiEnum.RetPt:
                            case (int) Agp2pEnums.SumapayApiEnum.RetCo:
                                respMsg = BaseRespMsg.NewInstance<ReturnPrinInteRespMsg>(reqStr);
                                break;
                            //债权转让
                            case (int) Agp2pEnums.SumapayApiEnum.CreAs:
                                respMsg = isSync ? new CreditAssignmentRespMsg(reqStr) : BaseRespMsg.NewInstance<CreditAssignmentRespMsg>(reqStr);
                                break;
                            //付款到个人
                            case (int) Agp2pEnums.SumapayApiEnum.TranU:
                                respMsg = BaseRespMsg.NewInstance<Transfer2UserRespMsg>(reqStr);
                                break;
                            default:
                                respMsg = new BaseRespMsg();
                                break;
                        }

                        //if (isSync)
                        //{
                        //    //发送响应消息同步处理
                        //    MessageBus.Main.Publish(respMsg);
                        //    UpdateLog(respMsg);
                        //    context.li_pay_response_log.InsertOnSubmit(ResponseLog);
                        //    context.SubmitChanges();
                        //}
                        //else
                        //{
                            //发送响应消息异步处理
                            MessageBus.Main.PublishAsync(respMsg, s =>
                            {
                                ResponseLog.user_id = respMsg.UserIdIdentity;
                                ResponseLog.project_id = respMsg.ProjectCode;
                                RequestLog.complete_time = DateTime.Now;

                                if (respMsg.HasHandle)
                                {
                                    ResponseLog.status = (int)Agp2pEnums.SumapayResponseEnum.Complete;
                                    RequestLog.status = (int)Agp2pEnums.SumapayRequestEnum.Complete;
                                }
                                else
                                {
                                    ResponseLog.status = (int)Agp2pEnums.SumapayResponseEnum.Invalid;
                                    RequestLog.status = (int)Agp2pEnums.SumapayRequestEnum.Fail;
                                    //记录失败信息
                                    ResponseLog.remarks += respMsg.Remarks + ";";
                                }
                                context.li_pay_response_log.InsertOnSubmit(ResponseLog);
                                context.SubmitChanges();
                            });
                        //}
                    }
                }
                else
                {
                    ResponseLog.remarks = "没有找到对应的请求流水号或请求已处理，RequestID:" + ResponseLog.request_id;
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
