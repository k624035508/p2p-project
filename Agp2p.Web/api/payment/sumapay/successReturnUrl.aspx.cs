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
                case (int)Agp2pEnums.SumapayApiEnum.URegM:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/center/index.html");
                    break;
                //个人自动投标续约
                case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#autoTender");
                    break;
                //个人自动投标解约
                case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#autoTenderCancel");
                    break;
                //个人自动账户/银行还款开通
                case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#autoAccount");
                    break;
                //个人自动还款取消
                case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#autoAccountCancel");
                    break;
                //个人网银/一键充值
                case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#recharge");
                    break;
                case (int)Agp2pEnums.SumapayApiEnum.WhReM:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/center/index.html");
                    break;
                //个人提现
                case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#withdraw");
                    break;
                case (int)Agp2pEnums.SumapayApiEnum.WdraM:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/center/index.html");
                    break;
                //投标
                case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                case (int)Agp2pEnums.SumapayApiEnum.McBid:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#invested");
                    break;
                case (int)Agp2pEnums.SumapayApiEnum.MaBiM:
                case (int)Agp2pEnums.SumapayApiEnum.McBiM:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/myreceiveplan/myreceiveplan-1-0-0-0.html#projects");
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
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/success_return.html#tranClaim");
                    break;
                case (int)Agp2pEnums.SumapayApiEnum.CreAM:
                    Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/center/index.html");
                    break;
            }
        }

    }
}
