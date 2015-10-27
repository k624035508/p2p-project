using System;
using System.Collections.Generic;
using System.Linq;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.statistic
{
    public partial class repay_summary : UI.ManagePage
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
            var beforePaging = GetRepaySummaryList();
            rptList.DataSource = beforePaging.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("repay_summary.aspx",
                "keywords={0}&page={1}", txtKeywords.Text, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IEnumerable<RepaySummary> GetRepaySummaryList()
        {
            var allRepayTask =
                context.li_repayment_tasks.Where(r => r.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid)
                    .OrderByDescending(r => r.should_repay_time);

            var query2 = allRepayTask.GroupBy(r => r.should_repay_time.Year + "年" + r.should_repay_time.Month + "月");
            var query3 = query2.AsEnumerable().OrderByDescending(r => r.Key).Zip(Utils.Infinite(1), (rt, no) => new { rt, no }).Select(rs =>
            {
                var repayTask = rs.rt;
                var alreadyRepayQuery = repayTask.Where(r => r.status >= (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid).AsQueryable();//已还借款数
                var repaySummary = new RepaySummary
                {
                    Index = rs.no.ToString(),
                    YearMonth = repayTask.Key,
                    ShouldRepayCount = repayTask.Count(),
                    ShouldRepayAmount = repayTask.Sum(r => r.repay_interest + r.repay_principal).ToString("N"),
                    RepayCount = alreadyRepayQuery.Count(),
                    RepayAmount = alreadyRepayQuery.Sum(r => r.repay_interest + r.repay_principal).ToString("N"),
                    RepayOnTimeCount = alreadyRepayQuery.Count(r => r.status != (int)Agp2pEnums.RepaymentStatusEnum.OverTimePaid),
                    OverNoRepayCount = repayTask.Count(r => r.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime), 
                };
                repaySummary.RepayRate = (repaySummary.RepayCount/repaySummary.ShouldRepayCount).ToString("P1");
                repaySummary.RepayOnTimeRate =
                    (repaySummary.RepayOnTimeCount/repaySummary.ShouldRepayCount).ToString("P1");
                repaySummary.OverNoRepayRate =
                    (repaySummary.OverNoRepayCount/repaySummary.ShouldRepayCount).ToString("P1");
                repaySummary.OverCount = repaySummary.RepayCount - repaySummary.RepayOnTimeCount +
                                         repaySummary.OverNoRepayCount;
                repaySummary.OverRate = (repaySummary.OverCount/repaySummary.ShouldRepayCount).ToString("P1");
                return repaySummary;
            }).AsQueryable();
            totalCount = query3.Count();
            return query3;
        }

        public class RepaySummary
        {
            public string Index { get; set; }
            /// <summary>
            /// 统计年月
            /// </summary>
            public string YearMonth { get; set; }
            /// <summary>
            /// 应还总数
            /// </summary>
            public decimal ShouldRepayCount { get; set; }
            /// <summary>
            /// 应还总额
            /// </summary>
            public string ShouldRepayAmount { get; set; }
            /// <summary>
            /// 已还款数
            /// </summary>
            public decimal RepayCount { get; set; }
            /// <summary>
            /// 已还款金额
            /// </summary>
            public string RepayAmount { get; set; }
            /// <summary>
            /// 已还款完成率
            /// </summary>
            public string RepayRate { get; set; }
            /// <summary>
            /// 按时还款数
            /// </summary>
            public decimal RepayOnTimeCount { get; set; }
            /// <summary>
            /// 按时完成率
            /// </summary>
            public string RepayOnTimeRate { get; set; }
            /// <summary>
            /// 逾期还款数
            /// </summary>
            public decimal OverCount { get; set; }
            /// <summary>
            /// 逾期占比
            /// </summary>
            public string OverRate { get; set; }
            /// <summary>
            /// 逾期未还数
            /// </summary>
            public decimal OverNoRepayCount { get; set; }
            /// <summary>
            /// 逾期未还占比
            /// </summary>
            public string OverNoRepayRate { get; set; }
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