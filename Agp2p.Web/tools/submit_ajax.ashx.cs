using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Agp2p.Web.UI;
using Agp2p.Common;
using LitJson;
using Agp2p.Web.UI.Page;
using Agp2p.BLL;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Agp2p.Core;
using Agp2p.Core.ActivityLogic;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Core.Message.PayApiMsg.Transaction;
using Agp2p.Core.Message.UserPointMsg;
using Newtonsoft.Json;
using Agp2p.Model.DTO;

namespace Agp2p.Web.tools
{
    /// <summary>
    /// AJAX提交处理
    /// </summary>
    public class submit_ajax : IHttpHandler, IRequiresSessionState
    {
        Model.siteconfig siteConfig = new BLL.siteconfig().loadConfig();
        Model.userconfig userConfig = new BLL.userconfig().loadConfig();
        public void ProcessRequest(HttpContext context)
        {
            //取得处事类型
            string action = DTRequest.GetQueryString("action");

            switch (action)
            {
                case "comment_add": //提交评论
                    comment_add(context);
                    break;
                case "comment_list": //评论列表
                    comment_list(context);
                    break;
                case "user_register": //用户注册
                    user_register(context);
                    break;
                case "user_invite_code": //申请邀请码
                    user_invite_code(context);
                    break;
                case "user_register_smscode": //发送注册短信
                    user_register_smscode(context);
                    break;
                case "user_verify_sendsms": //发送用户手机短信验证码
                    user_verify_sendsms(context);
                    break;
                case "user_verify_smscode": //验证用户手机短信验证码
                    user_verify_smscode(context);
                    break;
                case "user_verify_email": //发送注册验证邮件
                    user_verify_email(context);
                    break;
                case "user_login": //用户登录
                    user_login(context);
                    break;
                case "user_check_login": //检查用户是否登录
                    user_check_login(context);
                    break;
                case "user_oauth_bind": //绑定第三方登录账户
                    user_oauth_bind(context);
                    break;
                case "user_oauth_register": //注册第三方登录账户
                    user_oauth_register(context);
                    break;
                case "order_save": //保存订单
                    order_save(context);
                    break;
                case "order_cancel": //用户取消订单
                    order_cancel(context);
                    break;
                case "view_article_click": //统计及输出阅读次数
                    view_article_click(context);
                    break;
                case "view_comment_count": //输出评论总数
                    view_comment_count(context);
                    break;
                case "view_attach_count": //输出附件下载总数
                    view_attach_count(context);
                    break;
                case "invest_project":  //投标
                    invest_project(context);
                    break;
                case "add_bank_card":   //新增银行卡
                    add_bank_card(context);
                    break;
                case "recharge":   //客户充值
                    recharge(context);
                    break;
                case "withdraw":   //客户提现
                    withdraw(context);
                    break;
                case "user_bank_card_delete": //删除客户银行卡
                    user_bank_card_delete(context);
                    break;
                case "bind_idcard": // 实名认证
                    bind_idcard(context);
                    break;
                case "user_edit": //修改会员资料
                    user_edit(context);
                    break;
                case "update_bank_card":   //新增银行卡
                    update_bank_card(context);
                    break;
                case "generate_user_invest_contract": //显示用户协议合同信息
                    GenerateUserInvestContract(context);
                    break;
                case "user_password_edit": //修改密码
                    user_password_edit(context);
                    break;
                case "point_lottery": //积分抽奖
                    point_lottery(context);
                    break;
                case "point_qiandao": //积分签到
                    point_qiandao(context);
                    break;
            }
        }

        #region 提交评论的处理方法OK===========================
        private void comment_add(HttpContext context)
        {
            StringBuilder strTxt = new StringBuilder();
            BLL.article_comment bll = new BLL.article_comment();
            Model.article_comment model = new Model.article_comment();

            string code = DTRequest.GetFormString("txtCode");
            int article_id = DTRequest.GetQueryInt("article_id");
            string _content = DTRequest.GetFormString("txtContent");
            //校检验证码
            string result = verify_code(context, code);
            if (result != "success")
            {
                context.Response.Write(result);
                return;
            }
            if (article_id == 0)
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"对不起，参数传输有误！\"}");
                return;
            }
            if (string.IsNullOrEmpty(_content))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"对不起，请输入评论的内容！\"}");
                return;
            }
            //检查该文章是否存在
            Model.article artModel = new BLL.article().GetModel(article_id);
            if (artModel == null)
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"对不起，主题不存在或已删除！\"}");
                return;
            }
            //检查用户是否登录
            int user_id = 0;
            string user_name = "匿名用户";
            Model.users userModel = BasePage.GetUserInfo();
            if (userModel != null)
            {
                user_id = userModel.id;
                user_name = userModel.user_name;
            }
            model.channel_id = artModel.channel_id;
            model.article_id = artModel.id;
            model.content = Utils.ToHtml(_content);
            model.user_id = user_id;
            model.user_name = user_name;
            model.user_ip = DTRequest.GetIP();
            model.is_lock = siteConfig.commentstatus; //审核开关
            model.add_time = DateTime.Now;
            model.is_reply = 0;
            if (bll.Add(model) > 0)
            {
                context.Response.Write("{\"status\": 1, \"msg\": \"恭喜您，留言提交成功啦！\"}");
                return;
            }
            context.Response.Write("{\"status\": 0, \"msg\": \"对不起，保存过程中发生错误！\"}");
            return;
        }
        #endregion

        #region 取得评论列表方法OK=============================
        private void comment_list(HttpContext context)
        {
            int article_id = DTRequest.GetQueryInt("article_id");
            int page_index = DTRequest.GetQueryInt("page_index");
            int page_size = DTRequest.GetQueryInt("page_size");
            int totalcount;
            StringBuilder strTxt = new StringBuilder();

            if (article_id == 0 || page_size == 0)
            {
                context.Response.Write("获取失败，传输参数有误！");
                return;
            }

            BLL.article_comment bll = new BLL.article_comment();
            DataSet ds = bll.GetList(page_size, page_index, string.Format("is_lock=0 and article_id={0}", article_id.ToString()), "add_time asc", out totalcount);
            //如果记录存在
            if (ds.Tables[0].Rows.Count > 0)
            {
                strTxt.Append("[");
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = ds.Tables[0].Rows[i];
                    strTxt.Append("{");
                    strTxt.Append("\"user_id\":" + dr["user_id"]);
                    strTxt.Append(",\"user_name\":\"" + dr["user_name"] + "\"");
                    if (Convert.ToInt32(dr["user_id"]) > 0)
                    {
                        Model.users userModel = new BLL.users().GetModel(Convert.ToInt32(dr["user_id"]));
                        if (userModel != null)
                        {
                            strTxt.Append(",\"avatar\":\"" + userModel.avatar + "\"");
                        }
                    }
                    strTxt.Append("");
                    strTxt.Append(",\"content\":\"" + Microsoft.JScript.GlobalObject.escape(dr["content"]) + "\"");
                    strTxt.Append(",\"add_time\":\"" + dr["add_time"] + "\"");
                    strTxt.Append(",\"is_reply\":" + dr["is_reply"]);
                    if (Convert.ToInt32(dr["is_reply"]) == 1)
                    {
                        strTxt.Append(",\"reply_content\":\"" + Microsoft.JScript.GlobalObject.escape(dr["reply_content"]) + "\"");
                        strTxt.Append(",\"reply_time\":\"" + dr["reply_time"] + "\"");
                    }
                    strTxt.Append("}");
                    //是否加逗号
                    if (i < ds.Tables[0].Rows.Count - 1)
                    {
                        strTxt.Append(",");
                    }

                }
                strTxt.Append("]");
            }
            context.Response.Write(strTxt.ToString());
        }
        #endregion

        #region 验证用户名是否可用OK===========================
        private void validate_username(HttpContext context)
        {
            string username = DTRequest.GetString("param");
            //如果为Null，退出
            if (string.IsNullOrEmpty(username))
            {
                context.Response.Write("{ \"info\":\"用户名不可为空\", \"status\":\"n\" }");
                return;
            }
            //过滤注册用户名字符
            string[] strArray = userConfig.regkeywords.Split(',');
            foreach (string s in strArray)
            {
                if (s.ToLower() == username.ToLower())
                {
                    context.Response.Write("{ \"info\":\"该用户名不可用\", \"status\":\"n\" }");
                    return;
                }
            }
            BLL.users bll = new BLL.users();
            //查询数据库
            if (!bll.Exists(username.Trim()))
            {
                context.Response.Write("{ \"info\":\"该用户名可用\", \"status\":\"y\" }");
                return;
            }
            context.Response.Write("{ \"info\":\"该用户名已被注册\", \"status\":\"n\" }");
            return;
        }
        #endregion

        #region 用户注册OK=====================================
        private void user_register(HttpContext context)
        {
            string code = DTRequest.GetFormString("txtVerifyCode").Trim();
            string invitecode = DTRequest.GetFormString("txtInviteCode").Trim();
            //string username = userConfig.regstatus == 2 ? Utils.ToHtml(DTRequest.GetFormString("txtMobile").Trim()) : Utils.ToHtml(DTRequest.GetFormString("txtUserName").Trim());
            string username = Utils.ToHtml(DTRequest.GetFormString("txtMobile").Trim());
            string password = DTRequest.GetFormString("txtPassword").Trim();
            //string password1 = DTRequest.GetFormString("txtPassword1").Trim();
            string email = Utils.ToHtml(DTRequest.GetFormString("txtEmail").Trim());
            string mobile = Utils.ToHtml(DTRequest.GetFormString("txtMobile").Trim());
            string userip = DTRequest.GetIP();

            #region 检查各项并提示
            if (username.Length < 11)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"用户名请输入正确的手机号码！\"}");
                return;
            }
            if (password.Length < 6 || password.Length > 16)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请输入6-16数字或字母的密码！\"}");
                return;
            }
            //检查是否开启会员功能
            if (siteConfig.memberstatus == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，会员功能已关闭，无法注册！\"}");
                return;
            }
            if (userConfig.regstatus == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，系统暂不允许注册新用户！\"}");
                return;
            }
            /*if (!password.Equals(password1))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"两次输入的密码不一样！\"}");
                return;
            }*/

            //校检验证码,如果注册使用手机短信则只需验证手机验证码，否则使用网页验证码
            if (userConfig.regstatus == 2) //手机验证码
            {
                string result = verify_sms_code(context, code, mobile);
                if (result != "success")
                {
                    context.Response.Write(result);
                    return;
                }
            }
            else //网页验证码
            {
                string result = verify_code(context, code);
                if (result != "success")
                {
                    context.Response.Write(result);
                    return;
                }
            }
            //检查用户输入信息是否为空
            if (username == "" || password == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"错误：用户名和密码不能为空！\"}");
                return;
            }
            if (userConfig.regstatus == 2 && mobile == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"错误：手机号码不能为空！\"}");
                return;
            }

            //检查用户名
            BLL.users bll = new BLL.users();
            dt_users model = new dt_users();
            if (bll.Exists(username))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，该用户名已经存在！\"}");
                return;
            }
            //检查同一IP注册时隔
            if (userConfig.regctrl > 0)
            {
                if (bll.Exists(userip, userConfig.regctrl))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"对不起，同IP在" + userConfig.regctrl + "小时内禁止重复注册！\"}");
                    return;
                }
            }
            //不允许同一Email注册不同用户
            if (!string.IsNullOrEmpty(email) && (userConfig.regemailditto == 0 || userConfig.emaillogin == 1))
            {
                if (bll.ExistsEmail(email))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"对不起，该邮箱已被注册！\"}");
                    return;
                }
            }
            //不允许同一手机号码注册不同用户
            if (!string.IsNullOrEmpty(mobile) && userConfig.mobilelogin == 1)
            {
                if (bll.ExistsMobile(mobile))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"对不起，该手机号码已被注册！\"}");
                    return;
                }
            }
            //检查默认组别是否存在
            Model.user_groups modelGroup = new BLL.user_groups().GetDefault();
            if (modelGroup == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"用户尚未分组，请联系网站管理员！\"}");
                return;
            }
            //检查是否通过邀请码注册
            if (userConfig.regstatus == 4)
            {
                string result1 = verify_invite_reg(username, invitecode);
                if (result1 != "success")
                {
                    context.Response.Write(result1);
                    return;
                }
            }
            #endregion

            //保存注册信息
            model.group_id = modelGroup.id;
            model.user_name = username;
            model.salt = Utils.GetCheckCode(6); //获得6位的salt加密字符串
            model.password = DESEncrypt.Encrypt(password, model.salt);
            model.email = email;
            model.mobile = mobile;
            model.reg_ip = userip;
            model.reg_time = DateTime.Now;
            //设置对应的状态
            switch (userConfig.regverify)
            {
                case 0:
                    model.status = 0; //正常
                    break;
                case 3:
                    model.status = 2; //人工审核
                    break;
                default:
                    model.status = 1; //待验证
                    break;
            }

            try
            {
                var linqContext = new Agp2pDataContext();
                linqContext.dt_users.InsertOnSubmit(model);

                #region 邀请信息
                //如果有邀请人手机或用户名、邀请码则记录邀请信息
                if (!string.IsNullOrWhiteSpace(invitecode))
                {
                    dt_users inviteUser = linqContext.dt_users.FirstOrDefault(u => u.user_name == invitecode || u.mobile == invitecode);
                    if (inviteUser == null)
                    {
                        var invitecodeModel = linqContext.dt_user_code.FirstOrDefault(c => c.str_code == invitecode && c.type == DTEnums.CodeEnum.Register.ToString());
                        if (invitecodeModel != null)
                        {
                            inviteUser = linqContext.dt_users.FirstOrDefault(u => u.id == invitecodeModel.user_id);
                        }
                    }

                    if (inviteUser != null)
                    {
                        li_invitations invitation = new li_invitations
                        {
                            dt_users = model,
                            inviter = inviteUser.id
                        };
                        linqContext.li_invitations.InsertOnSubmit(invitation);
                        var msg = new UserPointMsg(inviteUser.id, inviteUser.user_name, (int)Agp2pEnums.PointEnum.InviteRegister)
                        {
                            Remark = "推荐新用户注册积分获取"
                        };
                        MessageBus.Main.Publish(msg);
                        var msg2 = new UserPointMsg(model.id, model.user_name, (int)Agp2pEnums.PointEnum.InviteRegister)
                        {
                            Remark = "受推荐注册积分获取"
                        };
                        MessageBus.Main.Publish(msg2);

                        // 会员部功能，被推荐人自动归组
                        if (inviteUser.li_user_group_servers != null)
                        {
                            model.group_id = inviteUser.li_user_group_servers.group_id;
                        }
                    }
                    else
                    {
                        context.Response.Write("{\"status\":0, \"msg\":\"没有找到推荐人信息！\"}");
                        return;
                    }
                }

                //获取邀请码                
                /*string str_code = Utils.GetCheckCode(8);
                dt_user_code codeModel = new dt_user_code
                {
                    dt_users = model,
                    user_name = model.user_name,
                    type = DTEnums.CodeEnum.Register.ToString(),
                    str_code = str_code,
                    eff_time = DateTime.Now,
                    add_time = DateTime.Now
                };
                linqContext.dt_user_code.InsertOnSubmit(codeModel);*/
                #endregion                
                linqContext.SubmitChanges();
                // 广播新用户注册消息
                MessageBus.Main.Publish(new NewUserCreatedMsg(model.id, (DateTime)model.reg_time)); // 发同步消息，避免跳转到用户中心后钱包为空

            }
            catch (Exception ex)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"系统故障，请联系网站管理员！\"}");
                return;
            }

            //赠送积分金额
            if (modelGroup.point > 0)
            {
                //new BLL.user_point_log().Add(model.id, model.user_name, modelGroup.point, "注册赠送积分", false);
            }

            #region 判断是否发送欢迎消息
            if (userConfig.regmsgstatus == 1) //站内短消息
            {
                //new BLL.user_message().Add(1, "", model.user_name, "欢迎您成为本站会员", userConfig.regmsgtxt);
            }
            else if (userConfig.regmsgstatus == 2) //发送邮件
            {
                //取得邮件模板内容
                Model.mail_template mailModel = new BLL.mail_template().GetModel("welcomemsg");
                if (mailModel != null)
                {
                    //替换标签
                    string mailTitle = mailModel.maill_title;
                    mailTitle = mailTitle.Replace("{username}", model.user_name);
                    string mailContent = mailModel.content;
                    mailContent = mailContent.Replace("{webname}", siteConfig.webname);
                    mailContent = mailContent.Replace("{weburl}", siteConfig.weburl);
                    mailContent = mailContent.Replace("{webtel}", siteConfig.webtel);
                    mailContent = mailContent.Replace("{username}", model.user_name);
                    //发送邮件
                    DTMail.sendMail(siteConfig.emailsmtp, siteConfig.emailusername, siteConfig.emailpassword, siteConfig.emailnickname,
                        siteConfig.emailfrom, model.email, mailTitle, mailContent);
                }
            }
            else if (userConfig.regmsgstatus == 3 && mobile != "") //发送短信
            {
                Model.sms_template smsModel = new BLL.sms_template().GetModel("welcomemsg"); //取得短信内容
                if (smsModel != null)
                {
                    //替换标签
                    string msgContent = smsModel.content;
                    msgContent = msgContent.Replace("{webname}", siteConfig.webname);
                    msgContent = msgContent.Replace("{weburl}", siteConfig.weburl);
                    msgContent = msgContent.Replace("{webtel}", siteConfig.webtel);
                    msgContent = msgContent.Replace("{username}", model.user_name);
                    msgContent = msgContent.Replace("{id}", model.id.ToString());
                    //发送短信
                    string tipMsg = string.Empty;
                    SMSHelper.Send(model.mobile, msgContent, 2, out tipMsg);
                }
            }
            #endregion

            //需要Email验证
            if (userConfig.regverify == 1)
            {
                string result2 = verify_email(model.id, model.user_name, model.email);
                if (result2 != "success")
                {
                    context.Response.Write(result2);
                    return;
                }
                context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("register") + "?action=sendmail&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"注册成功，请进入邮箱验证激活账户！\"}");
            }
            //手机短信验证
            else if (userConfig.regverify == 2)
            {
                string result3 = verify_mobile(model.id, model.user_name, model.mobile);
                if (result3 != "success")
                {
                    context.Response.Write(result3);
                    return;
                }
                context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("register") + "?action=sendsms&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"注册成功，请查收短信验证激活账户！\"}");
            }
            //需要人工审核
            else if (userConfig.regverify == 3)
            {
                context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("register") + "?action=verify&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"注册成功，请等待审核通过！\"}");
            }
            else
            {
                context.Session[DTKeys.SESSION_USER_INFO] = bll.GetModel(model.id);
                context.Session.Timeout = 45;

                //防止Session提前过期
                Utils.WriteCookie(DTKeys.COOKIE_USER_NAME_REMEMBER, "Agp2p", model.user_name, true);
                //Utils.WriteCookie(DTKeys.COOKIE_USER_PWD_REMEMBER, "Agp2p", model.password, true);

                //写入登录日志
                new BLL.user_login_log().Add(model.id, model.user_name, "会员登录");

                var msg = new UserPointMsg(model.id, model.user_name, (int) Agp2pEnums.PointEnum.Register);
                MessageBus.Main.Publish(msg);

                //context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("usercenter", "index") + "?action=succeed&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"注册成功，欢迎成为本站会员！\"}");
                context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("register") + "?action=succeed&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"注册成功，欢迎成为本站会员！\"}");
            }
            return;
        }

        #region 邀请注册处理方法OK=============================
        private string verify_invite_reg(string user_name, string invite_code)
        {
            if (string.IsNullOrEmpty(invite_code))
            {
                return "{\"status\":0, \"msg\":\"邀请码不能为空！\"}";
            }
            BLL.user_code codeBll = new BLL.user_code();
            Model.user_code codeModel = codeBll.GetModel(invite_code);
            if (codeModel == null)
            {
                return "{\"status\":0, \"msg\":\"邀请码不正确或已过期！\"}";
            }
            if (userConfig.invitecodecount > 0)
            {
                if (codeModel.count >= userConfig.invitecodecount)
                {
                    codeModel.status = 1;
                    return "{\"status\":0, \"msg\":\"该邀请码已经被使用！\"}";
                }
            }
            //检查是否给邀请人增加积分
            if (userConfig.pointinvitenum > 0)
            {
                new BLL.user_point_log().Add(codeModel.user_id, codeModel.user_name, userConfig.pointinvitenum, "邀请用户【" + user_name + "】注册获得积分", true);
            }
            //更改邀请码状态
            codeModel.count += 1;
            codeBll.Update(codeModel);
            return "success";
        }
        #endregion

        #region Email验证发送邮件OK============================
        private string verify_email(int user_id, string user_name, string email)
        {
            //生成随机码
            string strcode = Utils.GetCheckCode(20);
            BLL.user_code codeBll = new BLL.user_code();
            Model.user_code codeModel;
            //检查是否重复提交
            codeModel = codeBll.GetModel(user_name, DTEnums.CodeEnum.RegVerify.ToString(), "d");
            if (codeModel == null)
            {
                codeModel = new Model.user_code();
                codeModel.user_id = user_id;
                codeModel.user_name = user_name;
                codeModel.type = DTEnums.CodeEnum.RegVerify.ToString();
                codeModel.str_code = strcode;
                codeModel.eff_time = DateTime.Now.AddDays(userConfig.regemailexpired);
                codeModel.add_time = DateTime.Now;
                new BLL.user_code().Add(codeModel);
            }
            //获得邮件内容
            Model.mail_template mailModel = new BLL.mail_template().GetModel("regverify");
            if (mailModel == null)
            {
                return "{\"status\":0, \"msg\":\"邮件发送失败，邮件模板内容不存在！\"}";
            }
            //替换模板内容
            string titletxt = mailModel.maill_title;
            string bodytxt = mailModel.content;
            titletxt = titletxt.Replace("{webname}", siteConfig.webname);
            titletxt = titletxt.Replace("{username}", user_name);
            bodytxt = bodytxt.Replace("{webname}", siteConfig.webname);
            bodytxt = bodytxt.Replace("{webtel}", siteConfig.webtel);
            bodytxt = bodytxt.Replace("{weburl}", siteConfig.weburl);
            bodytxt = bodytxt.Replace("{username}", user_name);
            bodytxt = bodytxt.Replace("{valid}", userConfig.regemailexpired.ToString());
            bodytxt = bodytxt.Replace("{linkurl}", "http://" + HttpContext.Current.Request.Url.Authority.ToLower() + new Web.UI.BasePage().linkurl("register") + "?action=checkmail&strcode=" + codeModel.str_code);
            //发送邮件
            try
            {
                DTMail.sendMail(siteConfig.emailsmtp,
                    siteConfig.emailusername,
                    DESEncrypt.Decrypt(siteConfig.emailpassword),
                    siteConfig.emailnickname,
                    siteConfig.emailfrom,
                    email,
                    titletxt, bodytxt);
            }
            catch
            {
                return "{\"status\":0, \"msg\":\"邮件发送失败，请联系本站管理员！\"}";
            }
            return "success";
        }
        #endregion

        #region 手机验证发送短信OK=============================
        private string verify_mobile(int user_id, string user_name, string mobile)
        {
            //生成随机码
            string strcode = Utils.Number(4);
            BLL.user_code codeBll = new BLL.user_code();
            Model.user_code codeModel;
            //检查是否重复提交
            codeModel = codeBll.GetModel(user_name, DTEnums.CodeEnum.RegVerify.ToString(), "n");
            if (userConfig.regsmsexpired > 0 && codeModel != null)
            {
                return "{\"status\":0, \"msg\":\"短信已发送，请过" + userConfig.regsmsexpired + "分钟再试！\"}";
            }
            codeModel = new Model.user_code();
            codeModel.user_id = user_id;
            codeModel.user_name = user_name;
            codeModel.type = DTEnums.CodeEnum.RegVerify.ToString();
            codeModel.str_code = strcode;
            codeModel.eff_time = DateTime.Now.AddMinutes(userConfig.regsmsexpired);
            codeModel.add_time = DateTime.Now;
            new BLL.user_code().Add(codeModel);
            //获得短信模板内容
            Model.sms_template smsModel = new BLL.sms_template().GetModel("usercode");
            if (smsModel == null)
            {
                return "{\"status\":0, \"msg\":\"发送失败，短信模板内容不存在！\"}";
            }
            //替换模板内容
            string msgContent = smsModel.content;
            msgContent = msgContent.Replace("{webname}", siteConfig.webname);
            msgContent = msgContent.Replace("{webtel}", siteConfig.webtel);
            msgContent = msgContent.Replace("{weburl}", siteConfig.weburl);
            msgContent = msgContent.Replace("{username}", user_name);
            msgContent = msgContent.Replace("{code}", codeModel.str_code);
            msgContent = msgContent.Replace("{valid}", userConfig.regsmsexpired.ToString());
            //发送短信
            string tipMsg = string.Empty;
            bool sendStatus = SMSHelper.SendSmsCode(mobile, msgContent, out tipMsg);
            if (!sendStatus)
            {
                return "{\"status\": 0, \"msg\": \"短信发送失败，" + tipMsg + "\"}";
            }
            return "success";
        }
        #endregion

        #endregion

        #region 申请邀请码OK===================================
        private void user_invite_code(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            //检查是否开启邀请注册
            if (userConfig.regstatus != 4)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，系统不允许通过邀请注册！\"}");
                return;
            }
            BLL.user_code codeBll = new BLL.user_code();
            //检查申请是否超过限制
            if (userConfig.invitecodenum > 0)
            {
                int result = codeBll.GetCount("user_name='" + model.user_name + "' and type='" + DTEnums.CodeEnum.Register.ToString() + "' and datediff(d,add_time,getdate())=0");
                if (result >= userConfig.invitecodenum)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"对不起，您申请邀请码的数量已超过每天限制！\"}");
                    return;
                }
            }
            //删除过期的邀请码
            codeBll.Delete("type='" + DTEnums.CodeEnum.Register.ToString() + "' and status=1 or datediff(d,eff_time,getdate())>0");
            //随机取得邀请码
            string str_code = Utils.GetCheckCode(8);
            Model.user_code codeModel = new Model.user_code();
            codeModel.user_id = model.id;
            codeModel.user_name = model.user_name;
            codeModel.type = DTEnums.CodeEnum.Register.ToString();
            codeModel.str_code = str_code;
            if (userConfig.invitecodeexpired > 0)
            {
                codeModel.eff_time = DateTime.Now.AddDays(userConfig.invitecodeexpired);
            }
            else
            {
                codeModel.eff_time = DateTime.Now.AddDays(1);
            }
            codeBll.Add(codeModel);
            context.Response.Write("{\"status\":1, \"msg\":\"恭喜您，申请邀请码已成功！\"}");
            return;
        }
        #endregion

        #region 发送注册验证码短信OK===========================
        private void user_register_smscode(HttpContext context)
        {
            string mobile = DTRequest.GetFormString("mobile");
            if (mobile == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"发送失败，请填写手机号码！\"}");
                return;
            }
            //检查是否过快
            var sendTime = (DateTime?)SessionHelper.Get(DTKeys.SESSION_SMS_MOBILE_SEND_TIME);
            if (sendTime != null && DateTime.Now.Subtract(sendTime.Value).TotalMinutes < 2)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"刚已发送过短信，请2分钟后再试！\"}");
                return;
            }
            // 发送注册验证码时验证图片验证码
            string picCode = DTRequest.GetFormString("txtPicCode").Trim();
            string verifyPicCodeResult = verify_code(context, picCode);
            if (verifyPicCodeResult != "success")
            {
                context.Response.Write(verifyPicCodeResult);
                return;
            }
            Model.sms_template smsModel = new sms_template().GetModel("usercode"); //取得短信内容
            if (smsModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"发送失败，短信模板不存在！\"}");
                return;
            }
            if (new users().ExistsMobile(mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，该手机号码已被注册！\"}");
                return;
            }
            string strcode = Utils.Number(4); //随机验证码
            //替换标签
            string msgContent = smsModel.content
                .Replace("{webname}", siteConfig.webname)
                .Replace("{weburl}", siteConfig.weburl)
                .Replace("{webtel}", siteConfig.webtel)
                .Replace("{code}", strcode)
                .Replace("{valid}", userConfig.regsmsexpired.ToString());

            //发送短信
            string tipMsg = string.Empty;
            bool result = SMSHelper.SendSmsCode(mobile, msgContent, out tipMsg);
            if (!result)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"发送失败，" + tipMsg + "\"}");
                return;
            }
            //写入SESSION，保存验证码
            context.Session[DTKeys.SESSION_SMS_CODE] = strcode;
            context.Session[DTKeys.SESSION_SMS_MOBILE_CODE] = mobile;
            SessionHelper.Set(DTKeys.SESSION_SMS_MOBILE_SEND_TIME, DateTime.Now);

            context.Response.Write("{\"status\":1, \"msg\":\"短信发送成功，请注意查收验证码！\"}");
        }
        #endregion

        #region 发送用户手机短信验证码OK=======================
        private void user_verify_sendsms(HttpContext context)
        {
            string username = DTRequest.GetFormString("username");
            if (username == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户名参数有误！\"}");
                return;
            }
            Model.users model = new BLL.users().GetModel(username);
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，无法获取用户信息！\"}");
                return;
            }
            if (model.status != 1)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，该账户不需要验证！\"}");
                return;
            }
            string result3 = verify_mobile(model.id, model.user_name, model.mobile);
            if (result3 != "success")
            {
                context.Response.Write(result3);
                return;
            }
            context.Response.Write("{\"status\":1, \"msg\":\"短信发送成功，请注意查收！\"}");
        }
        #endregion

        #region 验证用户手机短信验证码OK=======================
        private void user_verify_smscode(HttpContext context)
        {
            string smscode = DTRequest.GetFormString("smscode");
            if (smscode == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请填写手机收到的验证码！\"}");
                return;
            }
            BLL.user_code bll = new BLL.user_code();
            Model.user_code model = bll.GetModel(smscode);
            if (model == null) //返回出错
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，您填写的手机验证码有误！\"}");
                return;
            }
            //修改申请码状态
            model.status = 1;
            bll.Update(model);
            //修改用户状态
            new BLL.users().UpdateField(model.user_id, "status=0");
            context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("register") + "?action=checksms&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"手机验证成功，欢迎成为本站会员！\"}");
            return;
        }
        #endregion

        #region 发送注册验证邮件OK=============================
        private void user_verify_email(HttpContext context)
        {
            string username = DTRequest.GetFormString("username");
            //检查是否过快
            string cookie = Utils.GetCookie("user_reg_email");
            if (cookie == username)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"发送邮件间隔为20分钟，您刚才已经提交过啦，休息一下再来吧！\"}");
                return;
            }
            Model.users model = new BLL.users().GetModel(username);
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"该用户不存在或已删除！\"}");
                return;
            }
            if (model.status != 1)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"该用户无法进行邮箱验证！\"}");
                return;
            }
            string result = verify_email(model.id, model.user_name, model.email);
            if (result != "success")
            {
                context.Response.Write(result);
                return;
            }
            context.Response.Write("{\"status\":1, \"msg\":\"邮件已经发送成功啦！\"}");
            Utils.WriteCookie("user_reg_email", username, 20); //20分钟内无重复发送
            return;
        }
        #endregion

        #region 用户登录OK=====================================
        private void user_login(HttpContext context)
        {
            string username = DTRequest.GetFormString("txtUserName");
            string password = DTRequest.GetFormString("txtPassword");
            string remember = DTRequest.GetFormString("chkRemember");
            string code = DTRequest.GetFormString("code");
            //检查用户名密码
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"温馨提示：请输入用户名或密码！\"}");
                return;
            }

            BLL.users bll = new BLL.users();
            Model.users model = bll.GetModel(username, password, userConfig.emaillogin, userConfig.mobilelogin, true);
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"错误提示：用户名或密码错误，请重试！\"}");
                return;
            }
            //检查用户是否通过验证
            if (model.status == 1) //待验证
            {
                if (userConfig.regverify == 1)
                {
                    context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("register") + "?action=sendmail&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"会员尚未通过验证！\"}");
                }
                else
                {
                    context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("register") + "?action=sendsms&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"会员尚未通过验证！\"}");
                }
                return;
            }
            else if (model.status == 2) //待审核
            {
                context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("register") + "?action=verify&username=" + Utils.UrlEncode(model.user_name) + "\", \"msg\":\"会员尚未通过审核！\"}");
                return;
            }

            context.Session[DTKeys.SESSION_USER_INFO] = model;
            context.Session.Timeout = 45;

            //返回URL
            context.Response.Write("{\"status\":1, \"msg\":\"会员登录成功！\"}");

            MessageBus.Main.Publish(new UserLoginMsg(model.id, remember.ToLower() == "true", code, null)); // 广播会员登录消息
        }
        #endregion

        #region 检查用户是否登录OK=============================
        private void user_check_login(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"username\":\"匿名用户\"}");
                return;
            }
            context.Response.Write("{\"status\":1, \"username\":\"" + (string.IsNullOrEmpty(model.nick_name) ? model.user_name : model.nick_name) + "\"}");
        }
        #endregion

        #region 绑定第三方登录账户OK===========================
        private void user_oauth_bind(HttpContext context)
        {
            //检查URL参数
            if (context.Session["oauth_name"] == null)
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"错误提示：授权参数不正确！\"}");
                return;
            }
            //获取授权信息
            string result = Utils.UrlExecute(siteConfig.webpath + "api/oauth/" + context.Session["oauth_name"].ToString() + "/result_json.aspx");
            if (result.Contains("error"))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"错误提示：请检查URL是否正确！\"}");
                return;
            }
            //反序列化JSON
            Dictionary<string, object> dic = JsonMapper.ToObject<Dictionary<string, object>>(result);
            if (dic["ret"].ToString() != "0")
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"错误代码：" + dic["ret"] + "，描述：" + dic["msg"] + "\"}");
                return;
            }

            //检查用户名密码
            string username = DTRequest.GetString("txtUserName");
            string password = DTRequest.GetString("txtPassword");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"温馨提示：请输入用户名或密码！\"}");
                return;
            }
            BLL.users bll = new BLL.users();
            Model.users model = bll.GetModel(username, password, userConfig.emaillogin, userConfig.mobilelogin, true);
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"错误提示：用户名或密码错误！\"}");
                return;
            }
            //开始绑定
            Model.user_oauth oauthModel = new Model.user_oauth();
            oauthModel.oauth_name = dic["oauth_name"].ToString();
            oauthModel.user_id = model.id;
            oauthModel.user_name = model.user_name;
            oauthModel.oauth_access_token = dic["oauth_access_token"].ToString();
            oauthModel.oauth_openid = dic["oauth_openid"].ToString();
            int newId = new BLL.user_oauth().Add(oauthModel);
            if (newId < 1)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"错误提示：绑定过程中出错，请重新获取！\"}");
                return;
            }
            context.Session[DTKeys.SESSION_USER_INFO] = model;
            context.Session.Timeout = 45;
            //记住登录状态，防止Session提前过期
            Utils.WriteCookie(DTKeys.COOKIE_USER_NAME_REMEMBER, "Agp2p", model.user_name, true);
            //Utils.WriteCookie(DTKeys.COOKIE_USER_PWD_REMEMBER, "Agp2p", model.password, true);
            //写入登录日志
            new BLL.user_login_log().Add(model.id, model.user_name, "会员登录");
            //返回URL
            context.Response.Write("{\"status\":1, \"msg\":\"会员登录成功！\"}");
            return;
        }
        #endregion

        #region 注册第三方登录账户OK===========================
        private void user_oauth_register(HttpContext context)
        {
            //检查URL参数
            if (context.Session["oauth_name"] == null)
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"错误提示：授权参数不正确！\"}");
                return;
            }
            //获取授权信息
            string result = Utils.UrlExecute(siteConfig.webpath + "api/oauth/" + context.Session["oauth_name"].ToString() + "/result_json.aspx");
            if (result.Contains("error"))
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"错误提示：请检查URL是否正确！\"}");
                return;
            }
            //反序列化JSON
            Dictionary<string, object> dic = JsonMapper.ToObject<Dictionary<string, object>>(result);
            if (dic["ret"].ToString() != "0")
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"错误代码：" + dic["ret"] + "，" + dic["msg"] + "\"}");
                return;
            }

            string password = DTRequest.GetFormString("txtPassword").Trim();
            string email = Utils.ToHtml(DTRequest.GetFormString("txtEmail").Trim());
            string mobile = Utils.ToHtml(DTRequest.GetFormString("txtMobile").Trim());
            string userip = DTRequest.GetIP();

            BLL.users bll = new BLL.users();
            Model.users model = new Model.users();
            //检查默认组别是否存在
            Model.user_groups modelGroup = new BLL.user_groups().GetDefault();
            if (modelGroup == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"用户尚未分组，请联系管理员！\"}");
                return;
            }
            //保存注册信息
            model.group_id = modelGroup.id;
            model.user_name = bll.GetRandomName(10); //随机用户名
            model.salt = Utils.GetCheckCode(6); //获得6位的salt加密字符串
            model.password = DESEncrypt.Encrypt(password, model.salt);
            model.email = email;
            model.mobile = mobile;
            if (!string.IsNullOrEmpty(dic["nick"].ToString()))
            {
                model.nick_name = dic["nick"].ToString();
            }
            if (dic["avatar"].ToString().StartsWith("http://"))
            {
                model.avatar = dic["avatar"].ToString();
            }
            if (!string.IsNullOrEmpty(dic["sex"].ToString()))
            {
                model.sex = dic["sex"].ToString();
            }
            if (!string.IsNullOrEmpty(dic["birthday"].ToString()))
            {
                model.birthday = Utils.StrToDateTime(dic["birthday"].ToString());
            }
            model.reg_ip = userip;
            model.reg_time = DateTime.Now;
            model.status = 0; //设置为正常状态
            int newId = bll.Add(model);
            if (newId < 1)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"注册失败，请联系网站管理员！\"}");
                return;
            }
            model = bll.GetModel(newId);
            //判断是否发送欢迎消息
            if (userConfig.regmsgstatus == 1) //站内短消息
            {
                new BLL.user_message().Add(1, "", model.user_name, "欢迎您成为本站会员", userConfig.regmsgtxt, model.id);
            }
            else if (userConfig.regmsgstatus == 2) //发送邮件
            {
                //取得邮件模板内容
                Model.mail_template mailModel = new BLL.mail_template().GetModel("welcomemsg");
                if (mailModel != null)
                {
                    //替换标签
                    string mailTitle = mailModel.maill_title;
                    mailTitle = mailTitle.Replace("{username}", model.user_name);
                    string mailContent = mailModel.content;
                    mailContent = mailContent.Replace("{webname}", siteConfig.webname);
                    mailContent = mailContent.Replace("{weburl}", siteConfig.weburl);
                    mailContent = mailContent.Replace("{webtel}", siteConfig.webtel);
                    mailContent = mailContent.Replace("{username}", model.user_name);
                    //发送邮件
                    DTMail.sendMail(siteConfig.emailsmtp, siteConfig.emailusername, siteConfig.emailpassword, siteConfig.emailnickname,
                        siteConfig.emailfrom, model.email, mailTitle, mailContent);
                }
            }
            else if (userConfig.regmsgstatus == 3 && mobile != "") //发送短信
            {
                Model.sms_template smsModel = new BLL.sms_template().GetModel("welcomemsg"); //取得短信内容
                if (smsModel != null)
                {
                    //替换标签
                    string msgContent = smsModel.content;
                    msgContent = msgContent.Replace("{webname}", siteConfig.webname);
                    msgContent = msgContent.Replace("{weburl}", siteConfig.weburl);
                    msgContent = msgContent.Replace("{webtel}", siteConfig.webtel);
                    msgContent = msgContent.Replace("{username}", model.user_name);
                    //发送短信
                    string tipMsg = string.Empty;
                    SMSHelper.Send(model.mobile, msgContent, 2, out tipMsg);
                }
            }
            //绑定到对应的授权类型
            Model.user_oauth oauthModel = new Model.user_oauth();
            oauthModel.oauth_name = dic["oauth_name"].ToString();
            oauthModel.user_id = model.id;
            oauthModel.user_name = model.user_name;
            oauthModel.oauth_access_token = dic["oauth_access_token"].ToString();
            oauthModel.oauth_openid = dic["oauth_openid"].ToString();
            new BLL.user_oauth().Add(oauthModel);

            context.Session[DTKeys.SESSION_USER_INFO] = model;
            context.Session.Timeout = 45;
            //记住登录状态，防止Session提前过期
            Utils.WriteCookie(DTKeys.COOKIE_USER_NAME_REMEMBER, "Agp2p", model.user_name, true);
            //Utils.WriteCookie(DTKeys.COOKIE_USER_PWD_REMEMBER, "Agp2p", model.password, true);
            //写入登录日志
            new BLL.user_login_log().Add(model.id, model.user_name, "会员登录");
            //返回URL
            context.Response.Write("{\"status\":1, \"msg\":\"会员登录成功！\"}");
            return;
        }
        #endregion

        #region 保存用户订单OK=================================
        private void order_save(HttpContext context)
        {
            var agContext = new Agp2pDataContext();
            //检查用户是否登录
            Model.users userModel = BasePage.GetUserInfo();
            if (userModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            //投资记录查询
            var investRecord = agContext.li_project_transactions.FirstOrDefault(p => p.investor == userModel.id);
            if (investRecord == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，你还没有完成过投资。\"}");
                return;
            }
            //获得客户地址信息
            int addressId = DTRequest.GetFormInt("addressId");
            string message = Utils.ToHtml(DTRequest.GetFormString("message"));
            var userAddr = agContext.dt_user_addr_book.SingleOrDefault(a => a.id == addressId);
            if (userAddr == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请先选择收获地址。\"}");
                return;
            }
            //获取商品信息
            int goodId = DTRequest.GetFormInt("goodId");
            int goodCount = DTRequest.GetFormInt("goodCount");
            var goods = agContext.dt_article.SingleOrDefault(g => g.id == goodId);
            if (goods == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，查询兑换物信息出错！请联系客服。\"}");
                return;
            }
            //获取商品详细信息
            var goodFields = agContext.dt_article_attribute_value.SingleOrDefault(f => f.article_id == goodId);
            if (goodFields == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，查询兑换物详细信息出错！请联系客服。\"}");
                return;
            }
            //检查收货人
            if (string.IsNullOrEmpty(userAddr.accept_name))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入收货人姓名！\"}");
                return;
            }
            //检查手机和电话
            if (string.IsNullOrEmpty(userAddr.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入收货人联系电话或手机！\"}");
                return;
            }
            //检查地址
            if (string.IsNullOrEmpty(userAddr.address))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入详细的收货地址！\"}");
                return;
            }
            //检查积分是否足够
            if (userModel.point < goodFields.point * goodCount)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"积分不足，不能兑换该商品！\"}");
                return;
            }

            //保存订单
            Model.orders model = new Model.orders
            {
                order_no = "B" + Utils.GetOrderNumber(), //订单号B开头为商品订单
                user_id = userModel.id,
                user_name = userModel.user_name,
                payment_id = 1,
                express_id = 1,
                accept_name = userAddr.accept_name,
                post_code = userAddr.post_code,
                telphone = userAddr.telphone,
                mobile = userAddr.mobile,
                address = userAddr.area + userAddr.address,
                message = message,
                payable_amount = 0,
                real_amount = 0,
                express_status = 1,
                express_fee = 0,
                order_amount = 0,
                point = 0,
                add_time = DateTime.Now,

                status = goodFields.isVirtual.GetValueOrDefault(0) == 1 ? 3 : 1
            };

            var orderGoods = new List<Model.order_goods>()
            {
                new Model.order_goods
                {
                    goods_id = goods.id,
                    goods_title = goods.title,
                    goods_price = 0,
                    real_price = 0,
                    quantity = goodCount,
                    point = goodFields.point.GetValueOrDefault(0) * goodCount
                }
            };
            model.order_goods = orderGoods;
            var orderBll = new BLL.orders();
            int result = orderBll.Add(model);
            if (result < 1)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"订单保存过程中发生错误，请重新提交！\"}");
                return;
            }

            //如果是虚拟物直接扣除积分
            if (goodFields.isVirtual.GetValueOrDefault(0) == 1)
            {
                //TODO 生成虚拟物并绑定会员

                var msg = new UserPointMsg(userModel.id, userModel.user_name, (int)Agp2pEnums.PointEnum.Exchange,
                    -goodFields.point.GetValueOrDefault(0) * goodCount)
                {
                    Remark = "积分换购，订单号：" + model.order_no
                };
                MessageBus.Main.Publish(msg);
                for (int i = 0; i < goodCount; i++)
                {
                    if (goodFields.hongbao != 0)
                    {
                        HongBaoActivity.GiveUser(userModel.id);
                    }
                    if (Convert.ToInt32(goodFields.jiaxijuan) != 0)
                    {
                        InterestRateTicketActivity.GiveUser(userModel.id, 1, 100, 1);
                    }
                }
                //兑换成功，返回URL
                context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("payment", "confirm", model.order_no) + "\", \"msg\":\"恭喜您，已成功兑换！\"}");
            }
            else
            {
                //提交成功，返回URL
                context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("payment", "confirm", model.order_no) + "\", \"msg\":\"恭喜您，订单已成功提交！\"}");
            }
        }
        #endregion

        #region 用户取消订单OK=================================
        private void order_cancel(HttpContext context)
        {
            //检查用户是否登录
            Model.users userModel = BasePage.GetUserInfo();
            if (userModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            //检查订单是否存在
            string order_no = DTRequest.GetQueryString("order_no");
            Model.orders orderModel = new BLL.orders().GetModel(order_no);
            if (order_no == "" || orderModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，订单号不存在或已删除！\"}");
                return;
            }
            //检查是否自己的订单
            if (userModel.id != orderModel.user_id)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，不能取消别人的订单状态！\"}");
                return;
            }
            //检查订单状态
            if (orderModel.status > 1)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，订单不是生成状态，不能取消！\"}");
                return;
            }
            bool result = new BLL.orders().UpdateField(order_no, "status=4");
            if (!result)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，操作过程中发生不可遇知的错误！\"}");
                return;
            }
            //如果是积分换购则返还积分
            if (orderModel.point < 0)
            {
                new BLL.user_point_log().Add(orderModel.user_id, orderModel.user_name, -1 * orderModel.point, "取消订单，返还换购积分，订单号：" + orderModel.order_no, false);
            }
            context.Response.Write("{\"status\":1, \"msg\":\"取消订单成功！\"}");
            return;
        }
        #endregion

        #region 统计及输出阅读次数OK===========================
        private void view_article_click(HttpContext context)
        {
            int article_id = DTRequest.GetInt("id", 0);
            int click = DTRequest.GetInt("click", 0);
            int count = 0;
            if (article_id > 0)
            {
                BLL.article bll = new BLL.article();
                count = bll.GetClick(article_id);
                if (click > 0)
                {
                    bll.UpdateField(article_id, "click=click+1");
                }
            }
            context.Response.Write("document.write('" + count + "');");
        }
        #endregion

        #region 输出评论总数OK=================================
        private void view_comment_count(HttpContext context)
        {
            int article_id = DTRequest.GetInt("id", 0);
            int count = 0;
            if (article_id > 0)
            {
                count = new BLL.article_comment().GetCount("is_lock=0 and article_id=" + article_id);
            }
            context.Response.Write("document.write('" + count + "');");
        }
        #endregion

        #region 输出附件下载总数OK=============================
        private void view_attach_count(HttpContext context)
        {
            int attach_id = DTRequest.GetInt("id", 0);
            int count = 0;
            if (attach_id > 0)
            {
                count = new BLL.article_attach().GetDownNum(attach_id);
            }
            context.Response.Write("document.write('" + count + "');");
        }
        #endregion

        #region 通用外理方法OK=================================
        //校检网站验证码
        private string verify_code(HttpContext context, string strcode)
        {
            if (string.IsNullOrEmpty(strcode))
            {
                return "{\"status\":0, \"msg\":\"对不起，请输入验证码！\"}";
            }
            if (context.Session[DTKeys.SESSION_CODE] == null)
            {
                return "{\"status\":0, \"msg\":\"对不起，验证码超时或已过期！\"}";
            }
            if (strcode.ToLower() != (context.Session[DTKeys.SESSION_CODE].ToString()).ToLower())
            {
                return "{\"status\":0, \"msg\":\"您输入的验证码与系统的不一致！\"}";
            }
            context.Session[DTKeys.SESSION_CODE] = null;
            return "success";
        }
        //校检网站手机验证码
        private string verify_sms_code(HttpContext context, string strcode, string mobile)
        {
            if (string.IsNullOrEmpty(strcode))
            {
                return "{\"status\":0, \"msg\":\"对不起，请输入验证码！\"}";
            }
            if (context.Session[DTKeys.SESSION_SMS_CODE] == null)
            {
                return "{\"status\":0, \"msg\":\"对不起，验证码超时或已过期！\"}";
            }
            if (strcode.ToLower() != (context.Session[DTKeys.SESSION_SMS_CODE].ToString()).ToLower())
            {
                return "{\"status\":0, \"msg\":\"您输入的验证码与系统的不一致！\"}";
            }
            if (mobile.ToLower() != (context.Session[DTKeys.SESSION_SMS_MOBILE_CODE].ToString()).ToLower())
            {
                return "{\"status\":0, \"msg\":\"对不起，手机验证失败！\"}";
            }
            context.Session[DTKeys.SESSION_SMS_CODE] = null;
            context.Session[DTKeys.SESSION_SMS_MOBILE_CODE] = null;
            SessionHelper.Remove(DTKeys.SESSION_SMS_MOBILE_SEND_TIME);
            return "success";
        }
        #endregion END通用方法=================================================

        /// <summary>
        /// 投标
        /// </summary>
        /// <param name="context"></param>
        private void invest_project(HttpContext context)
        {
            var linqContext = new Agp2pDataContext();
            var user = BasePage.GetUserInfoByLinq(linqContext);
            if (user == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.identity_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"为了您的资金安全，请先到安全中心开通第三方托管账户！\"}");
                return;
            }
            try
            {
                var investingAmount = DTRequest.GetFormDecimal("investingAmount", 0);
                var projectId = DTRequest.GetFormInt("projectId", 0);
                var buyClaimId = DTRequest.GetFormInt("buyClaimId", 0);
                var projectSum = DTRequest.GetFormDecimal("projectSum", 0);
                var projectDescription = DTRequest.GetFormString("projectDescription");
                var huoqi = DTRequest.GetFormString("huoqi");
                var backUrl = DTRequest.GetFormString("backUrl");
                var ticketId = DTRequest.GetFormInt("ticketId", 0);

                if (buyClaimId != 0)
                {
                    //债权转让
                    if (string.IsNullOrEmpty(backUrl))
                    {
                        context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" +
                                               (int)Agp2pEnums.SumapayApiEnum.CreAs
                                               + "&userId=" + user.id + "&claimId=" + buyClaimId + "&undertakeSum=" +
                                               investingAmount + "\"}");
                    }
                    else
                    {
                        context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" +
                                               (int)Agp2pEnums.SumapayApiEnum.CreAM
                                               + "&userId=" + user.id + "&claimId=" + buyClaimId + "&undertakeSum=" +
                                               investingAmount + "&backUrl = " + backUrl + "\"}");
                    }
                }
                else
                {
                    //新手标二期前端限制
                    var pr = linqContext.li_projects.SingleOrDefault(p => p.id == projectId);
                    if (pr.IsNewbieProject2())
                    {
                        var wallet = linqContext.li_wallets.Single(w => w.user_id == user.id);
                        if (100000 < wallet.total_investment)
                        {
                            context.Response.Write(JsonConvert.SerializeObject(new { msg = "对不起，您的累计投资金额已经超过100000，不能再投资新手标！", status = 0 }));
                            return;
                        }
                        var newbieProjectInvested = wallet.dt_users.li_project_transactions.Where(tra =>
                            tra.li_projects.IsNewbieProject2() &&
                            tra.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                            tra.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest)
                            .Aggregate(0m, (sum, ptr) => sum + ptr.principal);
                        if (100000 < newbieProjectInvested + investingAmount)
                        {
                            context.Response.Write(JsonConvert.SerializeObject(new { msg = $"新手标累计投资不能超过 100000，您剩余可投 {100000 - newbieProjectInvested}元", status = 0 }));
                            return;
                        }
                    }

                    if (string.IsNullOrEmpty(backUrl))
                    {
                        int reqApi = huoqi.Equals("True") ? (int)Agp2pEnums.SumapayApiEnum.McBid : (int)Agp2pEnums.SumapayApiEnum.MaBid;
                        context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" + reqApi
                                           + "&userId=" + user.id + "&projectCode=" + projectId + "&sum=" + investingAmount
                                           + "&projectSum=" + projectSum + "&projectDescription=" +
                                           projectDescription + "&ticketId=" + ticketId + "\"}");
                    }
                    else
                    {
                        int reqApi = huoqi.Equals("True") ? (int)Agp2pEnums.SumapayApiEnum.McBiM : (int)Agp2pEnums.SumapayApiEnum.MaBiM;
                        context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" + reqApi
                                               + "&userId=" + user.id + "&projectCode=" + projectId + "&sum=" +
                                               investingAmount + "&projectSum=" + projectSum + "&projectDescription=" +
                                               projectDescription + "&backUrl = " + backUrl + "\"}");
                    }
                }
            }
            catch (Exception e)
            {
                context.Response.Write(JsonConvert.SerializeObject(new { msg = "投资失败：" + e.Message, status = 0 }));
            }
        }

        #region 修改用户信息OK=================================
        private void user_edit(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }

            model.nick_name = DTRequest.GetFormString("nickName");
            if (!string.IsNullOrEmpty(model.nick_name) && model.nick_name.Length< 4)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户昵称不能少于4个字符！\"}");
                return;
            }
            var xssCheckRegex = new Regex(@"^[^<>]*$");
            if (!xssCheckRegex.IsMatch(model.nick_name))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户昵称不能包含特殊字符！\"}");
                return;
            }
            model.sex = DTRequest.GetFormString("sex");
            if (!xssCheckRegex.IsMatch(model.sex))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，性别不能包含特殊字符！\"}");
                return;
            }
            var birthdayStr = DTRequest.GetFormString("birthday");
            if (!xssCheckRegex.IsMatch(birthdayStr))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，生日不能包含特殊字符！\"}");
                return;
            }
            DateTime birthday;
            if (DateTime.TryParse(birthdayStr, out birthday))
            {
                model.birthday = birthday;
            }

            model.area = DTRequest.GetFormString("area"); // 逗号隔开
            if (!xssCheckRegex.IsMatch(model.area))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，所在城市不能包含特殊字符！\"}");
                return;
            }
            model.qq = DTRequest.GetFormString("qq");
            if (!xssCheckRegex.IsMatch(model.qq))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，QQ不能包含特殊字符！\"}");
                return;
            }
            model.address = DTRequest.GetFormString("address");
            if (!xssCheckRegex.IsMatch(model.address))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，居住地址不能包含特殊字符！\"}");
                return;
            }

            //执行修改操作
            //model.password = DESEncrypt.Encrypt(password, model.salt);
            //model.password = DESEncrypt.Encrypt(password, model.salt);
            new users().Update(model);
            context.Response.Write("{\"status\":1, \"msg\":\"您的会员信息已修改成功！\"}");
        }
        #endregion

        #region 修改登录密码OK=================================
        private void user_password_edit(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            int user_id = model.id;
            string oldpassword = DTRequest.GetFormString("txtOldPassword");
            string password = DTRequest.GetFormString("txtPassword");
            //检查输入的旧密码
            if (string.IsNullOrEmpty(oldpassword))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请输入您的旧登录密码！\"}");
                return;
            }
            //检查输入的新密码
            if (string.IsNullOrEmpty(password))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请输入您的新登录密码！\"}");
                return;
            }
            //旧密码是否正确
            if (model.password != DESEncrypt.Encrypt(oldpassword, model.salt))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，您输入的旧密码不正确！\"}");
                return;
            }
            //执行修改操作
            model.password = DESEncrypt.Encrypt(password, model.salt);
            new BLL.users().Update(model);
            context.Response.Write("{\"status\":1, \"msg\":\"您的密码已修改成功，请记住新密码！\"}");
            return;
        }
        #endregion

        #region  积分抽奖======================
        private void point_lottery(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先登录\"}");
                return;
            }
            //检查积分是否足够
            if (model.point < 10)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，您的积分不足\"}");
                return;
            }
            var msg = new UserPointMsg(model.id, model.user_name, (int) Agp2pEnums.PointEnum.Lottery);
            MessageBus.Main.Publish(msg);
            int getPoints = DTRequest.GetFormInt("getPoints");
            var msg2 = new UserPointMsg(model.id, model.user_name, (int) Agp2pEnums.PointEnum.LotteryGet, getPoints);
            MessageBus.Main.Publish(msg2);
            context.Response.Write("{\"status\":1, \"msg\":\"抽奖完成\"}");
        }
        #endregion

        #region  签到积分====================
        private void point_qiandao(HttpContext context)
        {
            var agContext = new Agp2pDataContext();
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先登录\"}");
                return;
            }
            var signLogs = agContext.dt_user_sign_log.Where(s => s.user_id == model.id && s.sign_time == DateTime.Today);
            if (signLogs.Any())
            {
                context.Response.Write("{\"status\":0, \"msg\":\"您今天已签到\"}");
            }
            else
            {
                var signYesterday = agContext.dt_user_sign_log.Where(s => s.user_id == model.id && s.sign_time == DateTime.Today.AddDays(-1));
                var signCount = 1;
                if (signYesterday.Any())
                {
                    signCount = Convert.ToInt32(signYesterday.First().sign_count) + 1;
                }
                var newSign = new dt_user_sign_log
                {
                    user_id = model.id,
                    sign_time = DateTime.Today,
                    sign_count = signCount.ToString()
                };
                agContext.dt_user_sign_log.InsertOnSubmit(newSign);
                agContext.SubmitChanges();
                if (signCount % 5 == 1)
                {
                    var msg = new UserPointMsg(model.id, model.user_name, (int)Agp2pEnums.PointEnum.Sign, 1)
                    {
                        Remark = "第一天签到"
                    };
                    MessageBus.Main.Publish(msg);
                    context.Response.Write("{\"status\":1, \"msg\":\"第一天签到\"}");
                    return;
                }
                if (signCount % 5 == 2)
                {
                    var msg = new UserPointMsg(model.id, model.user_name, (int)Agp2pEnums.PointEnum.Sign, 2)
                    {
                        Remark = "第二天签到"
                    };
                    MessageBus.Main.Publish(msg);
                    context.Response.Write("{\"status\":1, \"msg\":\"第二天签到\"}");
                    return;
                }
                if (signCount % 5 == 3)
                {
                    var msg = new UserPointMsg(model.id, model.user_name, (int)Agp2pEnums.PointEnum.Sign, 3)
                    {
                        Remark = "第三天签到"
                    };
                    MessageBus.Main.Publish(msg);
                    context.Response.Write("{\"status\":1, \"msg\":\"第三天签到\"}");
                    return;
                }
                if (signCount % 5 == 4)
                {
                    var msg = new UserPointMsg(model.id, model.user_name, (int)Agp2pEnums.PointEnum.Sign, 4)
                    {
                        Remark = "第四天签到"
                    };
                    MessageBus.Main.Publish(msg);
                    context.Response.Write("{\"status\":1, \"msg\":\"第四天签到\"}");
                    return;
                }
                if (signCount % 5 == 0)
                {
                    var msg = new UserPointMsg(model.id, model.user_name, (int)Agp2pEnums.PointEnum.Sign, 5)
                    {
                        Remark = "第五天签到"
                    };
                    MessageBus.Main.Publish(msg);
                    context.Response.Write("{\"status\":1, \"msg\":\"第五天签到\"}");
                    return;
                }
            }
        }
        #endregion

        /// <summary>
        /// 添加银行卡
        /// </summary>
        /// <param name="context"></param>
        private void add_bank_card(HttpContext context)
        {
            var user = BasePage.GetUserInfoByLinq();
            if (user == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.identity_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"为了您的资金安全，请先到安全中心开通第三方托管账户！\"}");
                return;
            }

            var linq_context = new Agp2pDataContext();
            try
            {
                string cardNo = DTRequest.GetFormString("card_no");
                string bank_name = DTRequest.GetFormString("bank_name");
                string province = DTRequest.GetFormString("province");
                string city = DTRequest.GetFormString("city");
                string area = DTRequest.GetFormString("area");
                string branch_name = DTRequest.GetFormString("branch_name");

                if (!new Regex(@"^\d{16,}$").IsMatch(cardNo))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"银行卡号格式不正确\"}");
                    return;
                }
                if (!new Regex(@"^[\u4e00-\u9fa5]+$").IsMatch(bank_name) || bank_name.Contains("请选择"))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"银行名称格式不正确\"}");
                    return;
                }
                if (!new Regex(@"^[^<>]*$").IsMatch(branch_name))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"开户行名称格式不正确\"}");
                    return;
                }

                var bank = new li_bank_accounts
                {
                    account = cardNo,
                    bank = bank_name,
                    opening_bank = branch_name,
                    location = province + ";" + city + ";" + area,
                    last_access_time = DateTime.Now,
                    owner = user.id,
                    type = (int)Agp2pEnums.BankAccountType.Unknown,
                };
                linq_context.li_bank_accounts.InsertOnSubmit(bank);
                linq_context.SubmitChanges();

                context.Response.Write("{\"status\":1, \"msg\":\"添加银行卡成功！\"}");
            }
            catch (Exception e)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"添加银行卡失败：" + e.Message + "\"}");
            }
        }

        /// <summary>
        /// 修改银行卡
        /// </summary>
        /// <param name="context"></param>
        private void update_bank_card(HttpContext context)
        {
            var user = BasePage.GetUserInfoByLinq();
            if (user == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.identity_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"为了您的资金安全，请先到安全中心开通第三方托管账户！\"}");
                return;
            }

            var linq_context = new Agp2pDataContext();
            try
            {
                int cid = DTRequest.GetFormInt("hidid");
                string bank_name = DTRequest.GetFormString("bank_name_u");
                string province = DTRequest.GetFormString("province_u");
                string city = DTRequest.GetFormString("city_u");
                string area = DTRequest.GetFormString("area_u");
                string branch_name = DTRequest.GetFormString("branch_name_u");

                if (!new Regex(@"^[\u4e00-\u9fa5]+$").IsMatch(bank_name) || bank_name.Contains("请选择"))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"银行名称格式不正确\"}");
                    return;
                }
                if (!new Regex(@"^[^<>]*$").IsMatch(branch_name))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"开户行名称格式不正确\"}");
                    return;
                }
                var bank = linq_context.li_bank_accounts.First(b => b.owner == user.id && b.id == cid);
                bank.bank = bank_name;
                bank.opening_bank = branch_name;
                bank.location = province + ";" + city + ";" + area;
                bank.last_access_time = DateTime.Now;
                bank.owner = user.id;
                linq_context.SubmitChanges();

                context.Response.Write("{\"status\":1, \"msg\":\"修改银行卡成功！\"}");
            }
            catch (Exception e)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"修改银行卡失败：" + e.Message + "\"}");
            }
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="context"></param>
        private void recharge(HttpContext context)
        {
            var user = BasePage.GetUserInfoByLinq();
            if (user == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.identity_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"为了您的资金安全，请先到安全中心开通第三方托管账户！\"}");
                return;
            }

            var bankCode = DTRequest.GetFormString("bankCode");
            var rechargeSum = DTRequest.GetFormString("rechargeSum");
            var quickPayment = bool.Parse(DTRequest.GetFormString("quickPayment"));
            var backUrl = DTRequest.GetFormString("backUrl");
            if (string.IsNullOrWhiteSpace(rechargeSum))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请输入正确的金额！\"}");
                return;
            }
            if (Utils.StrToDecimal(rechargeSum, 0) < 100)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"抱歉，最低充值100元！\"}");
                return;
            }
            if (!quickPayment)
            {
                if (string.IsNullOrWhiteSpace(bankCode))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"请选择银行卡！\"}");
                    return;
                }
                context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" +
                                       (int)Agp2pEnums.SumapayApiEnum.WeRec + "&userId=" + user.id + "&sum=" +
                                       rechargeSum + "&bankCode=" +
                                       bankCode + "\"}");
            }
            else
            {
                //有返回地址参数的是移动端请求
                if (string.IsNullOrEmpty(backUrl))
                    context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" +
                                       (int)Agp2pEnums.SumapayApiEnum.WhRec + "&userId=" + user.id + "&sum=" +
                                       rechargeSum + "\"}");
                else
                    context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" +
                                       (int)Agp2pEnums.SumapayApiEnum.WhReM + "&userId=" + user.id + "&sum=" +
                                       rechargeSum + "&backUrl=" + backUrl + "\"}");
            }

        }

        /// <summary>
        /// 客户提现
        /// </summary>
        /// <param name="context"></param>
        private void withdraw(HttpContext context)
        {
            var user = BasePage.GetUserInfoByLinq();
            if (user == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.identity_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"为了您的资金安全，请先到安全中心开通第三方托管账户！\"}");
                return;
            }

            var cardId = DTRequest.GetFormInt("cardId", 0);
            var bankName = DTRequest.GetFormString("bankName");
            var bankAccount = DTRequest.GetFormString("bankAccount");
            var howmany = DTRequest.GetFormDecimal("howmany", 0);
            var backUrl = DTRequest.GetFormString("backUrl");
            if (howmany <= 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请输入正确的金额！\"}");
                return;
            }
            // 提现 100 起步，50w 封顶
            if (howmany < 100)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"提现金额最低 100 元！\"}");
                return;
            }
            if (howmany > 500000)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"提现金额最高 500000 元！\"}");
                return;
            }
            if (cardId <= 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请选择银行卡！\"}");
                return;
            }
            // 判断提现次数，每人每日的提现次数不能超过 3 次
            var withdrawTimesToday = new Agp2pDataContext().li_bank_transactions.Count(btr => btr.li_bank_accounts.owner == user.id
                && btr.type == (int)Agp2pEnums.BankTransactionTypeEnum.Withdraw
                && btr.status != (int)Agp2pEnums.BankTransactionStatusEnum.Cancel && btr.create_time.Date == DateTime.Today);
            if (3 <= withdrawTimesToday)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"每日每张卡的提现次数不能超过 3 次！\"}");
            }

            //TODO 在丰付托管平台绑定银行卡后只能使用绑定卡来提现
            if (string.IsNullOrEmpty(backUrl))
            {
                context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" +
                                       (int)Agp2pEnums.SumapayApiEnum.Wdraw + "&userId=" + user.id + "&sum=" +
                                       howmany + "&bankId=" +
                                       cardId + "&bankName=" +
                                       bankName + "&bankAccount=" +
                                       bankAccount + "\"}");
            }
            else
            {
                context.Response.Write("{\"status\":1, \"url\":\"/api/payment/sumapay/index.aspx?api=" +
                                       (int)Agp2pEnums.SumapayApiEnum.WdraM + "&userId=" + user.id + "&sum=" +
                                       howmany + "&bankId=" + cardId + "&backUrl=" + backUrl + "\"}");
            }
        }

        /// <summary>
        /// 删除客户银行卡
        /// </summary>
        /// <param name="context"></param>
        private void user_bank_card_delete(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            int card_id = DTRequest.GetFormInt("cardId", 0);
            if (card_id == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"删除失败，请检查传输参数！\"}");
                return;
            }

            try
            {
                var linqContext = new Agp2pDataContext();
                var card = linqContext.li_bank_accounts.FirstOrDefault(b => b.id == card_id);
                if (card == null)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"查询银行卡信息失败！\"}");
                    return;
                }
                linqContext.li_bank_accounts.DeleteOnSubmit(card);
                linqContext.SubmitChanges();
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"删除银行卡失败！银行卡有提现记录不允许删除。（" + ex.Message + "）\"}");
                return;
            }

            context.Response.Write("{\"status\":1, \"msg\":\"删除银行卡成功！\"}");
        }

        /// <summary>
        /// 生成用户投资协议
        /// </summary>
        /// <param name="httpContext"></param>
        private void GenerateUserInvestContract(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "text/html; charset=utf-8";
            httpContext.Response.TrySkipIisCustomErrors = true;
            try
            {
                int id = DTRequest.GetQueryInt("id", 0);
                int userId = DTRequest.GetQueryInt("user_id", 0);
                var dbContext = new Agp2pDataContext();
                //检查用户是否登录
                var user = userId > 0 ? dbContext.dt_users.SingleOrDefault(u => u.id == userId) : BasePage.GetUserInfoByLinq(dbContext);
                if (user == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    httpContext.Response.Write("对不起，请先登录！");
                    return;
                }

                li_project_transactions investment;
                if (id == 0)
                {
                    int projectId = DTRequest.GetQueryInt("projectId", 0);
                    decimal investAmount = DTRequest.GetQueryDecimal("investAmount", 0);
                    var proj = dbContext.li_projects.Single(p => p.id == projectId);

                    // 生成一个临时的交易记录来显示投资协议
                    investment = new li_project_transactions
                    {
                        create_time = DateTime.Now,
                        agree_no = "（投资后生成）",
                        li_projects = proj,
                        dt_users = user,
                        principal = investAmount,
                        interest = Math.Round(proj.GetFinalProfitRate(DateTime.Now) * investAmount, 2),
                        status = (byte)Agp2pEnums.ProjectTransactionStatusEnum.Pending,
                        type = (byte)Agp2pEnums.ProjectTransactionTypeEnum.Invest,
                    };
                }
                else
                {
                    investment = dbContext.li_project_transactions.SingleOrDefault(t => t.id == id && t.investor == user.id);
                }

                if (investment == null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    httpContext.Response.Write("对不起，没有找到投资记录！");
                    return;
                }

                //获取投资协议
                string bodytxt = dbContext.GetInvestContractContext(investment, httpContext.Request.MapPath("./invest-agreement.html"));
                httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                httpContext.Response.Write(bodytxt);
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.Write("内部错误：生成投资协议失败，请联系客服");
            }
        }

        /// <summary>
        /// 实名认证
        /// </summary>
        /// <param name="context"></param>
        private void bind_idcard(HttpContext context)
        {
            try
            {
                //检查用户是否登录
                var dataContext = new Agp2pDataContext();
                var user = BasePage.GetUserInfoByLinq(dataContext);
                if (user == null)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                    return;
                }

                var idcard = DTRequest.GetFormString("idCardNumber");
                var truename = DTRequest.GetFormString("trueName");

                // 判断身份证是否重复
                var count = dataContext.dt_users.Count(u => u.id != user.id && u.id_card_number == idcard);
                if (count != 0)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"对不起，身份证号已经存在！\"}");
                    return;
                }
                // 判断是否已锁定，锁定后不能再改
                //var dtUsers = dataContext.dt_users.Single(u => u.id == user.id);
                if (!string.IsNullOrWhiteSpace(user.real_name) && !string.IsNullOrWhiteSpace(user.id_card_number))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"你已经认证过了\"}");
                    return;
                }
                if (!new Regex(@"^[\u4e00-\u9fa5·]{2,15}$").IsMatch(truename))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"请输入正确的中文名称\"}");
                    return;
                }
                if (!new Regex(@"^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9]|X)$", RegexOptions.IgnoreCase).IsMatch(idcard))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"请输入正确的 18 位中华人民共和国公民身份号码\"}");
                    return;
                }

                if (6 <= dataContext.QueryEventTimesDuring(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, TimeSpan.FromDays(365)))
                {
                    context.Response.Write(JsonConvert.SerializeObject(new { status = 0, msg = "你在一年内已经进行过 6 次身份认证，不能再继续认证了。如有疑问，请联系客服" }));
                    return;
                }
                if (3 <= dataContext.QueryEventTimesDuring(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, TimeSpan.FromDays(1)))
                {
                    context.Response.Write(JsonConvert.SerializeObject(new { status = 0, msg = "你在一天内已经进行过 3 次身份认证，请明日再试，并务必填写正确的认证信息" }));
                    return;
                }

#if !DEBUG
                //使用免费接口先核实有效的身份证
                var result = Utils.HttpGet("http://apis.juhe.cn/idcard/index?key=dc1c29e8a25f095fd7069193fb802144&cardno=" + idcard);
                var resultModel = JsonConvert.DeserializeObject<dto_user_idcard>(result);
                if (resultModel != null)
                {
                    if (resultModel.Resultcode == "200")
                    {
                        //保存地区信息
                        user.address = resultModel.Result.Area;
                        user.sex = resultModel.Result.Sex;
                        user.birthday = DateTime.Parse(resultModel.Result.Birthday);
#endif
                        // 记录接口调用
                        dataContext.MarkEventOccurNotSave(user.id, Agp2pEnums.EventRecordTypeEnum.IdcardChecking, DateTime.Now);
                        dataContext.SubmitChanges();

                        //调用托管平台实名验证接口
                        var msg = new UserRealNameAuthReqMsg(user.id, truename, idcard);
                        MessageBus.Main.Publish(msg);
                        //处理实名验证返回结果
                        var msgResp = BaseRespMsg.NewInstance<UserRealNameAuthRespMsg>(msg.SynResult);
                        MessageBus.Main.Publish(msgResp);
                        if (msgResp.HasHandle)
                        {
                            var msgPoints = new UserPointMsg(user.id, user.user_name, (int)Agp2pEnums.PointEnum.Sign);
                            MessageBus.Main.Publish(msgPoints);
                            context.Response.Write("{\"status\":1, \"msg\":\"身份证认证成功！\"}");
                        }
                        else
                        {
                            context.Response.Write("{\"status\":0, \"msg\":\"身份证认证失败：" + msgResp.Remarks + "\"}");
                        }
#if !DEBUG
                    }
                    else
                        context.Response.Write("{\"status\":0, \"msg\":\"身份证认证失败：" + resultModel.Reason + "\"}");
                }
#endif
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"身份证认证失败：" + ex.Message + "\"}");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}