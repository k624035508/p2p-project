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
                    case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                        Response.Write("自动投标续约失败！");
                        break;
                    //个人自动投标解约
                    case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                        Response.Write("自动投标解约失败！");
                        break;
                    //个人自动账户/银行还款开通
                    case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                    case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                        Response.Write("个人自动账户/银行还款开通失败！");
                        break;
                    //个人自动还款取消
                    case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                        Response.Write("个人自动账户/银行还款取消失败！");
                        break;
                    //个人网银/一键充值
                    case (int)Agp2pEnums.SumapayApiEnum.WeRec:
                    case (int)Agp2pEnums.SumapayApiEnum.WhRec:
                        Response.Write("个人充值失败！");
                        break;
                    //个人提现
                    case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                        Response.Write("个人提现失败！");
                        break;
                }
            }
        }
    }
}
