using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Net;
using System.Web.Script.Services;
using System.Web.Services;
using Agp2p.Core;
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

        protected decimal GetLotteriesValue()
        {
            return GetUserInfoByLinq().li_activity_transactions
                .Where(a => LotteryType.Contains(a.activity_type) && a.status == (int) Agp2pEnums.ActivityTransactionStatusEnum.Acting)
                .Aggregate(0m, (sum, a) => sum + a.value);
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
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var wallet = userInfo.li_wallets;
            var userCode = context.dt_user_code.FirstOrDefault(u => u.user_id == userInfo.id && u.type == DTEnums.CodeEnum.Register.ToString());
            var lotteriesValue = userInfo.li_activity_transactions.Where(a => LotteryType.Contains(a.activity_type) && a.status == (int) Agp2pEnums.ActivityTransactionStatusEnum.Acting)
                .Aggregate(0m, (sum, a) => sum + a.value);
            return JsonConvert.SerializeObject(new
            {
                walletInfo = new
                {
                    idleMoney = wallet.idle_money,
                    lockedMoney = wallet.locked_money,
                    investingMoney = wallet.investing_money,
                    profitingMoney = wallet.profiting_money,
                    lotteriesValue,
                    totalCharge = wallet.total_charge,
                    totalInvestment = wallet.total_investment,
                    totalProfit = wallet.total_profit,
                    totalWithdraw = wallet.total_withdraw,
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
                    birthday = userInfo.birthday != null ? userInfo.birthday.Value.ToString("yyyy-MM-dd") : "",
                    userInfo.area,
                    userInfo.address,
                    invitationCode = userCode == null ? myreward.GetInviteCode(context) : userCode.str_code,
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
        public static string AjaxModifyCard(int cardId, string bankName, string bankLocation, string openingBank)
        {
            return mycard.AjaxModifyCard(cardId, bankName, bankLocation, openingBank);
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
        public static string AjaxQueryUserMessages(short type = 1, short pageIndex = 0, short pageSize = 8)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var queryable = context.dt_user_message.Where(m => m.receiver == userInfo.id && m.type == type);
            var totalCount = queryable.Count();
            var msgs = queryable.OrderByDescending(m => m.id).Skip(pageSize * pageIndex).Take(pageSize).AsEnumerable()
                .Select(m => new
                {
                    m.id,
                    isRead = m.is_read == 1,
                    m.title,
                    m.content,
                    receiveTime = m.post_time.ToString("yyyy/MM/dd HH:mm"),
                });
            return JsonConvert.SerializeObject(new {totalCount, msgs});
        }

        [WebMethod]
        public static string AjaxSetMessagesRead(string messageIds)
        {
            var userInfo = GetUserInfo();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var ids = messageIds.Split(';').Select(str => Convert.ToInt32(str)).ToArray();
            if (!ids.Any())
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "请先选择消息";
            }
            var context = new Agp2pDataContext();
            var now = DateTime.Now;
            context.dt_user_message.Where(m => ids.Contains(m.id)).ForEach(m =>
            {
                m.is_read = 1;
                m.read_time = now;
            });
            context.SubmitChanges();
            return "成功设置消息为已读";
        }

        [WebMethod]
        public static string AjaxDeleteMessages(string messageIds)
        {
            return usermessage.AjaxDeleteMessages(messageIds);
        }

        [WebMethod]
        public static string AjaxQueryInvitationInfo(short pageIndex, short pageSize)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var query = userInfo.li_invitations1;
            var data = query.Skip(pageIndex * pageSize).Take(pageSize).Select(i => new
            {
                inviteeId = i.user_id,
                inviteeName = string.IsNullOrWhiteSpace(i.dt_users.real_name) ? i.dt_users.user_name : i.dt_users.real_name,
                firstInvestmentAmount = i.li_project_transactions == null ? 0 : i.li_project_transactions.principal,
            });
            return JsonConvert.SerializeObject(new {totalCount = query.Count, data});
        }

        private static readonly int[] LotteryType =
        {
            (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.Trial,
        };

        [WebMethod]
        public static string AjaxQueryLotteries(short pageIndex, short pageSize)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var query = userInfo.li_activity_transactions.Where(a => LotteryType.Contains(a.activity_type));
            var totalCount = query.Count();

            var data = query.Skip(pageSize * pageIndex).Take(pageSize).AsEnumerable().Select(a => new
            {
                a.id,
                a.activity_type,
                a.status,
                a.value,
                details = string.IsNullOrWhiteSpace(a.details) ? null : JsonConvert.DeserializeObject(a.details),
                a.create_time,
                a.transact_time,
            });
            return JsonConvert.SerializeObject(new {totalCount, data});
        }

        [WebMethod]
        public static string AjaxQueryProjectsDetail(short pageIndex, short pageSize)
        {
            int totalCount;
            var data = QueryProjects(pageSize, pageIndex, out totalCount).Select(p =>
            {
                var pr = GetProjectInvestmentProgress(p);
                return new ProjectDetail
                {
                    id = p.id,
                    img_url = GetProjectImageUrl(p.img_url, p.category_id),
                    no = p.no,
                    title = p.title,
                    status = p.status,
                    sort_id = p.sort_id,
                    repayment_type = p.repayment_type,
                    repayment_term = p.GetProjectTermSpanEnumDesc(),
                    repayment_number = p.repayment_term_span_count,
                    profit_rate_year = p.profit_rate_year,
                    category_id = p.category_id,
                    categoryTitle = p.dt_article_category.title,
                    categoryCallIndex = p.dt_article_category.call_index,
                    amount = p.financing_amount,
                    add_time = p.publish_time ?? p.add_time,
                    publish_time = p.publish_time,
                    tag = p.tag.GetValueOrDefault(),
                    //category_img = get_category_icon_by_categoryid(categoryList, p.category_id),//类别图标路径
                    //project_repayment = p.GetProjectTermSpanEnumDesc(),//项目还款期限单位
                    project_amount_str = p.financing_amount.ToString("n0"), //项目金额字符
                    project_investment_progress = pr.GetInvestmentProgress(), //项目进度
                    project_investment_balance = pr.GetInvestmentBalance(), //项目投资剩余金额
                    project_investment_count = p.GetInvestedUserCount(), //项目投资人数
                    conversionBank = p.GetMortgageInfo("bank") // 承兑银行
                };
            });
            return JsonConvert.SerializeObject(new { totalCount, data });
        }

        [WebMethod(CacheDuration = 600)]
        public static string AjaxQueryEnumInfo(string enumFullName)
        {
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

        [WebMethod]
        public static string AjaxQueryNotificationSettings()
        {
            var userInfo = GetUserInfoByLinq();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var valueTable = Enum.GetValues(typeof (Agp2pEnums.NotificationTypeEnum))
                .Cast<Agp2pEnums.NotificationTypeEnum>()
                .Select(e =>
                {
                    var split = Utils.GetAgp2pEnumDes(e).Split('-');
                    return
                        new
                        {
                            row = split[0],
                            column = split[1],
                            value = (int) e,
                        };
                })
                .GroupBy(a => a.row)
                .ToDictionary(g => g.Key, g => g.ToDictionary(a => a.column, a => a.value));

            var enabledNotificationTypes = userInfo.li_notification_settings.Select(n => n.type).ToList();

            return JsonConvert.SerializeObject(new {valueTable, enabledNotificationTypes });
        }

        [WebMethod]
        public static string AjaxSaveNotificationSettings(string enabledNotificationTypes)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            context.li_notification_settings.DeleteAllOnSubmit(userInfo.li_notification_settings);
            var preAdd = enabledNotificationTypes.Split(',')
                .Select(typeStr => new li_notification_settings {dt_users = userInfo, type = Convert.ToInt32(typeStr)})
                .ToList();
            context.li_notification_settings.InsertAllOnSubmit(preAdd);
            context.SubmitChanges();

            return "保存成功";
        }

        private static IEnumerable<Tuple<string,DateTime,DateTime>> GenMountlyTimeSpan(DateTime startTime, DateTime endTime)
        {
            for (var i = startTime; i <= endTime; i = i.AddMonths(1))
            {
                var name = i.Month == 1 ? i.ToString("yyyy/M") : i.ToString("M月");
                var startSpan = new DateTime(i.Year, i.Month, 1);
                yield return
                    new Tuple<string, DateTime, DateTime>(name, startSpan, startSpan.AddMonths(1));
            }
        }

        [WebMethod]
        public static string AjaxQueryMouthlyHistory(short type)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            var histories = userInfo.li_wallet_histories.ToList();

            Func<li_wallet_histories, decimal> selector;
            switch ((Agp2pEnums.ChartQueryEnum)type)
            {
                case Agp2pEnums.ChartQueryEnum.TotalInvestment:
                    selector = h => h.total_investment;
                    break;
                case Agp2pEnums.ChartQueryEnum.InvestingMoney:
                    selector = h => h.investing_money;
                    break;
                case Agp2pEnums.ChartQueryEnum.RepaidInvestment:
                    selector = h => h.total_investment - h.investing_money;
                    break;
                case Agp2pEnums.ChartQueryEnum.AccumulativeProfit:
                    selector = h => h.total_profit + h.profiting_money;
                    break;
                case Agp2pEnums.ChartQueryEnum.ProfitingMoney:
                    selector = h => h.profiting_money;
                    break;
                case Agp2pEnums.ChartQueryEnum.TotalProfit:
                    selector = h => h.total_profit;
                    break;
                default:
                    throw new Exception("不支持的类型");
            }

            var newestHistoryCreateTime = histories.Any() ? histories.Last().create_time : DateTime.Now;

            // 生成时间间隔
            var timeSpans = GenMountlyTimeSpan(newestHistoryCreateTime.AddYears(-1), newestHistoryCreateTime).ToList();

            // 查询数据
            var totalInvestments = timeSpans.Select(s =>
            {
                return
                    histories.Where(h => s.Item2 <= h.create_time && h.create_time < s.Item3)
                        .Select(selector)
                        .LastOrDefault();
            });

            // [{'2015年1月':0},{'2月':100}]
            return
                JsonConvert.SerializeObject(timeSpans.Zip(totalInvestments, (tuple, arg2) => new Dictionary<string,decimal> { {tuple.Item1, arg2} }));
        }

    }
}
