using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.UserPointMsg;
using Agp2p.Linq2SQL;
using Senparc.Weixin.MP.AdvancedAPIs;

namespace Agp2p.Core.InitLogic
{
    class UserLogin
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<UserLoginMsg>(m => NewUser(m.UserId, m.Remember, m.Code, m.DoLogin));
        }

        /// <summary>
        /// 用户登陆回调
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="regTime"></param>
        private static void NewUser(int userId, bool remember, string code, Action doLogin)
        {
            var context = new Agp2pDataContext();
            //string openId = string.Empty;
            dt_users user = null;

            //微信端进入时获取客户openId
            //if (!string.IsNullOrEmpty(code))
            //{
            //    try
            //    {
            //        //根据微信oauth返回的code获取用户openid
            //        var accountModel = context.dt_weixin_account.SingleOrDefault(a => a.id == 1);
            //        var accessToken = OAuth.GetAccessToken(accountModel.appid, accountModel.appsecret, code);
            //        openId = accessToken.openid;
            //    }
            //    catch (Exception)
            //    {
            //        //暂不处理
            //        return;
            //    }
            //}

            //userId为0则是通过微信号自动登录
            //if (userId == 0)
            //{
            //    user = context.dt_users.SingleOrDefault(u => u.openid == openId);
            //    //没有绑定微信号的不自动登录
            //    if (user == null) return;
            //}
            //else
                user = context.dt_users.Single(u => u.id == userId);

            //检查用户每天登录是否获得积分
            /* var userLog = context.dt_user_login_log.OrderByDescending(u => u.login_time).FirstOrDefault(u => u.user_name == user.user_name);
            if (userLog != null && userLog.login_time != null && ((DateTime)userLog.login_time).Day != DateTime.Now.Day)
            {
                MessageBus.Main.PublishAsync(new UserPointMsg(user.id, user.user_name, (int)Agp2pEnums.PointEnum.Sign));
            } */

            //记住登录状态下次自动登录
            if (remember)
            {
                Utils.WriteCookie(DTKeys.COOKIE_USER_NAME_REMEMBER, "Agp2p", user.user_name, 43200, true);
            }
            else
            {
                // 清除用户名 cookie
                //Utils.WriteCookie(DTKeys.COOKIE_USER_NAME_REMEMBER, "Agp2p", user.user_name, true);
                Utils.WriteCookie(DTKeys.COOKIE_USER_NAME_REMEMBER, "Agp2p", -43200);
            }

            //写入登录日志
            var dtUserLoginLog = new dt_user_login_log
            {
                user_id = user.id,
                user_name = user.user_name,
                remark = "会员登录",
                login_ip = DTRequest.GetIP(),
                login_time = DateTime.Now
            };
            context.dt_user_login_log.InsertOnSubmit(dtUserLoginLog);

            //绑定微信openId
            //if (string.IsNullOrEmpty(user.openid) && !string.IsNullOrEmpty(openId))
            //    user.openid = openId;               

            context.SubmitChanges();

            //微信自动登录回调
            //if (doLogin != null) doLogin();
        }

    }
}
