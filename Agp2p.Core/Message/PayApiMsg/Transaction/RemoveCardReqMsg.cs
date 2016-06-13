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
    public class RemoveCardReqMsg : BackEndReqMsg
    {
        public string UserName { get; set; }
        public string IdNumber { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Reason { get; set; }

        public RemoveCardReqMsg(int userId, string userName, string idNumber, string telephone, string email, string reason = "银行卡遗失")
        {
            UserId = userId;
            UserName = userName;
            IdNumber = idNumber;
            Telephone = telephone;
            Email = email;
            Reason = reason;

            Api = (int)Agp2pEnums.SumapayApiEnum.RemCa;
            ApiInterface = SumapayConfig.ApiUrl + "main/UserForFT_replaceBankCard";
            RequestId = Agp2pEnums.SumapayApiEnum.RemCa.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public RemoveCardReqMsg()
        {

        }

        public override string GetSignature()
        {
            return
               SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + UserName + IdNumber + Telephone + Email + Reason + NoticeUrl, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&userIdIdentity={UserId}&userName={UserName}&idNumber={IdNumber}&telephone={Telephone}&email={Email}&reason={Reason}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            return postStr;
        }
    }
}
