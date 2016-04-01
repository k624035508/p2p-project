using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人自动还款普通/集合项目
    /// </summary>
    public class AutoRepayReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//还款金额
        public string FeeType { get; set; }//手续费收取方式
        public bool Collective { get; set; }//集合项目标识

        public AutoRepayReqMsg(int userId, string sum, bool collective = false, string fayType = "0")
        {
            UserId = userId;
            Sum = sum;
            FeeType = fayType;
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.AbRep : (int) Agp2pEnums.SumapayApiEnum.AcRep;
            ApiInterface = SumapayConfig.TestApiUrl + (collective ? "main/CollectiveFinance_repay" : "main/TransactionForFT_repay");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId + ProjectCode + Sum  + FeeType + NoticeUrl);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&userIdIdentity={UserId}&projectCode={ProjectCode}&sum={Sum}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            if (!string.IsNullOrEmpty(FeeType)) postStr += $"&feeType={FeeType}";
            return postStr;
        }
    }
}
