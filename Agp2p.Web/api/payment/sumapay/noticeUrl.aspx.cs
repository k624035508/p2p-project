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
                //根据响应报文找到对应的request，生成处理消息，对应各种消息处理逻辑
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
                            respMsg = BaseRespMsg.NewInstance<UserRegisterRespMsg>(reqStr);
                            break;
                        //个人自动投标续约
                        case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                            respMsg = BaseRespMsg.NewInstance<AutoBidSignRespMsg>(reqStr);
                            break;
                        //个人自动投标取消
                        case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                            respMsg = BaseRespMsg.NewInstance<AutoBidSignRespMsg>(reqStr);
                            ((AutoBidSignRespMsg) respMsg).Cancel = true;
                            break;
                        //个人自动账户/银行还款开通
                        case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                        case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                            respMsg = BaseRespMsg.NewInstance<AutoRepaySignRespMsg>(reqStr);
                            break;
                        //个人自动还款取消
                        case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                            respMsg = BaseRespMsg.NewInstance<AutoRepaySignRespMsg>(reqStr);
                            ((AutoRepaySignRespMsg)respMsg).Cancel = true;
                            break;
                        //个人网银/一键充值
                        case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                        case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                            respMsg = BaseRespMsg.NewInstance<RechargeRespMsg>(reqStr);
                            break;
                        //个人投标/自动投标 普通/集合项目
                        case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                        case (int)Agp2pEnums.SumapayApiEnum.McBid:
                        case (int)Agp2pEnums.SumapayApiEnum.AmBid:
                        case (int)Agp2pEnums.SumapayApiEnum.AcBid:
                            respMsg = BaseRespMsg.NewInstance<BidRespMsg>(reqStr);
                            break;
                        //个人撤标 普通/集合项目
                        case (int)Agp2pEnums.SumapayApiEnum.CaPro:
                        case (int)Agp2pEnums.SumapayApiEnum.CoPro:
                            respMsg = BaseRespMsg.NewInstance<WithDrawalRespMsg>(reqStr);
                            break;
                        //个人流标普通项目
                        case (int)Agp2pEnums.SumapayApiEnum.RePro:
                            respMsg = BaseRespMsg.NewInstance<RepealProjectRespMsg>(reqStr);
                            break;
                        //普通/集合项目放款
                        case (int)Agp2pEnums.SumapayApiEnum.ALoan:
                        case (int)Agp2pEnums.SumapayApiEnum.CLoan:
                            respMsg = BaseRespMsg.NewInstance<MakeLoanRespMsg>(reqStr);
                            break;
                        //个人提现
                        case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                            respMsg = BaseRespMsg.NewInstance<WithdrawRespMsg>(reqStr);
                            break;
                        //个人存管账户还款普通/集合项目
                        case (int)Agp2pEnums.SumapayApiEnum.MaRep:
                        case (int)Agp2pEnums.SumapayApiEnum.McRep:
                            respMsg = BaseRespMsg.NewInstance<RepayRespMsg>(reqStr);
                            break;
                        //个人协议还款普通/集合项目
                        case (int)Agp2pEnums.SumapayApiEnum.BaRep:
                        case (int)Agp2pEnums.SumapayApiEnum.BcRep:
                            respMsg = BaseRespMsg.NewInstance<RepayRespMsg>(reqStr);
                            ((RepayRespMsg)respMsg).BankRepay = true;
                            break;
                        //个人自动还款普通/集合项目
                        case (int)Agp2pEnums.SumapayApiEnum.AcRep:
                        case (int)Agp2pEnums.SumapayApiEnum.AbRep:
                            respMsg = BaseRespMsg.NewInstance<RepayRespMsg>(reqStr);
                            ((RepayRespMsg)respMsg).AutoRepay = true;
                            break;
                        //普通/集合项目本息到账
                        case (int)Agp2pEnums.SumapayApiEnum.RetPt:
                        case (int)Agp2pEnums.SumapayApiEnum.RetCo:
                            respMsg = BaseRespMsg.NewInstance<ReturnPrinInteRespMsg>(reqStr);
                            break;
                        //债权转让
                        case (int)Agp2pEnums.SumapayApiEnum.CreAs:
                            respMsg = BaseRespMsg.NewInstance<CreditAssignmentRespMsg>(reqStr);
                            break;
                        //付款到个人
                        case (int)Agp2pEnums.SumapayApiEnum.TranU:
                            respMsg = BaseRespMsg.NewInstance<Transfer2UserRespMsg>(reqStr);
                            break;
                        default:
                            respMsg = new BaseRespMsg();
                            break;
                    }
                    //发送响应消息异步处理
                    MessageBus.Main.PublishAsync(respMsg, s =>
                    {
                        //响应处理完后更新日志信息
                        if (respMsg.HasHandle)
                        {
                            //更新响应日志
                            responseLog.user_id = respMsg.UserIdIdentity;
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
                //通知托管平台已收到异步消息
                Response.Write("success");
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
