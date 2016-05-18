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
            if (RequestLog != null)
            {                
                 switch (RequestLog.api)
                {

                    //个人/企业 开户/激活
                    case (int)Agp2pEnums.SumapayApiEnum.URegi:
                    case (int)Agp2pEnums.SumapayApiEnum.Activ:
                    case (int)Agp2pEnums.SumapayApiEnum.CRegi:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/register.html?action=4&result=fail");
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.URegM:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/center/index.html");
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#autoTenderFail");
                        break;
                    //个人自动投标解约
                    case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#autoTenderCancelFail");
                        break;
                    //个人/企业 自动账户/银行还款开通
                    case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                    case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                    case (int)Agp2pEnums.SumapayApiEnum.CcReO:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#autoAccountFail");
                        break;
                    //个人/企业 自动还款取消
                    case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#autoAccountCancelFail");
                        break;
                    //个人/企业 网银/一键充值
                    case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                    case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                    case (int)Agp2pEnums.SumapayApiEnum.CeRec:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#rechargeFail");
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.WhReM:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/center/index.html");
                        break;
                    //个人/企业 提现
                    case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                    case (int)Agp2pEnums.SumapayApiEnum.Cdraw:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#withdrawFail");
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.WdraM:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/center/index.html");
                        break;
                    //投标
                    case (int)Agp2pEnums.SumapayApiEnum.MaBid:
                    case (int)Agp2pEnums.SumapayApiEnum.McBid:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#investedFail");
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.MaBiM:
                    case (int)Agp2pEnums.SumapayApiEnum.McBiM:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/myreceiveplan/myreceiveplan-1-0-0-0.html#projects");
                        break;
                    //个人/企业 还款
                    case (int)Agp2pEnums.SumapayApiEnum.MaRep:
                    case (int)Agp2pEnums.SumapayApiEnum.McRep:
                    case (int)Agp2pEnums.SumapayApiEnum.BaRep:
                    case (int)Agp2pEnums.SumapayApiEnum.BcRep:
                    case (int)Agp2pEnums.SumapayApiEnum.CaRep:
                    case (int)Agp2pEnums.SumapayApiEnum.CoRep:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#repayFail");
                        break;
                    //债权转让
                    case (int)Agp2pEnums.SumapayApiEnum.CreAs:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/fail_return.html#tranClaimFail");
                        break;
                    case (int)Agp2pEnums.SumapayApiEnum.CreAM:
                        Response.Redirect("http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "mobile/user/center/index.html");
                        break;
                }
            }
        }
    }
}
