using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Text;

namespace Agp2p.Web.admin.statistic
{
    public class OfflineTransaction
    {
        public int? index { get; set; }
        public string occurTime { get; set; }
        public decimal? income { get; set; }
        public decimal? outcome { get; set; }
        public string type { get; set; }
        public string user { get; set; }
        public string remark { get; set; }
        public string project { get; set; }
    }

    public partial class offline_transact_timeline : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;
        protected string transactType;
        protected string today;

        protected string keywords = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {

            pageSize = GetPageSize(GetType().Name + "_page_size");
            page = DTRequest.GetQueryInt("page", 1);
            transactType = DTRequest.GetQueryString("transactType");
            keywords = DTRequest.GetQueryString("keywords");
            today = DTRequest.GetQueryString("today");
            if(!IsPostBack)
            {
                ChkAdminLevel("statistics_offline_transactions", DTEnums.ActionEnum.View.ToString()); //检查权限
                var startTime = DTRequest.GetQueryString("startTime");
                if (!string.IsNullOrEmpty(startTime))
                    txtStartTime.Text = startTime;
                var endTime = DTRequest.GetQueryString("endTime");
                if (!string.IsNullOrEmpty(endTime))
                    txtEndTime.Text = endTime;
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                if (!string.IsNullOrEmpty(today))
                    cb_today.Checked = bool.Parse(today);

                TreeBind();
                RptBind();
            }
        }

        #region 绑定用户分组=================================
        protected void TreeBind()
        {
            ddlRecordType.Items.Clear();
            ddlRecordType.Items.Add(new ListItem("所有操作类型", ""));

            ddlRecordType.Items.AddRange(
                Enum.GetValues(typeof (Agp2pEnums.OfflineTransactionTypeEnum))
                    .Cast<Agp2pEnums.OfflineTransactionTypeEnum>()
                    .Select(e => new ListItem(Utils.GetAgp2pEnumDes(e), "" + ((int) e))).ToArray());
            ddlRecordType.Items.RemoveAt(3);

            if (!string.IsNullOrEmpty(transactType))
                ddlRecordType.SelectedValue = transactType;
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind()
        {
            var transactions = QueryProjectTransactions();
            if (rblType.SelectedValue == "0")
            {
                var pageData = transactions.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
                rptList.DataSource = pageData.Concat(Enumerable.Range(0, 1).Select(i => new OfflineTransaction
                {
                    index = null,
                    occurTime = "总计",
                    income = pageData.Aggregate(0m, (sum, tr) => sum + tr.income.GetValueOrDefault()),
                    outcome = pageData.Aggregate(0m, (sum, tr) => sum + tr.outcome.GetValueOrDefault()),
                }));
                rptList.DataBind();
                //绑定页码
                txtPageNum.Text = pageSize.ToString();
                string pageUrl = Utils.CombUrlTxt("offline_transact_timeline.aspx", "keywords={0}&page={1}&startTime={2}&endTime={3}&transactType={4}",
                    txtKeywords.Text, "__id__", txtStartTime.Text, txtEndTime.Text, transactType);
                PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
            }
            else
            {
                rptList_summary.DataSource = QueryGroupData(transactions);
                rptList_summary.DataBind();
            }
        }

        private List<OfflineTransaction> QueryGroupData(IEnumerable<OfflineTransaction> query)
        {
            var summaryData = query.GroupBy(tr => tr.type).Zip(Utils.Infinite(1), (trg, no) => new { trg = trg, no }).Select(tr =>
            {
                var summaryRechange = new OfflineTransaction { index = tr.no, type = tr.trg.Key };
                if (tr.trg.Key == Utils.GetAgp2pEnumDes(Agp2pEnums.OfflineTransactionTypeEnum.ReChangeFee))
                {
                    summaryRechange.outcome = tr.trg.Sum(t => t.outcome);
                }
                else
                {
                    summaryRechange.income = tr.trg.Sum(t => t.income);
                }
                return summaryRechange;
            }).ToList();
            summaryData.Add(new OfflineTransaction
            {
                index = null,
                type = "总计",
                income = summaryData.Aggregate(0m, (sum, tr) => sum + tr.income.GetValueOrDefault()),
                outcome = summaryData.Aggregate(0m, (sum, tr) => sum + tr.outcome.GetValueOrDefault()),
            });
            return summaryData;
        }

        private static readonly int[] OfflineProjectTransactionType =
        {
            (int) Agp2pEnums.OfflineTransactionTypeEnum.ManagementFeeOfLoanning,
            (int) Agp2pEnums.OfflineTransactionTypeEnum.ManagementFeeOfOverTime,
            (int) Agp2pEnums.OfflineTransactionTypeEnum.ReChangeFee,
        };

        private IEnumerable<OfflineTransaction> QueryProjectTransactions()
        {
            var context = new Agp2pDataContext();
            //查找管理费、逾期费用、充值手续费
            IQueryable<li_company_inoutcome> queryTran = context.li_company_inoutcome.Where(ptr =>
                string.IsNullOrWhiteSpace(transactType) || transactType == "所有操作类型" ||
                ((Agp2pEnums.OfflineTransactionTypeEnum)ptr.type).ToString() == transactType);

            if (!string.IsNullOrWhiteSpace(txtKeywords.Text))
            {
                queryTran =
                    queryTran.Where(
                        b =>
                            b.dt_users.user_name.Contains(txtKeywords.Text) ||
                            b.dt_users.real_name.Contains(txtKeywords.Text) ||
                             b.li_projects.title.Contains(txtKeywords.Text));
            }

            if (cb_today.Checked)
            {
                queryTran = queryTran.Where(h => h.create_time.Date == DateTime.Now.Date);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                {
                    queryTran = queryTran.Where(h => Convert.ToDateTime(txtStartTime.Text) <= h.create_time);
                }
                if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                {
                    queryTran = queryTran.Where(h => h.create_time <= Convert.ToDateTime(txtEndTime.Text));
                }
            }
            queryTran = queryTran.Where(ptr => (OfflineProjectTransactionType.Contains(ptr.type)));

            totalCount = queryTran.Count();
            return queryTran.OrderByDescending(q => q.create_time).AsEnumerable()
                .Zip(Utils.Infinite(1), (prt, no) => new { prt, no })
                .Select(pptr => new OfflineTransaction
                {
                    index = pptr.no,
                    income = pptr.prt.income,
                    outcome = pptr.prt.outcome,
                    occurTime = pptr.prt.create_time.ToString("yyyy-MM-dd HH:mm"),
                    type = Utils.GetAgp2pEnumDes(
                        (Agp2pEnums.OfflineTransactionTypeEnum)pptr.prt.type),
                    remark = pptr.prt.remark,
                    user = string.IsNullOrEmpty(pptr.prt.dt_users.real_name) ? pptr.prt.dt_users.user_name : pptr.prt.dt_users.real_name,
                    project = pptr.prt.li_projects == null ? "" : pptr.prt.li_projects.title
                });
        }

        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("offline_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&transactType={3}&today={4}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, transactType, today));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("offline_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&transactType={3}&today={4}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, transactType, today));
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var data = QueryProjectTransactions();
            if (rblType.SelectedValue == "0")
            {
                var xlsData = data.Skip(pageSize*(page - 1)).Take(pageSize).ToList();
                var xlsData2 = xlsData.Concat(Enumerable.Range(0, 1).Select(i => new OfflineTransaction
                {
                    index = null,
                    occurTime = "总计",
                    income = xlsData.Aggregate(0m, (sum, tr) => sum + tr.income.GetValueOrDefault()),
                    outcome = xlsData.Aggregate(0m, (sum, tr) => sum + tr.outcome.GetValueOrDefault()),
                }));
                var titles = new[] {"序号", "时间", "收入", "支出", "操作类型", "关联人员", "备注"};
                Utils.ExportXls("平台收支明细", titles, xlsData2, Response);
            }
            else
            {
                var xlsData = QueryGroupData(data).Select(d => new
                {
                    d.index,
                    d.type,
                    d.income,
                    d.outcome   
                });
                var titles = new[] { "序号", "操作类型", "收入", "支出" };
                Utils.ExportXls("平台收支汇总", titles, xlsData, Response);
            }

        }

        //筛选类别
        protected void ddlRecordType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("offline_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&transactType={3}&today={4}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, ddlRecordType.SelectedItem.Value, today));
        }

        protected void cb_today_OnCheckedChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("offline_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&transactType={3}&today={4}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, ddlRecordType.SelectedItem.Value, cb_today.Checked.ToString()));
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