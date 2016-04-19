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
                case (int)Agp2pEnums.SumapayMobileApiEnum.URegi:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/register.html?action=4&result=success");
                    break;
                //个人自动投标续约
                case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                   // Response.Write("自动投标续约成功！");
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#autoTender");
                    break;
                //个人自动投标解约
                case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                    // Response.Write("自动投标解约成功！");
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#autoTenderCancel");
                    break;
                //个人自动账户/银行还款开通
                case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                   // Response.Write("个人自动账户/银行还款开通成功！");
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#autoAccount");
                    break;
                //个人自动还款取消
                case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                    //  Response.Write("个人自动账户/银行还款取消成功！");
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#autoAccountCancel");
                    break;
                //个人网银/一键充值
                case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                case (int)Agp2pEnums.SumapayMobileApiEnum.WhRec:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#recharge");
                    break;
                //个人提现
                case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                case (int)Agp2pEnums.SumapayMobileApiEnum.Wdraw:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#withdraw");
                    break;
                //投标
                case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                case (int)Agp2pEnums.SumapayApiEnum.McBid:
                case (int)Agp2pEnums.SumapayMobileApiEnum.MaBid:
                case (int)Agp2pEnums.SumapayMobileApiEnum.McBid:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#invested");
                    break;
                //个人还款
                case (int)Agp2pEnums.SumapayApiEnum.MaRep:
                case (int)Agp2pEnums.SumapayApiEnum.McRep:
                case (int)Agp2pEnums.SumapayApiEnum.BaRep:
                case (int)Agp2pEnums.SumapayApiEnum.BcRep:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#repay");                    
                    break;
                //债权转让
                case (int)Agp2pEnums.SumapayApiEnum.CreAs:
                    // Response.Write("债权转让成功！");
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#tranClaim");
                    break;
                case (int)Agp2pEnums.SumapayMobileApiEnum.CreAs:
                    Response.Write("债权转让成功！");
                    break;
            }
        }

    }
}
