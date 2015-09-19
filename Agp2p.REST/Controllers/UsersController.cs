using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Results;
using Lip2p.Common;
using Lip2p.Core;
using Lip2p.Core.Message;
using Lip2p.Linq2SQL;
using Lip2p.REST.Common;
using Lip2p.REST.Filters;
using Lip2p.Common;
using Lip2p.REST.Bindings;
using Lip2p.REST.Models;

namespace Lip2p.REST.Controllers
{
    [RoutePrefix("v1/users")]
    public class UsersController : ApiController
    {
        [Route("login")] // v1/users/login
        [HttpPost]
        public IHttpActionResult Login([FromFormData]string txtUserName, [FromFormData]string txtPassword, [FromFormData]string openId)
        {
            //检查用户名密码
            if (string.IsNullOrEmpty(txtUserName))
            {
                return BadRequest("温馨提示：请输入用户名！");
            }
            if (string.IsNullOrEmpty(txtPassword))
            {
                return BadRequest("温馨提示：请输入密码！");
            }

            var context = new Lip2pDataContext();
            var user = context.dt_users.SingleOrDefault(u => u.user_name == txtUserName);             
            if (user == null)
                return BadRequest("错误提示：未找到该用户！");

            //2015-4-21 minson 判断是否天润旧用户，如果是用md5验证旧密码          
            var password = txtPassword;
            if (string.IsNullOrEmpty(user.password))
            {
                if (user.salt.Equals(Utils.MD5(password)))
                {
                    try
                    {
                        //使用当前系统加密方法更新密码
                        var salt = Utils.GetCheckCode(6);
                        password = DESEncrypt.Encrypt(password, salt);
                        user.salt = salt;
                        user.password = password;
                        if (!string.IsNullOrEmpty(openId)) user.openid = openId;
                        context.SubmitChanges();
                        return Ok(user);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("错误提示：旧用户匹配密码失败！");
                    }
                }                    
            }
            else
            {
                //把明文进行加密
                password = DESEncrypt.Encrypt(password, user.salt);
                user = context.dt_users.SingleOrDefault(u => u.user_name == txtUserName && u.password == password);
                if (user != null)
                {
                    if(!string.IsNullOrEmpty(openId) && string.IsNullOrEmpty(user.openid))
                    {
                        user.openid = openId;
                        context.SubmitChanges();
                    }
                    return Ok(new User{ Id = user.id, UserName = user.user_name });
                }
            }
            return BadRequest("错误提示：用户名或密码错误！");
        }

        /// <summary>
        /// 根据微信openid查询用户信息
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        [Route("getUser/{openId}")] // v1/users/getUser/*
        public IHttpActionResult GetUserByOpenId([FromUri]string openId)
        {
            var user = new Lip2pDataContext().dt_users.SingleOrDefault(u => u.openid == openId);
            if (user == null)
                return BadRequest("没有找到微信号所关联的用户信息.");
            else
            {
                return Ok(new User { Id = user.id, UserName = user.user_name });
            }
        }


        static readonly Regex MobileRegex = new Regex(@"^1\d{10}$", RegexOptions.IgnoreCase);

        [Route("reg-verify/{mobile}")] // v1/users/reg-verify/1*
        [HttpGet]
        [ThrottleByParam(Name = "SendRegistVerifyCodeThrottle", ParamName = "mobile", Message = "发送验证码的时间间隔为{n}秒，请稍后再试", Seconds = 120, ThrottleSuccessRequestOnly = true)]
        public IHttpActionResult SendRegistVerifyCode([FromUri]string mobile)
        {
            if (!MobileRegex.IsMatch(mobile))
                return BadRequest("手机号码的格式不正确");
            var context = new Lip2pDataContext();
            if (context.dt_users.Any(u => u.mobile == mobile))
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "此手机已被注册"));
            var template = context.dt_sms_template.SingleOrDefault(te => te.call_index == "usercode");

            return SendVerifyCode(mobile, template == null ? null : template.content, "Reg-verify");
        }

        private IHttpActionResult SendVerifyCode(string mobile, string templateContent, string reason)
        {
            templateContent = templateContent ?? "您的验证码为：{code}，{valid}分钟内有效。";

            var strcode = Utils.Number(4); //随机验证码
            var siteConfig = ConfigLoader.loadSiteConfig();
            var regSmsExpired = ConfigLoader.loadUserConfig().regsmsexpired;
            //替换标签
            var msgContent = templateContent
                    .Replace("{webname}", siteConfig.webname)
                    .Replace("{weburl}", siteConfig.weburl)
                    .Replace("{webtel}", siteConfig.webtel)
                    .Replace("{code}", strcode)
                    .Replace("{valid}", regSmsExpired.ToString());
            //发送短信
            string tipMsg;
            if (Utils.IsDebugging())
            {
                Debug.WriteLine(msgContent);
            }
            else if (!SMSHelper.SendSmsCode(mobile, msgContent, out tipMsg))
            {
                return InternalServerError(new Exception("发送失败，" + tipMsg));
            }

            MemoryCache.Default.Set(reason + ":" + mobile, strcode, DateTime.Now.AddMinutes(regSmsExpired));
            return Ok("发送短信验证码成功");
        }

        static readonly Regex PasswordRegex = new Regex(@"^\w{6,16}$", RegexOptions.IgnoreCase);
        [Route("register")] // v1/users/register
        [HttpPost]
        public IHttpActionResult Register([FromFormData]string mobile, [FromFormData]string password, [FromFormData]string verifyCode, [FromFormData]string inviter)
        {
            if (!MobileRegex.IsMatch(mobile))
                return BadRequest("手机号码的格式不正确");
            if (!PasswordRegex.IsMatch(password))
                return BadRequest("请输入6-16数字或字母的密码");

            var context = new Lip2pDataContext();
            if (context.dt_users.Any(u => u.mobile == mobile))
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "此手机已被注册"));

            var strcode = (string) MemoryCache.Default["Reg-verify:" + mobile];
            if (strcode != verifyCode)
            {
                Debug.WriteLine("Verify: {0}, infact: {1}", strcode, verifyCode);
                return BadRequest("短信验证码错误");
            }

            var defaultGroup = context.dt_user_groups.FirstOrDefault(g => g.is_default == 1);

            if (defaultGroup == null)
                return InternalServerError(new Exception("用户尚未分组，请联系网站管理员"));

            var salt = Utils.GetCheckCode(6);
            var newUser = new dt_users
            {
                group_id = defaultGroup.id,
                user_name = mobile,
                salt = salt, //获得6位的salt加密字符串
                password = DESEncrypt.Encrypt(password, salt),
                mobile = mobile,
                reg_ip = Request.GetClientIp(),
                reg_time = DateTime.Now,
                status = 0,
            };
            context.dt_users.InsertOnSubmit(newUser);

            // 查出邀请人
            if (!string.IsNullOrWhiteSpace(inviter))
            {
                var inviteUser = context.dt_users.SingleOrDefault(u => u.mobile == inviter || u.user_name == inviter);
                if (inviteUser == null)
                {
                    var code = context.dt_user_code.SingleOrDefault(c => c.str_code == inviter && c.type == DTEnums.CodeEnum.Register.ToString());
                    if (code != null)
                    {
                        inviteUser = code.dt_users;
                    }
                }
                if (inviteUser != null)
                {
                    var liInvitations = new li_invitations
                    {
                        dt_users = newUser,
                        dt_users1 = inviteUser
                    };
                    context.li_invitations.InsertOnSubmit(liInvitations);
                    // 会员部功能，被推荐人自动归组
                    if (inviteUser.li_user_group_servers != null)
                    {
                        newUser.group_id = inviteUser.li_user_group_servers.group_id;
                    }
                }
            }

            try
            {
                context.SubmitChanges();
                // 广播新用户注册消息
                MessageBus.Main.PublishAsync(new NewUserCreatedMsg(newUser.id, newUser.reg_time.Value));
                return Ok("注册成功");
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("注册失败：" + ex.Message));
            }
        }

        [Route("resetpwd-verify/{mobile}")] // v1/users/resetpwd-verify/1*
        [HttpGet]
        [ThrottleByParam(Name = "SendResetPwdVerifyCodeThrottle", ParamName = "mobile", Message = "发送验证码的时间间隔为{n}秒，请稍后再试", Seconds = 120, ThrottleSuccessRequestOnly = true)]
        public IHttpActionResult SendForgotPasswordVerifyShortMsg(string mobile)
        {
            if (!MobileRegex.IsMatch(mobile))
                return BadRequest("手机号码的格式不正确");
            var context = new Lip2pDataContext();
            if (!context.dt_users.Any(u => u.mobile == mobile))
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "错误提示：不存在手机号码为 " + mobile + " 用户"));
            var template = context.dt_sms_template.SingleOrDefault(te => te.call_index == "usercode");

            return SendVerifyCode(mobile, template == null ? null : template.content, "Resetpwd-verify");
        }

        [Route("change-password")] // v1/users/change-password
        [HttpPost]
        public IHttpActionResult ChangePassword([FromFormData] string mobile, [FromFormData] string verifyCode, [FromFormData] string password)
        {
            if (!MobileRegex.IsMatch(mobile))
                return BadRequest("手机号码的格式不正确");
            if (!PasswordRegex.IsMatch(password))
                return BadRequest("请输入6-16数字或字母的密码");

            var context = new Lip2pDataContext();
            var user = context.dt_users.SingleOrDefault(u => u.mobile == mobile);

            if (user == null)
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "错误提示：不存在手机号码为 " + mobile + " 用户"));

            var strcode = (string)MemoryCache.Default["Resetpwd-verify:" + mobile];
            if (strcode != verifyCode)
                return BadRequest("短信验证码错误");

            user.salt = Utils.GetCheckCode(6);
            user.password = DESEncrypt.Encrypt(password, user.salt);
            context.SubmitChanges();
            return Ok("密码修改成功");
        }
    }
}
