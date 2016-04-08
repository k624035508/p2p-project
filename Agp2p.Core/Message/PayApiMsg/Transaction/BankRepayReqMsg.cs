using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人协议还款（银行）普通/集合项目
    /// </summary>
    public class BankRepayReqMsg : FrontEndReqMsg
    {
        public string Sum { get; set; }//还款金额
        public string GiftFlag { get; set; }//红包标识
        public string ProjectDescription { get; set; }//项目描述
        public string PayType { get; set; }//手续费收取方式
        public bool Collective { get; set; }//集合项目标识
        /// <summary>
        /// 分账列表 当红包标识字段不为空时，此字段必输。当红包标识为 0 时， 则只对应记录一笔用户的出账明细；当红包标识为 1 时， 
        /// 分账列表内对应零笔或一笔用户的出账明细， 一笔商户的出账明细。用户需对应第三方用户标识，商 户需对应商户编码；
        /// 用户的出账金额和商户出账金额之和等于投标的总金额；用户出账时 bizFlag 为 0 正常业务（选输）；商户出账时 bizFlag 为 2 红包（必输），金额为红包金额
        /// </summary>
        public string SubledgerList { get; set; }

        public BankRepayReqMsg(int userId, string projectCode, string sum, bool collective = false, string projectDescription = "", string payType = "2", string giftFlag = "", string subledgerList = "")
        {
            UserId = userId;
            ProjectCode = projectCode;
            Sum = sum;
            PayType = payType;
            SubledgerList = subledgerList;
            ProjectDescription = projectDescription;
            GiftFlag = giftFlag;
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.BcRep : (int) Agp2pEnums.SumapayApiEnum.BaRep;
            ApiInterface = SumapayConfig.TestApiUrl + (collective ? "user/collectiveWithholdingRepay_toWithholdingRepay" : "user/withholdingRepay_toWithholdingRepay");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + ProjectCode + Sum  + PayType +
                SuccessReturnUrl + FailReturnUrl, SumapayConfig.Key);
        }

        public override SortedDictionary<string, string> GetSubmitPara()
        {
            var sd = new SortedDictionary<string, string>
            {
                {"requestId", RequestId},
                {"merchantCode", SumapayConfig.MerchantCode},
                {"userIdIdentity", UserId.ToString()},
                {"projectCode", ProjectCode},
                {"sum", Sum},
                {"payType", PayType},
                {"successReturnUrl", SuccessReturnUrl},
                {"failReturnUrl", FailReturnUrl},
                {"noticeUrl", SumapayConfig.NoticeUrl},
                {"signature", GetSignature()}
            };
            if (!string.IsNullOrEmpty(ProjectDescription)) sd.Add("projectDescription ", HttpUtility.UrlEncode(ProjectDescription, Encoding.GetEncoding("GBK")));
            if (!string.IsNullOrEmpty(GiftFlag)) sd.Add("giftFlag", GiftFlag);
            if (!string.IsNullOrEmpty(SubledgerList)) sd.Add("subledgerList", SubledgerList);

            return sd;
        }
    }
}
