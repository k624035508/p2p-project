using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 实名认证
    /// </summary>
    public class UserRealNameAuthReqMsg : BackEndReqMsg
    {
        public string UserName { get; set; }
        public string IdNumber { get; set; }
        public string PayType { get; set; }

        public UserRealNameAuthReqMsg(string userName, string idNumber)
        {
            UserName = userName;
            IdNumber = idNumber;

            PayType = "0";
            Api = (int)Agp2pEnums.SumapayApiEnum.URegi;
            ApiInterface = SumapayConfig.TestApiUrl + "main/UserForFT_realNameAuth";
            RequestId = Agp2pEnums.SumapayApiEnum.URegi.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserName + IdNumber + PayType);
        }

        public override string GetPostPara()
        {
            return
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&userName={UserName}&idNumber={IdNumber.ToUpper()}&signature={GetSignature()}";
        }
    }
}