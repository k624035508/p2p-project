using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Web.Services;

namespace Agp2p.Web.UI.Page
{
    public partial class usercenter : Web.UI.UserPage
    {
        protected string action = string.Empty;
        protected string curr_login_ip = string.Empty;//当前登陆ip
        protected string pre_login_ip = string.Empty;//上次登陆ip
        protected string pre_login_time = string.Empty;//上次登陆时间
        protected int total_order;
        protected int total_msg;

        protected li_wallets wallet;//用户钱包

        protected string GetTotalMoney()
        {
            var total = wallet.idle_money + wallet.investing_money + wallet.locked_money + wallet.profiting_money;
            return 1e6m < total ? Math.Floor(total).ToString("N0") : total.ToString("N2");
        }

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            action = DTRequest.GetQueryString("action");

            //获得最后登录日志
            DataTable dt = new BLL.user_login_log().GetList(2, "user_name='" + userModel.user_name + "'", "id desc").Tables[0];
            if (dt.Rows.Count == 2)
            {
                curr_login_ip = dt.Rows[0]["login_ip"].ToString();
                pre_login_ip = dt.Rows[1]["login_ip"].ToString();
                pre_login_time = dt.Rows[1]["login_time"].ToString();
            }
            else if (dt.Rows.Count == 1)
            {
                curr_login_ip = dt.Rows[0]["login_ip"].ToString();
            }
            //未读短信息
            total_msg = new BLL.user_message().GetCount("accept_user_name='" + userModel.user_name + "' and is_read=0");

            //退出登录==========================================================
            if (action == "exit")
            {
                //清险Session
                HttpContext.Current.Session[DTKeys.SESSION_USER_INFO] = null;
                //清除Cookies
                Utils.WriteCookie(DTKeys.COOKIE_USER_NAME_REMEMBER, "Agp2p", -43200);
                Utils.WriteCookie(DTKeys.COOKIE_USER_PWD_REMEMBER, "Agp2p", -43200);
                Utils.WriteCookie("UserName", "Agp2p", -1);
                Utils.WriteCookie("Password", "Agp2p", -1);
                //自动登录,跳转URL
                HttpContext.Current.Response.Redirect(linkurl("login"));
            }

            //查询用户钱包
            var context = new Linq2SQL.Agp2pDataContext();
            wallet = context.li_wallets.FirstOrDefault(w => w.user_id == userModel.id);

        }


        [WebMethod]
        public static string AjaxQueryTransactionHistory(short pageIndex, short pageSize)
        {
            return mytrade.AjaxQueryTransactionHistory(pageIndex, pageSize);
        }
    }
}
