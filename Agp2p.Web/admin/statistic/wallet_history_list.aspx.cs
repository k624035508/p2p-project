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
        protected string today;

        protected int action_type;
        protected string user_id = string.Empty;

        protected decimal TransactionIncome = 0;
        protected decimal TransactionOutcome = 0;

        //过滤充值、提现的申请待确定和取消状态
        readonly int[] _ignoringHistoryTypesSpecificUser =
            {
                (int) Agp2pEnums.WalletHistoryTypeEnum.WithdrawConfirm,
                (int) Agp2pEnums.WalletHistoryTypeEnum.Charging, (int) Agp2pEnums.WalletHistoryTypeEnum.ChargeCancel,
            };
        readonly int[] _ignoringHistoryTypes =
            {
                (int) Agp2pEnums.WalletHistoryTypeEnum.WithdrawConfirm,
                (int) Agp2pEnums.WalletHistoryTypeEnum.Charging, (int) Agp2pEnums.WalletHistoryTypeEnum.ChargeCancel,
                (int) Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess,
            };

        protected void Page_Load(object sender, EventArgs e)
        {
            user_id = DTRequest.GetQueryString("user_id");
            action_type = DTRequest.GetQueryInt("action_type");
            userGroup = DTRequest.GetQueryInt("userGroup");
            today = DTRequest.GetQueryString("today");

            pageSize = GetPageSize(GetType().Name + "_page_size");
            page = DTRequest.GetQueryInt("page", 1);
            if (!Page.IsPostBack)
            {
                txtKeywords.Text = DTRequest.GetQueryString("keywords");
                txtStartTime.Text = DTRequest.GetQueryString("startTime");
                txtEndTime.Text = DTRequest.GetQueryString("endTime");
                if (!string.IsNullOrEmpty(today))
                    cb_today.Checked = bool.Parse(today);

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
                .Select(te => new ListItem(Utils.GetAgp2pEnumDes(te), ((int)te).ToString()))
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
                "keywords={0}&startTime={1}&endTime={2}&userGroup={3}&action_type={4}&today={5}", txtKeywords.Text, txtStartTime.Text,
                txtEndTime.Text, ddlUserGroud.SelectedValue, action_type.ToString(), today));
        }
        #endregion

        #region 数据绑定=================================

        protected class UserGroupData
        {
            public string Index { get; set; }
            public string GroupName { get; set; }
            public decimal ReCharge { get; set; }
            public decimal WithDraw { get; set; }
            public decimal Invest { get; set; }
            public decimal Principal { get; set; }
            public decimal Interest { get; set; }
        }

        private void RptBind()
        {
            var query = QueryWalletHistories();
            if (rblType.SelectedValue == "0")
            {
                totalCount = query.Count();
                rptList.DataSource =
                    query.OrderByDescending(h => h.id)
                        .Skip(pageSize * (page - 1))
                        .Take(pageSize)
                        .ToList();
                rptList.DataBind();

                //绑定页码
                txtPageNum.Text = pageSize.ToString();
                var pageUrl = Utils.CombUrlTxt("wallet_history_list.aspx",
                    "user_id={0}&page={1}&action_type={2}&startTime={3}&endTime={4}&keywords={5}&userGroup={6}&today={7}",
                    user_id, "__id__",
                    action_type.ToString(), txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(),
                    userGroup.ToString(), today);
                PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
            }
            else
            {
                rptList_summary.DataSource = QueryGroupData(query);
                rptList_summary.DataBind();
            }
        }

        private List<UserGroupData> QueryGroupData(IQueryable<li_wallet_histories> query)
        {
            var gData = query.AsEnumerable().GroupBy(d => d.dt_users.group_id)
                    .Zip(Utils.Infinite(1), (dg, no) => new { dg, no })
                        .Select(d => new UserGroupData()
                        {
                            Index = d.no.ToString(),
                            GroupName = d.dg.First().dt_users.dt_user_groups.title,
                            //充值金额
                            ReCharge =
                                    d.dg.Where(
                                        i => i.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.ChargeConfirm)
                                        .Sum(i => i.li_bank_transactions.value),
                            //提现金额
                            WithDraw =
                                    d.dg.Where(
                                        i => i.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.WithdrawConfirm)
                                        .Sum(i => i.li_bank_transactions.value),
                            //投资金额
                            Invest =
                                    d.dg.Where(i => i.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.Invest 
                                        && i.li_project_transactions.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success)
                                        .Sum(i => i.li_project_transactions.principal),
                            //返还本金
                            Principal =
                                    d.dg.Where(
                                        i => i.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipal
                                             ||
                                             i.action_type ==
                                             (int)Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest)
                                        .Sum(i => i.li_project_transactions.principal),
                            //返还利息
                            Interest =
                                    d.dg.Where(
                                        i => i.action_type == (int)Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest
                                             ||
                                             i.action_type ==
                                             (int)Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest
                                             ||
                                             i.action_type ==
                                             (int)Agp2pEnums.WalletHistoryTypeEnum.RepaidOverdueFine)
                                        .Sum(i => i.li_project_transactions.interest ?? 0)

                        }).ToList();
            gData.Add(
                new UserGroupData()
                {
                    Index = null,
                    GroupName = "总计",
                    ReCharge = gData.Sum(s => s.ReCharge),
                    WithDraw = gData.Sum(s => s.WithDraw),
                    Invest = gData.Sum(s => s.Invest),
                    Principal = gData.Sum(s => s.Principal),
                    Interest = gData.Sum(s => s.Interest)
                }
                );
            return gData;
        }

        private IQueryable<li_wallet_histories> QueryWalletHistories()
        {
            var userId = string.IsNullOrWhiteSpace(user_id) ? 0 : Convert.ToInt32(user_id);

            var options = new DataLoadOptions();
            options.LoadWith<li_wallet_histories>(h => h.li_project_transactions);
            options.LoadWith<li_wallet_histories>(h => h.li_bank_transactions);
            options.LoadWith<li_wallet_histories>(h => h.li_activity_transactions);
            options.LoadWith<li_project_transactions>(tr => tr.li_projects);
            options.LoadWith<li_project_transactions>(tr => tr.li_claims_from);
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

            if (cb_today.Checked)
            {
                query = query.Where(h => h.create_time.Date == DateTime.Now.Date);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                    query = query.Where(h => h.create_time.Date >= Convert.ToDateTime(txtStartTime.Text));
                if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                    query = query.Where(h => h.create_time.Date <= Convert.ToDateTime(txtEndTime.Text));
            }

            query = query.Where(h =>
                    (h.dt_users.real_name.Contains(txtKeywords.Text) || h.dt_users.user_name.Contains(txtKeywords.Text)) || h.dt_users.mobile.Contains(txtKeywords.Text))
                    .Where(h => userId == 0 ? !_ignoringHistoryTypes.Contains(h.action_type) : !_ignoringHistoryTypesSpecificUser.Contains(h.action_type));

            return query;
        }

        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("wallet_history_list.aspx",
                "user_id={0}&action_type={1}&startTime={2}&endTime={3}&keywords={4}&userGroup={5}&today={6}", user_id, action_type.ToString(),
                txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(), userGroup.ToString(), today));
        }

        //筛选类别
        protected void ddlWalletHistoryTypeId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("wallet_history_list.aspx",
                "user_id={0}&action_type={1}&startTime={2}&endTime={3}&keywords={4}&userGroup={5}&today={6}", user_id,
                ddlWalletHistoryTypeId.SelectedValue, txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(), userGroup.ToString(), today));
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
            {Agp2pEnums.WalletHistoryTypeEnum.Invest, "投资 {0} {1}"},
            {Agp2pEnums.WalletHistoryTypeEnum.InvestorRefund, "投资撤回 {0}"},
            {Agp2pEnums.WalletHistoryTypeEnum.InvestSuccess, "项目满标 {0}"},
            {Agp2pEnums.WalletHistoryTypeEnum.RepaidInterest, "{0} 回款 {1}"},
            {Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipal, "{0} 回款 {1}"},
            {Agp2pEnums.WalletHistoryTypeEnum.RepaidPrincipalAndInterest, "{0} 回款 {1}"},
            {Agp2pEnums.WalletHistoryTypeEnum.HuoqiProjectWithdrawSuccess, "{0} 活期项目提现 {1}"},
        };

        protected string QueryTransactionRemark(li_wallet_histories his, Func<li_wallet_histories, string> projectNameMapper)
        {
            if (his.li_project_transactions != null)
            {
                if (!RemarkHintMap.ContainsKey((Agp2pEnums.WalletHistoryTypeEnum)his.action_type))
                {
                    RemarkHintMap[(Agp2pEnums.WalletHistoryTypeEnum)his.action_type] = "{0} " + Utils.GetAgp2pEnumDes((Agp2pEnums.WalletHistoryTypeEnum)his.action_type);
                }
                if (his.li_project_transactions.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest)
                {
                    return
                        string.Format(RemarkHintMap[(Agp2pEnums.WalletHistoryTypeEnum) his.action_type],
                            projectNameMapper(his),
                            his.li_project_transactions.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success
                                ? ""
                                : "已撤销");
                }
                var proj = his.li_project_transactions.li_projects;
                if (proj.IsNewbieProject1())
                {
                    return string.Format(RemarkHintMap[(Agp2pEnums.WalletHistoryTypeEnum) his.action_type], projectNameMapper(his), "");
                }
                if (proj.IsHuoqiProject())
                {
                    var claim = his.li_project_transactions.li_claims_from;
                    return string.Format(RemarkHintMap[(Agp2pEnums.WalletHistoryTypeEnum) his.action_type],
                        projectNameMapper(his), claim == null ? "" : "债权: " + claim.number);
                }
                // 查出 还款期数/总期数
                var term = proj.li_repayment_tasks.SingleOrDefault(t => t.repay_at == his.li_project_transactions.create_time)?.term.ToString() ?? "?";
                var repaytaskInfo = string.Format("{0}/{1}", term,
                    proj.li_repayment_tasks.Count(t => t.status != (int)Agp2pEnums.RepaymentStatusEnum.Invalid));
                return string.Format(RemarkHintMap[(Agp2pEnums.WalletHistoryTypeEnum)his.action_type], projectNameMapper(his), repaytaskInfo);
            }
            if (his.li_bank_transactions != null)
            {
                if (his.li_bank_transactions.type == (int) Agp2pEnums.BankTransactionTypeEnum.LoanerMakeLoan)
                {
                    var projectId = Convert.ToInt32(his.li_bank_transactions.remarks);
                    return $"关联项目：{context.li_projects.Single(p => p.id == projectId).title}";
                }
                if (his.li_bank_transactions.type == (int) Agp2pEnums.BankTransactionTypeEnum.GainLoanerRepay)
                {
                    var repaymentTaskId = Convert.ToInt32(his.li_bank_transactions.remarks);
                    var task = context.li_repayment_tasks.Single(p => p.id == repaymentTaskId);
                    return $"关联还款计划：{task.li_projects.title} 第 {task.term} 期";
                }
                return his.li_bank_transactions.remarks;
            }
            return his.li_activity_transactions != null ? his.li_activity_transactions.remarks : "";
        }

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("wallet_history_list.aspx",
                "user_id={0}&action_type={1}&startTime={2}&endTime={3}&keywords={4}&userGroup={5}&today={6}", user_id,
                ddlWalletHistoryTypeId.SelectedValue, txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(), userGroup.ToString(), today));
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
            var query = QueryWalletHistories();
            var userName = getUserName();
            if (rblType.SelectedValue == "0")
            {
                var data = query.OrderByDescending(h => h.id)
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
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    Utils.ExportXls(userName + "的资金明细", titles, data, Response);
                }
                else
                {
                    Utils.ExportXls("会员资金明细", titles, data, Response);
                }
            }
            else
            {
                var data = QueryGroupData(query).Select(h => new
                {
                    h.GroupName,
                    h.ReCharge,
                    h.WithDraw,
                    h.Invest,
                    h.Principal,
                    h.Interest
                });
                var titles = new[] { "会员组", "充值金额", "提现金额", "投资金额", "返还本金", "返还利息"};
                Utils.ExportXls("会员资金明细汇总", titles, data, Response);
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
            }
        }

        protected void cb_today_OnCheckedChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("wallet_history_list.aspx",
                "user_id={0}&action_type={1}&startTime={2}&endTime={3}&keywords={4}&userGroup={5}&today={6}", user_id,
                ddlWalletHistoryTypeId.SelectedValue, txtStartTime.Text, txtEndTime.Text, txtKeywords.Text.Trim(), userGroup.ToString(), cb_today.Checked.ToString()));
        }

        /// <summary>
        /// 切换汇总/明细列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rblType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblType.SelectedValue == "0")
            {
                rptList.Visible = true;
                rptList_summary.Visible = false;
                div_page.Visible = true;
            }
            else
            {
                rptList.Visible = false;
                rptList_summary.Visible = true;
                div_page.Visible = false;
            }
            RptBind();
        }
    }
}