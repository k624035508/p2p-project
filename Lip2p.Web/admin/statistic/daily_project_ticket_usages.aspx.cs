using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using Lip2p.BLL;
using Lip2p.Core.ActivityLogic;
using Newtonsoft.Json;

namespace Lip2p.Web.admin.statistic
{
    public partial class daily_project_ticket_usages : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("statistics_daily_project_ticket_usages", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords =DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            var query = QueryUsageItem();
            totalCount = query.Count();
            rptList.DataSource =
                query.Skip(pageSize*(page - 1))
                    .Take(pageSize)
                    .ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("daily_project_ticket_usages.aspx", "page={0}&keywords={1}", "__id__", txtKeywords.Text.Trim());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        class UsageItem
        {
            public dt_users User { get; private set; }

            public UsageItem(dt_users u)
            {
                User = u;
                _expiredTicketCount = new Lazy<int>(() =>
                {
                    return u.li_activity_transactions.Count(atr =>
                                atr.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject &&
                                atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Cancel);
                });
                _unuseTicketCount = new Lazy<int>(() =>
                {
                    return u.li_activity_transactions.Count(atr =>
                                atr.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject &&
                                atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting);
                });
                _usedTicketCount = new Lazy<int>(() =>
                {
                    return u.li_activity_transactions.Count(atr =>
                                atr.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject &&
                                atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm && atr.transact_time == null);
                });
                _paidTicketCount = new Lazy<int>(() =>
                {
                    return u.li_activity_transactions.Count(atr =>
                                atr.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject &&
                                atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm && atr.transact_time != null);
                });
            }

            private readonly Lazy<int> _unuseTicketCount; 
            private readonly Lazy<int> _expiredTicketCount; 
            private readonly Lazy<int> _usedTicketCount;
            private readonly Lazy<int> _paidTicketCount;

            public int UsedTicketCount { get { return _usedTicketCount.Value; } }
            public int ExpiredTicketCount { get { return _expiredTicketCount.Value; } }
            public int UnusedTicketCount { get { return _unuseTicketCount.Value; } }
            public int PaidTicketCount { get { return _paidTicketCount.Value; } }
        }

        [WebMethod]
        public static string QueryDetails(int userId, int status, bool paid)
        {
            if (!IsAdminLogin())
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录为管理员";
            }
            var context = new Lip2pDataContext();
            var atrs = context.li_activity_transactions.Where(atr =>
                        atr.activity_type == (int)Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject &&
                        atr.user_id == userId && atr.status == status)
                    .Where(atr => atr.transact_time.HasValue == paid)
                    .ToList();
            var results = atrs.Select(atr =>
            {
                var ticket = new DailyProjectActivity.DailyProjectTicket(atr);
                var repayTime = ticket.GetRepayTime();
                return new
                {
                    CreateTime = atr.create_time.ToString("yyyy-MM-dd"),
                    TicketValue = ticket.GetTicketValue(),
                    Profit = atr.value,
                    RepayTime = repayTime == null ? "" : repayTime.Value.ToString("yyyy-MM-dd"),
                    Deadline = ticket.GetDeadline().ToString("yyyy-MM-dd"),
                    Status = atr.status,
                    Paid = atr.transact_time != null
                };
            });
            return JsonConvert.SerializeObject(results);
        }

        private List<UsageItem> QueryUsageItem()
        {
            var context = new Lip2pDataContext();
            var option = new DataLoadOptions();
            option.LoadWith<dt_users>(u => u.li_activity_transactions);
            context.LoadOptions = option;

            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();
            var users = context.dt_users.Where(w => !canAccessGroups.Any() || canAccessGroups.Contains(w.group_id))
                .Where(u => u.user_name.Contains(txtKeywords.Text) || u.real_name.Contains(txtKeywords.Text) || u.mobile.Contains(txtKeywords.Text))
                .Where(u => 0 < u.li_invitations1.Count || 0 < u.li_wallets.total_investment)
                .ToList();
            return users.Where(u => u.li_activity_transactions.Any(atr => atr.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject))
                    .Select(u => new UsageItem(u))
                    .ToList();
        }

        #endregion

        #region 返回每页数量=============================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie(GetType().Name + "_page_size"), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    return _pagesize;
                }
            }
            return _default_size;
        }
        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie(GetType().Name + "_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("daily_project_ticket_usages.aspx", "keywords={0}", txtKeywords.Text.Trim()));
        }
        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("daily_project_ticket_usages.aspx", "keywords={0}", txtKeywords.Text.Trim()));
        }
    }
}