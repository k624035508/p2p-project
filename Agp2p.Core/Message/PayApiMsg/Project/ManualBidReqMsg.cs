using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Agp2p.Common;


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
        public string ProjectSum { get; set; }//项目总额
        public bool Collective { get; set; }//集合项目标识
        /// <summary>
        /// 分账列表 当红包标识字段不为空时，此字段必输。当红包标识为 0 时， 则只对应记录一笔用户的出账明细；当红包标识为 1 时， 
        /// 分账列表内对应零笔或一笔用户的出账明细， 一笔商户的出账明细。用户需对应第三方用户标识，商 户需对应商户编码；
        /// 用户的出账金额和商户出账金额之和等于投标的总金额；用户出账时 bizFlag 为 0 正常业务（选输）；商户出账时 bizFlag 为 2 红包（必输），金额为红包金额
        /// </summary>
        public string SubledgerList { get; set; }

        public string BackUrl { get; set; }
        public string RequestType { get; set; }

        public ManualBidReqMsg(int userId, int projectCode, string sum, string projectSum, string projectDescription, bool collective = false, string giftFlag = "", string subledgerList = "")
        {
            UserId = userId;
            ProjectCode = projectCode;
            Sum = sum;
            SubledgerList = subledgerList;
            ProjectSum = projectSum;
            ProjectDescription = projectDescription;
            GiftFlag = giftFlag;
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.McBid : (int) Agp2pEnums.SumapayApiEnum.MaBid;
            ApiInterface = SumapayConfig.ApiUrl + (collective ? "user/collectiveBid_toCollectiveBid" : "user/manualBid_toManualBid");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public ManualBidReqMsg(int userId, int projectCode, string sum, string projectSum, string projectDescription, string backUrl, bool collective = false, string giftFlag = "", string subledgerList = "")
        {
            UserId = userId;
            ProjectCode = projectCode;
            Sum = sum;
            SubledgerList = subledgerList;
            ProjectSum = projectSum;
            ProjectDescription = projectDescription;
            GiftFlag = giftFlag;
            BackUrl = backUrl;

            RequestType = collective ? "PFT0014" : "PFT0003";
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.McBiM : (int)Agp2pEnums.SumapayApiEnum.MaBiM;
            ApiInterface = SumapayConfig.ApiUrl + "p2pMobileUser/merchant.do";
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public override string GetSignature()
        {
            return !string.IsNullOrEmpty(RequestType)
                ? SumaPayUtils.GenSign(
                    RequestType + RequestId + SumapayConfig.MerchantCode + UserId + Sum + ProjectCode +
                    ProjectDescription + ProjectSum + GiftFlag + SubledgerList + SumapayConfig.NoticeUrl +
                    SuccessReturnUrl + FailReturnUrl, SumapayConfig.Key)
                : SumaPayUtils.GenSign(
                    RequestId + SumapayConfig.MerchantCode + UserId + Sum + ProjectCode + ProjectDescription +
                    SuccessReturnUrl + FailReturnUrl + SumapayConfig.NoticeUrl, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"sum", Sum},
                {"projectCode", ProjectCode.ToString()},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(ProjectDescription)) sd.Add("projectDescription", ProjectDescription);
            if (!string.IsNullOrEmpty(GiftFlag)) sd.Add("giftFlag", GiftFlag);
            if (!string.IsNullOrEmpty(SubledgerList)) sd.Add("subledgerList", SubledgerList);
            if (!string.IsNullOrEmpty(ProjectSum)) sd.Add("projectSum", ProjectSum);
            if (!string.IsNullOrEmpty(BackUrl)) sd.Add("backUrl", BackUrl);
            if (!string.IsNullOrEmpty(RequestType)) sd.Add("requestType", RequestType);

            return sd;
        }
    }
}
