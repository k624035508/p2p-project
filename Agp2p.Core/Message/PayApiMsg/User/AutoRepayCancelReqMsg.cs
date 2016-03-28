using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人取消自动还款请求
    /// </summary>
    public class AutoRepayCancelReqMsg : FrontEndReqMsg
    {
        public AutoRepayCancelReqMsg(int userId, string projectCode)
        {
            UserId = userId;
            ProjectCode = projectCode;

            Api = (int)Agp2pEnums.SumapayApiEnum.ClRep;
            ApiInterface = SumapayConfig.TestApiUrl + "user/cancelAutoRepay_toCancelAutoRepay";
            RequestId = Agp2pEnums.SumapayApiEnum.ClRep.ToString().ToUpper() + Utils.GetOrderNumberLonger();
            SuccessReturnUrl = "";
            FailReturnUrl = "";
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId + ProjectCode + SuccessReturnUrl + FailReturnUrl);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            return new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"projectCode", ProjectCode},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
        }
    }
}
