using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人取消自动还款请求
    /// </summary>
    public class AutoRepayCancelReqMsg : FrontEndReqMsg
    {
        public AutoRepayCancelReqMsg(int userId, int projectCode)
        {
            UserId = userId;
            ProjectCode = projectCode;

            Api = (int)Agp2pEnums.SumapayApiEnum.ClRep;
            ApiInterface = SumapayConfig.ApiUrl + "user/cancelAutoRepay_toCancelAutoRepay";
            RequestId = Agp2pEnums.SumapayApiEnum.ClRep.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + ProjectCode + SuccessReturnUrl + FailReturnUrl, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            return new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"projectCode", ProjectCode.ToString()},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
        }
    }
}
