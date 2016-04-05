using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人一键充值
    /// </summary>
    public class WhRechargeReqMsg : FrontEndReqMsg
    {
        public string Sum { get; set; }//充值金额
        public string PayType { get; set; }
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public string PassThrough { get; set; }//透传信息
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


        public WhRechargeReqMsg(int userId, string sum, string passThrough = "", string payType = "3",
            string mainAccountType = "", string mainAccountCode = "")
        {
            UserId = userId;
            Sum = sum;
            PayType = payType;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            PassThrough = passThrough;

            Api = (int) Agp2pEnums.SumapayApiEnum.WeRec;
            ApiInterface = SumapayConfig.TestApiUrl + "user/withholdingRecharge_toWithholdingRecharge";
            RequestId = Agp2pEnums.SumapayApiEnum.WeRec.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + Sum  +
                SuccessReturnUrl + FailReturnUrl  + PayType + SubledgerList, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"sum", Sum},
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
