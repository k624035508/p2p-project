using System;
using System.Collections.Generic;
using System.Linq;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.statistic
{
    public partial class bad_debt_summary : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("repay_summary", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");  //关键字查询
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            var beforePaging = GetSummaryList();
            rptList.DataSource = beforePaging.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("repay_summary.aspx",
                "keywords={0}&page={1}", txtKeywords.Text, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IEnumerable<BadDebtSummary> GetSummaryList()
        {
            var allRepayTask =
                context.li_repayment_tasks.Where(r => r.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid)
                    .OrderByDescending(r => r.should_repay_time);

            var query2 = allRepayTask.GroupBy(r => r.should_repay_time.Year + "年" + r.should_repay_time.Month + "月");
            var query3 = query2.AsEnumerable().OrderByDescending(r => r.Key).Zip(Utils.Infinite(1), (rt, no) => new { rt, no }).Select(rs =>
            {
                var repayTask = rs.rt;
                var overTimeRepay =
                    repayTask.Where(r => r.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTimePaid).AsQueryable();
                var overTimeNotRepay =
                    repayTask.Where(r => r.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime).AsQueryable();
                var badDebtSummary = new BadDebtSummary
                {
                    Index = rs.no.ToString(),
                    YearMonth = rs.rt.Key,
                    TotalCount = overTimeRepay.Count() + overTimeNotRepay.Count(),
                    TotalAmount = (overTimeRepay.Sum(o => o.repay_interest + o.repay_principal) + overTimeNotRepay.Sum(o => o.repay_interest + o.repay_principal)).ToString("N"),
                    NotRepayCount = overTimeNotRepay.Count(),
                    NotRepayAmount = overTimeNotRepay.Sum(o => o.repay_interest + o.repay_principal).ToString("N"),
                    PepayCount = repayTask.Count(r => r.prepay != null),
                    PepayAmount = repayTask.Where(r => r.prepay != null).Sum(r => r.prepay)?.ToString("N"),
                    Cost = repayTask.Where(r => r.status != (int)Agp2pEnums.RepaymentStatusEnum.EarlierPaid).Sum(r => r.cost)?.ToString("N")
                };
                badDebtSummary.Rate = (badDebtSummary.TotalCount/repayTask.Count()).ToString("P1");

                return badDebtSummary;
            }).AsQueryable();
            totalCount = query3.Count();
            return query3;
        }

        public class BadDebtSummary
        {
            public string Index { get; set; }
            public string YearMonth { get; set; }
            public int TotalCount { get; set; }
            public string TotalAmount { get; set; }
            public int NotRepayCount { get; set; }
            public string NotRepayAmount { get; set; }
            public int PepayCount { get; set; }
            public string PepayAmount { get; set; }
            public string Cost { get; set; }
            public string Rate { get; set; }
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
            Response.Redirect(Utils.CombUrlTxt("repay_summary.aspx", "keywords={0}&status={1}&year={2}&month={3}&orderby={4}",txtKeywords.Text));
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
            Response.Redirect(Utils.CombUrlTxt("repay_summary.aspx", "keywords={0}&status={1}&year={2}&month={3}&orderby={4}", txtKeywords.Text));
        }
        

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            
        }
    }
}