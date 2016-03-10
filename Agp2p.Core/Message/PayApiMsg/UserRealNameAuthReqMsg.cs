using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class UserRealNameAuthReqMsg : BackEndReqMsg
    {
        public string UserName { get; set; }
        public string IdNumber { get; set; }
        public string PayType { get; set; }

        public UserRealNameAuthReqMsg(string userName, string idNumber, string payType, Action<string, string> callback)
        {
            UserName = userName;
            IdNumber = idNumber;
            PayType = payType;
            Api = (int)Agp2pEnums.SumapayApiEnum.UReg;
            ApiInterface = TestApiUrl + "main/UserForFT_realNameAuth";
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            CallBack = callback;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return hmac.ComputeHashToBase64String(RequestId + MerchantCode + UserName + IdNumber + PayType);
        }

        public override string GetPostPara()
        {
            return
                $"requestId={RequestId}&merchantCode={MerchantCode}&userName={UserName}&idNumber={IdNumber.ToUpper()}&payType={PayType}&signature={GetSignature()}";
        }
    }
}