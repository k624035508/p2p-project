using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 跳转个人账户管理
    /// </summary>
    public class UserToAccountReqMsg : FrontEndReqMsg
    {
        public UserToAccountReqMsg()
        {
            Api = (int) Agp2pEnums.SumapayApiEnum.Activ;
            ApiInterface = SumapayConfig.TestApiUrl + "user/accountManage_toAccountManage";
            RequestId = Agp2pEnums.SumapayApiEnum.Activ.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            return new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"signature", GetSignature()}
            };
        }
    }
}
