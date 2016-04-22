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
    public class RequestApiHandle
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
                    status = (int)Agp2pEnums.SumapayRequestEnum.Waiting,
                    request_time = DateTime.Now,
                    remarks = msg.Remarks
                };
                //创建交易流水
                switch (requestLog.api)
                {
                    case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                        //网银充值
                        context.Charge((int)requestLog.user_id, Utils.StrToDecimal(((WebRechargeReqMsg)msg).Sum, 0), Agp2pEnums.PayApiTypeEnum.Sumapay, msg.RequestId);
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                        //快捷充值
                        context.Charge((int)requestLog.user_id, Utils.StrToDecimal(((WhRechargeReqMsg)msg).Sum, 0), Agp2pEnums.PayApiTypeEnum.SumapayQ, msg.RequestId);
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                        //提现
                        var withdrawReqMsg = (WithdrawReqMsg)msg;
                        context.Withdraw(Utils.StrToInt(withdrawReqMsg.BankId, 0),
                            Utils.StrToDecimal(withdrawReqMsg.Sum, 0), withdrawReqMsg.RequestId);
                        break;
                        //债权转让
                    case (int)Agp2pEnums.SumapayApiEnum.CreAs:
                        var creditAssignmentReqMsg = (CreditAssignmentReqMsg)msg;
                        //通过债权找出对应的投资信息
                        var claim = context.li_claims.SingleOrDefault(c => c.id == creditAssignmentReqMsg.ClaimId);
                        creditAssignmentReqMsg.AssignmentSum = (claim.principal + claim.legacyInterest.GetValueOrDefault(0)).ToString();
                        creditAssignmentReqMsg.ProjectCode = claim.projectId;
                        creditAssignmentReqMsg.ProjectDescription = claim.li_projects.title;
                        requestLog.project_id = claim.projectId;
                        //计算手续费
                        var staticWithdrawCostPercent = ConfigLoader.loadCostConfig().static_withdraw / 100;
                        var finalCost = Math.Round(Utils.StrToDecimal(creditAssignmentReqMsg.UndertakeSum, 0) * staticWithdrawCostPercent, 2);
                        //父债权才有原投资流水号
                        var rooClaim = claim.GetRootClaim();
                        creditAssignmentReqMsg.OriginalOrderSum = rooClaim.li_project_transactions_invest.principal.ToString("f");
                        creditAssignmentReqMsg.OriginalRequestId = rooClaim.li_project_transactions_invest.no_order;
                        creditAssignmentReqMsg.SetSubledgerList(finalCost, claim.userId.ToString());
                        break;

                        //case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                        //    //投资
                        //    var manualBidReqMsg = (ManualBidReqMsg) msg;
                        //    TransactionFacade.Invest((int)requestLog.user_id, Utils.StrToInt(manualBidReqMsg.ProjectCode, 0), Utils.StrToDecimal(manualBidReqMsg.Sum, 0), manualBidReqMsg.RequestId);
                        //    break;
                }
                //生成发送报文
                msg.RequestContent = BuildFormHtml(msg.GetSubmitPara(), msg.ApiInterface);
                requestLog.request_content = msg.RequestContent;
                //保存日志
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                context.SubmitChanges();
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
                    remarks = msg.Remarks,
                };
                //保存日志
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                //创建交易记录
                switch (requestLog.api)
                {
                    //放款请求
                    case (int)Agp2pEnums.SumapayApiEnum.ALoan:
                    case (int)Agp2pEnums.SumapayApiEnum.CLoan:
                        var makeLoanReqMsg = (MakeLoanReqMsg)msg;
                        var project = context.li_projects.SingleOrDefault(p => p.id == makeLoanReqMsg.ProjectCode);
                        //非活期和新手标项目，以及没有生成过服务费 TODO 只计算？
                        if (project != null && !project.IsHuoqiProject() && !project.IsNewbieProject() 
                            && !context.li_company_inoutcome.Any(c => c.project_id == project.id 
                            && (c.type == (int)Agp2pEnums.OfflineTransactionTypeEnum.SumManagementFeeOfLoanning
                            || (c.type == (int)Agp2pEnums.OfflineTransactionTypeEnum.SumBondFee))))
                        {
                            decimal loanFee = 0;
                            decimal bondFee = 0;

                            //计算平台服务费
                            if (project.loan_fee_rate != null && project.loan_fee_rate > 0)
                            {
                                loanFee = (decimal)(project.financing_amount * (project.loan_fee_rate / 100));
                                context.li_company_inoutcome.InsertOnSubmit(new li_company_inoutcome
                                {
                                    user_id = (int)msg.UserId,
                                    income = loanFee,
                                    project_id = project.id,
                                    type = (int)Agp2pEnums.OfflineTransactionTypeEnum.SumManagementFeeOfLoanning,
                                    create_time = DateTime.Now,
                                    remark = $"借款项目'{project.title}'收取平台服务费"
                                });
                            }

                            //计算风险保证金
                            if (project.bond_fee_rate != null && project.bond_fee_rate > 0)
                            {
                                bondFee = project.financing_amount * (project.bond_fee_rate / 100) ?? 0;
                                context.li_company_inoutcome.InsertOnSubmit(new li_company_inoutcome
                                {
                                    user_id = (int)msg.UserId,
                                    income = bondFee,
                                    project_id = project.id,
                                    type = (int)Agp2pEnums.OfflineTransactionTypeEnum.SumBondFee,
                                    create_time = DateTime.Now,
                                    remark = $"借款项目'{project.title}'收取风险保证金"
                                });
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

        /// <summary>
        /// 发送本息到账请求
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="sum"></param>
        /// <param name="repayTaskId"></param>
        /// <param name="isEarlyPay"></param>
        public static void SendReturnPrinInte(int projectCode, string sum, int repayTaskId, bool isEarlyPay)
        {
            var context = new Agp2pDataContext();
            //计算投资者本息明细
            var repayRask = context.li_repayment_tasks.SingleOrDefault(r => r.id == repayTaskId);
            var transList = TransactionFacade.GenerateRepayTransactions(repayRask, DateTime.Now);
            //创建本息到账请求并设置分账列表
            var returnPrinInteReqMsg = new ReturnPrinInteReqMsg(projectCode, sum);
            returnPrinInteReqMsg.SetSubledgerList(transList);
            //发送请求
            MessageBus.Main.PublishAsync(returnPrinInteReqMsg, ar =>
            {
                //处理请求同步返回结果
                var returnPrinInteRespMsg = BaseRespMsg.NewInstance<ReturnPrinInteRespMsg>(returnPrinInteReqMsg.SynResult);
                returnPrinInteRespMsg.Sync = true;
                returnPrinInteRespMsg.RepayTaskId = repayTaskId;
                returnPrinInteRespMsg.IsEarlyPay = isEarlyPay;
                MessageBus.Main.PublishAsync(returnPrinInteRespMsg);
            });
        }
    }
}
