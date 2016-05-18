using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 企业存管账户还款普通/集合项目
    /// </summary>
    public class CompanyAccountRepayReqMsg : AccountRepayReqMsg
    {
        public CompanyAccountRepayReqMsg(int userId, int projectCode, string sum, string rechargeUrl, bool collective = false, string giftFlag = "", string subledgerList = "")
        {
            UserId = userId;
            ProjectCode = projectCode;
            Sum = sum;
            SubledgerList = subledgerList;
            RechargeUrl = rechargeUrl;
            GiftFlag = giftFlag;

            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.CoRep : (int) Agp2pEnums.SumapayApiEnum.CaRep;
            ApiInterface = SumapayConfig.ApiUrl + (collective ? "businessUser/collectiveRepay_toRepayDetail" : "businessUser/repay_toRepayDetail");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }
    }
}
