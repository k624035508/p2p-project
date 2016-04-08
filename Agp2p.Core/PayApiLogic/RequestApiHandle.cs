using System;
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
                    remarks = msg.Remarks,
                    //生成发送报文
                    request_content = BuildFormHtml(msg.GetSubmitPara(), msg.ApiInterface)
                };
                //保存日志
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                //创建交易流水
                switch (requestLog.api)
                {
                    case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                        //网银充值
                        context.Charge((int)requestLog.user_id, Utils.StrToDecimal(((WebRechargeReqMsg)msg).Sum, 0), (byte)Agp2pEnums.PayApiTypeEnum.Sumapay, msg.RequestId);
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                        //快捷充值
                        context.Charge((int)requestLog.user_id, Utils.StrToDecimal(((WhRechargeReqMsg)msg).Sum, 0), (byte)Agp2pEnums.PayApiTypeEnum.SumapayQ, msg.RequestId);
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                        //提现
                        var withdrawReqMsg = (WithdrawReqMsg)msg;
                        context.Withdraw(Utils.StrToInt(withdrawReqMsg.BankId, 0),
                            Utils.StrToDecimal(withdrawReqMsg.Sum, 0), withdrawReqMsg.RequestId);
                        break;
                        //case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                        //    //投资
                        //    var manualBidReqMsg = (ManualBidReqMsg) msg;
                        //    TransactionFacade.Invest((int)requestLog.user_id, Utils.StrToInt(manualBidReqMsg.ProjectCode, 0), Utils.StrToDecimal(manualBidReqMsg.Sum, 0), manualBidReqMsg.RequestId);
                        //    break;
                }
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
                    request_time = DateTime.Now
                };
                //保存日志
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                //创建交易记录
                switch (requestLog.api)
                {
                    //放款请求
                    case (int) Agp2pEnums.SumapayApiEnum.ALoan:
                    case (int) Agp2pEnums.SumapayApiEnum.CLoan:
                        var makeLoanReqMsg = (MakeLoanReqMsg) msg;
                        var project = context.li_projects.SingleOrDefault(p => p.id == Utils.StrToInt(makeLoanReqMsg.ProjectCode, 0));
                        if (project != null)
                        {
                            decimal loanFee = 0;
                            decimal bondFee = 0;
                            if (!project.IsHuoqiProject() && !project.IsNewbieProject())
                            {
                                //计算平台服务费
                                if (project.loan_fee_rate != null && project.loan_fee_rate > 0)
                                {
                                    loanFee = (decimal) (project.financing_amount*(project.loan_fee_rate/100));
                                    context.li_company_inoutcome.InsertOnSubmit(new li_company_inoutcome
                                    {
                                        user_id = (int)msg.UserId,
                                        income = loanFee,
                                        project_id = project.id,
                                        type = (int) Agp2pEnums.OfflineTransactionTypeEnum.SumManagementFeeOfLoanning,
                                        create_time = DateTime.Now,
                                        remark = $"借款项目'{project.title}'收取平台服务费"
                                    });
                                }

                                //计算风险保证金
                                if (project.bond_fee_rate != null && project.bond_fee_rate > 0)
                                {
                                    bondFee = project.financing_amount*(project.bond_fee_rate/100) ?? 0;
                                    context.li_company_inoutcome.InsertOnSubmit(new li_company_inoutcome
                                    {
                                        user_id = (int)msg.UserId,
                                        income = bondFee,
                                        project_id = project.id,
                                        type = (int) Agp2pEnums.OfflineTransactionTypeEnum.SumBondFee,
                                        create_time = DateTime.Now,
                                        remark = $"借款项目'{project.title}'收取风险保证金"
                                    });
                                }
                            }
                            makeLoanReqMsg.SetSubledgerList(loanFee, bondFee);
                        }
                        break;
                }
                //生成发送报文
                requestLog.request_content = msg.GetPostPara();
                msg.SynResult = Utils.HttpPostGbk(msg.ApiInterface, requestLog.request_content);
                context.SubmitChanges();
            }
            catch (Exception ex)
            {
                //TODO 返回错误信息
                throw ex;
            }
        }
    }
}
