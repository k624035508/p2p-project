using System;
using System.Collections.Generic;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 查询项目信息
    /// </summary>
    public class QueryProjectReqMsg : BackEndReqMsg
    {
        //交易类型 固定值
        //0：全部
        //1：普通投标冻结
        //2：普通放款
        //3：个人用户存管账户还款
        //4：个人用户协议还款
        //5：个人用户本息到账
        //6：个人用户债权转让
        //7：集合理财投标冻结
        //8：集合理财放款
        //9：企业用户账户还款
        public string TradeType { get; set; }

        public QueryProjectReqMsg(int projectCode, string tradeType = "0")
        {
            ProjectCode = projectCode;
            TradeType = tradeType;
            Api = (int) Agp2pEnums.SumapayApiEnum.QuPro;
            ApiInterface = SumapayConfig.ApiUrl + "main/TransactionForFT_projectQuery";
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + ProjectCode + TradeType, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&projectCode={ProjectCode}&tradeType={TradeType}&signature={GetSignature()}";
            return postStr;
        }
    }
}
