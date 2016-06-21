using System;
using System.Collections.Generic;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 跳转企业账户管理
    /// </summary>
    public class CompanyToAccountReqMsg : UserToAccountReqMsg
    {
        public CompanyToAccountReqMsg(int userId)
        {
            UserId = userId;

            Api = (int) Agp2pEnums.SumapayApiEnum.CAcco;
            ApiInterface = SumapayConfig.ApiUrl + "businessUser/accountManage_toAccountManage";
            RequestId = Agp2pEnums.SumapayApiEnum.CAcco.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }
    }
}
