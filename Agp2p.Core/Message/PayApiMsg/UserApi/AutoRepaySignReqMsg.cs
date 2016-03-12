using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人开通存管/银行账户自动还款请求
    /// </summary>
    public class AccountRepaySignReqMsg : FrontEndReqMsg
    {
        public string Cycle { get; set; }//还款周期
        public string RepayLimit { get; set; }//还款上限金额

        public AccountRepaySignReqMsg(int userId, string projectCode, string repayLimit, bool useBank, Action<string> callback, string cycle = "")
        {
            UserId = userId;
            ProjectCode = projectCode;
            RepayLimit = repayLimit;
            Cycle = cycle;
            Api = useBank ? (int)Agp2pEnums.SumapayApiEnum.AbReO : (int) Agp2pEnums.SumapayApiEnum.AcReO;
            ApiInterface = TestApiUrl + (useBank ? "user/autoWithholdingRepay_toAutoRepaySign" : "user/autoAccountRepay_toAutoRepaySign");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            CallBack = callback;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return
                hmac.ComputeHashToBase64String(RequestId + MerchantCode + UserId + ProjectCode + Cycle + RepayLimit +
                                               SuccessReturnUrl + FailReturnUrl);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"projectCode", ProjectCode},
                {"repayLimit", RepayLimit},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(Cycle)) sd.Add("cycle", Cycle);
            return sd;
        }
    }
}
