using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 企业取消自动还款请求
    /// </summary>
    public class CompanyAutoRepayCancelReqMsg : AutoRepayCancelReqMsg
    {
        public CompanyAutoRepayCancelReqMsg(int userId, int projectCode)
        {
            UserId = userId;
            ProjectCode = projectCode;

            Api = (int)Agp2pEnums.SumapayApiEnum.CancR;
            ApiInterface = SumapayConfig.ApiUrl + "businessUser/cancelAutoRepay_toCancelAutoRepay";
            RequestId = Agp2pEnums.SumapayApiEnum.CancR.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }
    }
}
