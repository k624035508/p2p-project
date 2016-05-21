using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg.Transaction
{
    /// <summary>
    /// 个人用户签约银行卡查询
    /// </summary>
    public class SignBankCardQueryRequest : BackEndReqMsg
    {
        public string QueryType { get; set; }//查询类型 0：全部1：用户提现2：一键充值3：协议还款

        public SignBankCardQueryRequest(int userId, string queryType = "2")
        {
            UserId = userId;
            QueryType = queryType;

            Api = (int) Agp2pEnums.SumapayApiEnum.QCard;
            ApiInterface = SumapayConfig.ApiUrl + "main/UserForFT_signBankCardQuery";
            RequestId = Agp2pEnums.SumapayApiEnum.QCard.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + QueryType, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&userIdIdentity={UserId}&queryType={QueryType}&signature={GetSignature()}";
            return postStr;
        }
    }
}
