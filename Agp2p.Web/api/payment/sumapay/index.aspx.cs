using System;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
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
            int requestApi = DTRequest.GetQueryInt("api", 0);
            switch (requestApi)
            {
                //个人开户/激活
                case (int)Agp2pEnums.SumapayApiEnum.URegi:
                case (int)Agp2pEnums.SumapayApiEnum.Activ:
                    var model = BasePage.GetUserInfoByLinq();
                    //TODO 在失败页面显示错误提示
                    if (model == null)
                    {
                        Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                        return;
                    }

                    if (string.IsNullOrEmpty(model.token))
                    {
                        Response.Write("{\"status\":0, \"msg\":\"请先进行实名验证！\"}");
                        return;
                    }
                    //调用托管平台实名验证接口
                    reqMsg = new UserRegisterReqMsg(model.id, model.mobile, model.real_name, model.id_card_number, model.token);
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
