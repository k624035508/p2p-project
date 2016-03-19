using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人网银充值
    /// </summary>
    public class WebRechargeReqMsg : FrontEndReqMsg
    {
        public string Sum { get; set; }//充值金额
        public string BankCode { get; set; }//银行编码
        public string BankCardTypeFlag { get; set; }//借贷分离标识
        public string PayType { get; set; }
        public string SubledgerList { get; set; }//分账列表
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public string PassThrough { get; set; }//透传信息

        public WebRechargeReqMsg(int userId, string sum, string bankCode, string subledgerList, 
            string payType = "2", string mainAccountType = "", string mainAccountCode = "",
            string bankCardTypeFlag = "0", string passThrough = "")
        {
            UserId = userId;
            Sum = sum;
            BankCode = bankCode;
            BankCardTypeFlag = bankCardTypeFlag;
            PayType = payType;
            SubledgerList = subledgerList;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            PassThrough = passThrough;

            Api = (int) Agp2pEnums.SumapayApiEnum.WeRec;
            ApiInterface = SumapayConfig.TestApiUrl + "user/webBankRecharge_toRecharge";
            RequestId = Agp2pEnums.SumapayApiEnum.WeRec.ToString().ToUpper() + Utils.GetOrderNumberLonger();
            SuccessReturnUrl = "";
            FailReturnUrl = "";
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(SumapayConfig.Key);
            return
                hmac.ComputeHashToBase64String(RequestId + SumapayConfig.MerchantCode + UserId + Sum + BankCode +
                SuccessReturnUrl + FailReturnUrl + BankCardTypeFlag + PayType + SubledgerList);
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
                {"bankcardTypeFlag", BankCardTypeFlag},
                {"payType", PayType},
                {"subledgerList", SubledgerList},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(MainAccountType)) sd.Add("mainAccountType", MainAccountType);
            if (!string.IsNullOrEmpty(MainAccountCode)) sd.Add("mainAccountCode", MainAccountCode);
            if (!string.IsNullOrEmpty(PassThrough)) sd.Add("passThrough", PassThrough);

            return sd;
        }
    }
}
