using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.SessionState;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.tools
{
    public class trade_pwd : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.TrySkipIisCustomErrors = true;

            Model.users model = HttpContext.Current.Session[DTKeys.SESSION_USER_INFO] as Model.users;
            if (model == null)
            {
                httpContext.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = "登录超时" }));
                return;
            }

            string act = DTRequest.GetFormString("action");
            if (act == "set")
            {
                string tradePwd = DTRequest.GetFormString("tradepwdVal");
                setTradePassword(model.id, tradePwd, (i, s) =>
                {
                    httpContext.Response.StatusCode = i;
                    httpContext.Response.Write(JsonConvert.SerializeObject(new {msg = s}));
                });
            }
            else if (act == "mod")
            {
                string oldTradePwd = DTRequest.GetFormString("oldtpwdVal");
                string newTradePwd = DTRequest.GetFormString("newtpwdVal");
                modTradePassword(model.id, oldTradePwd, newTradePwd, (i, s) =>
                {
                    httpContext.Response.StatusCode = i;
                    httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = s }));
                });
            }
            else if (act == "reset") // 生成 6 位随机密码发送至手机
            {
                var newTradePwd = Utils.Number(6);
                var context = new Agp2pDataContext();
                var dtUser = context.dt_users.Single(u => u.id == model.id);
                sendNewTradePwd(dtUser.mobile, newTradePwd, (i, s) =>
                {
                    if (i == (int) HttpStatusCode.OK)
                    {
                        dtUser.pay_password = Utils.MD5(newTradePwd);
                        context.SubmitChanges();
                    }
                    httpContext.Response.StatusCode = i;
                    httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = s }));
                });
            }
            else
            {
                httpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                httpContext.Response.Write(JsonConvert.SerializeObject(new { msg = "act 参数不正确" }));
            }
        }

        private void sendNewTradePwd(string mobile, string newTradePwd, Action<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(mobile))
            {
                callback((int)HttpStatusCode.PreconditionFailed, "请先验证手机号");
                return;
            }
            // last_send_ran_trade_pwd_at : DateTime?
            // 限制发送时间间隔
            var lastSendVerifyingSMSAt = (DateTime?)SessionHelper.Get("last_send_ran_trade_pwd_at");
            if (lastSendVerifyingSMSAt != null && DateTime.Now.Subtract(lastSendVerifyingSMSAt.Value).TotalSeconds < 600)
            {
                callback(429, "发送重置交易密码的短信的间隔为 10 分钟，您刚才已经发送过啦，休息一下再来吧！");
                return;
            }
            SessionHelper.Set("last_send_ran_trade_pwd_at", DateTime.Now);

            //获得短信模版内容
            var smsModel = new sms_template().GetModel("reset_trade_pwd");
            if (smsModel == null)
            {
                callback((int)HttpStatusCode.Gone, "短信发送失败，短信模板不存在！");
                return;
            }
            //替换模板内容
            var siteConfig = new BLL.siteconfig().loadConfig();
            var msgContent = smsModel.content.Replace("{webtel}", siteConfig.webtel).Replace("{code}", newTradePwd);

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

        protected void setTradePassword(int userId, string tradePwd, Action<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(tradePwd))
            {
                callback((int) HttpStatusCode.LengthRequired, "交易密码不能为空");
                return;
            }
            try
            {
                //为了能查询到最新的用户信息，必须查询最新的用户资料
                var context = new Agp2pDataContext();
                var user = context.dt_users.Single(u => u.id == userId);
                if (!string.IsNullOrWhiteSpace(user.pay_password)) // 原先已经设置过了就不能再设置，只能修改
                {
                    callback((int)HttpStatusCode.Unauthorized, "你已经设置过交易密码了，不能再设置；只能输入旧密码再修改");
                    return;
                }
                user.pay_password = Utils.MD5(tradePwd);
                context.SubmitChanges();
                callback((int) HttpStatusCode.OK, "设置交易密码成功，请记住新密码");
            }
            catch (Exception ex)
            {
                callback((int) HttpStatusCode.InternalServerError, "设置交易密码失败，请联系客服");
            }
        }

        protected void modTradePassword(int userId, string oldTradePwd, string newTradePwd, Action<int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(newTradePwd))
            {
                callback((int) HttpStatusCode.LengthRequired, "交易密码不能为空");
                return;
            }
            try
            {
                var context = new Agp2pDataContext();
                var user = context.dt_users.Single(u => u.id == userId);
                if (Utils.MD5(oldTradePwd) != user.pay_password)
                {
                    callback((int)HttpStatusCode.OK, "旧交易密码错误");
                    return;
                }
                user.pay_password = Utils.MD5(newTradePwd);
                context.SubmitChanges();
                callback((int)HttpStatusCode.OK, "设置交易密码成功，请记住新密码");
            }
            catch (Exception ex)
            {
                callback((int)HttpStatusCode.InternalServerError, "设置交易密码失败，请联系客服");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}