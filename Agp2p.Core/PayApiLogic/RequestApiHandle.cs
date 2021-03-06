﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

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
                request_content = ""
            };
            try
            {
                //创建交易流水
                switch (requestLog.api)
                {
                    case (int) Agp2pEnums.SumapayApiEnum.WeRec:
                    case (int) Agp2pEnums.SumapayApiEnum.CeRec:
                        //网银充值
                        context.Charge((int) requestLog.user_id, Utils.StrToDecimal(((WebRechargeReqMsg) msg).Sum, 0),
                            Agp2pEnums.PayApiTypeEnum.Sumapay, "丰付网银支付", msg.RequestId);
                        break;
                    case (int) Agp2pEnums.SumapayApiEnum.WhRec:
                    case (int) Agp2pEnums.SumapayApiEnum.WhReM:
                        //快捷充值
                        context.Charge((int) requestLog.user_id, Utils.StrToDecimal(((WhRechargeReqMsg) msg).Sum, 0),
                            Agp2pEnums.PayApiTypeEnum.SumapayQ, "丰付一键支付", msg.RequestId);
                        break;
                    case (int) Agp2pEnums.SumapayApiEnum.Wdraw:
                    case (int) Agp2pEnums.SumapayApiEnum.Cdraw:
                    case (int) Agp2pEnums.SumapayApiEnum.WdraM:
                        //提现
                        var withdrawReqMsg = (WithdrawReqMsg) msg;
                        requestLog.remarks = withdrawReqMsg.BankId;
                        //context.Withdraw(Utils.StrToInt(withdrawReqMsg.BankId, 0),
                        //    Utils.StrToDecimal(withdrawReqMsg.Sum, 0), withdrawReqMsg.RequestId);
                        break;
                    //债权转让
                    case (int) Agp2pEnums.SumapayApiEnum.CreAs:
                    case (int) Agp2pEnums.SumapayApiEnum.CreAM:
                        var creditAssignmentReqMsg = (CreditAssignmentReqMsg) msg;
                        //通过债权找出对应的投资信息
                        var claim = context.li_claims.SingleOrDefault(c => c.id == creditAssignmentReqMsg.ClaimId);
                        creditAssignmentReqMsg.AssignmentSum =
                            (claim.principal + claim.keepInterest.GetValueOrDefault(0)).ToString();
                        creditAssignmentReqMsg.ProjectCode = claim.projectId;
                        creditAssignmentReqMsg.ProjectDescription = claim.li_projects.title;
                        requestLog.project_id = claim.projectId;
                        //计算手续费
                        var staticWithdrawCostPercent = ConfigLoader.loadCostConfig().static_withdraw;
                        var finalCost =
                            Math.Round(
                                Utils.StrToDecimal(creditAssignmentReqMsg.UndertakeSum, 0)*staticWithdrawCostPercent, 2);
                        //父债权才有原投资流水号
                        var rooClaim = claim.GetRootClaim();
                        creditAssignmentReqMsg.OriginalOrderSum =
                            rooClaim.li_project_transactions_invest.principal.ToString("f");
                        creditAssignmentReqMsg.OriginalRequestId = rooClaim.li_project_transactions_invest.no_order;
                        creditAssignmentReqMsg.SetSubledgerList(finalCost, claim.userId.ToString());
                        break;

                    case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                    case (int)Agp2pEnums.SumapayApiEnum.MaBiM:
                        //投资
                        var manualBidReqMsg = (ManualBidReqMsg) msg;
                        //TransactionFacade.Invest((int)requestLog.user_id, Utils.StrToInt(manualBidReqMsg.ProjectCode, 0), Utils.StrToDecimal(manualBidReqMsg.Sum, 0), manualBidReqMsg.RequestId);
                        var jiaxiquan = new li_jiaxiquan_transaction
                        {
                            userId = (int)msg.UserId,
                            type = (int)Agp2pEnums.InvestInterestRateTypeEnum.Invest,
                            requestId = msg.RequestId,
                            remarks = msg.Remarks
                        };
                        context.li_jiaxiquan_transaction.InsertOnSubmit(jiaxiquan);

                        context.InvestConfirm((int) requestLog.user_id, manualBidReqMsg.ProjectCode,
                            Utils.StrToDecimal(manualBidReqMsg.Sum, 0), manualBidReqMsg.RequestId);

                        context.SubmitChanges();

                        break;
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
                requestLog.remarks = ex.Message;
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                context.SubmitChanges();
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
            };
            try
            {
                //创建交易记录
                switch (requestLog.api)
                {
                    //放款请求
                    case (int) Agp2pEnums.SumapayApiEnum.ALoan:
                    case (int) Agp2pEnums.SumapayApiEnum.CLoan:
                        var makeLoanReqMsg = (MakeLoanReqMsg) msg;
                        var project = context.li_projects.SingleOrDefault(p => p.id == makeLoanReqMsg.ProjectCode);
                        //非活期和新手标项目计算平台服务费
                        if (project != null && !project.IsNewbieProject1())
                        {
                            decimal loanFee = decimal.Round(project.investment_amount * (project.loan_fee_rate) ?? 0, 2, MidpointRounding.AwayFromZero);
                            decimal bondFee = decimal.Round(project.investment_amount * (project.bond_fee_rate) ?? 0, 2, MidpointRounding.AwayFromZero);
                            makeLoanReqMsg.SetSubledgerList(loanFee + bondFee);
                        }
                        else
                            throw new ArgumentNullException("没有找到项目, ID=" + makeLoanReqMsg.ProjectCode);
                        break;
                }
                //生成发送报文
                msg.RequestContent = msg.GetPostPara();
                requestLog.request_content = msg.RequestContent;
                //保存日志
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                context.SubmitChanges();
                //发送请求
                msg.SynResult = Utils.HttpPostGbk(msg.ApiInterface, requestLog.request_content);
            }
            catch (Exception ex)
            {
                requestLog.remarks = ex.Message;
                context.li_pay_request_log.InsertOnSubmit(requestLog);
                context.SubmitChanges();
            }
        }

        /// <summary>
        /// 发送本息到账请求
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="sum"></param>
        /// <param name="repayTaskId">定期为还款计划id，活期为转出人id</param>
        /// <param name="isEarlyPay"></param>
        /// <param name="huoqi"></param>
        /// <param name="callBack"></param>
        public static void SendReturnPrinInte(int projectCode, string sum, int repayTaskId, bool isEarlyPay, bool isHuoqi)
        {
            var context = new Agp2pDataContext();
            //创建本息到账请求并设置分账列表
            var returnPrinInteReqMsg = new ReturnPrinInteReqMsg(projectCode, sum, isHuoqi);
            //定期项目需计算全部投资者本息明细
            var repayRask = context.li_repayment_tasks.SingleOrDefault(r => r.id == repayTaskId);
            var transList = TransactionFacade.GenerateRepayTransactions(repayRask, DateTime.Now);
            returnPrinInteReqMsg.SetSubledgerList(transList);
            returnPrinInteReqMsg.Remarks = $"isEarly=false&repayTaskId={repayTaskId}&isHuoqi={isHuoqi}";
            //发送请求
            MessageBus.Main.PublishAsync(returnPrinInteReqMsg, msg =>
            {
                //处理请求同步返回结果
                var returnPrinInteRespMsg =
                    BaseRespMsg.NewInstance<ReturnPrinInteRespMsg>(msg.SynResult);
                MessageBus.Main.PublishAsync(returnPrinInteRespMsg);
            });
        }

        /// <summary>
        /// 活期项目转出本金
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="grouping"></param>
        /// <param name="callBack"></param>
        public static void SendReturnPrinInte(int projectCode, IGrouping<li_projects, li_claims> grouping, Action callBack)
        {
            var context = new Agp2pDataContext();
            //创建本息到账请求并设置分账列表
            var returnPrinInteReqMsg = new ReturnPrinInteReqMsg(projectCode, grouping.Sum(pcsc => pcsc.principal).ToString("f"), true);
            //生成分账列表（每个转出用户的本金）
            returnPrinInteReqMsg.SetSubledgerList(grouping.ToLookup(c => c.dt_users));
            //发送请求
            MessageBus.Main.PublishAsync(returnPrinInteReqMsg, msg =>
            {
                //处理请求同步返回结果
                var returnPrinInteRespMsg =
                    BaseRespMsg.NewInstance<ReturnPrinInteRespMsg>(msg.SynResult);
                returnPrinInteRespMsg.Sync = true;
                returnPrinInteReqMsg.Remarks = "isEarly=false&isHuoqi=true";
                MessageBus.Main.PublishAsync(returnPrinInteRespMsg, result =>
                {
                    if (callBack != null && returnPrinInteRespMsg.HasHandle) callBack();
                });
            });
            
        }
    }
}
