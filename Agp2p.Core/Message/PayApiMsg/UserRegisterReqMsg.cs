using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class UserRegisterReqMsg : BaseReqMsg
    {
        public string Telephone { get; set; }
        public string Name { get; set; }
        public string IdNumber { get; set; }
        public string Token { get; set; }
        public string PayType { get; set; }

        public UserRegisterReqMsg(int userIdIdentity, string telephone, string name, string idNumber, string token, string payType, Action<string> callback)
        {
            UserIdIdentity = userIdIdentity;
            Telephone = telephone;
            Name = name;
            IdNumber = idNumber;
            Token = token;
            PayType = payType;
            Api = (int) Agp2pEnums.SumapayApiEnum.UserReg;
            ApiUrl = "https://www.sumapay.com/user/register_toRegister";
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            CallBack = callback;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5("My Key");
            return hmac.ComputeHashToBase64String(RequestId + MerchantCode + UserIdIdentity + SuccessReturnUrl + FailReturnUrl +
                                           PayType);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            return new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", MerchantCode},
                {"userIdIdentity", UserIdIdentity.ToString()},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", NoticeUrl},
                {"telephone", Telephone},
                {"name", Name},
                {"idNumber", IdNumber.ToUpper()},
                {"token", Token},
                {"payType", PayType},
                {"signature", GetSignature()}
            };
        }
    }
}
