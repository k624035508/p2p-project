using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

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

        public AutoBidSignReqMsg(int userId, string contractFund = null, string projectType = null, bool cancel = false, string remarks = "")
        {
            Cancel = cancel;
            UserId = userId;
            ContractFund = contractFund;
            ProjectType = projectType;

            if (!cancel)
            {
                Api = (int)Agp2pEnums.SumapayApiEnum.AtBid;
                ApiInterface = SumapayConfig.TestApiUrl + "user/autoBid_toAutoSign";
            }
            else
            {
                Api = (int)Agp2pEnums.SumapayApiEnum.ClBid;
                ApiInterface = SumapayConfig.TestApiUrl + "user/cancelAutoBid_toCancelAutoBid";
            }
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Remarks = remarks;
            SuccessReturnUrl = "";
            FailReturnUrl = "";
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId + ProtocolCode + SuccessReturnUrl +
                                               FailReturnUrl);
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
            if (!string.IsNullOrEmpty(Remarks)) sd.Add("remarks", Remarks);

            return sd;
        }
    }
}
