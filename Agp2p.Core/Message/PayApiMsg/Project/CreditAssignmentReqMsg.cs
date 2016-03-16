using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人债权转让
    /// </summary>
    public class CreditAssignmentReqMsg : FrontEndReqMsg
    {
        public string OriginalRequestId { get; set; }//原请求流水号
        public string OriginalOrderSum { get; set; }//原投标金额
        public string AssignmentSum { get; set; }//转让价格
        public string UndertakeSum { get; set; }//购买价格
        public string PayType { get; set; }//手续费收取方式
        public string SubledgerList { get; set; }//分账列表
        public string CreditValue { get; set; }//转让债权价值
        public string UndertakePercentage { get; set; }//承接系数
        public string ProjectDescription { get; set; }//项目描述
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码 

        public CreditAssignmentReqMsg(int userId, string projectCode, string originalRequestId, string originalOrderSum,
            string assignmentSum, string undertakeSum, string payType, string subledgerList, string projectDescription, string creditValue,
            string undertakePercentage, string mainAccountType, string mainAccountCode)
        {
            UserId = userId;
            ProjectCode = projectCode;
            OriginalRequestId = originalRequestId;
            OriginalOrderSum = originalOrderSum;
            AssignmentSum = assignmentSum;
            UndertakeSum = undertakeSum;
            PayType = payType;
            SubledgerList = subledgerList;
            ProjectDescription = projectDescription;
            CreditValue = creditValue;
            UndertakePercentage = undertakePercentage;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;

            Api =  (int) Agp2pEnums.SumapayApiEnum.CreAs;
            ApiInterface = SumapayConfig.TestApiUrl + "user/creditAssignment_toCreditAssignment";
            RequestId = Agp2pEnums.SumapayApiEnum.CreAs.ToString().ToUpper() + Utils.GetOrderNumberLonger();
            SuccessReturnUrl = "";
            FailReturnUrl = "";
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId + ProjectCode + OriginalRequestId +
                                               OriginalOrderSum + AssignmentSum + UndertakeSum + PayType + SubledgerList +
                                               SuccessReturnUrl + FailReturnUrl);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"projectCode", ProjectCode},
                {"originalRequestId", ProjectCode},
                {"originalOrderSum", ProjectCode},
                {"assignmentSum", ProjectCode},
                {"undertakeSum", ProjectCode},
                {"payType", ProjectCode},
                {"subledgerList", ProjectCode},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(ProjectDescription)) sd.Add("projectDescription", ProjectDescription);
            if (!string.IsNullOrEmpty(CreditValue)) sd.Add("creditValue", CreditValue);
            if (!string.IsNullOrEmpty(SubledgerList)) sd.Add("undertakePercentage", SubledgerList);
            if (!string.IsNullOrEmpty(MainAccountType)) sd.Add("mainAccountType", MainAccountType);
            if (!string.IsNullOrEmpty(MainAccountCode)) sd.Add("mainAccountCode", MainAccountCode);

            return sd;
        }
    }
}
