using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人提现
    /// </summary>
    public class WithdrawReqMsg : FrontEndReqMsg
    {
        public string Sum { get; set; }//充值金额
        public string BankCode { get; set; }//银行编码
        public string BankAccount { get; set; }//银行账号
        public string PayType { get; set; }
        public string SubledgerList { get; set; }//分账列表
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码

        public WithdrawReqMsg(int userId, string sum, string bankCode, string bankAccount,
            string payType, string subledgerList, string mainAccountType, string mainAccountCode)
        {
            UserId = userId;
            Sum = sum;
            BankCode = bankCode;
            BankAccount = bankAccount;
            PayType = payType;
            SubledgerList = subledgerList;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;

            Api = (int) Agp2pEnums.SumapayApiEnum.Wdraw;
            ApiInterface = SumapayConfig.TestApiUrl + "user/withdraw_toWithdraw";
            RequestId = Agp2pEnums.SumapayApiEnum.Wdraw.ToString().ToUpper() + Utils.GetOrderNumberLonger();
            SuccessReturnUrl = "";
            FailReturnUrl = "";
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId + Sum  +
                SuccessReturnUrl + FailReturnUrl + PayType + SubledgerList);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"sum", Sum},
                {"bankCode", BankCode},
                {"bankAccount", BankAccount},
                {"payType", PayType},
                {"subledgerList", SubledgerList},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(MainAccountType)) sd.Add("mainAccountType", MainAccountType);
            if (!string.IsNullOrEmpty(MainAccountCode)) sd.Add("mainAccountCode", MainAccountCode);

            return sd;
        }
    }
}
