﻿using System;
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
        public string BankId { get; set; }//银行编码
        public string BankCode { get; set; }//银行编码
        public string BankAccount { get; set; }//银行账号
        public string PayType { get; set; }
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        //分账列表
        private string subledgerList;
        public string SubledgerList
        {
            get
            {
                if (string.IsNullOrEmpty(subledgerList))
                {
                    subledgerList = JsonHelper.ObjectToJSON(new List<object>()
                    {
                        new
                        {
                            roleType = "0",
                            roleCode = UserId.ToString(),
                            inOrOut = "1",
                            sum = Sum
                        }
                    });
                }
                return subledgerList;
            }
            set { subledgerList = value; }
        }

        public WithdrawReqMsg(int userId, string sum, string bankId, string bankCode = "", string bankAccount = "",
            string payType = "3", string mainAccountType = "", string mainAccountCode = "")
        {
            UserId = userId;
            Sum = sum;
            PayType = payType;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            BankId = bankId;
            SetBankCodeAccount(bankCode, bankAccount);

            Api = (int) Agp2pEnums.SumapayApiEnum.Wdraw;
            ApiInterface = SumapayConfig.TestApiUrl + "user/withdraw_toWithdraw";
            RequestId = Agp2pEnums.SumapayApiEnum.Wdraw.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        private void SetBankCodeAccount(string bankCode, string bankAccount)
        {
            if (!string.IsNullOrEmpty(bankCode))
            {
                Utils.GetEnumValues<Agp2pEnums.SumapayBankCodeEnum>().ForEach(e =>
                {
                    if (Utils.GetAgp2pEnumDes(e).Equals(bankCode))
                    {
                        BankAccount = bankAccount;
                        BankCode = e.ToString();
                    }
                });
            }
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + Sum  +
                SuccessReturnUrl + FailReturnUrl + PayType + SubledgerList, SumapayConfig.Key);
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
