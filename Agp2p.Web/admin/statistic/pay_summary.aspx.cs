using System;
using System.Collections.Generic;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.statistic
{
    public partial class pay_summary : UI.ManagePage
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
                ChkAdminLevel("pay_summary", DTEnums.ActionEnum.View.ToString()); //检查权限
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
            string pageUrl = Utils.CombUrlTxt("pay_summary.aspx",
                "keywords={0}&page={1}", txtKeywords.Text, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IEnumerable<PaySummary> GetRepaySummaryList()
        {
            var allRepayTask =
                context.li_repayment_tasks.Where(r => r.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid)
                    .OrderByDescending(r => r.should_repay_time);

            var query2 = allRepayTask.GroupBy(r => r.should_repay_time.Year + "年" + r.should_repay_time.Month + "月");
            var query3 = query2.AsEnumerable().OrderByDescending(r => r.Key).Zip(Utils.Infinite(1), (rt, no) => new { rt, no }).Select(rs =>
            {
                var repayTasks = rs.rt;
                PaySummary paySummary = new PaySummary
                {
                    Index = rs.no.ToString(),
                    YearMonth = repayTasks.Key
                };
                //已付款
                repayTasks.Where(r => r.status >= (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid).ForEach(r =>
                {
                    getRepayTransactions(r).ForEach(p =>
                    {
                        paySummary.RepayAmount += p.interest + p.principal ?? p.principal;
                        paySummary.RepayCount++;
                    });
                });
                //逾期付款
                int overTimePay = repayTasks.Where(r => r.status == (int) Agp2pEnums.RepaymentStatusEnum.OverTimePaid).Sum(r => getRepayTransactions(r).Count());
                //逾期未付款
                int overTimeNotPay = 0;
                decimal overTimeNotPayAmount = 0;
                repayTasks.Where(r => r.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime).ForEach(r =>
                {
                    getRepayTransactions(r).ForEach(p =>
                    {
                        overTimeNotPayAmount += p.interest + p.principal ?? p.principal;
                        overTimeNotPay++;
                    });
                });
                //未到期付款
                int noYetPay = 0;
                decimal noYetPayAmount = 0;
                repayTasks.Where(r => r.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid).ForEach(r =>
                {
                    getRepayTransactions(r).ForEach(p =>
                    {
                        noYetPayAmount += p.interest + p.principal ?? p.principal;
                        noYetPay++;
                    });
                });

                paySummary.ShouldRepayCount = paySummary.RepayCount + overTimeNotPay + noYetPay;
                paySummary.ShouldRepayAmount = (paySummary.RepayAmount + overTimeNotPayAmount + noYetPayAmount).ToString("N");
                paySummary.RepayRate = (paySummary.RepayCount/paySummary.ShouldRepayCount).ToString("P1");
                paySummary.RepayOnTimeCount = paySummary.RepayCount - overTimePay;
                paySummary.RepayOnTimeRate = (paySummary.RepayOnTimeCount/paySummary.ShouldRepayCount).ToString("P1");
                paySummary.OverCount = overTimePay + overTimeNotPay;
                paySummary.OverRate = (paySummary.OverCount/ paySummary.ShouldRepayCount).ToString("P1");
                paySummary.OverNoRepayCount = overTimeNotPay;
                paySummary.OverNoRepayRate = (overTimeNotPay / paySummary.ShouldRepayCount).ToString("P1");

                return paySummary;
            }).AsQueryable();
            totalCount = query3.Count();
            return query3;
        }

        private List<li_project_transactions> getRepayTransactions(li_repayment_tasks repayment)
        {
            var pro = repayment.li_projects;
            List<li_project_transactions> profiting;
            if (repayment.status >= (int)Agp2pEnums.RepaymentStatusEnum.ManualPaid)
            {
                // 查询所有收益记录
                profiting = pro.li_project_transactions.Where(
                    t =>
                        t.create_time == repayment.repay_at && t.type != (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        t.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success).ToList();
            }
            else
            {
                profiting = TransactionFacade.GenerateRepayTransactions(repayment, repayment.should_repay_time); // 临时预计收益
            }
            return profiting;
        }

        public class PaySummary
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
            public decimal RepayAmount { get; set; }
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
            Response.Redirect(Utils.CombUrlTxt("pay_summary.aspx", "keywords={0}&status={1}&year={2}&month={3}&orderby={4}",txtKeywords.Text));
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
            Response.Redirect(Utils.CombUrlTxt("pay_summary.aspx", "keywords={0}&status={1}&year={2}&month={3}&orderby={4}", txtKeywords.Text));
        }
        

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var beforePaging = GetRepaySummaryList();
            var lsData = beforePaging.Skip(pageSize * (page - 1)).Take(pageSize).Select(d => new
            {
                d.Index,
                d.YearMonth,
                d.ShouldRepayCount,
                d.ShouldRepayAmount,
                d.RepayCount,
                d.RepayAmount,
                d.RepayRate,
                d.RepayOnTimeCount,
                d.RepayOnTimeRate,
                d.OverCount,
                d.OverRate,
                d.OverNoRepayCount,
                d.OverNoRepayRate
            });

            var titles = new[] { "序号", "时间", "应兑付总数", "应兑付总金额", "已兑付数", "已兑付金额", "已兑付完成率", "按时兑付数", "按时兑付占比", "逾期兑付数", "逾期兑付占比", "逾期未兑付数", "逾期未兑付占比" };
            Utils.ExportXls("应兑付汇总", titles, lsData, Response);
        }
    }
}