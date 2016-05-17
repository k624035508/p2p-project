using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;


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
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public string PassThrough { get; set; }//透传信息
        public string BankType { get; set; }//银行类型 0：个人网银1：企业网银
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
                            inOrOut = "0",
                            sum = Sum
                        }
                    });
                }
                return subledgerList;
            }
            set { subledgerList = value; }
        }

        public WebRechargeReqMsg(int userId, string sum, string bankCode, string passThrough = "",
            string payType = "3", string mainAccountType = "", string mainAccountCode = "",
            string bankCardTypeFlag = "1")
        {
            UserId = userId;
            Sum = sum;
            BankCode = bankCode;
            BankCardTypeFlag = bankCardTypeFlag;
            PayType = payType;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            PassThrough = passThrough;

            Api = (int) Agp2pEnums.SumapayApiEnum.WeRec;
            ApiInterface = SumapayConfig.ApiUrl + "user/webBankRecharge_toRecharge";
            RequestId = Agp2pEnums.SumapayApiEnum.WeRec.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        protected WebRechargeReqMsg()
        {
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + Sum + BankCode +
                SuccessReturnUrl + FailReturnUrl + BankCardTypeFlag + PayType + SubledgerList, SumapayConfig.Key);
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
            if (!string.IsNullOrEmpty(BankType)) sd.Add("bankType", BankType);

            return sd;
        }
    }
}
