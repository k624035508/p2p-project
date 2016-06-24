using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.statistic
{
    public partial class project_repay_detail : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected int CategoryId;
        protected Dictionary<int, string> CategoryIdTitleMap;

        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(GetType().Name + "_page_size");
            page = DTRequest.GetQueryInt("page", 1);
            this.CategoryId = DTRequest.GetQueryInt("category_id");
            CategoryIdTitleMap = new Agp2pDataContext().dt_article_category.Where(c => c.channel_id == 6).OrderBy(c => c.sort_id).ToDictionary(c => c.id, c => c.title);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("statistics_projects_repay_detail", DTEnums.ActionEnum.View.ToString()); //检查权限
                var status = DTRequest.GetQueryString("status");
                if (!string.IsNullOrEmpty(status))
                    rblRepaymentTaskStatus.SelectedValue = status;

                txtKeywords.Text = DTRequest.GetQueryString("keywords");
                txtStartTime.Text = DTRequest.GetQueryString("startTime");
                txtEndTime.Text = DTRequest.GetQueryString("endTime");

                TreeBind();
                RptBind();
            }
        }

        protected void TreeBind()
        {
            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            this.ddlCategoryId.Items.AddRange(CategoryIdTitleMap.Select(c => new ListItem(c.Value, c.Key.ToString())).ToArray());
            if (this.CategoryId > 0)
            {
                this.ddlCategoryId.SelectedValue = this.CategoryId.ToString();
            }
        }

        protected class RepayTaskDetail
        {
            public string Index { get; set; }
            public string ShouldRepayAt { get; set; }
            public string ProjectName { get; set; }
            public string CreditorName { get; set; }
            public decimal? FinancingAmount { get; set; }
            public string Category { get; set; }

            public string ProfitRateYear { get; set; }
            public string Term { get; set; }
            public string InvestCompleteTime { get; set; }
            public string RepayCompleteTime { get; set; }
            public string OverTimeDay { get; set; }
            public string RepayAt { get; set; }
        }
        protected class RepayTransactionDetail
        {
            public RepayTaskDetail RepayTask { get; set; }
            public string InvestorRealName { get; set; }
            public string InvestorUserName { get; set; }
            public decimal? InvestValue { get; set; }
            public string InvestTime { get; set; }

            public decimal RepayPrincipal { get; set; }
            public decimal RepayInterest { get; set; }
            public decimal RepayTotal { get; set; }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            if (string.IsNullOrWhiteSpace(txtStartTime.Text) && string.IsNullOrWhiteSpace(txtKeywords.Text) && string.IsNullOrWhiteSpace(txtEndTime.Text))
            {
                txtStartTime.Text = DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd");
            }

            var totalCount = 0;
            if (string.IsNullOrWhiteSpace(txtStartTime.Text))
            {
                var dataSource = QueryRepayDetails().Skip(pageSize * (page - 1)).Take(pageSize).ToList();

                totalCount = dataSource.Count == pageSize ? (page + 1) * pageSize : page * pageSize;
                rptList.DataSource = dataSource;
                rptList.DataBind();
            }
            else
            {
                var beforePaging = QueryRepayDetails().ToList();
                totalCount = beforePaging.Count;
                rptList.DataSource = beforePaging.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
                rptList.DataBind();
            }

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("project_repay_detail.aspx",
                "keywords={0}&page={1}&startTime={2}&endTime={3}&status={4}", txtKeywords.Text, "__id__", txtStartTime.Text,
                txtEndTime.Text, rblRepaymentTaskStatus.SelectedValue);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IEnumerable<RepayTransactionDetail> QueryRepayDetails()
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<li_repayment_tasks>(t => t.li_projects);
            loadOptions.LoadWith<li_projects>(t => t.li_project_transactions);
            loadOptions.LoadWith<li_project_transactions>(t => t.dt_users);
            context.LoadOptions = loadOptions;

            var query = context.li_repayment_tasks.Where(r => r.status != (int)Agp2pEnums.RepaymentStatusEnum.Invalid && r.li_projects.title.Contains(txtKeywords.Text));
            if (CategoryId > 0)
                query = query.Where(q => q.li_projects.category_id == CategoryId);
            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();

            if (rblRepaymentTaskStatus.SelectedValue != "0")
            {
                if (rblRepaymentTaskStatus.SelectedValue == "2") // 已还款
                    query = query.Where(r => (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid <= r.status);
                else
                    query = query.Where(r => r.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid || r.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime);
            }

            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
            {
                query = query.Where(q => Convert.ToDateTime(txtStartTime.Text) <= q.should_repay_time.Date);
            }
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
            {
                query = query.Where(q => q.should_repay_time.Date <= Convert.ToDateTime(txtEndTime.Text));
            }

            var repaymentTasks = query.OrderBy(r => r.should_repay_time).AsEnumerableAutoPartialQuery(16);

            decimal totalInterest = 0, totalPrincipal = 0, totalRepay = 0;

            var emptyRepayTaskDetail = new RepayTaskDetail();
            var appendedSums = repaymentTasks
                .Zip(Utils.Infinite(1), (repayment, index) => new {repayment, index})
                .SelectMany(ri =>
                {
                    var repaymentTask = ri.repayment;
                    var pro = repaymentTask.li_projects;
                    List<li_project_transactions> profiting;

                    if (ri.repayment.status >= (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid)
                    {
                        // 查询所有收益记录
                        profiting = pro.li_project_transactions.Where(
                            t =>
                                t.create_time == repaymentTask.repay_at && t.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                                (t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.RepayToInvestor ||
                                 t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.RepayOverdueFine)).ToList();
                    }
                    else
                    {
                        // 临时预计收益，直接查询有效的叶子节点（避免活期债权影响债权比率，会小于 1）
                        var claimRatio = repaymentTask.li_projects.li_claims
                            .Where(c => c.status < (int) Agp2pEnums.ClaimStatusEnum.Completed && c.IsLeafClaim())
                            .ToDictionary(c => c, c => c.principal/repaymentTask.li_projects.investment_amount);
                        profiting = claimRatio.GenerateRepayTransactions(repaymentTask, repaymentTask.should_repay_time, true);
                    }

                    //根据组权限查询数据
                    profiting = profiting.Where(ptr => !canAccessGroups.Any() || canAccessGroups.Contains(ptr.dt_users.group_id)).ToList();

                    if (profiting.Count == 0) return Enumerable.Empty<RepayTransactionDetail>();
                    var repaymentDetails = profiting.Select(tr => new RepayTransactionDetail
                    {
                        RepayTask = emptyRepayTaskDetail,
                        InvestorRealName = tr.dt_users.real_name,
                        InvestorUserName = tr.dt_users.user_name,
                        // 用最后一次的投资时间作为呈现的时间
                        InvestTime = tr.li_claims_from != null
                            ? tr.li_claims_from.createTime.ToString()
                            : pro.li_project_transactions.Last(t =>
                                t.investor == tr.investor &&
                                t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                                t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest).create_time.ToString(),
                        // 反查总投资金额
                        InvestValue = tr.li_claims_from != null
                            ? tr.li_claims_from.principal
                            : pro.li_project_transactions.Where(t =>
                                t.investor == tr.investor &&
                                t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                                t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest)
                                .Aggregate(0m, (sum, ptr) => sum + ptr.principal),
                        RepayPrincipal = tr.principal,
                        RepayInterest = tr.interest.GetValueOrDefault(0),
                        RepayTotal = (tr.principal + tr.interest.GetValueOrDefault(0))
                    }).ToList();

                    repaymentDetails.First().RepayTask = new RepayTaskDetail
                    {
                        Index = ri.index.ToString(),
                        ProjectName = pro.title,
                        Category = CategoryIdTitleMap[pro.category_id],
                        ShouldRepayAt = repaymentTask.should_repay_time.ToString("yyyy-MM-dd"),
                        CreditorName = context.GetLonerName(repaymentTask.project),
                        FinancingAmount = pro.financing_amount,
                        ProfitRateYear = pro.profit_rate_year.ToString(),
                        Term = repaymentTask.li_projects.repayment_term_span == (int) Agp2pEnums.ProjectRepaymentTermSpanEnum.Day
                            ? "1/1"
                            : $"{repaymentTask.term.ToString()}/{repaymentTask.li_projects.repayment_term_span_count}",
                        RepayAt = repaymentTask.repay_at?.ToString("yyyy-MM-dd") ?? "",

                        InvestCompleteTime = pro.invest_complete_time.ToString(),
                        RepayCompleteTime =
                            pro.li_repayment_tasks.Max(cr => cr.should_repay_time).ToString("yyyy-MM-dd"),
                    }; // 首个记录显示项目信息

                    var subSum = new RepayTransactionDetail
                    {
                        RepayTask = new RepayTaskDetail {ProjectName = "小计"},
                        InvestValue = repaymentDetails.Sum(p => p.InvestValue),
                        RepayInterest = repaymentDetails.Sum(p => p.RepayInterest),
                        RepayPrincipal = repaymentDetails.Sum(p => p.RepayPrincipal),
                        RepayTotal = repaymentDetails.Sum(p => p.RepayTotal),
                    };
                    totalInterest += subSum.RepayInterest;
                    totalPrincipal += subSum.RepayPrincipal;
                    totalRepay += subSum.RepayTotal;
                    return repaymentDetails.Concat(Enumerable.Repeat(subSum, 1));
                });

            var totalSum = Enumerable.Range(0, 1).Select(i => new RepayTransactionDetail
            {
                RepayTask = new RepayTaskDetail {ProjectName = "总计"},
                InvestValue = null,
                RepayInterest = totalInterest,
                RepayPrincipal = totalPrincipal,
                RepayTotal = totalRepay
            });

            var beforePaging = appendedSums.Concat(totalSum);
            return beforePaging;
        }

        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project_repay_detail.aspx", "keywords={0}&startTime={1}&endTime={2}&status={3}&category_id={4}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, rblRepaymentTaskStatus.SelectedValue, ddlCategoryId.SelectedValue));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("project_repay_detail.aspx", "keywords={0}&startTime={1}&endTime={2}&status={3}&category_id={4}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, rblRepaymentTaskStatus.SelectedValue, ddlCategoryId.SelectedValue));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project_repay_detail.aspx", "keywords={0}&startTime={1}&endTime={2}&status={3}&category_id={4}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, rblRepaymentTaskStatus.SelectedValue, ddlCategoryId.SelectedValue));
        }

        protected DateTime GetRepaymentCompleteTime(li_project_transactions pro)
        {
            return pro.li_projects.li_repayment_tasks.Max(r => r.should_repay_time);
        }

        protected object EvalWhenFirstRepayment(RepeaterItem container, string field)
        {
            return IsFirstRepayment(container) ? Eval(field) : "";
        }

        protected bool IsFirstRepayment(RepeaterItem container)
        {
            var itemIndex = container.ItemIndex;
            if (itemIndex == 0) return true;

            var prevItem = (rptList.DataSource as List<li_project_transactions>)[itemIndex - 1];
            return prevItem.li_projects.id != ((li_project_transactions)container.DataItem).li_projects.id;
        }

        protected decimal GetInvestedMoney(li_project_transactions tr)
        {
            return context.li_project_transactions.Where(
                t =>
                    t.project == tr.project && t.investor == tr.investor &&
                    t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .Select(t => t.principal)
                .AsEnumerable()
                .Sum();
        }

        protected DateTime GetInvestTime(li_project_transactions tr)
        {
            return context.li_project_transactions.Where(
                t =>
                    t.project == tr.project && t.investor == tr.investor &&
                    t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .Select(t => t.create_time).Max();
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("statistics_projects_repay_detail", DTEnums.ActionEnum.DownLoad.ToString()); //检查权限
            var beforePaging = QueryRepayDetails();
            var lsData = beforePaging.Skip(pageSize*(page - 1)).Take(pageSize).Select(d => new
            {
                d.RepayTask.Index,
                d.RepayTask.ProjectName,
                d.RepayTask.CreditorName,
                d.RepayTask.Category,
                d.RepayTask.FinancingAmount,
                d.RepayTask.ProfitRateYear,
                d.RepayTask.InvestCompleteTime,
                d.RepayTask.RepayCompleteTime,
                d.RepayTask.Term,
                d.RepayTask.ShouldRepayAt,
                d.RepayTask.RepayAt,
                d.InvestorRealName,
                d.RepayPrincipal,
                d.RepayInterest,
                d.RepayTotal
            });
            var titles = new[] {
                "序号", "标题", "债权/借款人", "产品", "借款金额", "年利率", "满标时间", "到期日", "期数", "应付时间", "实付时间", "收款人", "兑付本金", "兑付利息", "本息合计"
            };
            Utils.ExportXls("应兑付明细", titles, lsData, Response);
        }

        protected void rblRepaymentTaskStatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
        }
    }
}