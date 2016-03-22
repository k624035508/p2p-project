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
using Agp2p.Core;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
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
                case "validate_username": //验证用户名
                    validate_username(context);
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
                case "user_info_edit": //修改用户信息
                    user_info_edit(context);
                    break;
                case "user_avatar_crop": //确认裁剪用户头像
                    user_avatar_crop(context);
                    break;
                case "user_password_edit": //修改密码
                    user_password_edit(context);
                    break;
                case "user_getpassword": //邮箱取回密码
                    user_getpassword(context);
                    break;
                case "user_repassword": //邮箱重设密码
                    user_repassword(context);
                    break;
                case "user_message_delete": //删除短信息
                    user_message_delete(context);
                    break;
                case "user_message_add": //发布站内短消息
                    user_message_add(context);
                    break;
                case "user_point_convert": //用户兑换积分
                    user_point_convert(context);
                    break;
                case "user_point_delete": //删除用户积分明细
                    user_point_delete(context);
                    break;
                case "user_amount_recharge": //用户在线充值
                    user_amount_recharge(context);
                    break;
                case "user_amount_delete": //删除用户收支明细
                    user_amount_delete(context);
                    break;
                case "cart_goods_add": //购物车加入商品
                    cart_goods_add(context);
                    break;
                case "cart_goods_update": //购物车修改商品
                    cart_goods_update(context);
                    break;
                case "cart_goods_delete": //购物车删除商品
                    cart_goods_delete(context);
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
                case "view_cart_count": //输出当前购物车总数
                    view_cart_count(context);
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
                case "user_bank_card_show": //显示客户银行卡
                    user_bank_card_show(context);
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
            string result =verify_code(context, code);
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
            string inviteNo = DTRequest.GetFormString("txtInviteNo").Trim();
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
                if (!string.IsNullOrEmpty(inviteNo) || !string.IsNullOrEmpty(invitecode))
                {
                    dt_users inviteUser = null;
                    if (!string.IsNullOrEmpty(inviteNo))
                        inviteUser = linqContext.dt_users.FirstOrDefault(u => u.user_name == inviteNo || u.mobile == inviteNo);
                    else
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
            //赠送积分金额
            if (modelGroup.point > 0)
            {
                new BLL.user_point_log().Add(model.id, string.IsNullOrEmpty(model.real_name) ? model.user_name :
                    $"{model.user_name}({model.real_name})", modelGroup.point, "注册赠送积分", false);
            }
            if (modelGroup.amount > 0)
            {
                new BLL.user_amount_log().Add(model.id, model.user_name, DTEnums.AmountTypeEnum.SysGive.ToString(), modelGroup.amount, "注册赠送金额", 1);
            }
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

        #region 修改用户信息OK=================================
        private void user_info_edit(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            string email = Utils.ToHtml(DTRequest.GetFormString("txtEmail"));
            string nick_name = Utils.ToHtml(DTRequest.GetFormString("txtNickName"));
            string sex = DTRequest.GetFormString("rblSex");
            string birthday = DTRequest.GetFormString("txtBirthday");
            string telphone = Utils.ToHtml(DTRequest.GetFormString("txtTelphone"));
            string mobile = Utils.ToHtml(DTRequest.GetFormString("txtMobile"));
            string qq = Utils.ToHtml(DTRequest.GetFormString("txtQQ"));
            string address = Utils.ToHtml(context.Request.Form["txtAddress"]);
            string safe_question = Utils.ToHtml(context.Request.Form["txtSafeQuestion"]);
            string safe_answer = Utils.ToHtml(context.Request.Form["txtSafeAnswer"]);
            //检查昵称
            if (nick_name == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入您的姓名昵称！\"}");
                return;
            }
            //检查邮箱
            if (userConfig.emaillogin == 1 && email == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入您邮箱帐号！\"}");
                return;
            }
            //检查手机
            if (userConfig.mobilelogin == 1 && mobile == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入手机号码！\"}");
                return;
            }
            
            //开始写入数据库
            model.email = email;
            model.nick_name = nick_name;
            model.sex = sex;
            DateTime _birthday;
            if (DateTime.TryParse(birthday, out _birthday))
            {
                model.birthday = _birthday;
            }
            model.telphone = telphone;
            model.mobile = mobile;
            model.qq = qq;
            model.address = address;
            model.safe_question = safe_question;
            model.safe_answer = safe_answer;

            
            new BLL.users().Update(model);
            context.Response.Write("{\"status\":1, \"msg\":\"账户资料已修改成功！\"}");
            return;
        }
        #endregion

        #region 确认裁剪用户头像OK=============================
        private void user_avatar_crop(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            string fileName = DTRequest.GetFormString("hideFileName");
            int x1 = DTRequest.GetFormInt("hideX1");
            int y1 = DTRequest.GetFormInt("hideY1");
            int w = DTRequest.GetFormInt("hideWidth");
            int h = DTRequest.GetFormInt("hideHeight");
            //检查是否图片

            //检查参数
            if (!Utils.FileExists(fileName) || w == 0 || h == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请先上传一张图片！\"}");
                return;
            }
            //取得保存的新文件名
            UpLoad upFiles = new UpLoad();
            bool result = upFiles.cropSaveAs(fileName, fileName, 180, 180, w, h, x1, y1);
            if (!result)
            {
                context.Response.Write("{\"status\": 0, \"msg\": \"图片裁剪过程中发生意外错误！\"}");
                return;
            }
            //删除原用户头像
            Utils.DeleteFile(model.avatar);
            model.avatar = fileName;
            //修改用户头像
            new BLL.users().UpdateField(model.id, "avatar='" + model.avatar + "'");
            context.Response.Write("{\"status\": 1, \"msg\": \"" + model.avatar + "\"}");
            return;
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

        #region 邮箱取回密码OK=================================
        private void user_getpassword(HttpContext context)
        {
            string code = DTRequest.GetFormString("txtCode");
            string username = DTRequest.GetFormString("txtUserName").Trim();
            //检查用户名是否正确
            if (string.IsNullOrEmpty(username))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户名不可为空！\"}");
                return;
            }
            //校检验证码
            string result = verify_code(context, code);
            if (result != "success")
            {
                context.Response.Write(result);
                return;
            }
            //检查用户信息
            BLL.users bll = new BLL.users();
            Model.users model = bll.GetModel(username);
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，您输入的用户名不存在！\"}");
                return;
            }
            if (string.IsNullOrEmpty(model.email))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"您尚未设置邮箱地址，无法使用取回密码功能！\"}");
                return;
            }
            //生成随机码
            string strcode = Utils.GetCheckCode(20);
            //获得邮件内容
            Model.mail_template mailModel = new BLL.mail_template().GetModel("getpassword");
            if (mailModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"邮件发送失败，邮件模板内容不存在！\"}");
                return;
            }
            //检查是否重复提交
            BLL.user_code codeBll = new BLL.user_code();
            Model.user_code codeModel;
            codeModel = codeBll.GetModel(username, DTEnums.CodeEnum.RegVerify.ToString(), "d");
            if (codeModel == null)
            {
                codeModel = new Model.user_code();
                //写入数据库
                codeModel.user_id = model.id;
                codeModel.user_name = model.user_name;
                codeModel.type = DTEnums.CodeEnum.Password.ToString();
                codeModel.str_code = strcode;
                codeModel.eff_time = DateTime.Now.AddDays(userConfig.regemailexpired);
                codeModel.add_time = DateTime.Now;
                codeBll.Add(codeModel);
            }
            //替换模板内容
            string titletxt = mailModel.maill_title;
            string bodytxt = mailModel.content;
            titletxt = titletxt.Replace("{webname}", siteConfig.webname);
            titletxt = titletxt.Replace("{username}", model.user_name);
            bodytxt = bodytxt.Replace("{webname}", siteConfig.webname);
            bodytxt = bodytxt.Replace("{weburl}", siteConfig.weburl);
            bodytxt = bodytxt.Replace("{webtel}", siteConfig.webtel);
            bodytxt = bodytxt.Replace("{valid}", userConfig.regemailexpired.ToString());
            bodytxt = bodytxt.Replace("{username}", model.user_name);
            bodytxt = bodytxt.Replace("{linkurl}", "http://" + HttpContext.Current.Request.Url.Authority.ToLower() + new BasePage().linkurl("repassword", "reset", strcode));
            //发送邮件
            try
            {
                DTMail.sendMail(siteConfig.emailsmtp,
                    siteConfig.emailusername,
                    DESEncrypt.Decrypt(siteConfig.emailpassword),
                    siteConfig.emailnickname,
                    siteConfig.emailfrom,
                    model.email,
                    titletxt, bodytxt);
            }
            catch
            {
                context.Response.Write("{\"status\":0, \"msg\":\"邮件发送失败，请联系本站管理员！\"}");
                return;
            }
            context.Response.Write("{\"status\":1, \"msg\":\"邮件发送成功，请登录您的邮箱找回登录密码！\"}");
            return;
        }
        #endregion

        #region 邮箱重设密码OK=================================
        private void user_repassword(HttpContext context)
        {
            string code = context.Request.Form["txtCode"];
            string strcode = context.Request.Form["hideCode"];
            string password = context.Request.Form["txtPassword"];

            //校检验证码
            string result = verify_code(context, code);
            if (result != "success")
            {
                context.Response.Write(result);
                return;
            }
            //检查验证字符串
            if (string.IsNullOrEmpty(strcode))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"邮件验证码不存在或已删除！\"}");
                return;
            }
            //检查输入的新密码
            if (string.IsNullOrEmpty(password))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请输入您的新密码！\"}");
                return;
            }

            BLL.user_code codeBll = new BLL.user_code();
            Model.user_code codeModel = codeBll.GetModel(strcode);
            if (codeModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"邮件验证码不存在或已过期！\"}");
                return;
            }
            //验证用户是否存在
            BLL.users userBll = new BLL.users();
            if (!userBll.Exists(codeModel.user_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"该用户不存在或已被删除！\"}");
                return;
            }
            Model.users userModel = userBll.GetModel(codeModel.user_id);
            //执行修改操作
            userModel.password = DESEncrypt.Encrypt(password, userModel.salt);
            userBll.Update(userModel);
            //更改验证字符串状态
            codeModel.count = 1;
            codeModel.status = 1;
            codeBll.Update(codeModel);
            context.Response.Write("{\"status\":1, \"msg\":\"修改密码成功，请记住新密码！\"}");
            return;
        }
        #endregion

        #region 删除短消息OK===================================
        private void user_message_delete(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            string check_id = DTRequest.GetFormString("checkId");
            if (check_id == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"删除失败，请检查传输参数！\"}");
                return;
            }
            string[] arrId = check_id.Split(',');
            for (int i = 0; i < arrId.Length; i++)
            {
                new BLL.user_message().Delete(int.Parse(arrId[i]), model.user_name);
            }
            context.Response.Write("{\"status\":1, \"msg\":\"删除短消息成功！\"}");
            return;
        }
        #endregion

        #region 发布站内短消息OK===============================
        private void user_message_add(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            string code = context.Request.Form["txtCode"];
            string send_save = DTRequest.GetFormString("sendSave");
            string user_name = Utils.ToHtml(DTRequest.GetFormString("txtUserName"));
            string title = Utils.ToHtml(DTRequest.GetFormString("txtTitle"));
            string content = Utils.ToHtml(DTRequest.GetFormString("txtContent"));
            //校检验证码
            string result = verify_code(context, code);
            if (result != "success")
            {
                context.Response.Write(result);
                return;
            }
            //检查用户名
            if (user_name == "" || !new BLL.users().Exists(user_name))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，该用户不存在或已删除！\"}");
                return;
            }
            //检查标题
            if (title == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入短消息标题！\"}");
                return;
            }
            //检查内容
            if (content == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入短消息内容！\"}");
                return;
            }
            //保存数据
            Model.user_message modelMessage = new Model.user_message
            {
                type = 2,
                post_user_name = model.user_name,
                accept_user_name = user_name,
                title = title,
                content = Utils.ToHtml(content),
                receiver = model.id
            };
            new BLL.user_message().Add(modelMessage);
            if (send_save == "true") //保存到收件箱
            {
                modelMessage.type = 3;
                new BLL.user_message().Add(modelMessage);
            }
            context.Response.Write("{\"status\":1, \"msg\":\"发布短信息成功！\"}");
            return;
        }
        #endregion

        #region 用户兑换积分OK=================================
        private void user_point_convert(HttpContext context)
        {
            //检查系统是否启用兑换积分功能
            if (userConfig.pointcashrate == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，网站已关闭兑换积分功能！\"}");
                return;
            }
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            //int amout = DTRequest.GetFormInt("txtAmount");
            string password = DTRequest.GetFormString("txtPassword");
            //if (model.amount < 1)
            //{
            //    context.Response.Write("{\"status\":0, \"msg\":\"对不起，您账户上的余额不足！\"}");
            //    return;
            //}
            //if (amout < 1)
            //{
            //    context.Response.Write("{\"status\":0, \"msg\":\"对不起，最小兑换金额为1元！\"}");
            //    return;
            //}
            //if (amout > model.amount)
            //{
            //    context.Response.Write("{\"status\":0, \"msg\":\"对不起，您兑换的金额大于账户余额！\"}");
            //    return;
            //}
            if (password == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入您账户的密码！\"}");
                return;
            }
            //验证密码
            if (DESEncrypt.Encrypt(password, model.salt) != model.password)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，您输入的密码不正确！\"}");
                return;
            }
            //计算兑换后的积分值
            //int convertPoint = (int)(Convert.ToDecimal(amout) * userConfig.pointcashrate);
            //扣除金额
            //int amountNewId = new BLL.user_amount_log().Add(model.id, model.user_name, DTEnums.AmountTypeEnum.Convert.ToString(), amout * -1, "用户兑换积分", 1);
            //增加积分
            //if (amountNewId < 1)
            //{
            //    context.Response.Write("{\"status\":0, \"msg\":\"转换过程中发生错误，请重新提交！\"}");
            //    return;
            //}
            //int pointNewId = new BLL.user_point_log().Add(model.id, model.user_name, convertPoint, "用户兑换积分", true);
            //if (pointNewId < 1)
            //{
            //    //返还金额
            //    new BLL.user_amount_log().Add(model.id, model.user_name, DTEnums.AmountTypeEnum.Convert.ToString(), amout, "用户兑换积分失败，返还金额", 1);
            //    context.Response.Write("{\"status\":0, \"msg\":\"转换过程中发生错误，请重新提交！\"}");
            //    return;
            //}

            context.Response.Write("{\"status\":1, \"msg\":\"恭喜您，积分兑换成功！\"}");
            return;
        }
        #endregion

        #region 删除用户积分明细OK=============================
        private void user_point_delete(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            string check_id = DTRequest.GetFormString("checkId");
            if (check_id == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"删除失败，请检查传输参数！\"}");
                return;
            }
            string[] arrId = check_id.Split(',');
            for (int i = 0; i < arrId.Length; i++)
            {
                new BLL.user_point_log().Delete(int.Parse(arrId[i]), model.user_name);
            }
            context.Response.Write("{\"status\":1, \"msg\":\"积分明细删除成功！\"}");
            return;
        }
        #endregion

        #region 用户在线充值OK=================================
        private void user_amount_recharge(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            decimal amount = DTRequest.GetFormDecimal("order_amount", 0);
            int payment_id = DTRequest.GetFormInt("payment_id");
            if (amount == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入正确的充值金额！\"}");
                return;
            }
            if (payment_id == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请选择正确的支付方式！\"}");
                return;
            }
            if (!new BLL.payment().Exists(payment_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，支付方式不存在或已删除！\"}");
                return;
            }
            //生成订单号
            string order_no = "R" + Utils.GetOrderNumber(); //订单号R开头为充值订单
            new BLL.user_amount_log().Add(model.id, model.user_name, DTEnums.AmountTypeEnum.Recharge.ToString(), order_no,
                payment_id, amount, "账户充值(" + new BLL.payment().GetModel(payment_id).title + ")", 0);
            //保存成功后返回订单号
            context.Response.Write("{\"status\":1, \"msg\":\"订单保存成功！\", \"url\":\"" + new Web.UI.BasePage().linkurl("payment", "confirm", order_no) + "\"}");
            return;

        }
        #endregion

        #region 删除用户收支明细OK=============================
        private void user_amount_delete(HttpContext context)
        {
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            string check_id = DTRequest.GetFormString("checkId");
            if (check_id == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"删除失败，请检查传输参数！\"}");
                return;
            }
            string[] arrId = check_id.Split(',');
            for (int i = 0; i < arrId.Length; i++)
            {
                new BLL.user_amount_log().Delete(int.Parse(arrId[i]), model.user_name);
            }
            context.Response.Write("{\"status\":1, \"msg\":\"收支明细删除成功！\"}");
            return;
        }
        #endregion

        #region 购物车加入商品OK===============================
        private void cart_goods_add(HttpContext context)
        {
            string goods_id = DTRequest.GetFormString("goods_id");
            int goods_quantity = DTRequest.GetFormInt("goods_quantity", 1);
            if (goods_id == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"您提交的商品参数有误！\"}");
                return;
            }
            //查找会员组
            int group_id = 0;
            Model.users groupModel = BasePage.GetUserInfo();
            if (groupModel != null)
            {
                group_id = groupModel.group_id;
            }
            //统计购物车
            Web.UI.ShopCart.Add(goods_id, goods_quantity);
            Model.cart_total cartModel = Web.UI.ShopCart.GetTotal(group_id);
            context.Response.Write("{\"status\":1, \"msg\":\"商品已成功添加到购物车！\", \"quantity\":" + cartModel.total_quantity + ", \"amount\":" + cartModel.real_amount + "}");
            return;
        }
        #endregion

        #region 修改购物车商品OK===============================
        private void cart_goods_update(HttpContext context)
        {
            string goods_id = DTRequest.GetFormString("goods_id");
            int goods_quantity = DTRequest.GetFormInt("goods_quantity");
            if (goods_id == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"您提交的商品参数有误！\"}");
                return;
            }

            if (Web.UI.ShopCart.Update(goods_id, goods_quantity))
            {
                context.Response.Write("{\"status\":1, \"msg\":\"商品数量修改成功！\"}");
            }
            else
            {
                context.Response.Write("{\"status\":0, \"msg\":\"商品数量更改失败，请检查操作是否有误！\"}");
            }
            return;
        }
        #endregion

        #region 删除购物车商品OK===============================
        private void cart_goods_delete(HttpContext context)
        {
            string goods_id = DTRequest.GetFormString("goods_id");
            if (goods_id == "")
            {
                context.Response.Write("{\"status\":0, \"msg\":\"您提交的商品参数有误！\"}");
                return;
            }
            Web.UI.ShopCart.Clear(goods_id);
            context.Response.Write("{\"status\":1, \"msg\":\"商品移除成功！\"}");
            return;
        }
        #endregion

        #region 保存用户订单OK=================================
        private void order_save(HttpContext context)
        {
            //获得传参信息
            int payment_id = DTRequest.GetFormInt("payment_id");
            int express_id = DTRequest.GetFormInt("express_id");
            string accept_name = Utils.ToHtml(DTRequest.GetFormString("accept_name"));
            string post_code = Utils.ToHtml(DTRequest.GetFormString("post_code"));
            string telphone = Utils.ToHtml(DTRequest.GetFormString("telphone"));
            string mobile = Utils.ToHtml(DTRequest.GetFormString("mobile"));
            string address = Utils.ToHtml(DTRequest.GetFormString("address"));
            string message = Utils.ToHtml(DTRequest.GetFormString("message"));
            //获取订单配置信息
            Model.orderconfig orderConfig = new BLL.orderconfig().loadConfig();

            //检查物流方式
            if (express_id == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请选择配送方式！\"}");
                return;
            }
            Model.express expModel = new BLL.express().GetModel(express_id);
            if (expModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，配送方式不存在或已删除！\"}");
                return;
            }
            //检查支付方式
            if (payment_id == 0)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请选择支付方式！\"}");
                return;
            }
            Model.payment payModel = new BLL.payment().GetModel(payment_id);
            if (payModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，支付方式不存在或已删除！\"}");
                return;
            }
            //检查收货人
            if (string.IsNullOrEmpty(accept_name))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入收货人姓名！\"}");
                return;
            }
            //检查手机和电话
            if (string.IsNullOrEmpty(telphone) && string.IsNullOrEmpty(mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入收货人联系电话或手机！\"}");
                return;
            }
            //检查地址
            if (string.IsNullOrEmpty(address))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，请输入详细的收货地址！\"}");
                return;
            }
            //如果开启匿名购物则不检查会员是否登录
            int user_id = 0;
            int user_group_id = 0;
            string user_name = string.Empty;
            //检查用户是否登录
            Model.users userModel = BasePage.GetUserInfo();
            if (userModel != null)
            {
                user_id = userModel.id;
                user_group_id = userModel.group_id;
                user_name = userModel.user_name;
            }
            if (orderConfig.anonymous == 0 && userModel == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                return;
            }
            //检查购物车商品
            IList<Model.cart_items> iList = Agp2p.Web.UI.ShopCart.GetList(user_group_id);
            if (iList == null)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"对不起，购物车为空，无法结算！\"}");
                return;
            }
            //统计购物车
            Model.cart_total cartModel = Agp2p.Web.UI.ShopCart.GetTotal(user_group_id);
            //保存订单=======================================================================
            Model.orders model = new Model.orders();
            model.order_no = "B" + Utils.GetOrderNumber(); //订单号B开头为商品订单
            model.user_id = user_id;
            model.user_name = user_name;
            model.payment_id = payment_id;
            model.express_id = express_id;
            model.accept_name = accept_name;
            model.post_code = post_code;
            model.telphone = telphone;
            model.mobile = mobile;
            model.address = address;
            model.message = message;
            model.payable_amount = cartModel.payable_amount;
            model.real_amount = cartModel.real_amount;
            model.express_status = 1;
            model.express_fee = expModel.express_fee; //物流费用
            //如果是先款后货的话
            if (payModel.type == 1)
            {
                model.payment_status = 1; //标记未付款
                if (payModel.poundage_type == 1) //百分比
                {
                    model.payment_fee = model.real_amount * payModel.poundage_amount / 100;
                }
                else //固定金额
                {
                    model.payment_fee = payModel.poundage_amount;
                }
            }
            //订单总金额=实付商品金额+运费+支付手续费
            model.order_amount = model.real_amount + model.express_fee + model.payment_fee;
            //购物积分,可为负数
            model.point = cartModel.total_point;
            model.add_time = DateTime.Now;
            //商品详细列表
            List<Model.order_goods> gls = new List<Model.order_goods>();
            foreach (Model.cart_items item in iList)
            {
                gls.Add(new Model.order_goods { goods_id = item.id, goods_title = item.title, goods_price = item.price, real_price = item.user_price, quantity = item.quantity, point = item.point });
            }
            model.order_goods = gls;
            int result = new BLL.orders().Add(model);
            if (result < 1)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"订单保存过程中发生错误，请重新提交！\"}");
                return;
            }
            //扣除积分
            if (model.point < 0)
            {
                new BLL.user_point_log().Add(model.user_id, model.user_name, model.point, "积分换购，订单号：" + model.order_no, false);
            }
            //清空购物车
            Agp2p.Web.UI.ShopCart.Clear("0");
            //提交成功，返回URL
            context.Response.Write("{\"status\":1, \"url\":\"" + new Web.UI.BasePage().linkurl("payment", "confirm", model.order_no) + "\", \"msg\":\"恭喜您，订单已成功提交！\"}");
            return;
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

        #region 输出购物车总数OK===============================
        private void view_cart_count(HttpContext context)
        {
            int group_id = 0;
            //检查用户是否登录
            Model.users model = BasePage.GetUserInfo();
            if (model != null)
            {
                group_id = model.group_id;
            }
            Model.cart_total cartModel = Web.UI.ShopCart.GetTotal(group_id);
            context.Response.Write("document.write('" + cartModel.total_quantity + "');");
        }
        #endregion

        #region 通用外理方法OK=================================
        //校检网站验证码
        private string verify_code(HttpContext context,string strcode)
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
            if (string.IsNullOrEmpty(user.pay_password))
            {
                context.Response.Write(JsonConvert.SerializeObject(new {msg = "请先到安全中心设置交易密码", status = 0}));
                return;
            }
            if (string.IsNullOrWhiteSpace(user.real_name) || string.IsNullOrWhiteSpace(user.id_card_number))
            {
                context.Response.Write(JsonConvert.SerializeObject(new {msg = "请先到安全中心进行实名认证", status = 0}));
                return;
            }
            try
            {
                var investingMoney = DTRequest.GetFormDecimal("investingAmount", 0);
                var projectId = DTRequest.GetFormInt("projectId", 0);
                var pw = DTRequest.GetFormString("transactPassword");
                if (Utils.MD5(pw).Equals(user.pay_password))
                {
                    TransactionFacade.Invest(user.id, projectId, investingMoney);

                    /*if (DateTime.Now.Date <= new DateTime(2015, 7, 12) && proj.tag != (int)Agp2pEnums.ProjectTagEnum.Trial)
                            context.Response.Write("{\"status\":3, \"msg\":\"<div style='height:50px; line-height:50px;'><font style='font-size:16px;'>投资成功！恭喜亲【" + user.user_name + "】您通过活动期间投资项目" + investingMoney + "元获得了" + investingMoney + "元的天标卷！<br>活动期间投多少返多少，天天秒标天天领奖券！</font></div>\"}");
                        else*/
                    //context.Response.Write(JsonConvert.SerializeObject(new { msg = "投资成功！", status = 1 }));

                    //投标前调用托管接口确认投标，在投标异步响应中执行投标行为 TODO 项目总额、项目描述
                    ManualBidReqMsg msg = new ManualBidReqMsg(user.id, projectId.ToString(), investingMoney.ToString("n"), "projectSum", "projectDes");
                    MessageBus.Main.PublishAsync(msg, result =>
                    {
                        context.Response.Redirect(msg.RequestContent);
                    });
                }
                else
                    context.Response.Write(JsonConvert.SerializeObject(new {msg = "交易密码错误！", status = 0}));
            }
            catch (Exception e)
            {
                context.Response.Write(JsonConvert.SerializeObject(new {msg = "投资失败：" + e.Message, status = 0}));
            }
        }

        /// <summary>
        /// 添加银行卡
        /// </summary>
        /// <param name="context"></param>
        private void add_bank_card(HttpContext context)
        {
            var user = BasePage.GetUserInfoByLinq();
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.id_card_number))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心进行实名认证！\"}");
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
                    owner = user.id
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
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.id_card_number))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心进行实名认证！\"}");
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
            if (string.IsNullOrEmpty(user.pay_password))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心设置交易密码！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.id_card_number))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心进行实名认证！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.identity_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心进行托管账户开户！\"}");
                return;
            }

            try
            {
                var bankCode = DTRequest.GetFormString("bankCode");
                var rechargeSum = DTRequest.GetFormString("rechargeSum");
                var quickPayment = bool.Parse(DTRequest.GetFormString("quickPayment"));
                if (string.IsNullOrWhiteSpace(bankCode))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"请选择银行卡！\"}");
                    return;
                }
                if (string.IsNullOrWhiteSpace(rechargeSum))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"请输入正确的金额！\"}");
                    return;
                }
                //发送充值请求 TODO 分账列表
                BaseReqMsg reqMsg;
                if (quickPayment) reqMsg = new WhRechargeReqMsg(user.id, rechargeSum, "subledgerlist");
                else reqMsg = new WebRechargeReqMsg(user.id, rechargeSum, bankCode, "subledgerlist");
                MessageBus.Main.PublishAsync(reqMsg, ar =>
                {
                    context.Response.Write(reqMsg.RequestContent);
                });
            }
            catch (Exception e)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"提交充值请求失败：" + e.Message + "\"}");
            }
        }

        /// <summary>
        /// 客户提现
        /// </summary>
        /// <param name="context"></param>
        private void withdraw(HttpContext context)
        {            
            var user = BasePage.GetUserInfoByLinq();
            if (string.IsNullOrEmpty(user.pay_password))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心设置交易密码！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.mobile))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心绑定手机！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.id_card_number))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心进行实名认证！\"}");
                return;
            }
            if (string.IsNullOrEmpty(user.identity_id))
            {
                context.Response.Write("{\"status\":0, \"msg\":\"请先到安全中心进行托管账户开户！\"}");
                return;
            }

            try
            {
                var cardId = DTRequest.GetFormInt("cardId", 0);
                var howmany = DTRequest.GetFormDecimal("howmany", 0);
                var pw = DTRequest.GetFormString("transactPassword");

                if (cardId <= 0)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"请选择银行卡！\"}");
                    return;
                }
                if (howmany <= 0)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"请输入正确的金额！\"}");
                    return;
                }
                if (string.IsNullOrEmpty(pw))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"请输入交易密码！\"}");
                    return;
                }
                if (!Utils.MD5(pw).Equals(user.pay_password))
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"交易密码错误！\"}");
                    return;
                }

                //TODO 参数，提现同步返回
                var reqMsg = new WithdrawReqMsg(user.id, howmany.ToString("N"), "bankCode", "bankAccount", "subledgerlist");
                MessageBus.Main.PublishAsync(reqMsg, ar =>
                {
                    context.Response.Write(reqMsg.RequestContent);
                });
            }
            catch (Exception e)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"提现申请提交失败：" + e.Message + "\"}");
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
                context.Response.Write("{\"status\":0, \"msg\":\"删除银行卡失败！银行卡有提现记录不允许删除。（"+ex.Message+"）\"}");
                return;
            }

            context.Response.Write("{\"status\":1, \"msg\":\"删除银行卡成功！\"}");
        }

        /// <summary>
        /// TODO 展示银行卡信息
        /// </summary>
        /// <param name="context"></param>
        private void user_bank_card_show(HttpContext context)
        {
            try
            {
                int card_id = DTRequest.GetFormInt("cardId", 0);
                var linqContext = new Agp2pDataContext();
                var card = linqContext.li_bank_accounts.FirstOrDefault(b => b.id == card_id);
                if (card == null)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"查询银行卡信息失败！\"}");
                    return;
                }
                context.Response.ContentType = "application/json";
                var json = JsonConvert.SerializeObject(
                    new
                    {
                        status = 1,
                        cardNo = card.account,
                        bankName = card.bank,
                        opening_bank = card.opening_bank,
                        location = card.location
                    });
                context.Response.Write(json);
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"删除银行卡失败！银行卡有提现记录不允许删除。（" + ex.Message + "）\"}");
            }            
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
                    httpContext.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
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
                        interest = Math.Round(proj.GetFinalProfitRate(DateTime.Now)*investAmount, 2),
                        status = (byte) Agp2pEnums.ProjectTransactionStatusEnum.Pending,
                        type = (byte) Agp2pEnums.ProjectTransactionTypeEnum.Invest,
                    };
                }
                else
                {
                    investment = dbContext.li_project_transactions.SingleOrDefault(t => t.id == id && t.investor == user.id);
                }

                if (investment == null)
                {
                    httpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
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
                Model.users model = BasePage.GetUserInfo();
                if (model == null)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"对不起，用户尚未登录或已超时！\"}");
                    return;
                }

                var idcard = DTRequest.GetFormString("idCardNumber");
                var truename = DTRequest.GetFormString("trueName");
                var licontext = new Agp2pDataContext();
                // 判断身份证是否重复
                var count = licontext.dt_users.Count(u => u.id != model.id && u.id_card_number == idcard);
                if (count != 0)
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"对不起，身份证号已经存在！\"}");
                    return;
                }
                // 判断是否已锁定，锁定后不能再改
                var dtUsers = licontext.dt_users.Single(u => u.id == model.id);
                if (!string.IsNullOrWhiteSpace(dtUsers.real_name) && !string.IsNullOrWhiteSpace(dtUsers.id_card_number))
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

                //var result = Utils.HttpGet("http://apis.juhe.cn/idcard/index?key=dc1c29e8a25f095fd7069193fb802144&cardno=" + idcard);
                //var resultModel = JsonConvert.DeserializeObject<dto_user_idcard>(result);
                //if (resultModel != null)
                //{
                //    if (resultModel.Resultcode == "200")
                //    {
                //        var user = licontext.dt_users.Single(u => u.id == model.id);
                //        user.real_name = truename;
                //        user.id_card_number = idcard;
                //        user.address = resultModel.Result.Area;
                //        user.sex = resultModel.Result.Sex;
                //        user.birthday = DateTime.Parse(resultModel.Result.Birthday);                        
                //        licontext.SubmitChanges();

                //        context.Response.Write("{\"status\":1, \"msg\":\"身份证认证成功！\"}");
                //    }
                //    else
                //        context.Response.Write("{\"status\":0, \"msg\":\"身份证认证失败：" + resultModel.Reason + "\"}");
                //}

                //调用托管平台实名验证接口
                var msg = new UserRealNameAuthReqMsg(truename, idcard);
                MessageBus.Main.Publish(msg);
                //处理实名验证返回结果
                var msgResp = BaseRespMsg.NewInstance<UserRealNameAuthRespMsg>(msg.SynResult);
                MessageBus.Main.Publish(msgResp);
                if (msgResp.HasHandle)
                {
                    context.Response.Write("{\"status\":1,\"token\":" + msgResp.Token + ", \"msg\":\"身份证认证成功！\"}");
                }
                else
                {
                    context.Response.Write("{\"status\":0, \"msg\":\"身份证认证失败：" + msgResp.Remarks + "\"}");
                }
            }
            catch (Exception)
            {
                context.Response.Write("{\"status\":0, \"msg\":\"身份证认证失败！请输入正确的身份证号码\"}");
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
            if (!string.IsNullOrEmpty(model.nick_name) && model.nick_name.Length < 4)
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}