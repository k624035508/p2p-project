using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class HongbaoPayReqMsg : BackEndReqMsg
    {
        public string UserAccountType { get; set; } //用户账户类型  固定值1   1代表可用账户；
        public decimal Sum { get; set; }  //付款金额

        public HongbaoPayReqMsg(int userId,decimal sum,string userAccountType = "1")
        {
            UserId = userId;
            UserAccountType = userAccountType;
            Sum = sum;

            Api = (int)Agp2pEnums.SumapayApiEnum.HbPay;
            ApiInterface = SumapayConfig.ApiUrl + "main/TransferFundForFT_transferToUser";
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public HongbaoPayReqMsg()
        {

        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + UserAccountType + Sum + NoticeUrl, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            var postStr = $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&userIdIdentity={UserId}&userAccountType={UserAccountType}&sum={Sum}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            return postStr;
        }
    }
}
