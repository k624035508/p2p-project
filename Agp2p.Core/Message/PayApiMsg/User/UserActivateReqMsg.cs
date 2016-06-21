using System;
using System.Collections.Generic;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人用户激活
    /// </summary>
    public class UserActivateReqMsg : FrontEndReqMsg
    {
        public string PayType { get; set; }

        public UserActivateReqMsg(int userId, string payType = "1")
        {
            UserId = userId;
            PayType = payType;
            Api = (int) Agp2pEnums.SumapayApiEnum.Activ;
            ApiInterface = SumapayConfig.ApiUrl + "user/activate_toActivate";
            RequestId = Agp2pEnums.SumapayApiEnum.Activ.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + SuccessReturnUrl + FailReturnUrl +
                                           PayType, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            return new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"payType", PayType},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
        }
    }
}
