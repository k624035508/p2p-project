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

        protected string keywords = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");

            pageSize = GetPageSize(GetType().Name + "_page_size");
            page = DTRequest.GetQueryInt("page", 1);
            transactType = DTRequest.GetQueryString("transactType");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("statistics_offline_transactions", DTEnums.ActionEnum.View.ToString()); //检查权限
                var startTime = DTRequest.GetQueryString("startTime");
                if (!string.IsNullOrEmpty(startTime))
                    txtStartTime.Text = startTime;
                var endTime = DTRequest.GetQueryString("endTime");
                if (!string.IsNullOrEmpty(endTime))
                    txtEndTime.Text = endTime;
                var keywords = DTRequest.GetQueryString("keywords");  //关键字查询
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
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
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind()
        {
            var transactions = QueryProjectTransactions();
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

        private static readonly int[] OfflineProjectTransactionType =
        {
            (int) Agp2pEnums.ProjectTransactionTypeEnum.ManagementFeeOfLoanning,
            (int) Agp2pEnums.ProjectTransactionTypeEnum.ManagementFeeOfOverTime,
        };

        private IEnumerable<OfflineTransaction> QueryProjectTransactions()
        {
            var context = new Agp2pDataContext();

            IQueryable<li_project_transactions> query = context.li_project_transactions.Where(ptr =>
                string.IsNullOrWhiteSpace(transactType) || transactType == "所有操作类型" ||
                ((Agp2pEnums.ProjectTransactionTypeEnum) ptr.type).ToString() == transactType);
            
            if (!string.IsNullOrWhiteSpace(txtKeywords.Text))
            { 
                query = query.Where(b => b.dt_users.user_name.Contains(txtKeywords.Text) || b.dt_users.real_name.Contains(txtKeywords.Text)); 
            }

            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(h => Convert.ToDateTime(txtStartTime.Text) <= h.create_time);
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(h => h.create_time <= Convert.ToDateTime(txtEndTime.Text));
            query = query.Where(ptr => OfflineProjectTransactionType.Contains(ptr.type));

            totalCount = query.Count();

            return query.OrderByDescending(ptr => ptr.id).AsEnumerable()
                .Zip(Utils.Infinite(1), (prt, no) => new { prt, no }).Select(pptr => new OfflineTransaction
                {
                    index = pptr.no,
                    income = pptr.prt.principal,
                    occurTime = pptr.prt.create_time.ToString("yyyy-MM-dd HH:mm"),
                    type = Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectTransactionTypeEnum)pptr.prt.type),
                    remark = pptr.prt.remark,
                    user = pptr.prt.dt_users.user_name,
                    project = pptr.prt.li_projects.title
                });
        }

        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("offline_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&transactType={3}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, transactType));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("offline_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&transactType={3}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, transactType));
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var data = QueryProjectTransactions();
            var xlsData = data.Skip(pageSize*(page - 1)).Take(pageSize);

            var titles = new[] { "序号", "时间", "收入", "支出", "操作类型", "关联人员", "备注"};
            Utils.ExportXls("用户资金", titles, xlsData, Response);
        }

        //筛选类别
        protected void ddlRecordType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("offline_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&transactType={3}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, ddlRecordType.SelectedItem.Text));
        }
    }
}