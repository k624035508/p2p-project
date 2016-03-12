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
    public class WbRechargeReqMsg : FrontEndReqMsg
    {
        public string Sum { get; set; }//充值金额
        public string BankCode { get; set; }//银行编码
        public string BankType { get; set; }//银行类型
        public string BankCardTypeFlag { get; set; }//借贷分离标识
        public string PayType { get; set; }
        public string SubledgerList { get; set; }//分账列表
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public string PassThrough { get; set; }//透传信息

        public WbRechargeReqMsg(int userId, string sum, string bankCode, string bankType,
            string payType, string subledgerList, string mainAccountType, string mainAccountCode,
            Action<string> callback, string bankCardTypeFlag = "0", string passThrough = "")
        {
            UserId = userId;
            Sum = sum;
            BankCode = bankCode;
            BankType = bankType;
            BankCardTypeFlag = bankCardTypeFlag;
            PayType = payType;
            SubledgerList = subledgerList;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            PassThrough = passThrough;
            Api = (int) Agp2pEnums.SumapayApiEnum.WeRec;
            ApiInterface = TestApiUrl + "user/webBankRecharge_toRecharge";
            RequestId = Agp2pEnums.SumapayApiEnum.WeRec.ToString().ToUpper() + Utils.GetOrderNumberLonger();
            CallBack = callback;
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return
                hmac.ComputeHashToBase64String(RequestId + MerchantCode + UserId + Sum + BankCode +
                SuccessReturnUrl + FailReturnUrl + BankCardTypeFlag + PayType + SubledgerList);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"sum", Sum},
                {"bankCode", BankCode},
                {"bankcardTypeFlag", BankCardTypeFlag},
                {"payType", PayType},
                {"subledgerList", SubledgerList},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(MainAccountType)) sd.Add("mainAccountType", MainAccountType);
            if (!string.IsNullOrEmpty(MainAccountCode)) sd.Add("mainAccountCode", MainAccountCode);
            if (!string.IsNullOrEmpty(PassThrough)) sd.Add("passThrough", PassThrough);

            return sd;
        }
    }
}
