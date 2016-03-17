using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 手动投标普通/集合项目
    /// </summary>
    public class ManualBidReqMsg : FrontEndReqMsg
    {
        public string Sum { get; set; }//投标金额
        public string ProjectDescription { get; set; }//项目描述
        public string GiftFlag { get; set; }//红包标识
        public string SubledgerList { get; set; }//分账列表
        public string ProjectSum { get; set; }//项目总额
        public bool Collective { get; set; }//集合项目标识

        public ManualBidReqMsg(int userId, string projectCode, string sum, string projectSum, string projectDescription,
            string subledgerList = "", string giftFlag = "0", bool collective = false)
        {
            UserId = userId;
            ProjectCode = projectCode;
            Sum = sum;
            SubledgerList = subledgerList;
            ProjectSum = projectSum;
            ProjectDescription = projectDescription;
            GiftFlag = giftFlag;
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.McBid : (int) Agp2pEnums.SumapayApiEnum.MaBid;
            ApiInterface = SumapayConfig.TestApiUrl + (collective ? "user/collectiveBid_toCollectiveBid" : "user/manualBid_toManualBid");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;

            SuccessReturnUrl = "";
            FailReturnUrl = "";
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId + Sum + ProjectCode + ProjectDescription +
                SuccessReturnUrl + FailReturnUrl + SumapayConfig.NoticeUrl);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"sum", Sum},
                {"projectCode", ProjectCode},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(ProjectDescription)) sd.Add("projectDescription", ProjectDescription);
            if (!string.IsNullOrEmpty(GiftFlag)) sd.Add("giftFlag", GiftFlag);
            if (!string.IsNullOrEmpty(SubledgerList)) sd.Add("subledgerList", SubledgerList);
            if (!string.IsNullOrEmpty(ProjectSum)) sd.Add("projectSum", ProjectSum);

            return sd;
        }
    }
}
