using System;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Linq2SQL;
using Agp2p.Web.UI;

namespace Agp2p.Web.api.payment.sumapay
{
    /// <summary>
    /// 所有第三方托管前台接口统一处理跳转
    /// </summary>
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BaseReqMsg reqMsg = null;
            dt_users user = null;
            int requestApi = DTRequest.GetQueryInt("api", 0);
            switch (requestApi)
            {
                //个人开户/激活
                case (int)Agp2pEnums.SumapayApiEnum.URegi:
                case (int)Agp2pEnums.SumapayApiEnum.Activ:
                    user = BasePage.GetUserInfoByLinq();
                    //TODO 在页面显示错误提示
                    if (user == null)
                    {
                        Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                        return;
                    }

                    if (string.IsNullOrEmpty(user.token))
                    {
                        Response.Write("{\"status\":0, \"msg\":\"请先进行实名验证！\"}");
                        return;
                    }
                    //调用托管平台实名验证接口
                    reqMsg = new UserRegisterReqMsg(user.id, user.mobile, user.real_name, user.id_card_number, user.token);
                    break;
                //个人自动投标续约
                case (int)Agp2pEnums.SumapayApiEnum.AtBid:
                    user = BasePage.GetUserInfoByLinq();
                    if (user == null)
                    {
                        Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                        return;
                    }
                    reqMsg = new AutoBidSignReqMsg(user.id);
                    break;
                //个人自动投标取消
                case (int)Agp2pEnums.SumapayApiEnum.ClBid:
                    user = BasePage.GetUserInfoByLinq();
                    if (user == null)
                    {
                        Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                        return;
                    }
                    reqMsg = new AutoBidSignReqMsg(user.id, true, user.protocolCode);
                    break;
                //个人自动账户/银行还款开通
                case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                    reqMsg = new AutoRepaySignReqMsg(DTRequest.GetQueryInt("userId", 0), DTRequest.GetQueryString("projectCode"), DTRequest.GetQueryString("repayLimit"), requestApi != (int)Agp2pEnums.SumapayApiEnum.AcReO);
                    break;
                default:
                    reqMsg = new BaseReqMsg();
                    break;
            }
            //生成、保存请求内容
            MessageBus.Main.Publish(reqMsg);
            //跳转丰付平台开通托管账户
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("GBK");
            Response.Write(reqMsg.RequestContent);
        }
    }
}
