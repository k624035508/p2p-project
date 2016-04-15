using System.Web;
using Agp2p.Common;

namespace Agp2p.Web.UI.Page
{
    public partial class register : Web.UI.BasePage
    {
        protected string action = string.Empty;
        protected string username = string.Empty;
        protected string invite_code = string.Empty;
        protected string result = string.Empty;

        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            action = DTRequest.GetQueryString("action");
            username = DTRequest.GetQueryString("username");
            invite_code = DTRequest.GetQueryString("invite_code");
            result = DTRequest.GetQueryString("result");
            username = Utils.DropHTML(username);
            //检查是否关闭会员注册服务
            if (action == "" && uconfig.regstatus == 0)
            {
                HttpContext.Current.Response.Redirect(linkurl("register") + "?action=close");
                return;
            }
            //Email验证
            if (action == "checkmail")
            {
                string strcode = DTRequest.GetQueryString("strcode");
                BLL.user_code bll = new BLL.user_code();
                Model.user_code model = bll.GetModel(strcode);
                if (model == null) //返回出错
                {
                    HttpContext.Current.Response.Redirect(linkurl("register") + "?action=checkerror");
                    return;
                }
                //修改申请码状态
                model.status = 1;
                bll.Update(model);
                //修改用户状态
                new BLL.users().UpdateField(model.user_id, "status=0");
            }
        }
    }
}
