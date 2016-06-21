using System.Collections.Generic;
using System.Text;
using System.Web;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人开户请求
    /// </summary>
    public class UserRegisterReqMsg : FrontEndReqMsg
    {
        public string Telephone { get; set; }
        public string Name { get; set; }
        public string IdNumber { get; set; }
        public string Token { get; set; }
        public string PayType { get; set; }

        public UserRegisterReqMsg(int userId, string telephone, string name, string idNumber, string token)
        {
            UserId = userId;
            Telephone = telephone;
            Name = name;
            IdNumber = idNumber;
            Token = token;

            PayType = "0";
            Api = (int) Agp2pEnums.SumapayApiEnum.URegi;
            ApiInterface = SumapayConfig.ApiUrl + "user/register_toRegister";
            RequestId = Agp2pEnums.SumapayApiEnum.URegi.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        protected UserRegisterReqMsg()
        {
        }

        public override string GetSignature()
        {
            return SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + SuccessReturnUrl + FailReturnUrl +
                                           PayType, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(Telephone)) sd.Add("telephone", Telephone);
            if (!string.IsNullOrEmpty(Name)) sd.Add("name", Name);
            if (!string.IsNullOrEmpty(IdNumber)) sd.Add("idNumber", IdNumber);
            if (!string.IsNullOrEmpty(Token)) sd.Add("token", Token);
            if (!string.IsNullOrEmpty(PayType)) sd.Add("payType", PayType);
            return sd;
        }
    }
}
