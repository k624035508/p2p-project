using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.SessionState;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.tools
{
    public class email_verify : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.TrySkipIisCustomErrors = true;

            Model.users model = HttpContext.Current.Session[DTKeys.SESSION_USER_INFO] as Model.users;
            if (model == null)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = "登录超时" }));
                return;
            }

            var action = DTRequest.GetQueryString("action");
            if (action == "sendVerifyEmail")
            {
                var email = DTRequest.GetQueryString("email");
                SendVerifyEmail(model.id, email, (i, s) =>
                {
                    httpContext.Response.StatusCode = i;
                    httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = s }));
                });
            }
            else if (action == "verifyEmail")
            {
                var codeFromEmail = DTRequest.GetQueryString("code");

                var cachedCode = (string)SessionHelper.Get("verifying_email_code");
                var sendVerifyMailAt = (DateTime?)SessionHelper.Get("last_send_verifying_mail_at");
                if (sendVerifyMailAt != null
                    && DateTime.Now.Subtract(sendVerifyMailAt.Value).TotalMinutes < SessionHelper.GetSessionTimeout()
                    && !string.IsNullOrWhiteSpace(cachedCode) && codeFromEmail == cachedCode)
                {
                    var context = new Agp2pDataContext();
                    var dtUsers = context.dt_users.Single(u => u.id == model.id);
                    dtUsers.email = model.email = SessionHelper.Get<string>("verifying_email");
                    context.SubmitChanges();
                    SessionHelper.Remove("verifying_email");
                    SessionHelper.Remove("last_send_verifying_mail_at");
                    SessionHelper.Remove("verifying_email_code");
                    httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = "邮箱绑定成功" }));
                }
                else
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = "邮箱验证码已失效" }));
                }
            }
            else
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = "参数不正确" }));
            }
        }

        private void SendVerifyEmail(int userId, string email, Action<int, string> callback)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(email))
            {
                callback((int) HttpStatusCode.BadRequest, "缺少参数");
                return;
            }
            // verifying_email : string | last_send_verifying_mail_at : DateTime? | verifying_email_code : string
            // 限制发送的时间间隔
            var lastSendVerifyingMailAt = (DateTime?)SessionHelper.Get("last_send_verifying_mail_at");
            if (lastSendVerifyingMailAt != null && DateTime.Now.Subtract(lastSendVerifyingMailAt.Value).TotalSeconds < 60)
            {
                callback(429, "发送邮件间隔为 60 秒，您刚才已经提交过啦，休息一下再来吧！");
                return;
            }
            var context = new Agp2pDataContext();
            // 检查是否有其他用户验证过此邮箱
            var count = context.dt_users.Count(u => u.email == email);
            if (count != 0)
            {
                callback((int)HttpStatusCode.Conflict, "这个电子邮箱已经被其他用户绑定了");
                return;
            }

            var strCode = Utils.GetCheckCode(20);
            SessionHelper.Set("verifying_email", email);
            SessionHelper.Set("last_send_verifying_mail_at", DateTime.Now);
            SessionHelper.Set("verifying_email_code", strCode);

            var user = context.dt_users.Single(u => u.id == userId);

            //获得邮件内容
            var mailModel = new mail_template().GetModel("emailverify");
            if (mailModel == null)
            {
                callback((int) HttpStatusCode.Gone, "邮件发送失败，邮件模板内容不存在！");
                return;
            }
            //替换模板内容
            var siteConfig = new siteconfig().loadConfig();
            var titletxt = mailModel.maill_title
                .Replace("{webname}", siteConfig.webname)
                .Replace("{username}", user.user_name);
            var bodytxt = mailModel.content
                .Replace("{webname}", siteConfig.webname)
                .Replace("{webtel}", siteConfig.webtel)
                .Replace("{username}", user.user_name)
                .Replace("{valid}", SessionHelper.GetSessionTimeout().ToString())
                .Replace("{linkurl}",
                    "http://" + HttpContext.Current.Request.Url.Authority.ToLower() +
                    "/user/center/index.html#/safe?action=verifyEmail&code=" + strCode);

            try
            {
                DTMail.sendMail(siteConfig.emailsmtp,
                    siteConfig.emailusername,
                    DESEncrypt.Decrypt(siteConfig.emailpassword),
                    siteConfig.emailnickname,
                    siteConfig.emailfrom,
                    email,
                    titletxt, bodytxt); //发送邮件
                callback((int) HttpStatusCode.OK, "邮件发送成功，请查收！");
            }
            catch(Exception e)
            {
                callback((int) HttpStatusCode.InternalServerError, "邮件发送失败，请联系本站管理员！");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}