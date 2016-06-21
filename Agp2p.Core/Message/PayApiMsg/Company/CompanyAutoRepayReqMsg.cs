using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 企业自动还款普通/集合项目
    /// </summary>
    public class CompanyAutoRepayReqMsg: AutoRepayReqMsg
    {
        public CompanyAutoRepayReqMsg(int userId, int projectCode, string sum, bool collective = false, string fayType = "2")
        {
            UserId = userId;
            ProjectCode = projectCode;
            Sum = sum;
            FeeType = fayType;
            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.CbRep : (int) Agp2pEnums.SumapayApiEnum.CcRep;
            ApiInterface = SumapayConfig.ApiUrl + (collective ? "main/CollectiveFinance_bizRepay" : "main/TransactionForBizUser_bizRepay");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }
    }
}
