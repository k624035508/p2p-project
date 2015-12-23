using Agp2p.Common;
using System;
using System.Web;

namespace Agp2p.Web.UI.Page
{
    public partial class login : Web.UI.BasePage
    {
        protected string turl = string.Empty;
        protected string code = string.Empty;//获取微信openid的code
        /// <summary>
        /// 重写父类的虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            Init += UserPage_Init;
        }

        /// <summary>
        /// OnInit事件,检查用户是否已经登录
        /// </summary>
        void UserPage_Init(object sender, EventArgs e)
        {
            turl = linkurl("usercenter", "index");
            code = DTRequest.GetQueryString("code");
            
            //if (HttpContext.Current.Request.Url != null && HttpContext.Current.Request.UrlReferrer != null)
            //{
            //    if (HttpContext.Current.Request.Url.ToString().ToLower() != HttpContext.Current.Request.UrlReferrer.ToString().ToLower() && !HttpContext.Current.Request.UrlReferrer.ToString().Contains("register"))
            //    {
            //        turl = HttpContext.Current.Request.UrlReferrer.ToString();
            //    }
            //}
            Utils.WriteCookie(DTKeys.COOKIE_URL_REFERRER, turl); //记住上一页面
            
            Model.users model = GetUserInfo();
            if (model != null)
            {
                //写入登录日志
                //new BLL.user_login_log().Add(model.id, model.user_name, "自动登录");
                //自动登录,跳转URL
                HttpContext.Current.Response.Redirect(turl);
            }
        }

    }
}
