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
        public AutoRepayCancelReqMsg(int userId, string projectCode, Action<string> callback)
        {
            UserId = userId;
            ProjectCode = projectCode;
            Api = (int)Agp2pEnums.SumapayApiEnum.ClRep;
            ApiInterface = TestApiUrl + "user/cancelAutoRepay_toCancelAutoRepay";
            RequestId = Agp2pEnums.SumapayApiEnum.ClRep.ToString().ToUpper() + Utils.GetOrderNumberLonger();
            CallBack = callback;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return
                hmac.ComputeHashToBase64String(RequestId + MerchantCode + UserId + ProjectCode + SuccessReturnUrl + FailReturnUrl);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            return new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"projectCode", ProjectCode},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", NoticeUrl},
                {"signature", GetSignature()}
            };
        }
    }
}
