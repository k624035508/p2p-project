using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Agp2p.Common;
using TinyMessenger;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人自动投标续约
    /// </summary>
    public class AutoBidSignReqMsg : FrontEndReqMsg
    {
        public string ProtocolCode { get; set; }//授权协议号
        public string ContractFund { get; set; }//签约金额
        public string ProjectType { get; set; }//项目类型0或空：全部项目1：普通项目2：集合理财项目
        public string Remarks { get; set; }
        public bool Cancel { get; set; }

        public AutoBidSignReqMsg(int userId, bool cancel = false, string protocolCode = "", string remarks = "",string contractFund = null, string projectType = null)
        {
            Cancel = cancel;
            UserId = userId;
            ContractFund = contractFund;
            ProjectType = projectType;
            ProtocolCode = protocolCode ?? "BidS" + Utils.GetOrderNumber();
            Remarks = remarks;

            if (!cancel)
            {
                Api = (int)Agp2pEnums.SumapayApiEnum.AtBid;
                ApiInterface = SumapayConfig.ApiUrl + "user/autoBid_toAutoSign";
            }
            else
            {
                Api = (int)Agp2pEnums.SumapayApiEnum.ClBid;
                ApiInterface = SumapayConfig.ApiUrl + "user/cancelAutoBid_toCancelAutoBid";
            }
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + ProtocolCode + SuccessReturnUrl +
                                               FailReturnUrl, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"protocolCode", ProtocolCode},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(ContractFund)) sd.Add("contractFund", ContractFund);
            if (!string.IsNullOrEmpty(ProjectType)) sd.Add("projectType", ProjectType);
            if (!string.IsNullOrEmpty(Remarks)) sd.Add("remarks", HttpUtility.UrlEncode(Remarks, Encoding.GetEncoding("GBK")));

            return sd;
        }
    }
}
