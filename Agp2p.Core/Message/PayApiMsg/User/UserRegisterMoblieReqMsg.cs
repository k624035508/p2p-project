using System.Collections.Generic;
using System.Text;
using System.Web;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人开户请求 移动端
    /// </summary>
    public class UserRegisterMoblieReqMsg : FrontEndReqMsg
    {
        public string MobileNo { get; set; }
        public string UserName { get; set; }
        public string IdNumber { get; set; }
        public string RealNameToken { get; set; }
        public string RequestType { get; set; }
        public string PayType { get; set; }
        public string BackUrl { get; set; }

        public UserRegisterMoblieReqMsg(int userId, string mobileNo, string userName, string idNumber, string realNameToken, string backUrl)
        {
            UserId = userId;
            MobileNo = mobileNo;
            UserName = userName;
            IdNumber = idNumber;
            RealNameToken = realNameToken;
            BackUrl = backUrl;  

            RequestType = "PFT0000";
            PayType = "1";
            Api = (int)Agp2pEnums.SumapayApiEnum.URegM;
            ApiInterface = SumapayConfig.TestApiUrl + "p2pMobileUser/merchant.do";
            RequestId = Agp2pEnums.SumapayApiEnum.URegM.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return SumaPayUtils.GenSign(RequestType + RequestId + SumapayConfig.MerchantCode + UserId + PayType + UserName +
                IdNumber + MobileNo + RealNameToken + SumapayConfig.NoticeUrl + SuccessReturnUrl + FailReturnUrl, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestType", RequestType},
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"mobileNo", MobileNo},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"backUrl", BackUrl},
                {"payType", PayType},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(UserName)) sd.Add("userName", UserName);
            if (!string.IsNullOrEmpty(IdNumber)) sd.Add("idNumber", IdNumber);
            if (!string.IsNullOrEmpty(RealNameToken)) sd.Add("realNameToken", RealNameToken);

            return sd;
        }
    }
}
