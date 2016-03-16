using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人存管账户还款普通/集合项目
    /// </summary>
    public class AccountRepayReqMsg : FrontEndReqMsg
    {
        public string Sum { get; set; }//还款金额
        public string RechargeUrl { get; set; }//P2P充值跳转地址
        public string GiftFlag { get; set; }//红包标识
        public string SubledgerList { get; set; }//分账列表
        public bool Collective { get; set; }//集合项目标识

        public AccountRepayReqMsg(int userId, string sum, string rechargeUrl, string giftFlag, string subledgerList, bool collective = false)
        {
            UserId = userId;
            Sum = sum;
            SubledgerList = subledgerList;
            RechargeUrl = rechargeUrl;
            GiftFlag = giftFlag;

            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.McRep : (int) Agp2pEnums.SumapayApiEnum.MaRep;
            ApiInterface = SumapayConfig.TestApiUrl + (collective ? "user/collectiveRepay_toRepayDetail" : "user/repay_toRepayDetail");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
            SuccessReturnUrl = "";
            FailReturnUrl = "";
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId + ProjectCode + Sum  +
                RechargeUrl + SuccessReturnUrl + FailReturnUrl);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"projectCode ", ProjectCode},
                {"sum", Sum},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(RechargeUrl)) sd.Add("rechargeUrl ", RechargeUrl);
            if (!string.IsNullOrEmpty(GiftFlag)) sd.Add("giftFlag", GiftFlag);
            if (!string.IsNullOrEmpty(SubledgerList)) sd.Add("subledgerList", SubledgerList);

            return sd;
        }
    }
}
