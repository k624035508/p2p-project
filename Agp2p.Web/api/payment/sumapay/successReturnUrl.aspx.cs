using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;
using Agp2p.Web.admin.repayment;

namespace Agp2p.Web.api.payment.sumapay
{
    public partial class successReturnUrl : noticeUrl
    {
        protected new void Page_Load(object sender, EventArgs e)
        {
            DoResponse(true);

            switch (RequestLog.api)
            {
                //个人开户/激活
                case (int)Agp2pEnums.SumapayApiEnum.URegi:
                case (int)Agp2pEnums.SumapayApiEnum.Activ:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/register.html?action=4&result=success");
                    break;
                //个人自动投标续约
                case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                    Response.Write("自动投标续约成功！");
                    break;
                //个人自动投标解约
                case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                    Response.Write("自动投标解约成功！");
                    break;
            }
        }

    }
}
