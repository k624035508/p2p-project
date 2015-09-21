using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Agp2p.BLL;

namespace Agp2p.Web.admin.statistic
{
    public partial class investment_rank_list : UI.ManagePage
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
                ChkAdminLevel("statistics_user_rank", DTEnums.ActionEnum.View.ToString()); //检查权限
                var startTime = DTRequest.GetQueryString("startTime");
                var now = DateTime.Now;
                txtStartTime.Text = !string.IsNullOrEmpty(startTime) ? startTime : now.ToString("yyyy-MM-01 00:00:00");

                var endTime = DTRequest.GetQueryString("endTime");
                txtEndTime.Text = !string.IsNullOrEmpty(endTime)
                    ? endTime
                    : now.Date.AddDays(DateTime.DaysInMonth(now.Year, now.Month) - now.Day).ToString("yyyy-MM-dd 00:00:00");

                var keywords =DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            var query = QueryRankUser();
            totalCount = query.Count();
            rptList.DataSource =
                query.OrderByDescending(h => h.InvestmentSum)
                    .Skip(pageSize*(page - 1))
                    .Take(pageSize)
                    .ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("investment_rank_list.aspx", "page={0}&startTime={1}&endTime={2}&keywords={3}", "__id__", txtStartTime.Text, txtEndTime.Text,txtKeywords.Text.Trim());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        class RankItem
        {
            public dt_users User { get; set; }

            public RankItem(dt_users u, DateTime? startTime, DateTime? endTime)
            {
                User = u;
                _investmentSum = new Lazy<decimal>(() => QueryInvestmentSum(u, startTime, endTime));
                _inviteeCount = new Lazy<int>(() =>
                    u.li_invitations1.Where(inv => inv.first_invest_transaction != null)
                        .Where(inv => startTime == null || startTime <= inv.dt_users.reg_time)
                        .Where(inv => endTime == null || inv.dt_users.reg_time <= endTime).Count());
                _inviteeInvestmentSum = new Lazy<decimal>(() => QueryInviteeInvestmentSum(u, startTime, endTime));
            }

            private readonly Lazy<int> _inviteeCount; 
            private readonly Lazy<decimal> _investmentSum, _inviteeInvestmentSum;
            public decimal InvestmentSum { get { return _investmentSum.Value; } }
            public int InviteeCount { get { return _inviteeCount.Value; } }
            public decimal InviteeInvestmentSum { get { return _inviteeInvestmentSum.Value; } }
        }

        private List<RankItem> QueryRankUser()
        {
            var context = new Agp2pDataContext();
            var option = new DataLoadOptions();
            option.LoadWith<dt_users>(u => u.li_project_transactions);
            option.LoadWith<dt_users>(u => u.li_invitations1);
            option.LoadWith<dt_users>(u => u.li_wallets);
            context.LoadOptions = option;

            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();
            var users = context.dt_users.Where(w => !canAccessGroups.Any() || canAccessGroups.Contains(w.group_id))
                .Where(u => u.user_name.Contains(txtKeywords.Text) || u.real_name.Contains(txtKeywords.Text) || u.mobile.Contains(txtKeywords.Text))
                .Where(u => 0 < u.li_invitations1.Count || 0 < u.li_wallets.total_investment)
                .ToList();
            var startTime = string.IsNullOrWhiteSpace(txtStartTime.Text) ? (DateTime?) null : Convert.ToDateTime(txtStartTime.Text);
            var endTime = string.IsNullOrWhiteSpace(txtEndTime.Text) ? (DateTime?) null : Convert.ToDateTime(txtEndTime.Text);
            return users.Select(u => new RankItem(u, startTime, endTime)).Where(r => r.InvestmentSum > 0 || r.InviteeCount > 0 || r.InviteeInvestmentSum > 0).ToList();
        }

        protected static decimal QueryInvestmentSum(dt_users user, DateTime? startTime, DateTime? endTime)
        {
            return user.li_project_transactions
                .Where(
                    t =>
                        t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                        t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest)
                .Where(t => startTime == null || startTime <= t.create_time)
                .Where(t => endTime == null || t.create_time <= endTime)
                .Select(t => t.principal).AsEnumerable().DefaultIfEmpty(0).Sum();
        }

        protected static decimal QueryInviteeInvestmentSum(dt_users user, DateTime? startTime, DateTime? endTime)
        {
            return user.li_invitations1.Where(
                i =>
                    i.li_project_transactions != null &&
                    i.li_project_transactions.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .Where(i => startTime == null || startTime <= i.dt_users.reg_time)
                .Where(i => endTime == null || i.dt_users.reg_time <= endTime)
                .Select(i => i.li_project_transactions.principal).AsEnumerable()
                .DefaultIfEmpty(0)
                .Sum();
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
            Response.Redirect(Utils.CombUrlTxt("investment_rank_list.aspx", "startTime={0}&endTime={1}&keywords={2}", txtStartTime.Text, txtEndTime.Text,txtKeywords.Text.Trim()));
        }
        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("investment_rank_list.aspx", "startTime={0}&endTime={1}&keywords={2}", txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim()));
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var query = QueryRankUser().OrderByDescending(h => h.InvestmentSum)
                .Skip(pageSize*(page - 1))
                .Take(pageSize).AsEnumerable().Select(h => new
                {
                    user = h.User == null ? "" : h.User.user_name,
                    name = h.User == null ? "" : h.User.real_name,
                    h.InvestmentSum,
                    h.InviteeCount,
                    h.InviteeInvestmentSum
                });

            var titles = new[] { "用户", "姓名", "投资金额", "邀请人数", "邀请人首次投资金额"};
            Utils.ExportXls("平台交易流水", titles, query, Response);
        }
    }
}