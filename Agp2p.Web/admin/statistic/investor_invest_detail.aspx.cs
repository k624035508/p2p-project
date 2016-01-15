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
    public partial class investor_invest_detail : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;
        protected Dictionary<int, string> CategoryIdTitleMap;

        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(GetType().Name + "_page_size");
            page = DTRequest.GetQueryInt("page", 1);
            CategoryIdTitleMap = new Agp2pDataContext().dt_article_category.Where(c => c.channel_id == 6).ToDictionary(c => c.id, c => c.title);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("statistics_investor_invest_detail", DTEnums.ActionEnum.View.ToString()); //检查权限

                txtKeywords.Text = DTRequest.GetQueryString("keywords");
                txtStartTime.Text = DTRequest.GetQueryString("startTime");
                txtEndTime.Text = DTRequest.GetQueryString("endTime");
                RptBind();
            }
        }

        protected class InvestorInvestDetail
        {
            public string Index { get; set; }
            public string InvestorRealName { get; set; }
            public string InvestorUserName { get; set; }
            public string ProjectName { get; set; }
            public DateTime? InvestTime { get; set; }
            public string ProjectCompleteTime { get; set; }
            public string Term { get; set; }
            public string ProfitRateYear { get; set; }
            public decimal? InvestValue { get; set; }
            public decimal? RepayTotal { get; set; }
            public decimal? Total { get; set; }
            public string Category { get; set; }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            var beforePaging = QueryRepayDetails(out totalCount);

            if (rblType.SelectedValue == "0")
            {
                rptList.DataSource = beforePaging.Skip(pageSize*(page - 1)).Take(pageSize).ToList();
                rptList.DataBind();

                //绑定页码
                txtPageNum.Text = pageSize.ToString();
                string pageUrl = Utils.CombUrlTxt("investor_invest_detail.aspx",
                    "keywords={0}&page={1}&startTime={2}&endTime={3}", txtKeywords.Text, "__id__", txtStartTime.Text,txtEndTime.Text);
                PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
            }
            else
            {
                rptList_summary.DataSource = QueryGroupData(beforePaging);
                rptList_summary.DataBind();
            }
        }

        private List<InvestorInvestDetail> QueryGroupData(IEnumerable<InvestorInvestDetail> query)
        {
            var summaryData =
                    query.Where(d => d.Index != null).GroupBy(d => d.InvestorUserName)
                        .Zip(Utils.Infinite(1), (dg, no) => new { dg, no })
                        .Select(d =>
                        {
                            return new InvestorInvestDetail()
                            {
                                Index = d.no.ToString(),
                                InvestorUserName = string.IsNullOrEmpty(d.dg.First().InvestorRealName) ? d.dg.First().InvestorUserName : d.dg.First().InvestorRealName,
                                InvestValue = d.dg.Sum(i => i.InvestValue),
                                RepayTotal = d.dg.Sum(i => i.RepayTotal),
                                Total = d.dg.Sum(i => i.Total)
                            };
                        }).ToList();
            summaryData.Add(new InvestorInvestDetail()
            {
                Index = null,
                InvestorUserName = "总计",
                InvestValue = summaryData.Sum(i => i.InvestValue),
                RepayTotal = summaryData.Sum(i => i.RepayTotal),
                Total = summaryData.Sum(i => i.Total)
            });
            return summaryData;
        }

        private IEnumerable<InvestorInvestDetail> QueryRepayDetails(out int count)
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<li_project_transactions>(t => t.dt_users);
            loadOptions.LoadWith<li_project_transactions>(t => t.li_projects);
            context.LoadOptions = loadOptions;

            var query = context.li_project_transactions.Where(tr =>
                tr.dt_users.user_name.Contains(txtKeywords.Text) || tr.dt_users.real_name.Contains(txtKeywords.Text))
                .Where(tr =>
                    tr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                    tr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest);

            // 查询自己的包括下属的组的组员
            var subordinatesId = context.dt_manager.Single(m => m.id == GetAdminInfo().id).dt_manager2.Select(s => s.id).ToArray();
            var canAccessGroups =
                context.li_user_group_access_keys.Where(
                    k => k.owner_manager == GetAdminInfo().id || subordinatesId.Contains(k.owner_manager))
                    .Select(k => k.user_group).Distinct()
                    .ToArray();
            query = query.Where(tr => !canAccessGroups.Any() || canAccessGroups.Contains(tr.dt_users.group_id));

            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
            {
                query = query.Where(q => Convert.ToDateTime(txtStartTime.Text) <= q.create_time.Date);
            }
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
            {
                query = query.Where(q => q.create_time.Date <= Convert.ToDateTime(txtEndTime.Text));
            }
            var qu = query.GroupBy(tr => tr.dt_users);

            var groupCount = qu.Count();
            count = groupCount == 0 ? 0 : groupCount + qu.Sum(g => g.Count()) + 1;
            if (groupCount == 0) return Enumerable.Empty<InvestorInvestDetail>();

            var now = DateTime.Now;
            int index = 1;
            var withSum = qu.AsEnumerable().SelectMany(g =>
            {
                var investorInvestDetails = g.Select(tr =>
                {
                    var proj = tr.li_projects;
                    var profitRate = proj.invest_complete_time == null
                        ? proj.profit_rate = // cache in project rate for 小计
                            proj.GetFinalProfitRate(now)
                        : proj.profit_rate;
                    var repayTotal = Math.Round(tr.principal*profitRate, 2);
                    return new InvestorInvestDetail
                    {
                        Index = (index++).ToString(),
                        InvestorRealName = g.Key.real_name,
                        InvestorUserName = g.Key.user_name,
                        InvestTime = tr.create_time,
                        InvestValue = tr.principal,
                        ProfitRateYear = proj.profit_rate_year.ToString(),
                        ProjectCompleteTime =
                            proj.make_loan_time == null
                                ? "(未放款)"
                                : proj.CalcRepayTimeByTerm(proj.CalcRealTermCount()).ToString(),
                        ProjectName = proj.title,
                        Term = proj.repayment_term_span_count + Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTermSpanEnum) proj.repayment_term_span),
                        RepayTotal = repayTotal,
                        Total = tr.principal + repayTotal,
                        Category = CategoryIdTitleMap[proj.category_id]
                    };
                });
                return investorInvestDetails.Concat(Enumerable.Range(0, 1).Select(i =>
                {
                    var investSum = g.Sum(tr => tr.principal);
                    var profitSum = g.Sum(tr => Math.Round(tr.principal*tr.li_projects.profit_rate, 2));
                    return new InvestorInvestDetail
                    {
                        InvestorUserName = "小计",
                        InvestValue = investSum,
                        RepayTotal = profitSum,
                        Total = investSum + profitSum
                    };
                }));
            });
            return withSum.Concat(Enumerable.Range(0, 1).Select(i =>
            {
                var investSum = qu.Sum(g => g.Sum(tr => tr.principal));
                var profitSum = qu.AsEnumerable().Sum(g =>
                {
                    return g.Sum(tr =>
                    {
                        var proj = tr.li_projects;
                        var profitRate = proj.invest_complete_time == null
                            ? proj.GetFinalProfitRate(now)
                            : proj.profit_rate;
                        return Math.Round(tr.principal*profitRate, 2);
                    });
                });
                return new InvestorInvestDetail
                {
                    InvestorUserName = "总计",
                    InvestValue = investSum,
                    RepayTotal = profitSum,
                    Total = investSum + profitSum
                };
            }));
        }

        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("investor_invest_detail.aspx", "keywords={0}&startTime={1}&endTime={2}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("investor_invest_detail.aspx", "keywords={0}&startTime={1}&endTime={2}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text));
        }

        protected DateTime GetRepaymentCompleteTime(li_project_transactions pro)
        {
            return pro.li_projects.li_repayment_tasks.Max(r => r.should_repay_time);
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            int count;
            var beforePaging = QueryRepayDetails(out count);
            if (rblType.SelectedValue == "0")
            {
                var lsData = beforePaging.Skip(pageSize*(page - 1)).Take(pageSize).Select(d => new
                {
                    d.InvestorRealName,
                    d.ProjectName,
                    d.Category,
                    d.InvestTime,
                    d.ProjectCompleteTime,
                    d.Term,
                    d.ProfitRateYear,
                    d.InvestValue,
                    d.RepayTotal,
                    d.Total
                });
                var titles = new[]
                {
                    "投资者", "标题", "产品", "投资时间", "到期时间", "期限", "年利率", "投资本金", "利息", "本息合计"
                };
                Utils.ExportXls("会员投资明细", titles, lsData, Response);
            }
            else
            {
                var lsData = QueryGroupData(beforePaging).ToList();
                var lsData2 = lsData.Select(d => new
                {
                    d.Index,
                    d.InvestorUserName,
                    d.InvestValue,
                    d.RepayTotal,
                    d.Total
                });
                var titles = new[]
                {
                    "序号", "投资者", "投资本金", "利息", "本息合计"
                };
                Utils.ExportXls("会员投资汇总", titles, lsData2, Response);
            }
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