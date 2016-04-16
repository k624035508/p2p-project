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
        public string SubledgerList { get; set; }//分账列表

        public MakeLoanReqMsg(int userId, int projectCode, string sum, bool collective = false, string payType = "3", string mainAccountType = "", string mainAccountCode = "")
        {
            UserId = userId;
            ProjectCode = projectCode;
            Sum = sum;
            PayType = payType;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            Collective = collective;

            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.CLoan : (int)Agp2pEnums.SumapayApiEnum.ALoan;
            ApiInterface = SumapayConfig.TestApiUrl + (collective ? "main/CollectiveFinance_loan" : "main/TransactionForFT_loan");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + ProjectCode + Sum + PayType +
                                               SubledgerList + NoticeUrl + MainAccountType + MainAccountCode, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&projectCode={ProjectCode}&sum={Sum}&payType={PayType}&subledgerList={SubledgerList}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            if (!string.IsNullOrEmpty(MainAccountType)) postStr += $"&mainAccountType={MainAccountType}";
            if (!string.IsNullOrEmpty(MainAccountCode)) postStr += $"&mainAccountCode={MainAccountCode}";
            return postStr;
        }

        public void SetSubledgerList(decimal loanFee, decimal bondFee)
        {
            var feeSum = loanFee + bondFee;
            var loanSum = Utils.StrToDecimal(Sum, 0);
            var list = new List<object>
            {
                //借款人收到的款
                new
                {
                    roleType = "0",
                    roleCode = UserId.ToString(),
                    inOrOut = "0",
                    sum = (loanSum - feeSum).ToString("f")
                }
            };
            //平台服务费为0不能发生生成分账列表
            if (feeSum > 0)
            {
                //平台服务费
                list.Add(new
                {
                    roleType = "1",
                    roleCode = SumapayConfig.MerchantCode,
                    inOrOut = "0",
                    sum = (loanFee + bondFee).ToString("f")
                });
            }
            SubledgerList = JsonHelper.ObjectToJSON(list);
        }
    }
}
