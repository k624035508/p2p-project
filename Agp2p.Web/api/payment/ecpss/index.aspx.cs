using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Agp2p.Linq2SQL;
using Agp2p.Common;
using Agp2p.API.Payment.Ecpss;
using Agp2p.Core;

namespace Agp2p.Web.api.payment.ecpss
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string bankCode = DTRequest.GetFormString("bankcode");
            int user_id = Utils.StrToInt(DTRequest.GetFormString("user_id"), 0);
            string amount = DTRequest.GetFormString("amount");

            //创建充值订单
            var context = new Agp2pDataContext();
            var charge_order = context.Charge(user_id, decimal.Parse(amount), Agp2pEnums.PayApiTypeEnum.Ecpss);

            Service server = new Service();
            string sHtmlText = server.BuildFormHtml(charge_order.no_order, charge_order.create_time.ToString("yyyyMMddHHmmss"), amount, bankCode);
            Response.Write(sHtmlText);
        }
    }
}
