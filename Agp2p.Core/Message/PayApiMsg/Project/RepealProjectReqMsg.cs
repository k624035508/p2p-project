using System;
using System.Collections.Generic;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 流标普通项目
    /// </summary>
    public class RepealProjectReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//原投标订单金额

        public RepealProjectReqMsg(int projectCode, string sum)
        {
            ProjectCode = projectCode;
            Sum = sum;
            Api = (int) Agp2pEnums.SumapayApiEnum.RePro;
            ApiInterface = SumapayConfig.TestApiUrl + "main/TransactionForFT_repealProject";
            RequestId = Agp2pEnums.SumapayApiEnum.RePro.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + ProjectCode + Sum + NoticeUrl, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            return
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&projectCode={ProjectCode}&sum={Sum}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
        }
    }
}
