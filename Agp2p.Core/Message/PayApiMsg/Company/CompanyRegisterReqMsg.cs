using System.Collections.Generic;
using System.Text;
using System.Web;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 企业开户请求
    /// </summary>
    public class CompanyRegisterReqMsg : UserRegisterReqMsg
    {
        public CompanyRegisterReqMsg(int userId)
        {
            UserId = userId;

            Api = (int) Agp2pEnums.SumapayApiEnum.CRegi;
            ApiInterface = SumapayConfig.ApiUrl + "businessUser/register_toRegister";
            RequestId = Agp2pEnums.SumapayApiEnum.CRegi.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }
    }
}
