using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 本息到账普通/集合项目
    /// </summary>
    public class ReturnPrinInteReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//本息到账金额
        public string PayType { get; set; }//手续费收取方式
        public string SubledgerList { get; set; }//分账列表
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public bool Collective { get; set; }//集合项目标识

        public ReturnPrinInteReqMsg(int userId, string sum, string payType, string subledgerList, string mainAccountType, string mainAccountCode, bool collective = false)
        {
            UserId = userId;
            Sum = sum;
            PayType = payType;
            SubledgerList = subledgerList;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;

            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.RetCo : (int) Agp2pEnums.SumapayApiEnum.RetPt;
            ApiInterface = TestApiUrl + (collective ? "main/CollectiveFinance_returnPrinInte" : "main/TransactionForFT_returnPrinInte");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return
                hmac.ComputeHashToBase64String(RequestId + MerchantCode + ProjectCode + Sum  + PayType + SubledgerList + NoticeUrl + MainAccountType + MainAccountCode);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={MerchantCode}&projectCode={ProjectCode}&sum={Sum}&payType={PayType}&subledgerList={SubledgerList}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            if (!string.IsNullOrEmpty(MainAccountType)) postStr += $"&mainAccountType={MainAccountType}";
            if (!string.IsNullOrEmpty(MainAccountCode)) postStr += $"&mainAccountCode={MainAccountCode}";
            return postStr;
        }
    }
}
