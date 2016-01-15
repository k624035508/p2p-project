using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.statistic
{
    public partial class project_repay_task : UI.ManagePage
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
                ChkAdminLevel("statistics_projects_repay_task", DTEnums.ActionEnum.View.ToString()); //检查权限
                txtKeywords.Text = DTRequest.GetQueryString("keywords");
                txtStartTime.Text = DTRequest.GetQueryString("startTime");
                txtEndTime.Text = DTRequest.GetQueryString("endTime");
                var status = DTRequest.GetQueryString("status");
                if (!string.IsNullOrEmpty(status))
                    rblRepaymentStatus.SelectedValue = status;
                var orderBy = DTRequest.GetQueryString("orderby");
                if (!string.IsNullOrWhiteSpace(orderBy))
                    ddlOrderBy.SelectedValue = orderBy;
                
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

        protected class ProjectDetail
        {
            public string Index { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal? FinancingAmount { get; set; }
            public string ProfitRateYear { get; set; }
            public string Term { get; set; }
            public DateTime? InvestCompleteTime { get; set; }
            public DateTime? RepayCompleteTime { get; set; }
            public string Creditor { get; set; }
        }
        protected class RepaymentTaskAmountDetail
        {
            public ProjectDetail Project { get; set; }
            public string RepayTime { get; set; }
            public string Status { get; set; }
            public decimal RepayPrincipal { get; set; }
            public decimal RepayInterest { get; set; }
            public decimal RepayTotal { get; set; }
            public string RepayTerm { get; set; }
            public int OverTimeDay { get; set; }
            public string RepayAt { get; set; }
            public int CategoryId { get; set; }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            var beforePaging = QueryProjectRepayTaskData(out totalCount);
            if (rblType.SelectedValue == "0")
            {
                var pageData = beforePaging.Skip(pageSize*(page - 1)).Take(pageSize).ToList();
                rptList.DataSource = pageData.Concat(Enumerable.Range(0, 1).Select(i => new RepaymentTaskAmountDetail
                {
                    Project = new ProjectDetail()
                    {
                        Index = null,
                        Name = "总计"
                    },
                    RepayInterest = pageData.Sum(p => p.RepayInterest),
                    RepayPrincipal = pageData.Sum(p => p.RepayPrincipal),
                    RepayTotal = pageData.Sum(p => p.RepayTotal)
                }));
                rptList.DataBind();

                //绑定页码
                txtPageNum.Text = pageSize.ToString();
                string pageUrl = Utils.CombUrlTxt("project_repay_task.aspx",
                    "keywords={0}&page={1}&status={2}&startTime={3}&endTime={4}&orderby={5}&category_id={6}", txtKeywords.Text,
                    "__id__",
                    rblRepaymentStatus.SelectedValue, txtStartTime.Text, txtEndTime.Text, ddlOrderBy.SelectedValue,
                    ddlCategoryId.SelectedValue);
                PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
            }
            else
            {
                rptList_summary.DataSource = QueryGroupData(beforePaging);
                rptList_summary.DataBind();
            }
        }

        private List<RepaymentTaskAmountDetail> QueryGroupData(IEnumerable<RepaymentTaskAmountDetail> query)
        {
            var summaryData =
                    query.GroupBy(d => d.CategoryId)
                        .Zip(Utils.Infinite(1), (dg, no) => new { dg, no })
                        .Select(d =>
                        {
                            return new RepaymentTaskAmountDetail()
                            {
                                Project = new ProjectDetail()
                                {
                                    Index = d.no.ToString(),
                                    Category = CategoryIdTitleMap[d.dg.Key]
                                },
                                RepayInterest = d.dg.Sum(t => t.RepayInterest),
                                RepayPrincipal = d.dg.Sum(t => t.RepayPrincipal),
                                RepayTotal = d.dg.Sum(t => t.RepayTotal)
                            };
                        }).ToList();
            summaryData.Add(new RepaymentTaskAmountDetail()
            {
                Project = new ProjectDetail()
                {
                    Index = null,
                    Category = "总计"
                },
                RepayInterest = summaryData.Sum(t => t.RepayInterest),
                RepayPrincipal = summaryData.Sum(t => t.RepayPrincipal),
                RepayTotal = summaryData.Sum(t => t.RepayTotal)
            });
            return summaryData;
        }

        private IEnumerable<RepaymentTaskAmountDetail> QueryProjectRepayTaskData(out int count)
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<li_repayment_tasks>(t => t.li_projects);
            context.LoadOptions = loadOptions;

            var query1 =
                context.li_repayment_tasks.Where(rt =>
                    (int) Agp2pEnums.ProjectStatusEnum.Financing < rt.li_projects.status && rt.li_projects.title.Contains(txtKeywords.Text));

            if (CategoryId > 0)
                query1 = query1.Where(q => q.li_projects.category_id == CategoryId);

            if (rblRepaymentStatus.SelectedValue == "20")
            {
                //逾期还款
                query1 =
                    query1.Where(
                        r =>
                            r.status == (int) Agp2pEnums.RepaymentStatusEnum.OverTime ||
                            r.status == (int) Agp2pEnums.RepaymentStatusEnum.OverTimePaid);
            }
            else
            {
                query1 = query1.Where(r =>
                    rblRepaymentStatus.SelectedValue == "0" ||
                    Convert.ToByte(rblRepaymentStatus.SelectedValue) == r.status
                    ||
                    (Convert.ToByte(rblRepaymentStatus.SelectedValue) == (int) Agp2pEnums.RepaymentStatusEnum.Unpaid &&
                     (int) Agp2pEnums.RepaymentStatusEnum.OverTime == r.status) ||
                    (Convert.ToByte(rblRepaymentStatus.SelectedValue) == (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid &&
                     (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid <= r.status));
            }

            if (ddlOrderBy.SelectedValue == "invest_complete_time")
            {
                if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                {
                    query1 = query1.Where(rt => Convert.ToDateTime(txtStartTime.Text) <= rt.li_projects.make_loan_time.GetValueOrDefault().Date);
                }
                if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                {
                    query1 = query1.Where(rt => rt.li_projects.make_loan_time.GetValueOrDefault().Date <= Convert.ToDateTime(txtEndTime.Text));
                }
            }
            else if (ddlOrderBy.SelectedValue == "should_repay_time")
            {
                if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                {
                    query1 = query1.Where(rt => Convert.ToDateTime(txtStartTime.Text) <= rt.should_repay_time.Date);
                }
                if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                {
                    query1 = query1.Where(rt => rt.should_repay_time.Date <= Convert.ToDateTime(txtEndTime.Text));
                }
            }
            count = query1.Count();

            var query2 = query1.GroupBy(rt => rt.project);
            if (ddlOrderBy.SelectedValue == "invest_complete_time")
            {
                query2 = query2.OrderBy(g => g.First(r => r.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid).li_projects.invest_complete_time);
            }
            else if (ddlOrderBy.SelectedValue == "should_repay_time")
            {
                query2 = query2.OrderBy(g => g.First(r => r.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid).should_repay_time);
            }

            return query2.AsEnumerable().Zip(Utils.Infinite(1), (rg, i) => new {rg, i}).SelectMany(rgi =>
            {
                var p = rgi.rg.First().li_projects;
                var projectDetail = new ProjectDetail
                {
                    Index = rgi.i.ToString(),
                    Name = p.title,
                    FinancingAmount = p.financing_amount == 0 ? (decimal?) null : p.financing_amount,
                    Category = CategoryIdTitleMap[p.category_id],
                    ProfitRateYear = p.profit_rate_year.ToString(),
                    Term =
                        p.repayment_term_span_count + " " +
                        Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTermSpanEnum) p.repayment_term_span),
                    InvestCompleteTime = p.make_loan_time,
                    RepayCompleteTime = p.li_repayment_tasks.Select(r => r.should_repay_time).Last(),
                    Creditor = context.GetLonerName(p.id)
                };
                int j = 0;
                return rgi.rg.Select(rg => new RepaymentTaskAmountDetail
                {
                    Project = j++ == 0 ? projectDetail : new ProjectDetail(),
                    RepayTime = rg.should_repay_time.ToString("yyyy-MM-dd"),
                    Status = Utils.GetAgp2pEnumDes((Agp2pEnums.RepaymentStatusEnum) rg.status),
                    RepayPrincipal = rg.repay_principal,
                    RepayInterest = rg.repay_interest,
                    RepayTotal = (rg.repay_interest + rg.repay_principal),
                    RepayTerm = rg.li_projects.repayment_term_span == (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Day
                                ? "1/1"
                                : $"{rg.term.ToString()}/{rg.li_projects.repayment_term_span_count}",
                    RepayAt = rg.repay_at?.ToString("yyyy-MM-dd") ?? "",
                    CategoryId = rg.li_projects.category_id
                });
            });
        }

        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project_repay_task.aspx", "keywords={0}&status={1}&startTime={2}&endTime={3}&orderby={4}&category_id={5}",
                txtKeywords.Text, rblRepaymentStatus.SelectedValue, txtStartTime.Text, txtEndTime.Text, ddlOrderBy.SelectedValue, ddlCategoryId.SelectedValue));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("project_repay_task.aspx", "keywords={0}&status={1}&startTime={2}&endTime={3}&orderby={4}&category_id={5}",
                txtKeywords.Text, rblRepaymentStatus.SelectedValue, txtStartTime.Text, txtEndTime.Text, ddlOrderBy.SelectedValue, ddlCategoryId.SelectedValue));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project_repay_task.aspx", "keywords={0}&status={1}&startTime={2}&endTime={3}&orderby={4}&category_id={5}",
                txtKeywords.Text, rblRepaymentStatus.SelectedValue, txtStartTime.Text, txtEndTime.Text, ddlOrderBy.SelectedValue, ddlCategoryId.SelectedValue));
        }

        protected DateTime GetRepaymentCompleteTime(li_projects liProjects)
        {
            return liProjects.li_repayment_tasks.Max(r => r.should_repay_time);
        }

        protected object EvalWhenFirstTerm(RepeaterItem container, string field)
        {
            return IsFirstTerm(container) ? Eval(field) : "";
        }

        protected static bool IsFirstTerm(RepeaterItem container)
        {
            return ((li_repayment_tasks) container.DataItem).term == 1;
        }

        protected void rblRepaymentStatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            int totalCount;
            var beforePaging = QueryProjectRepayTaskData(out totalCount);
            if (rblType.SelectedValue == "0")

            {
                var lsData =
                    beforePaging.Skip(pageSize*(page - 1)).Take(pageSize).ToList();
                var lsData2 = lsData.Concat(Enumerable.Range(0, 1).Select(i => new RepaymentTaskAmountDetail
                {
                    Project = new ProjectDetail()
                    {
                        Index = null,
                        Name = "总计"
                    },
                    RepayInterest = lsData.Sum(p => p.RepayInterest),
                    RepayPrincipal = lsData.Sum(p => p.RepayPrincipal),
                    RepayTotal = lsData.Sum(p => p.RepayTotal)
                })).Select(d => CategoryIdTitleMap != null
                    ? new
                    {
                        d.Project.Index,
                        d.Project.Name,
                        d.Project.Creditor,
                        d.Project.Category,
                        d.Project.FinancingAmount,
                        d.Project.ProfitRateYear,
                        d.Project.InvestCompleteTime,
                        d.Project.RepayCompleteTime,
                        d.RepayTerm,
                        d.RepayTime,
                        d.RepayAt,
                        d.Status,
                        d.RepayPrincipal,
                        d.RepayInterest,
                        d.RepayTotal,
                    }
                    : null);
                var titles = new[]
                {
                    "序号", "标题", "债权/借款人", "产品", "借款金额", "年利率", "放款时间", "到期日", "期数", "应还日期", "实还日期", "状态", "应还本金", "应还利息",
                    "本息合计"
                };
                Utils.ExportXls("应还款明细", titles, lsData2, Response);
            }
            else
            {
                var lsData = QueryGroupData(beforePaging).Select(d => new
                {
                    d.Project.Index,
                    d.Project.Category,
                    d.RepayPrincipal,
                    d.RepayInterest,
                    d.RepayTotal
                });
                var titles = new[] {"序号", "产品", "借款金额", "利息", "本息合计"};
                Utils.ExportXls("应还款汇总", titles, lsData, Response);
            }
        }

        protected void ddlOrderBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
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