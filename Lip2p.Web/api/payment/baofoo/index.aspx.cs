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
using Lip2p.API.Payment.Baofoo;
using Lip2p.Linq2SQL;
using Lip2p.BLL;
using Lip2p.Common;
using Lip2p.Core;

namespace Lip2p.Web.api.payment.baofoo
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string strPayID = Request.Params["PayID"];

            int user_id = 1;
            string amount = DTRequest.GetFormString("OrderMoney");
            //创建充值订单
            var context = new Lip2pDataContext();
            var charge_order = context.Charge(user_id, decimal.Parse(amount), Lip2pEnums.PayApiTypeEnum.Baofoo);

            Service server = new Service();
            string sHtmlText = server.BuildFormHtml(charge_order.no_order, charge_order.create_time.ToString("yyyyMMddHHmmss"), amount, strPayID);
            Response.Write(sHtmlText);
        }
    }
}
