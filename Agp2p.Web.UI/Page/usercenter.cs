using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Net;
using System.Web.Script.Services;
using System.Web.Services;
using Newtonsoft.Json;

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

        protected string GetTotalMoney()
        {
            var wallet = userModel.li_wallets;
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
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string AjaxQueryUserInfo()
        {
            var userInfo = GetUserInfoByLinq();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var wallet = userInfo.li_wallets;
            return JsonConvert.SerializeObject(new
            {
                walletInfo = new
                {
                    idleMoney = wallet.idle_money,
                    lockedMoney = wallet.locked_money,
                    investingMoney = wallet.investing_money,
                    profitingMoney = wallet.profiting_money,
                },
                userInfo = new
                {
                    nickName = userInfo.nick_name,
                    realName = userInfo.real_name,
                    idCardNumber = userInfo.id_card_number,
                    userInfo.mobile,
                    userInfo.email,
                    userInfo.qq,
                    userInfo.sex,
                    birthday = userInfo.birthday.HasValue ? userInfo.birthday.Value.ToString("yyyy-MM-dd") : "",
                    userInfo.area,
                    userInfo.address,
                    hasTransactPassword = !string.IsNullOrWhiteSpace(userInfo.pay_password)
                }
            });
        }

        [WebMethod]
        public static string AjaxQueryTransactionHistory(short type, short pageIndex, short pageSize, string startTime = "", string endTime = "")
        {
            return mytrade.AjaxQueryTransactionHistory(type, pageIndex, pageSize, startTime, endTime);
        }

        [WebMethod]
        public static string AjaxQueryInvestment(short type, short pageIndex, short pageSize, string startTime = "", string endTime = "")
        {
            return myinvest.AjaxQueryInvestment(type, pageIndex, pageSize, startTime, endTime);
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string AjaxQueryBankCards()
        {
            return withdraw.AjaxQueryBankCards();
        }

        [WebMethod]
        public static string AjaxAppendCard(string cardNumber, string bankName, string bankLocation, string openingBank)
        {
            return mycard.AjaxAppendCard(cardNumber, bankName, bankLocation, openingBank);
        }

        [WebMethod]
        public static string AjaxModifyCard(int cardId, string bankName, string bankLocation, string openingBank, string cardNumber)
        {
            return mycard.AjaxModifyCard(cardId, bankName, bankLocation, openingBank, cardNumber);
        }

        [WebMethod]
        public static string AjaxDeleteCard(int cardId)
        {
            return mycard.AjaxDeleteCard(cardId);
        }

        [WebMethod]
        public static string AjaxQueryRepayments(short type, short pageSize, short pageIndex, string startTime="", string endTime="")
        {
            var userInfo = GetUserInfo();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var myRepayments = myreceiveplan.QueryProjectRepayments(userInfo.id, (Agp2pEnums.MyRepaymentQueryTypeEnum) type, startTime, endTime);
            var repayments = myRepayments.Skip(pageSize * pageIndex).Take(pageSize);
            return JsonConvert.SerializeObject(new {totalCount = myRepayments.Count, data = repayments});
        }

        [WebMethod]
        public static string AjaxQueryEnumInfo(string enumFullName)
        {
            var userInfo = GetUserInfo();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(enumFullName))
                .Where(t => t != null).FirstOrDefault(t => t.IsEnum);
            if (type == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return "没找到枚举类型";
            }

            var values = Enum.GetValues(type).Cast<Enum>();
            return
                JsonConvert.SerializeObject(
                    values.Select(en => new {key = Utils.GetAgp2pEnumDes(en), value = Convert.ToInt32(en)}));
        }
    }
}
