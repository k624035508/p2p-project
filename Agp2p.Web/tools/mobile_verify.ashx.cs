using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.SessionState;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Agp2p.Model;
using Agp2p.Web.UI;
using Newtonsoft.Json;
using siteconfig = Agp2p.BLL.siteconfig;
using sms_template = Agp2p.BLL.sms_template;

namespace Agp2p.Web.tools
{
    public class mobile_verify : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.TrySkipIisCustomErrors = true;

            if (!BasePage.IsUserLogin())
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = "登录超时，请重新登陆" }));
                return;
            }

            var nvc = Utils.ParceQueryString();
            var action = nvc["act"];
            Action<int, string> defCallback = (i, s) =>
            {
                httpContext.Response.StatusCode = i;
                httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = s }));
            };
            switch (action)
            {
                case "sendCodeForBindMobile":
                    var model = HttpContext.Current.Session[DTKeys.SESSION_USER_INFO] as users;
                    if (model == null)
                    {
                        defCallback((int)HttpStatusCode.Unauthorized, "登录超时，请重新登陆");
                        return;
                    }
                    var mobile = nvc["mobile"];
                    var picCode = nvc["picCode"];
                    sendMobileVerifyCode(picCode, mobile, 60, true, defCallback);
                    break;
                case "verifyForBindMobile":
                    var verifyCode = nvc["verifyCode"];
                    SetMobile(verifyCode, defCallback);
                    break;
                case "sendCodeForResetPwd":
                    mobile = nvc["mobile"];

                    var context = new Agp2pDataContext();
                    if (!context.dt_users.Any(u => u.mobile == mobile))
                    {
                        defCallback((int) HttpStatusCode.NotFound, "不存在设有该电话号码的用户");
                        return;
                    }
                    sendMobileVerifyCode(nvc["picCode"], mobile, 60 * 2, false, defCallback);
                    break;
                case "verifyForResetPwd":
                    verifyCode = nvc["verifyCode"];
                    var pwd = DTRequest.GetFormString("newPwd");
                    SetPasswordByMobile(verifyCode, pwd, defCallback);
                    break;
                default:
                    defCallback((int)HttpStatusCode.BadRequest, "act 参数无效");
                    break;
            }
        }

        private void SetPasswordByMobile(string verifyCode, string pwd, Action<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(verifyCode) || string.IsNullOrWhiteSpace(pwd))
            {
                callback((int)HttpStatusCode.BadRequest, "参数不正确");
                return;
            }
            var cachedCode = (string)SessionHelper.Get("mobile_verify_code");
            if (!string.IsNullOrWhiteSpace(cachedCode) && verifyCode == cachedCode)
            {
                var context = new Agp2pDataContext();
                try
                {
                    var user = context.dt_users.SingleOrDefault(u => u.mobile == (string) SessionHelper.Get("verifying_mobile"));
                    if (user == null)
                    {
                        callback((int) HttpStatusCode.NotFound, "不存在设有该电话号码的用户");
                        return;
                    }
                    user.password = DESEncrypt.Encrypt(pwd, user.salt);
                    context.SubmitChanges();
                    SessionHelper.Remove("mobile_verify_code");
                    callback((int)HttpStatusCode.OK, "密码重置成功");
                }
                catch (Exception ex)
                {
                    callback((int)HttpStatusCode.OK, "密码重置失败，有多个用户的电话号码同时匹配该号码，请联系客服");
                }
            }
            else
            {
                callback((int)HttpStatusCode.PreconditionFailed, "电话验证码不正确或已失效");
            }
        }

        private void SetMobile(string verifyCodeFromInput, Action<int, string> callback)
        {
            var model = HttpContext.Current.Session[DTKeys.SESSION_USER_INFO] as users;
            if (model == null)
            {
                callback((int)HttpStatusCode.Unauthorized, "登录超时，请重新登陆");
                return;
            }
            if (string.IsNullOrWhiteSpace(verifyCodeFromInput))
            {
                callback((int)HttpStatusCode.BadRequest, "参数不正确");
                return;
            }
            var cachedCode = (string)SessionHelper.Get("mobile_verify_code");
            if (!string.IsNullOrWhiteSpace(cachedCode) && verifyCodeFromInput == cachedCode)
            {
                var context = new Agp2pDataContext();
                var dtUsers = context.dt_users.Single(u => u.id == model.id);
                dtUsers.mobile = (string)SessionHelper.Get("verifying_mobile");
                context.SubmitChanges();
                SessionHelper.Remove("mobile_verify_code");
                callback((int) HttpStatusCode.OK, "电话设置成功");
            }
            else
            {
                callback((int) HttpStatusCode.PreconditionFailed, "电话验证码不正确或已失效");
            }
        }

        private void sendMobileVerifyCode(string picCode, string mobile, int sendSecondsSpan, bool checkConflict, Action<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(mobile))
            {
                callback((int) HttpStatusCode.Unauthorized, "参数不正确");
                return;
            }
            // verifying_mobile : string | last_send_verifying_sms_at : DateTime? | mobile_verify_code : string

            // 限制发送时间间隔
            var lastSendVerifyingSMSAt = (DateTime?)SessionHelper.Get("last_send_verifying_sms_at");
            if (lastSendVerifyingSMSAt != null && DateTime.Now.Subtract(lastSendVerifyingSMSAt.Value).TotalSeconds < sendSecondsSpan)
            {
                callback(429, "发送短信的间隔为 " + sendSecondsSpan + " 秒，您刚才已经发送过啦，休息一下再来吧！");
                return;
            }

            // 验证图形验证码
            if (string.IsNullOrWhiteSpace(picCode) || !string.Equals((string)HttpContext.Current.Session[DTKeys.SESSION_CODE], picCode, StringComparison.CurrentCultureIgnoreCase))
            {
                callback((int)HttpStatusCode.BadRequest, "图形验证码不正确");
                return;
            }
            HttpContext.Current.Session[DTKeys.SESSION_CODE] = null;

            // 判断电话是否已经被验证过了
            var context = new Agp2pDataContext();
            if (checkConflict && context.dt_users.Count(u => u.mobile == mobile) != 0)
            {
                callback((int) HttpStatusCode.Conflict, "这个电话号码已经被其他用户绑定了");
                return;
            }

            var strCode = Utils.Number(4);
            SessionHelper.Set("verifying_mobile", mobile);
            SessionHelper.Set("last_send_verifying_sms_at", DateTime.Now);
            SessionHelper.Set("mobile_verify_code", strCode);

            //获得短信模版内容
            var smsModel = new sms_template().GetModel("mobile_verify_code");
            if (smsModel == null)
            {
                callback((int) HttpStatusCode.Gone, "短信发送失败，短信模板不存在！");
                return;
            }
            //替换模板内容
            var siteConfig = new siteconfig().loadConfig();
            var msgContent = smsModel.content
                .Replace("{webname}", siteConfig.webname)
                .Replace("{webtel}", siteConfig.webtel)
                .Replace("{code}", strCode)
                .Replace("{valid}", SessionHelper.GetSessionTimeout().ToString());

            try
            {
                //发送短信
                var tipMsg = string.Empty;
                bool result = false;
                /*if (Utils.IsDebugging())
                {
                    Debug.WriteLine("向手机 {0} 发送短信：{1}", mobile, msgContent);
                    result = true;
                }
                else*/
                result = SMSHelper.SendSmsCode(mobile, msgContent, out tipMsg);

                if (result)
                {
                    callback((int)HttpStatusCode.OK, "短信发送成功，请查收！");
                }
                else
                {
                    callback((int)HttpStatusCode.InternalServerError, tipMsg);
                }
            }
            catch
            {
                callback((int)HttpStatusCode.InternalServerError, "短信发送失败，请联系本站管理员！");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}