using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 撤标普通/集合项目
    /// </summary>
    public class WithDrawalReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//原投标订单金额
        public string BidRequestId { get; set; }//原请求流水号
        public string WithdrawalFund { get; set; }//撤标金额
        public bool Collective { get; set; }//集合项目标识

        public WithDrawalReqMsg(string projectCode, string sum, string bidRequestId, string withdrawalFund, bool collective = false)
        {
            ProjectCode = projectCode;
            Sum = sum;
            BidRequestId = bidRequestId;
            WithdrawalFund = withdrawalFund;
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.CoPro : (int) Agp2pEnums.SumapayApiEnum.CaPro;
            ApiInterface = TestApiUrl + (collective ? "main/CollectiveFinance_withdrawal" : "main/TransactionForFT_withdrawal");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return
                hmac.ComputeHashToBase64String(RequestId + MerchantCode + ProjectCode + BidRequestId + Sum +
                                               WithdrawalFund + NoticeUrl);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={MerchantCode}&projectCode={ProjectCode}&sum={Sum}&bidRequestId={BidRequestId}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            if (!string.IsNullOrEmpty(WithdrawalFund)) postStr += $"&withdrawalFund={WithdrawalFund}";
            return postStr;
        }
    }
}
