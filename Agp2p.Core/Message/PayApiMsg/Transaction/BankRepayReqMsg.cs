using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人协议还款（银行）普通/集合项目
    /// </summary>
    public class BankRepayReqMsg : FrontEndReqMsg
    {
        public string Sum { get; set; }//还款金额
        public string GiftFlag { get; set; }//红包标识
        public string SubledgerList { get; set; }//分账列表
        public string ProjectDescription { get; set; }//项目描述
        public string PayType { get; set; }//手续费收取方式
        public bool Collective { get; set; }//集合项目标识

        public BankRepayReqMsg(int userId, string sum, string payType, string projectDescription, string giftFlag, string subledgerList, bool collective = false)
        {
            UserId = userId;
            Sum = sum;
            PayType = payType;
            SubledgerList = subledgerList;
            ProjectDescription = projectDescription;
            GiftFlag = giftFlag;
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.BcRep : (int) Agp2pEnums.SumapayApiEnum.BaRep;
            ApiInterface = TestApiUrl + (collective ? "user/collectiveWithholdingRepay_toWithholdingRepay" : "user/withholdingRepay_toWithholdingRepay");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return
                hmac.ComputeHashToBase64String(RequestId + MerchantCode + UserId + ProjectCode + Sum  + PayType +
                SuccessReturnUrl + FailReturnUrl);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"projectCode ", ProjectCode},
                {"sum", Sum},
                {"payType ", PayType},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(ProjectDescription)) sd.Add("projectDescription ", ProjectDescription);
            if (!string.IsNullOrEmpty(GiftFlag)) sd.Add("giftFlag", GiftFlag);
            if (!string.IsNullOrEmpty(SubledgerList)) sd.Add("subledgerList", SubledgerList);

            return sd;
        }
    }
}
