using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 实名认证（同步返回响应）
    /// </summary>
    public class UserRealNameAuthReqMsg : BackEndReqMsg
    {
        public string UserName { get; set; }
        public string IdNumber { get; set; }

        public UserRealNameAuthReqMsg(string userName, string idNumber, Action<string, string> callback)
        {
            UserName = userName;
            IdNumber = idNumber;
            Api = (int)Agp2pEnums.SumapayApiEnum.URegi;
            ApiInterface = TestApiUrl + "main/UserForFT_realNameAuth";
            RequestId = Agp2pEnums.SumapayApiEnum.URegi.ToString().ToUpper() + Utils.GetOrderNumberLonger();
            CallBack = callback;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return hmac.ComputeHashToBase64String(RequestId + MerchantCode + UserName + IdNumber);
        }

        public override string GetPostPara()
        {
            return
                $"requestId={RequestId}&merchantCode={MerchantCode}&userName={UserName}&idNumber={IdNumber.ToUpper()}&signature={GetSignature()}";
        }
    }
}