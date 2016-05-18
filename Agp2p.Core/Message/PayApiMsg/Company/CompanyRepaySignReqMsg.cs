using System;
using System.Collections.Generic;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using TinyMessenger;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 企业开通存管/银行账户自动还款请求
    /// </summary>
    public class CompanyRepaySignReqMsg : AutoRepaySignReqMsg
    {
        public CompanyRepaySignReqMsg(int userId, int projectCode, string repayLimit, string cycle = " ")
        {
            UserId = userId;
            ProjectCode = projectCode;
            RepayLimit = repayLimit;
            Cycle = cycle;

            Api = (int) Agp2pEnums.SumapayApiEnum.CcReO;
            ApiInterface = SumapayConfig.ApiUrl + "businessUser/autoAccountRepay_toAutoRepaySign";
            RequestId = Agp2pEnums.SumapayApiEnum.CcReO.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }
    }
}
