using System;
using System.Web;
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
            Model.users model = HttpContext.Current.Session[DTKeys.SESSION_USER_INFO] as Model.users;
            if(model == null)
                throw new ArgumentNullException("用户信息为空！");

            string bankCode = DTRequest.GetQueryString("bankcode");
            string amount = DTRequest.GetQueryString("amount");

            decimal amountD = 0;
            if (!decimal.TryParse(amount, out amountD))
                throw new ArgumentNullException("请输入正确的金额！");

            //创建充值订单
            var context = new Agp2pDataContext();
            var charge_order = context.Charge(model.id, amountD,
                bankCode == "NOCARD" ? Agp2pEnums.PayApiTypeEnum.EcpssQ : Agp2pEnums.PayApiTypeEnum.Ecpss);
            //跳转到汇潮支付页面
            Service server = new Service(bankCode == "NOCARD");
            string sHtmlText = server.BuildFormHtml(charge_order.no_order, charge_order.create_time.ToString("yyyyMMddHHmmss"), amount, bankCode);
            Response.Write(sHtmlText);
        }
    }
}
