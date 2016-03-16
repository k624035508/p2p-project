using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 放款普通/集合项目
    /// </summary>
    public class MakeLoanReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//放款金额
        public string PayType { get; set; }//手续费收取方式
        public string SubledgerList { get; set; }//分账列表
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public bool Collective { get; set; }//集合项目标识

        public MakeLoanReqMsg(string projectCode, string sum, string payType, string subledgerList, string mainAccountType, string mainAccountCode, bool collective = false)
        {
            ProjectCode = projectCode;
            Sum = sum;
            PayType = payType;
            SubledgerList = subledgerList;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.CLoan : (int) Agp2pEnums.SumapayApiEnum.ALoan;
            ApiInterface = SumapayConfig.TestApiUrl + (collective ? "main/CollectiveFinance_loan" : "main/TransactionForFT_loan");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + ProjectCode  + Sum + PayType +
                                               SubledgerList + NoticeUrl + MainAccountType + MainAccountCode);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&projectCode={ProjectCode}&sum={Sum}&payType={PayType}&subledgerList={SubledgerList}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            if (!string.IsNullOrEmpty(MainAccountType)) postStr += $"&mainAccountType={MainAccountType}";
            if (!string.IsNullOrEmpty(MainAccountCode)) postStr += $"&mainAccountCode={MainAccountCode}";
            return postStr;
        }
    }
}
