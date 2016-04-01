using System;
using System.Web;
using Agp2p.Common;
using Agp2p.Web.UI.Page;

namespace Agp2p.Web.api.payment.sumapay
{
    public partial class failReturnUrl : noticeUrl
    {
        protected new void Page_Load(object sender, EventArgs e)
        {
            DoResponse(true);

            //TODO 根据请求类型显示不同的失败提示
            if (RequestLog != null)
            {
                switch (RequestLog.api)
                {
                    //个人开户/激活
                    case (int)Agp2pEnums.SumapayApiEnum.URegi:
                    case (int)Agp2pEnums.SumapayApiEnum.Activ:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/register.html?action=4&result=fail");
                        break;
                }
            }
        }
    }
}
