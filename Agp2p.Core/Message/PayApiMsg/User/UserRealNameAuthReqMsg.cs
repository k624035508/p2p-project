using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Agp2p.Common;


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

        public UserRealNameAuthReqMsg(int userId, string userName, string idNumber)
        {
            UserId = userId;
            UserName = userName;
            IdNumber = idNumber;

            PayType = "1";
            Api = (int)Agp2pEnums.SumapayApiEnum.UAuth;
            ApiInterface = SumapayConfig.TestApiUrl + "main/UserForFT_realNameAuth";
            RequestId = Agp2pEnums.SumapayApiEnum.UAuth.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserName + IdNumber + PayType,
                SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            return
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&userName={HttpUtility.UrlEncode(UserName, Encoding.GetEncoding("GBK"))}&idNumber={IdNumber.ToUpper()}&payType={PayType}&signature={GetSignature()}";
        }
    }
}