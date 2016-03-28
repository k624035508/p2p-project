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
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public bool Collective { get; set; }//集合项目标识
        public decimal FeeRate { get; set; }//手续费率
        //分账列表
        private string subledgerList;
        public string SubledgerList
        {
            get
            {
                if (string.IsNullOrEmpty(subledgerList))
                {
                    subledgerList = JsonHelper.ObjectToJSON(new List<object>
                    {
                        //借款人收到的款
                        new
                        {
                            roleType = "0",
                            roleCode = UserId,
                            inOrOut = "0",
                            sum = Sum
                        },
                        new
                        {
                            roleType = "1",
                            roleCode = SumapayConfig.MerchantCode,
                            inOrOut = "1",
                            sum = Utils.StrToDecimal(Sum, 0) * FeeRate
                        }
                    });
                }
                return subledgerList;
            }
            set { subledgerList = value; }
        }

        public MakeLoanReqMsg(string projectCode, string sum, decimal feeRate, bool collective = false, string payType = "2", string mainAccountType = "", string mainAccountCode = "")
        {
            ProjectCode = projectCode;
            Sum = sum;
            PayType = payType;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            FeeRate = feeRate;
            Collective = collective;

            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.CLoan : (int) Agp2pEnums.SumapayApiEnum.ALoan;
            ApiInterface = SumapayConfig.TestApiUrl + (collective ? "main/CollectiveFinance_loan" : "main/TransactionForFT_loan");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
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
