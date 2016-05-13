using System;
using System.Collections.Generic;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 自动投标普通/集合项目
    /// </summary>
    public class AutoBidReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//投标金额
        public string ProjectDescription { get; set; }//项目描述
        public string ProtocolCode { get; set; }//自动投标授权协议号
        public string ProjectSum { get; set; }//项目总额
        public bool Collective { get; set; }//集合项目标识

        public AutoBidReqMsg(string protocolCode, int projectCode, string sum, string projectSum, string projectDescription, bool collective = false)
        {
            ProtocolCode = protocolCode;
            ProjectCode = projectCode;
            Sum = sum;
            ProjectSum = projectSum;
            ProjectDescription = projectDescription;
            Collective = collective;

            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.AcBid : (int) Agp2pEnums.SumapayApiEnum.AmBid;
            ApiInterface = SumapayConfig.ApiUrl + (collective ? "main/CollectiveFinance_autoBidding" : "main/TransactionForFT_autoBidding");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + ProjectCode + ProjectDescription + Sum +
                                               ProtocolCode + NoticeUrl, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&projectCode={ProjectCode}&sum={Sum}&protocolCode={ProtocolCode}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            if (!string.IsNullOrEmpty(ProjectDescription)) postStr += $"&projectDescription={ProjectDescription}";
            if (!string.IsNullOrEmpty(ProjectSum)) postStr += $"&projectSum={ProjectSum}";
            return postStr;
        }
    }
}
