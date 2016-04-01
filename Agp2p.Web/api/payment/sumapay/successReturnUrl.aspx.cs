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
                    Response.Write("开通托管账户成功！");
                    break;
            }
        }

    }
}
