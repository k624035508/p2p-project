using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Agp2p.BLL;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.EnterpriseServices;
using Agp2p.Core;

namespace Agp2p.Web.admin.statistic
{
    public partial class wallet_history_list : UI.ManagePage
    {
        Agp2pDataContext context = new Agp2pDataContext();
        protected int totalCount;
        protected int page;
        protected int pageSize;
        protected int userGroup;

        protected int action_type;
        protected string user_id = string.Empty;

        protected decimal idle_money = 0;
        protected decimal investing_money = 0;
        protected decimal total_investment = 0;
        protected decimal profiting_money = 0;
        protected decimal total_profit = 0;
        protected decimal TransactionIncome = 0;
        protected decimal TransactionOutcome = 0;

        //过滤充值、提现的申请待确定和取消状态
        readonly int[] _ignoringHistoryTypesSpecificUser =
            {
                (int) Agp2pEnums.WalletHistoryTypeEnum.Withdrawing, (int) Agp2pEnums.WalletHistoryTypeEnum.WithdrawCancel,
                (int) Agp2pEnums.WalletHistoryTypeEnum.Charging, (int) Agp2pEnums.WalletHistoryTypeEnum.ChargeCancel,
            };
        readonly int[] _ignoringHistoryTypes =
            {
                (int) Agp2pEnums.WalletHistoryTypeEnum.Withdrawing, (int) Agp2pEnums.WalletHistoryTypeEnum.WithdrawCancel,
                (int) Agp2pEnums.WalletHistoryTypeEnum.Charging, (int) Agp2pEnums.WalletHistoryTypeEnum.ChargeCancel,
                (int) Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess,(int)Agp2pEnums.WalletHistoryTypeEnum.Gaining,
                (int) Agp2pEnums.WalletHistoryTypeEnum.Losting
            };

        protected void Page_Load(object sender, EventArgs e)
        {            
            user_id = DTRequest.GetQueryString("user_id");
            action_type = DTRequest.GetQueryInt("action_type");
            userGroup = DTRequest.GetQueryInt("userGroup");

            pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            if (!Page.IsPostBack)
            {
                var startTime = DTRequest.GetQueryString("startTime");
                if (!string.IsNullOrEmpty(startTime))
                    txtStartTime.Text = startTime;
                var endTime = DTRequest.GetQueryString("endTime");
                if (!string.IsNullOrEmpty(endTime))
                    txtEndTime.Text = endTime;
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                ChkAdminLevel("statistics_wallets_histories", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind();
                RptBind();
            }
        }

        #region 绑定操作类别、用户组=================================
        protected void TreeBind()
        {
            //操作类别
            ddlWalletHistoryTypeId.Items.Clear();
            ddlWalletHistoryTypeId.Items.Add(new ListItem("所有操作类型", ""));

            var userId = string.IsNullOrWhiteSpace(user_id) ? 0 : Convert.ToInt32(user_id);
            var listItems = Utils.GetEnumValues<Agp2pEnums.WalletHistoryTypeEnum>()
                .Where(en => userId == 0 ? !_ignoringHistoryTypes.Contains((int)en) : !_ignoringHistoryTypesSpecificUser.Contains((int)en))
                .Select(te => new ListItem(Utils.GetAgp2pEnumDes(te), ((int) te).ToString()))
                .ToArray();
            ddlWalletHistoryTypeId.Items.AddRange(listItems);

            //用户组
            var context = new Agp2pDataContext();
            ddlUserGroud.Items.Clear();
            ddlUserGroud.Items.Add(new ListItem("所有会员组", ""));

            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();

            ddlUserGroud.Items.AddRange(context.dt_user_groups.Where(g => g.is_lock == 0 && (!canAccessGroups.Any() || canAccessGroups.Contains(g.id)))
                .OrderByDescending(g => g.id)
                .Select(g => new ListItem(g.title, g.id.ToString()))
                .ToArray());
        }

        protected void ddlUserGroud_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("wallet_history_list.aspx",
                "keywords={0}&startTime={1}&endTime={2}&userGroup={3}&action_type={4}", txtKeywords.Text, txtStartTime.Text,
                txtEndTime.Text, ddlUserGroud.SelectedValue, action_type.ToString()));
        }
        #endregion

        #region 数据绑定=================================

        private void RptBind()
        {
            var query = QueryWalletHistories();

            totalCount = query.Count();
            rptList.DataSource =
                query.OrderByDescending(h => h.id)
                    .Skip(pageSize*(page - 1))
                    .Take(pageSize)
                    .ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("wallet_history_list.aspx",
                "user_id={0}&page={1}&action_type={2}&startTime={3}&endTime={4}&keywords={5}&userGroup={6}", user_id, "__id__",
                action_type.ToString(), txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(), userGroup.ToString());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IQueryable<li_wallet_histories> QueryWalletHistories()
        {
            var userId = string.IsNullOrWhiteSpace(user_id) ? 0 : Convert.ToInt32(user_id);

            var options = new DataLoadOptions();
            options.LoadWith<li_wallet_histories>(h => h.li_project_transactions);
            options.LoadWith<li_wallet_histories>(h => h.li_bank_transactions);
            options.LoadWith<li_wallet_histories>(h => h.li_activity_transactions);
            options.LoadWith<li_project_transactions>(tr => tr.li_projects);
            context.LoadOptions = options;

            IQueryable<li_wallet_histories> query = context.li_wallet_histories;
            if (userId != 0)
                query = query.Where(h => h.user_id == userId);
            if (0 < action_type)
            {
                ddlWalletHistoryTypeId.SelectedValue = action_type.ToString();
                query = query.Where(h => h.action_type == action_type);
            }
            //用户分组查询
            if (0 < userGroup) // 选择了某一组
            {
                ddlUserGroud.SelectedValue = userGroup.ToString();
                query = query.Where(w => w.dt_users.group_id == userGroup);
            }
            else
            {
                // 限制当前管理员对会员的查询
                var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();
                query = query.Where(w => !canAccessGroups.Any() || canAccessGroups.Contains(w.dt_users.group_id));
            }

            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(h => h.create_time.Date >= Convert.ToDateTime(txtStartTime.Text));
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(h => h.create_time.Date <= Convert.ToDateTime(txtEndTime.Text));

            query = query.Where(h =>
                    (h.dt_users.real_name.Contains(txtKeywords.Text) || h.dt_users.user_name.Contains(txtKeywords.Text)) || h.dt_users.mobile.Contains(txtKeywords.Text))
                    .Where(h => userId == 0 ? !_ignoringHistoryTypes.Contains(h.action_type) : !_ignoringHistoryTypesSpecificUser.Contains(h.action_type));

            return query;
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
            Response.Redirect(Utils.CombUrlTxt("wallet_history_list.aspx",
                "user_id={0}&action_type={1}&startTime={2}&endTime={3}&keywords={4}&userGroup={5}", user_id, action_type.ToString(),
                txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(), userGroup.ToString()));
        }

        //筛选类别
        protected void ddlWalletHistoryTypeId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("wallet_history_list.aspx",
                "user_id={0}&action_type={1}&startTime={2}&endTime={3}&keywords={4}&userGroup={5}", user_id,
                ddlWalletHistoryTypeId.SelectedValue, txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(), userGroup.ToString()));
        }

        private const string ProjectInvestDetailHref = "<a target='_blank' href='project_investor_detail.aspx?keywords={1}&year=-&month=-&status=0'>{0}</a>";
        private const string ProjectRepayDetailHref = "<a target='_blank' href='project_repay_detail.aspx?keywords={1}&year=-&month=-&status=0'>{0}</a>";
        //private const string ProjectAgreeHref = "<a onclick='ShowAgreeContract({0},{1}); ' href='javascript:;'>《投资协议》</a>";

        protected readonly Func<li_wallet_histories, string> GetHrefByProjectStatus = pth =>
        {
            var project = pth.li_project_transactions.li_projects;
            return
                string.Format(project.invest_complete_time != null ? ProjectRepayDetailHref : ProjectInvestDetailHref,
                    project.title, project.title.Replace("+", "%2b"));
        };

        readonly Dictionary<Agp2pEnums.WalletHistoryTypeEnum, string> RemarkHintMap = new Dictionary<Agp2pEnums.WalletHistoryTypeEnum, string>
        {
            {Agp2pEnums.WalletHistoryTypeEnum.Invest, "投资 {0}"},
            {Agp2pEnums.WalletHistoryTypeEnum.InvestorRefund, "投资撤回 {0}"},
            {Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess, "项目满标 {0}"},
            {Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest, "{0} 还款 {1}"},
            {Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipal, "{0} 还款 {1}"},
            {Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest, "{0} 还款 {1}"}
        };

        protected string QueryTransactionRemark(li_wallet_histories his, Func<li_wallet_histories, string> projectNameMapper)
        {
            if (his.li_project_transactions != null)
            {
                if (his.li_project_transactions.type != (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest)
                {
                    // 查出 还款期数/总期数
                    var term = context.li_wallet_histories.Count(
                        h => h.user_id == his.user_id && h.create_time <= his.create_time &&
                             (h.action_type == (int) Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest ||
                              h.action_type == (int) Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipal ||
                              h.action_type == (int) Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest) &&
                             h.li_project_transactions.project == his.li_project_transactions.project);
                    var repaytaskInfo = string.Format("{0}/{1}", term, his.li_project_transactions.li_projects.li_repayment_tasks.Count(t => t.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid));
                    return string.Format(RemarkHintMap[(Agp2pEnums.WalletHistoryTypeEnum)his.action_type], projectNameMapper(his), repaytaskInfo);
                }
                return 
                    string.Format(RemarkHintMap[(Agp2pEnums.WalletHistoryTypeEnum)his.action_type], projectNameMapper(his));
            }
            if (his.li_bank_transactions != null)
            {
                return his.li_bank_transactions.remarks;
            }
            return his.li_activity_transactions != null ? his.li_activity_transactions.remarks : "";
        }

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("wallet_history_list.aspx",
                "user_id={0}&action_type={1}&startTime={2}&endTime={3}&keywords={4}&userGroup={5}", user_id,
                ddlWalletHistoryTypeId.SelectedValue, txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(), userGroup.ToString()));
        }

        protected string getUserName()
        {
            if (!string.IsNullOrWhiteSpace(user_id))
            {
                var user = context.dt_users.Single(u => u.id == Convert.ToInt32(user_id));
                return !string.IsNullOrWhiteSpace(user.real_name)
                    ? user.real_name : user.user_name;
            }
            return "";
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var query = QueryWalletHistories().OrderByDescending(h => h.id)
                .Skip(pageSize * (page - 1))
                .Take(pageSize).AsEnumerable().Select(h => new
                {
                    user = h.dt_users == null ? "" : h.dt_users.user_name,
                    name = h.dt_users == null ? "" : h.dt_users.real_name,
                    actionType = Utils.GetAgp2pEnumDes((Agp2pEnums.WalletHistoryTypeEnum)h.action_type),
                    income = TransactionFacade.QueryTransactionIncome<decimal?>(h),
                    outcome = TransactionFacade.QueryTransactionOutcome(h),
                    h.idle_money,
                    h.investing_money,
                    h.total_investment,
                    h.profiting_money,
                    h.total_profit,
                    h.create_time,
                    remark = QueryTransactionRemark(h, his => his.li_project_transactions.li_projects.title) 
                });

            var titles = new[] { "用户", "姓名", "操作类型", "收入金额", "支出金额", "可用余额", "在投金额", "累计投资", "待收利润", "已收利润", "交易时间", "备注" };
            var userName = getUserName();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                Utils.ExportXls(userName + "的交易记录", titles, query, Response);
            }
            else
            {
                Utils.ExportXls("平台交易流水", titles, query, Response);
            }
        }

        protected string MoneyToString(decimal? money)
        {
            return money == null ? "" : money.Value.ToString("c");
        }

        protected void rptList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                var wh = (li_wallet_histories)e.Item.DataItem;
                decimal Tin = TransactionFacade.QueryTransactionIncome(wh, (receivedPrincipal, profited) => receivedPrincipal.GetValueOrDefault(0) + profited.GetValueOrDefault(0));
                decimal Tout = TransactionFacade.QueryTransactionOutcome(wh).GetValueOrDefault(0);
                TransactionIncome += Tin;
                TransactionOutcome += Tout;

                idle_money += wh.idle_money;
                investing_money += wh.investing_money;
                total_investment += wh.total_investment;
                profiting_money += wh.profiting_money;
                total_profit += wh.total_profit;
            }
        }
    }
}