using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人银行卡解绑
    /// </summary>
    public class RemoveCardReqMsg : FrontEndReqMsg
    {
        public string UserName { get; set; }
        public string IdNumber { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Reason { get; set; }

        public RemoveCardReqMsg(int userId, string userName, string idNumber, string telephone, string email, string reason)
        {
            UserId = userId;
            UserName = userName;
            IdNumber = idNumber;
            Telephone = telephone;
            Email = email;
            Reason = reason;

            Api = (int)Agp2pEnums.SumapayApiEnum.CanCard;
            ApiInterface = SumapayConfig.ApiUrl + "main/UserForFT_replaceBankCard";
            RequestId = Agp2pEnums.SumapayApiEnum.CanCard.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
               SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + UserName + IdNumber + Telephone + Email + Reason + SumapayConfig.NoticeUrl, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"userName", UserName},
                {"idNumber", IdNumber},
                {"telephone", Telephone},
                {"email", Email},
                {"reason", Reason},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            return sd;
        }
    }
}
