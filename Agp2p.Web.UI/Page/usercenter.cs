using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Net;
using System.Web.Script.Services;
using System.Web.Services;
using Agp2p.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        protected bool IsLoaner()
        {
            return userModel.li_loaners.Any();
        }

        protected bool IsIdentity()
        {
#if DEBUG
            return false;
#endif
#if !DEBUG
            //5月13号托管更新前的用户才需要激活操作
            return userModel.identity_id == null && userModel.reg_time <= DateTime.Parse("2016-5-13");
#endif
        }

        protected int QueryBanner(int aid)
        {
            var context = new Agp2pDataContext();
            var invokeBanner =
                context.dt_advert_banner.Where(
                    a =>
                        a.is_lock == 0 && a.aid == aid && a.end_time >= DateTime.Today)
                    .OrderBy(a => a.sort_id).ToList();

            return invokeBanner.Count();
        }

        protected bool IsQuestionnaire()
        {
            return userModel.li_questionnaire_results.Any();
        }

        protected List<dt_advert_banner> QueryBannerList(int aid)
        {
            var context = new Agp2pDataContext();
            var invokeBanner =
                context.dt_advert_banner.Where(
                    a =>
                        a.is_lock == 0 && a.aid == aid && a.end_time >= DateTime.Today)
                    .OrderBy(a => a.sort_id).ToList();

            return invokeBanner;
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
                //Utils.WriteCookie(DTKeys.COOKIE_USER_NAME_REMEMBER, "Agp2p", -43200);
                //Utils.WriteCookie(DTKeys.COOKIE_USER_PWD_REMEMBER, "Agp2p", -43200);
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
                    userName = userInfo.user_name,
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
                    hasTransactPassword = !string.IsNullOrWhiteSpace(userInfo.pay_password),
                    groupName = userInfo.dt_user_groups.title,
                    isLoaner = userInfo.li_loaners.Any(),
                    identityId = userInfo.identity_id,
                    questionnaireScore = AjaxLoadQuestionnaireResult(1),
                    isQuestionnaire = userInfo.li_questionnaire_results.Any()
                }
            });
        }

        [WebMethod]
        public static string AjaxQueryTransactionHistory(short type, short pageIndex, short pageSize, string startTime = "", string endTime = "")
        {
            return mytrade.AjaxQueryTransactionHistory(type, pageIndex, pageSize, startTime, endTime);
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string AjaxQueryBanner()
        {
            return advert.AjaxQueryBanner();
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
        public static string AjaxAppendCard(string cardNumber, string bankName)
        {
            return mycard.AjaxAppendCard(cardNumber, bankName);
        }

        [WebMethod]
        public static string AjaxModifyCard(int cardId, string bankName)
        {
            return mycard.AjaxModifyCard(cardId, bankName);
        }

        [WebMethod]
        public static string AjaxDeleteCard(int cardId)
        {
            return mycard.AjaxDeleteCard(cardId);
        }

        [WebMethod]
        public static string AjaxQueryRepayments(short type, short pageSize, short pageIndex, string startTime="", string endTime="")
        {
            var userInfo = GetUserInfoByLinq();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var myRepayments = myreceiveplan.QueryProjectRepayments(userInfo, (Agp2pEnums.MyRepaymentQueryTypeEnum) type, startTime, endTime);
            var repayments = myRepayments.Skip(pageSize * pageIndex).Take(pageSize);
            return JsonConvert.SerializeObject(new {totalCount = myRepayments.Count, data = repayments});
        }

        [WebMethod]
        public static string AjaxQueryUserMessages(short readStatus, short pageIndex, int pageSize, bool idOnly)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var queryable = userInfo.dt_user_message.Where(m => m.is_read.GetValueOrDefault() == readStatus);
            var totalCount = queryable.Count();
            var msgs = queryable.OrderByDescending(m => m.id).Skip(pageSize*pageIndex).Take(pageSize).AsEnumerable()
                .Select(idOnly
                    ? (Func<dt_user_message, object>) (m => new {m.id})
                    : m => new
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
            // 查询自己的奖励
            var myRewards = context.li_activity_transactions.Where(
                tr =>
                    tr.activity_type == (int) Agp2pEnums.ActivityTransactionActivityTypeEnum.RefereeFirstTimeProfitBonus)
                .ToDictionary(t => ((JObject) JsonConvert.DeserializeObject(t.details)).Value<int>("Invitee"),
                    atr =>
                        atr.value.ToString("c") +
                        (atr.status == (int) Agp2pEnums.ActivityTransactionStatusEnum.Confirm ? "（已发放）" : "（待发放）"));

            var data = query.Skip(pageIndex*pageSize).Take(pageSize).Select(i =>
            {
                var firstInvestmentAmount = i.li_project_transactions?.principal ?? 0;
                return new
                {
                    inviteeId = i.user_id,
                    inviteeName = string.IsNullOrWhiteSpace(i.dt_users.real_name) ? i.dt_users.user_name : i.dt_users.real_name,
                    firstInvestmentAmount = firstInvestmentAmount == 0 ? "" : firstInvestmentAmount.ToString(),
                    reward = myRewards.ContainsKey(i.user_id) ? myRewards[i.user_id] : (firstInvestmentAmount == 0 ? "（未投资）" : "（已投资，未放款）")
                };
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
            Model.siteconfig config = new BLL.siteconfig().loadConfig();
            var data = QueryInvestables(pageSize, pageIndex, out totalCount).Select(inv =>
            {
                var p = inv.Project;
                var pr = GetProjectInvestmentProgress(p);
                return new ProjectDetail
                {
                    id = p.id,
                    no = p.no,
                    title = p.title,
                    status = p.status,
                    sort_id = p.sort_id,
                    repayment_type = p.GetProjectRepaymentTypeDesc(),
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
                    conversionBank = p.GetMortgageInfo("bank"), // 承兑银行
                    linkurl = linkurl(config, "project", p.id)
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

            var valueTable = Enum.GetValues(typeof (Agp2pEnums.DisabledNotificationTypeEnum))
                .Cast<Agp2pEnums.DisabledNotificationTypeEnum>()
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

            var disabledNotificationTypes = userInfo.li_notification_settings.Select(n => n.type).ToList();

            return JsonConvert.SerializeObject(new {valueTable, disabledNotificationTypes });
        }

        [WebMethod]
        public static string AjaxSaveNotificationSettings(string disabledNotificationTypes)
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
            if (!string.IsNullOrWhiteSpace(disabledNotificationTypes))
            {
                var preAdd = disabledNotificationTypes.Split(',')
                    .Select(typeStr => new li_notification_settings { dt_users = userInfo, type = Convert.ToInt32(typeStr) })
                    .ToList();
                context.li_notification_settings.InsertAllOnSubmit(preAdd);
            }
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

        private static decimal MouthlyProfiting(List<li_wallet_histories> mouthlyHistories)
        {
            if (!mouthlyHistories.Any())
            {
                return 0;
            }
            var firstHis = mouthlyHistories.First();
            var mouthlyProfit = mouthlyHistories.Last().total_profit - firstHis.total_profit;
            if (firstHis.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest
                || firstHis.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest)
            {
                return firstHis.li_project_transactions.interest.GetValueOrDefault(0) + mouthlyProfit;
            }
            return mouthlyProfit;
        }

        private static DateTime GetFirstDayOfThisMouth(DateTime day)
        {
            return day.Date.AddDays(-(day.Day - 1));
        }

        private static T PredictMouthlyProfiting<T>(dt_users user, Func<IEnumerable<Tuple<string, DateTime, DateTime>>, IEnumerable<decimal>, T> callback)
        {
            var myRepayments = myreceiveplan.QueryProjectRepayments(user, Agp2pEnums.MyRepaymentQueryTypeEnum.Unpaid, DateTime.Now.AddMonths(-13).ToString("yyyy-MM-dd")); // 不需要查一年前的

            var predictMap =
                myRepayments.GroupBy(r => GetFirstDayOfThisMouth(Convert.ToDateTime(r.ShouldRepayDay)))
                    .ToDictionary(g => g.Key, g => g.Sum(rt => rt.RepayInterest));

            var now = DateTime.Now;
            // 生成时间间隔
            var timeSpans = GenMountlyTimeSpan(now, now.AddYears(1)).ToList();

            var predictsTimeline = timeSpans.Select(m => predictMap.ContainsKey(m.Item2) ? predictMap[m.Item2] : 0m);

            return callback(timeSpans, predictsTimeline);
        }

        private static T QueryMouthlyPrincipleRepayment<T>(dt_users user, Func<IEnumerable<Tuple<string, DateTime, DateTime>>, IEnumerable<decimal>, T> callback)
        {
            var myRepayments = myreceiveplan.QueryProjectRepayments(user, Agp2pEnums.MyRepaymentQueryTypeEnum.Paid, DateTime.Now.AddMonths(-13).ToString("yyyy-MM-dd"));  // 不需要查一年前的

            // 回款将会算到项目的满标时间；如果未满标（新手标），则算到最后一次投资时间
            var repaidMputhMap = myRepayments.GroupBy(r =>
            {
                if (r.Project.InvestCompleteTime != null)
                {
                    return GetFirstDayOfThisMouth(r.Project.InvestCompleteTime.Value);
                }
                var lastInvestmentTime = user.li_project_transactions.Last(
                    ptr =>
                        ptr.project == r.Project.Id && ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success).create_time;
                return GetFirstDayOfThisMouth(lastInvestmentTime);
            }).ToDictionary(g => g.Key, g => g.Sum(rt => rt.RepayPrincipal));

            var now = DateTime.Now;
            // 生成时间间隔
            var timeSpans = GenMountlyTimeSpan(now.AddYears(-1), now).ToList();

            var predictsTimeline = timeSpans.Select(m => repaidMputhMap.ContainsKey(m.Item2) ? repaidMputhMap[m.Item2] : 0m);

            return callback(timeSpans, predictsTimeline);
        }

        private static T QueryMouthlyTotalInvestment<T>(dt_users user, Func<IEnumerable<Tuple<string, DateTime, DateTime>>, IEnumerable<decimal>, T> callback)
        {
            // 月度投资金额：某个项目的投资金额以项目满标时间的那个月为准，如果未满标则按最后一次投资时间为准
            var mouthlyInvestedAmount = user.li_project_transactions.Where(
                ptr =>
                    DateTime.Now.AddMonths(-13) <= ptr.create_time && // 不需要查一年前的
                    ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success) // 排除流标 / 退款的
                .GroupBy(ptr => ptr.li_projects)
                .GroupBy(pptr =>
                    pptr.Key.invest_complete_time == null
                        ? GetFirstDayOfThisMouth(pptr.Last().create_time)
                        : GetFirstDayOfThisMouth(pptr.Key.invest_complete_time.Value))
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x).Sum(ptr => ptr.principal));

            var now = DateTime.Now;
            // 生成时间间隔
            var timeSpans = GenMountlyTimeSpan(now.AddYears(-1), now).ToList();

            var investedAmounts = timeSpans.Select(span => mouthlyInvestedAmount.ContainsKey(span.Item2) ? mouthlyInvestedAmount[span.Item2] : 0m);

            return callback(timeSpans, investedAmounts);
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

            var histories = userInfo.li_wallet_histories.ToLazyList();
            Func<List<li_wallet_histories>, decimal> queryStrategy;

            switch ((Agp2pEnums.ChartQueryEnum)type)
            {
                case Agp2pEnums.ChartQueryEnum.TotalInvestment:
                    return JsonConvert.SerializeObject(QueryMouthlyTotalInvestment(userInfo,
                            (pastMouths, investments) => pastMouths.Zip(investments, (tuple, arg2) => new Dictionary<string, decimal> { { tuple.Item1, arg2 } })));
                case Agp2pEnums.ChartQueryEnum.InvestingMoney:
                    return QueryMouthlyPrincipleRepayment(userInfo, (pastMouth, repaidPrincipleList) =>
                    {
                        return QueryMouthlyTotalInvestment(userInfo, (tuples, mouthlyInvestments) =>
                        {
                            var repaidInvestmentsVal = mouthlyInvestments.Zip(repaidPrincipleList, (investment, repaidPrinciple) => investment - repaidPrinciple);
                            return JsonConvert.SerializeObject(pastMouth.Zip(repaidInvestmentsVal, (tuple, arg2) => new Dictionary<string, decimal> { { tuple.Item1, arg2 } }));
                        });
                    });
                case Agp2pEnums.ChartQueryEnum.RepaidInvestment:
                    return JsonConvert.SerializeObject(QueryMouthlyPrincipleRepayment(userInfo, (pastMouth, repaidPrincipleList) =>
                            pastMouth.Zip(repaidPrincipleList, (tuple, arg2) => new Dictionary<string, decimal> { { tuple.Item1, arg2 } })));
                case Agp2pEnums.ChartQueryEnum.AccumulativeProfit:
                    return PredictMouthlyProfiting(userInfo, (predictTimeSpans, predictsTimeline) =>
                    {
                        var zipped = predictTimeSpans.Zip(predictsTimeline, (tuple, prof) => new {tuple, prof}).ToList();
                        var lastMouthProfiting = zipped.LastOrDefault(x => x.prof != 0);
                        var chartLastMouth = lastMouthProfiting?.tuple.Item2 ?? DateTime.Today;

                        var accumulativeProfitTimeSpans = GenMountlyTimeSpan(chartLastMouth.AddYears(-1), chartLastMouth).ToList();
                        var profited = accumulativeProfitTimeSpans.Select(span =>
                                    MouthlyProfiting(histories.Where(h => span.Item2 <= h.create_time && h.create_time < span.Item3).ToList())).ToList();

                        IEnumerable<Dictionary<string, decimal>> accumulativeProfitData;
                        if (lastMouthProfiting != null)
                        {
                            var dict = zipped.ToDictionary(v => v.tuple.Item2, v => v.prof);
                            accumulativeProfitData = accumulativeProfitTimeSpans.Zip(profited,
                                (tuple, arg2) =>
                                    new Dictionary<string, decimal>
                                    {
                                        {tuple.Item1, dict.ContainsKey(tuple.Item2) ? dict[tuple.Item2] + arg2 : arg2}
                                    });
                        }
                        else
                        {
                            accumulativeProfitData = accumulativeProfitTimeSpans.Zip(profited, (tuple, arg2) => new Dictionary<string, decimal> { { tuple.Item1, arg2 } });
                        }
                        return JsonConvert.SerializeObject(accumulativeProfitData);
                    });
                case Agp2pEnums.ChartQueryEnum.ProfitingMoney:
                    // 预测待收益
                    return JsonConvert.SerializeObject(PredictMouthlyProfiting(userInfo, (predictTimeSpans, predictsTimeline) =>
                                predictTimeSpans.Zip(predictsTimeline, (tuple, arg2) => new Dictionary<string, decimal> { { tuple.Item1, arg2 } })));
                case Agp2pEnums.ChartQueryEnum.TotalProfit:
                    queryStrategy = MouthlyProfiting;
                    break;
                default:
                    throw new Exception("不支持的类型");
            }


            var now = DateTime.Now;

            // 生成时间间隔
            var timeSpans = GenMountlyTimeSpan(now.AddYears(-1), now).ToList();

            // 查询数据
            var currencyVals = timeSpans.Select(s =>
            {
                return queryStrategy(histories.Where(h => s.Item2 <= h.create_time && h.create_time < s.Item3).ToList());
            });

            // [{'2015年1月':0},{'2月':100}]
            return JsonConvert.SerializeObject(timeSpans.Zip(currencyVals, (tuple, arg2) => new Dictionary<string,decimal> { {tuple.Item1, arg2} }));
        }

        private static readonly Dictionary<Agp2pEnums.StaticClaimQueryEnum, Agp2pEnums.ClaimStatusEnum[]> StaticClaimQueryTypeStatusMap = new Dictionary
            <Agp2pEnums.StaticClaimQueryEnum, Agp2pEnums.ClaimStatusEnum[]>
        {
            { Agp2pEnums.StaticClaimQueryEnum.Profiting, new[] {Agp2pEnums.ClaimStatusEnum.Nontransferable, Agp2pEnums.ClaimStatusEnum.Transferable} },
            { Agp2pEnums.StaticClaimQueryEnum.Transfering, new[] { Agp2pEnums.ClaimStatusEnum.NeedTransfer, Agp2pEnums.ClaimStatusEnum.CompletedUnpaid, Agp2pEnums.ClaimStatusEnum.TransferredUnpaid } },
            { Agp2pEnums.StaticClaimQueryEnum.Transferred, new[] { Agp2pEnums.ClaimStatusEnum.Transferred } },
        };
        [WebMethod]
        public static string AjaxQueryStaticClaim(byte claimQueryType, int pageIndex, int pageSize)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var reverseMap =
                StaticClaimQueryTypeStatusMap
                    .SelectMany(pair => pair.Value.Select(st => new {st, pair.Key}))
                    .ToDictionary(t => t.st, t => t.Key);

            var claimQueryEnum = (Agp2pEnums.StaticClaimQueryEnum)claimQueryType;

            var query = context.li_claims.Where(c =>
                    c.userId == userInfo.id &&
                    c.projectId == c.profitingProjectId &&
                    (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying <= c.li_projects.status &&
                    StaticClaimQueryTypeStatusMap[claimQueryEnum].Cast<int>().ToArray().Contains(c.status) &&
                    !c.Children.Any());

            if (claimQueryEnum == 0)
            {
                query = context.li_claims.Where(c =>
                      c.userId == userInfo.id &&
                      c.projectId == c.profitingProjectId &&
                      (int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying <= c.li_projects.status &&
                      !c.Children.Any());
            }


            var count = query.Count();
            var data =
                query.OrderByDescending(c => c.id)
                    .Skip(pageIndex*pageSize)
                    .Take(pageSize)
                    .AsEnumerable()
                    .Select(c => new
                    {
                        c.id,
                        c.number,
                        profitingProject = c.li_projects.title,
                        profitingYearly = c.li_projects.profit_rate_year.ToString("n1") + "%",
                        c.principal,
                        queryType = Utils.GetAgp2pEnumDes(reverseMap[(Agp2pEnums.ClaimStatusEnum) c.status]),
                        createTime = c.createTime.ToString("yyyy-MM-dd HH:mm"),
                        nextProfitDay = c.li_projects.li_repayment_tasks.FirstOrDefault(t => t.IsUnpaid())?.should_repay_time.ToString("yyyy-MM-dd"),
                        c.status,
                        buyerCount = c.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer ? c.li_project_transactions_profiting.Count(
                            ptr =>
                                ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn &&
                                ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Pending) : 0,
                    });
            return JsonConvert.SerializeObject(new { totalCount = count, data});
        }

        [WebMethod]
        public static string AjaxQueryWithdrawClaimExtraInfo(int claimId)
        {
            // 查询债权实际产生的收益（提现后能产生的收益）
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var claim = context.li_claims.Single(c => c.id == claimId);
            if ((int) Agp2pEnums.ClaimStatusEnum.NeedTransfer <= claim.status)
                throw new InvalidOperationException("只能查询未申请转让的债权");

            var withdrawClaim = claim.NewStatusChild(DateTime.Now, Agp2pEnums.ClaimStatusEnum.NeedTransfer);
            withdrawClaim.li_projects = claim.li_projects;
            withdrawClaim.li_projects_profiting = claim.li_projects_profiting;
            withdrawClaim.dt_users = claim.dt_users;

            var withdrawClaimFinalInterest = TransactionFacade.QueryWithdrawClaimFinalInterest(withdrawClaim);
            var originalClaimFinalInterest = TransactionFacade.QueryOriginalClaimFinalInterest(withdrawClaim);

            var task = withdrawClaim.li_projects.li_repayment_tasks.Last(t => t.status != (int)Agp2pEnums.RepaymentStatusEnum.Invalid);
            var remainDays = (int) (task.should_repay_time.Date - withdrawClaim.createTime.Date).TotalDays;

            var staticWithdrawCostPercent = withdrawClaim.dt_users.IsCompanyAccount() ? 0 : ConfigLoader.loadCostConfig().static_withdraw;

            return JsonConvert.SerializeObject(new { withdrawClaimFinalInterest, originalClaimFinalInterest, remainDays, staticWithdrawCostPercent });
        }

        [WebMethod]
        public static string AjaxApplyForClaimTransfer(int claimId, decimal keepInterestPercent)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            if (userInfo.li_claims.Any(c => c.id == claimId))
            {
                try
                {
                    TransactionFacade.StaticProjectWithdraw(context, claimId, keepInterestPercent);
                    return "ok";
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return ex.Message;
                }
            }

            HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return "不存在的债权ID";
        }

        [WebMethod]
        public static string AjaxApplyForCancelClaimTransfer(int withdrawClaimId)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var withdrawingClaim = userInfo.li_claims.SingleOrDefault(c => c.id == withdrawClaimId);
            if (withdrawingClaim != null)
            {
                if (withdrawingClaim.li_project_transactions_profiting.Any(
                ptr =>
                    ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn &&
                    ptr.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Pending))
                {
                    HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return "已经有人买入此债权，不能取消转让申请";
                }

                TransactionFacade.StaticClaimWithdrawCancel(context, withdrawClaimId);
                return "ok";
            }

            HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return "不存在的债权ID";
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string AjaxQueryClaimTransferSummery()
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            /*成功转出金额 已转出债权笔数 成功买入金额 已买入债权笔数*/

            var transferredClaims = userInfo.li_claims.Where(
                c => c.status == (int) Agp2pEnums.ClaimStatusEnum.Transferred).ToList();

            var buyedClaims = userInfo.li_claims.Where(
                c =>
                    c.status < (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer && c.Parent != null &&
                    c.Parent.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                    c.Parent.userId != c.userId).ToList();
            return
                JsonConvert.SerializeObject(
                    new
                    {
                        StaticClaimWithdrawAmount = transferredClaims.Aggregate(0m, (sum, c) => sum + c.principal).ToString("n"),
                        StaticClaimWithdrawCount = transferredClaims.Count,
                        BuyedStaticClaimAmount = buyedClaims.Aggregate(0m, (sum, c) => sum + c.principal).ToString("n"),
                        BuyedStaticClaimCount = buyedClaims.Count
                    });
        }

        /* 暂无使用这个 api */
        //[WebMethod]
        //public static string AjaxInvestHuoqiProject(decimal amount)
        //{
        //    var context = new Agp2pDataContext();
        //    var userInfo = GetUserInfoByLinq(context);
        //    HttpContext.Current.Response.TrySkipIisCustomErrors = true;
        //    if (userInfo == null)
        //    {
        //        HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        //        return "请先登录";
        //    }

        //    var huoqiProject = context.li_projects.Single(
        //        p =>
        //            p.dt_article_category.call_index == "huoqi" &&
        //            p.status == (int) Agp2pEnums.ProjectStatusEnum.Financing);
        //    TransactionFacade.Invest(userInfo.id, huoqiProject.id, amount);

        //    return "ok";
        //}

        [WebMethod]
        public static string AjaxWithdrawHuoqiProject(string transactPassword, decimal amount)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            if (Utils.MD5(transactPassword) != userInfo.pay_password)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "交易密码错误";
            }

            var huoqiProject = context.li_projects.Single(
                p =>
                    p.dt_article_category.call_index == "huoqi" &&
                    p.status == (int) Agp2pEnums.ProjectStatusEnum.Financing);
            context.HuoqiProjectWithdraw(userInfo.id, huoqiProject.id, amount);
            return "ok";
        }

        

        private static readonly Dictionary<Agp2pEnums.HuoqiTransactionQueryEnum, Agp2pEnums.WalletHistoryTypeEnum[]> HuoqiTradeQueryTypeMap = new Dictionary
            <Agp2pEnums.HuoqiTransactionQueryEnum, Agp2pEnums.WalletHistoryTypeEnum[]>
        {
            {Agp2pEnums.HuoqiTransactionQueryEnum.All, new []{Agp2pEnums.WalletHistoryTypeEnum.Invest, Agp2pEnums.WalletHistoryTypeEnum.AutoInvest,
                Agp2pEnums.WalletHistoryTypeEnum.HuoqiProjectWithdrawSuccess, Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest }},
            {Agp2pEnums.HuoqiTransactionQueryEnum.BuyIn, new []{Agp2pEnums.WalletHistoryTypeEnum.Invest, Agp2pEnums.WalletHistoryTypeEnum.AutoInvest }},
            {Agp2pEnums.HuoqiTransactionQueryEnum.TransferOut, new []{Agp2pEnums.WalletHistoryTypeEnum.HuoqiProjectWithdrawSuccess}},
            {Agp2pEnums.HuoqiTransactionQueryEnum.Profiting, new []{Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest }},
        };
        [WebMethod]
        public static string AjaxQueryHuoqiTransactions(byte huoqiQueryType, short pageIndex, short pageSize)
        {
            var context = new Agp2pDataContext();
            var options = new DataLoadOptions();
            options.LoadWith<li_wallet_histories>(his => his.li_project_transactions);
            context.LoadOptions = options;

            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var huoqiQueryEnum = (Agp2pEnums.HuoqiTransactionQueryEnum) huoqiQueryType;
            var query =
                context.li_wallet_histories.Where(
                    his =>
                        his.user_id == userInfo.id &&
                        his.li_project_transactions != null &&
                        his.li_project_transactions.li_projects.dt_article_category.call_index == "huoqi" &&
                        HuoqiTradeQueryTypeMap[huoqiQueryEnum].Cast<int>().Contains(his.action_type));

            var dataBeforePaging = query.OrderByDescending(his => his.id)
                .AsEnumerableAutoPartialQuery()
                .GroupBy(his => new {his.action_type, his.create_time})
                .SelectMany(hises =>
                {
                    if (hises.Key.action_type == (int) Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest)
                    {
                        return Enumerable.Repeat(new
                        {
                            hises.First().id,
                            createTime = hises.Key.create_time.ToString("yyyy-MM-dd HH:mm"),
                            type = Utils.GetAgp2pEnumDes((Agp2pEnums.WalletHistoryTypeEnum) hises.Key.action_type),
                            outcome = "",
                            income = hises.Aggregate(0m, (sum, his) =>
                                sum +
                                TransactionFacade.QueryTransactionIncome(his,
                                    (principal, interest) => interest.GetValueOrDefault())).ToString("c")
                        }, 1);
                    }
                    return hises.Select(his => new
                    {
                        his.id,
                        createTime = his.create_time.ToString("yyyy-MM-dd HH:mm"),
                        type = Utils.GetAgp2pEnumDes((Agp2pEnums.WalletHistoryTypeEnum) his.action_type),
                        outcome = his.QueryTransactionOutcome()?.ToString("n"),
                        income = TransactionFacade.QueryTransactionIncome<string>(his)
                    });
                });
            var data = dataBeforePaging.Skip(pageIndex*pageSize).Take(pageSize);

            var count = query.Count(his => his.action_type != (int) Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest) +
                    query.Where(his => his.action_type == (int) Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest)
                        .GroupBy(his => his.create_time)
                        .Count();

            return JsonConvert.SerializeObject(new {totalCount = count, data});
        }

        private static readonly Dictionary<Agp2pEnums.HuoqiClaimQueryEnum, Agp2pEnums.ClaimStatusEnum[]> HuoqiClaimQueryTypeStatusMap = new Dictionary
            <Agp2pEnums.HuoqiClaimQueryEnum, Agp2pEnums.ClaimStatusEnum[]>
        {
            { Agp2pEnums.HuoqiClaimQueryEnum.All, Utils.GetEnumValues<Agp2pEnums.ClaimStatusEnum>().ToArray() },
            { Agp2pEnums.HuoqiClaimQueryEnum.Profiting, new[] {Agp2pEnums.ClaimStatusEnum.Nontransferable, Agp2pEnums.ClaimStatusEnum.Transferable} },
            { Agp2pEnums.HuoqiClaimQueryEnum.Transfering, new[] { Agp2pEnums.ClaimStatusEnum.NeedTransfer, Agp2pEnums.ClaimStatusEnum.CompletedUnpaid, Agp2pEnums.ClaimStatusEnum.TransferredUnpaid } },
            { Agp2pEnums.HuoqiClaimQueryEnum.Completed, new[] { Agp2pEnums.ClaimStatusEnum.Completed, Agp2pEnums.ClaimStatusEnum.Invalid, Agp2pEnums.ClaimStatusEnum.Transferred } },
        };
        [WebMethod]
        public static string AjaxQueryHuoqiClaims(byte claimQueryType, short pageIndex, short pageSize)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var claimQueryEnum = (Agp2pEnums.HuoqiClaimQueryEnum) claimQueryType;
            var query = context.li_claims.Where(
                c =>
                    c.userId == userInfo.id &&
                    c.projectId != c.profitingProjectId &&
                    HuoqiClaimQueryTypeStatusMap[claimQueryEnum].Cast<int>().Contains(c.status) && !c.Children.Any());

            var reverseMap =
                HuoqiClaimQueryTypeStatusMap.Where(pair => pair.Key != Agp2pEnums.HuoqiClaimQueryEnum.All)
                    .SelectMany(pair => pair.Value.Select(st => new { st, pair.Key }))
                    .ToDictionary(t => t.st, t => t.Key);

            var count = query.Count();
            var data = query.OrderByDescending(c => c.id).Skip(pageSize*pageIndex).Take(pageSize).AsEnumerable().Select(c => new
            {
                c.id,
                c.number,
                project = c.li_projects.title,
                principal = c.principal.ToString("n"),
                queryType = Utils.GetAgp2pEnumDes(reverseMap[(Agp2pEnums.ClaimStatusEnum)c.status]),
                createTime = c.createTime.ToString("yyyy-MM-dd HH:mm"),
                completeDay = c.li_projects.li_repayment_tasks.LastOrDefault()?.should_repay_time.ToString("yyyy-MM-dd"),
                c.status,
            });

            return JsonConvert.SerializeObject(new {totalCount = count, data});
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string AjaxQueryHuoqiSummary()
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            // 今日预期收益 本金总额 累计收益 当前年化收益率
            var currentHuoqiProject = context.li_projects.FirstOrDefault(
                p =>
                    p.dt_article_category.call_index == "huoqi" &&
                    p.status == (int) Agp2pEnums.ProjectStatusEnum.Financing);

            var myHuoqiClaims =
                currentHuoqiProject?.li_claims_profiting.Where(c => c.userId == userInfo.id && c.status < (int) Agp2pEnums.ClaimStatusEnum.Completed && c.IsLeafClaim()).ToList() ??
                Enumerable.Empty<li_claims>().ToList();

            var totalPrincipal = myHuoqiClaims.Aggregate(0m, (sum, c) => sum + c.principal);
            var totalProfitingPrincipal = myHuoqiClaims.Where(c => c.IsProfiting()).Aggregate(0m, (sum, c) => sum + c.principal);

            var totalProfit = context.li_project_transactions.Where(
                ptr =>
                    ptr.investor == userInfo.id &&
                    ptr.li_projects.dt_article_category.call_index == "huoqi" &&
                    ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.RepayToInvestor).AsEnumerable()
                .Aggregate(0m, (sum, ptr) => sum + ptr.principal + ptr.interest.GetValueOrDefault());

            return JsonConvert.SerializeObject(new
            {
                TodayProfitPredict = Math.Round(
                        1m/TransactionFacade.HuoqiProjectProfitingDay*totalProfitingPrincipal*
                        (currentHuoqiProject?.profit_rate_year).GetValueOrDefault()/100, 2).ToString("n"),
                TotalHuoqiClaimPrincipal = totalPrincipal.ToString("n"),
                TotalHuoqiProfit = totalProfit.ToString("n"),
                CurrentHuoqiProjectProfitRateYearly =
                    currentHuoqiProject == null
                        ? "(目前没有活期项目)"
                        : currentHuoqiProject.profit_rate_year.ToString("n1") + "%",
                CurrentHuoqiProjectId = currentHuoqiProject?.id
            });
        }

        [WebMethod]
        public static string AjaxQueryLoan(short type, short pageIndex, short pageSize, string startTime = "", string endTime = "")
        {
            return myloan.AjaxQueryLoan(type, pageIndex, pageSize, startTime, endTime);
        }

        private static decimal SumOfScore(Agp2pEnums.QuestionnaireEnum questionnaire, List<string> results)
        {
            switch(questionnaire)
            {
                case Agp2pEnums.QuestionnaireEnum.LenderRiskAssessmentTest:
                    return results.Select(s => s.ToUpper()).Aggregate(0m, (sum, str) =>
                    {
                        switch (str)
                        {
                            case "A": return sum + 1;
                            case "B": return sum + 2;
                            case "C": return sum + 3;
                            case "D": return sum + 4;
                            case "X": return sum + 25; //跳过测试
                            default:
                                throw new NotImplementedException();
                        }
                    });
                default:
                    throw new NotImplementedException();
            }
        }

        [WebMethod]
        public static string AjaxSaveQuestionnaireResult(int questionnaireId, string result)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            var deleteResult = userInfo.li_questionnaire_results;
            context.li_questionnaire_results.DeleteAllOnSubmit(deleteResult);
            // result 的格式： ["A", "A&B", ...]
            var results = JsonConvert.DeserializeObject<List<string>>(result);
            var qrs = results.Zip(Utils.Infinite(), (r, i) => new li_questionnaire_result
            {
                answer = r,
                userId = userInfo.id,
                questionId = i,
                questionnaireId = questionnaireId,
            });
            context.li_questionnaire_results.InsertAllOnSubmit(qrs);
            var score = SumOfScore((Agp2pEnums.QuestionnaireEnum)questionnaireId, results);

            context.SubmitChanges();

            return  score.ToString();
        }

        [WebMethod]
        public static string SkipQuestionnaireResult(int questionnaireId)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            var deleteResult = userInfo.li_questionnaire_results;
            context.li_questionnaire_results.DeleteAllOnSubmit(deleteResult);
            context.li_questionnaire_results.InsertOnSubmit( new li_questionnaire_result
            {
                answer = "X",
                userId = userInfo.id,
                questionId = 0,
                questionnaireId = questionnaireId
            });
            var score = "25";
            context.SubmitChanges();
            return score;
        }

        [WebMethod]
        public static string AjaxLoadQuestionnaireResult(int questionnaireId)
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            // 返回的格式：{"answer": ["A", "A&B", ...], "score": 999}

            var answers = userInfo.li_questionnaire_results.Where(q => q.questionnaireId == questionnaireId)
                .OrderBy(q => q.questionId).Select(q => q.answer).ToList();
            /* return JsonConvert.SerializeObject(new { answers,
                score = SumOfScore((Agp2pEnums.QuestionnaireEnum)questionnaireId, answers) }); */
            var score = SumOfScore((Agp2pEnums.QuestionnaireEnum)questionnaireId, answers);
            return score.ToString();
        }

    }
}
