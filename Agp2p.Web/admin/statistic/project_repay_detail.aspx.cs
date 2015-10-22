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
        protected Dictionary<int, string> CategoryIdTitleMap;

        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            CategoryIdTitleMap = new Agp2pDataContext().dt_article_category.Where(c => c.channel_id == 6).ToDictionary(c => c.id, c => c.title);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("statistics_projects_repay_detail", DTEnums.ActionEnum.View.ToString()); //检查权限
                var status = DTRequest.GetQueryString("status");
                if (!string.IsNullOrEmpty(status))
                    rblRepaymentTaskStatus.SelectedValue = status;

                var keywords = DTRequest.GetQueryString("keywords");
                var y = DTRequest.GetQueryString("year");
                var m = DTRequest.GetQueryString("month");

                txtKeywords.Text = keywords;
                txtYear.Text = y == "" ? "-" : y;
                txtMonth.Text = m == "" ? "-" : m;
                RptBind();
            }
        }

        protected class RepaymentTaskDetail
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
        protected class RepaymentDetail
        {
            public RepaymentTaskDetail RepaymentTask { get; set; }
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
            var beforePaging = QueryRepayDetails();
            totalCount = beforePaging.Count();
            rptList.DataSource = beforePaging.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("project_repay_detail.aspx",
                "keywords={0}&page={1}&year={2}&month={3}&status={4}", txtKeywords.Text, "__id__", txtYear.Text,
                txtMonth.Text, rblRepaymentTaskStatus.SelectedValue);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private List<RepaymentDetail> QueryRepayDetails()
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<li_repayment_tasks>(t => t.li_projects);
            loadOptions.LoadWith<li_projects>(t => t.li_project_transactions);
            loadOptions.LoadWith<li_project_transactions>(t => t.dt_users);
            context.LoadOptions = loadOptions;

            var query = context.li_repayment_tasks.Where(r => r.li_projects.title.Contains(txtKeywords.Text));
            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();

            if (rblRepaymentTaskStatus.SelectedValue != "0")
            {
                if (rblRepaymentTaskStatus.SelectedValue == "2") // 已还款
                    query = query.Where(r => (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid <= r.status);
                else
                    query = query.Where(r => Convert.ToByte(rblRepaymentTaskStatus.SelectedValue) == r.status);
            }

            if (txtYear.Text != "-")
                query = query.Where(t => t.should_repay_time.Year == Convert.ToInt32(txtYear.Text));
            if (txtMonth.Text != "-")
                query = query.Where(t => t.should_repay_time.Month == Convert.ToInt32(txtMonth.Text));

            var sorted = query.OrderBy(r => r.should_repay_time).AsEnumerable();

            var appendedSums = sorted
                    .Zip(Utils.Infinite(1), (repayment, index) => new {repayment, index})
                    .SelectMany(ri =>
                    {
                        var r = ri.repayment;
                        var pro = r.li_projects;
                        List<li_project_transactions> profiting;
                        if (ri.repayment.status != (int) Agp2pEnums.RepaymentStatusEnum.Unpaid)
                        {
                            // 查询所有收益记录
                            profiting = pro.li_project_transactions.Where(
                                t =>
                                    t.create_time == r.repay_at && t.type != (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                                    t.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success).ToList();
                        }
                        else
                        {
                            profiting = TransactionFacade.GenerateRepayTransactions(r, r.should_repay_time); // 临时预计收益
                        }
                        //根据组权限查询数据
                        profiting = profiting.Where(ptr => !canAccessGroups.Any() || canAccessGroups.Contains(ptr.dt_users.group_id)).ToList();

                        if (profiting.Count == 0) return Enumerable.Empty<RepaymentDetail>();
                        var repaymentDetails = profiting.Select(tr => new RepaymentDetail
                        {
                            RepaymentTask = new RepaymentTaskDetail(),
                            InvestorRealName = tr.dt_users.real_name,
                            InvestorUserName = tr.dt_users.user_name,
                            InvestValue = pro.li_project_transactions.Where(
                                t =>
                                    t.investor == tr.investor &&
                                    t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                                    t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                                .Select(t => t.principal).AsEnumerable().Sum(), // 反查总投资金额
                            InvestTime =
                                pro.li_project_transactions.Last(t => t.investor == tr.investor &&
                                                                      t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                                                                      t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                                    .create_time.ToString(), // 用最后一次的投资时间作为呈现的时间
                            RepayPrincipal = tr.principal,
                            RepayInterest = tr.interest.GetValueOrDefault(0),
                            RepayTotal = (tr.principal + tr.interest.GetValueOrDefault(0)),
                        }).ToList();

                        repaymentDetails.First().RepaymentTask = new RepaymentTaskDetail
                        {
                            Index = ri.index.ToString(),
                            ProjectName = pro.title,
                            Category = CategoryIdTitleMap[pro.category_id],
                            ShouldRepayAt = r.should_repay_time.ToString("yyyy-MM-dd"),
                            CreditorName = pro.li_risks.li_creditors == null ? pro.li_risks.li_loaners.dt_users.real_name : pro.li_risks.li_creditors.dt_users.real_name,
                            FinancingAmount = pro.financing_amount,
                            ProfitRateYear = pro.profit_rate_year.ToString(),
                            Term = r.term + "/" + pro.repayment_term_span_count,
                            RepayAt = r.repay_at != null ? ((DateTime)r.repay_at).ToString("yyyy-MM-dd") : "",
                            //OverTimeDay = r.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid && DateTime.Now > r.should_repay_time 
                            //    ? r.repay_at == null ? (r.should_repay_time.Subtract(DateTime.Now)).Days.ToString() : (r.should_repay_time.Subtract((DateTime)r.repay_at)).Days.ToString() : "0",

                            InvestCompleteTime = pro.invest_complete_time.ToString(),
                            RepayCompleteTime = pro.li_repayment_tasks.Max(cr => cr.should_repay_time).ToString("yyyy-MM-dd"),
                        }; // 首个记录显示项目信息
                        return repaymentDetails.Concat(new[]
                        {
                            new RepaymentDetail
                            {
                                RepaymentTask = new RepaymentTaskDetail {ProjectName = "合计"},
                                InvestValue = repaymentDetails.Sum(p => p.InvestValue),
                                RepayInterest = repaymentDetails.Sum(p => p.RepayInterest),
                                RepayPrincipal = repaymentDetails.Sum(p => p.RepayPrincipal),
                                RepayTotal = repaymentDetails.Sum(p => p.RepayTotal),
                            }
                        });
                    }).ToList();

            var sumRows = appendedSums.Where(p => p.RepaymentTask.ProjectName != null && p.RepaymentTask.ProjectName.EndsWith("合计")).ToList();
            var beforePaging = appendedSums.Concat(new[]
            {
                new RepaymentDetail
                {
                    RepaymentTask = new RepaymentTaskDetail {ProjectName = "总合计"},
                    InvestValue = null,
                    RepayInterest = sumRows.Sum(p => p.RepayInterest),
                    RepayPrincipal = sumRows.Sum(p => p.RepayPrincipal),
                    RepayTotal = sumRows.Sum(p => p.RepayTotal),
                }
            }).ToList();
            return beforePaging;
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

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project_repay_detail.aspx", "keywords={0}&year={1}&month={2}&status={3}",
                txtKeywords.Text, txtYear.Text, txtMonth.Text, rblRepaymentTaskStatus.SelectedValue));
        }

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
            Response.Redirect(Utils.CombUrlTxt("project_repay_detail.aspx", "keywords={0}&year={1}&month={2}&status={3}",
                txtKeywords.Text, txtYear.Text, txtMonth.Text, rblRepaymentTaskStatus.SelectedValue));
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

        protected void txtMonth_OnTextChanged(object sender, EventArgs e)
        {
            if (txtMonth.Text == "")
                txtMonth.Text = "-"; // 减号表示不限，如果不用减号的话会认为是首次加载网页，未填写月份，会默认设为当前月份
            RptBind();
        }

        protected void txtYear_OnTextChanged(object sender, EventArgs e)
        {
            if (txtYear.Text == "")
                txtYear.Text = "-";
            RptBind();
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var beforePaging = QueryRepayDetails();
            var lsData = beforePaging.Skip(pageSize*(page - 1)).Take(pageSize).Select(d => new
            {
                d.RepaymentTask.Index,
                d.RepaymentTask.ProjectName,
                d.RepaymentTask.CreditorName,
                d.RepaymentTask.Category,
                d.RepaymentTask.FinancingAmount,
                d.RepaymentTask.ProfitRateYear,
                d.RepaymentTask.InvestCompleteTime,
                d.RepaymentTask.RepayCompleteTime,
                d.RepaymentTask.Term,
                d.RepaymentTask.ShouldRepayAt,
                d.RepaymentTask.RepayAt,
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