using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人用户激活
    /// </summary>
    public class UserActivateReqMsg : FrontEndReqMsg
    {
        public string PayType { get; set; }

        public UserActivateReqMsg(int userId, string payType)
        {
            UserId = userId;
            PayType = payType;
            Api = (int) Agp2pEnums.SumapayApiEnum.Activ;
            ApiInterface = TestApiUrl + "user/activate_toActivate";
            RequestId = Agp2pEnums.SumapayApiEnum.Activ.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return hmac.ComputeHashToBase64String(RequestId + MerchantCode + UserId + SuccessReturnUrl + FailReturnUrl +
                                           PayType);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            return new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"payType", PayType},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", NoticeUrl},
                {"signature", GetSignature()}
            };
        }
    }
}
