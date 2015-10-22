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
            pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            CategoryIdTitleMap = new Agp2pDataContext().dt_article_category.Where(c => c.channel_id == 6).ToDictionary(c => c.id, c => c.title);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("statistics_investor_invest_detail", DTEnums.ActionEnum.View.ToString()); //检查权限

                var keywords = DTRequest.GetQueryString("keywords");
                var y = DTRequest.GetQueryString("year");
                var m = DTRequest.GetQueryString("month");

                txtKeywords.Text = keywords;
                txtYear.Text = y == "" ? DateTime.Now.Year.ToString() : y;
                txtMonth.Text = m == "" ? DateTime.Now.Month.ToString() : m;
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

            rptList.DataSource = beforePaging.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("investor_invest_detail.aspx",
                "keywords={0}&page={1}&year={2}&month={3}", txtKeywords.Text, "__id__", txtYear.Text,
                txtMonth.Text);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
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

            if (txtYear.Text != "-")
            {
                query = query.Where(tr => tr.create_time.Year == Convert.ToInt32(txtYear.Text));
            }
            if (txtMonth.Text != "-")
            {
                query = query.Where(tr => tr.create_time.Month == Convert.ToInt32(txtMonth.Text));
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
                            TransactionFacade.CalcFinalProfitRate(now, proj.profit_rate_year, (Agp2pEnums.ProjectRepaymentTermSpanEnum) proj.repayment_term_span, proj.repayment_term_span_count)
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
                            proj.invest_complete_time == null
                                ? "(未满标)"
                                : TransactionFacade.CalcRepayTime(proj.invest_complete_time.Value, (Agp2pEnums.ProjectRepaymentTermSpanEnum) proj.repayment_term_span,
                                    proj.repayment_term_span_count, proj.repayment_term_span_count).ToString(),
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
                            ? TransactionFacade.CalcFinalProfitRate(now, proj.profit_rate_year, (Agp2pEnums.ProjectRepaymentTermSpanEnum) proj.repayment_term_span, proj.repayment_term_span_count)
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
            Response.Redirect(Utils.CombUrlTxt("investor_invest_detail.aspx", "keywords={0}&year={1}&month={2}",
                txtKeywords.Text, txtYear.Text, txtMonth.Text));
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
            Response.Redirect(Utils.CombUrlTxt("investor_invest_detail.aspx", "keywords={0}&year={1}&month={2}",
                txtKeywords.Text, txtYear.Text, txtMonth.Text));
        }

        protected DateTime GetRepaymentCompleteTime(li_project_transactions pro)
        {
            return pro.li_projects.li_repayment_tasks.Max(r => r.should_repay_time);
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
            int count;
            var beforePaging = QueryRepayDetails(out count);
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
            var titles = new[] {
                "投资者", "标题", "产品", "投资时间", "到期时间", "期限", "年利率", "投资本金", "利息", "合计"
            };
            Utils.ExportXls("投资人投资明细汇总", titles, lsData, Response);
        }
    }
}