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
    public class BondTransaction
    {
        public int index { get; set; }
        public DateTime occurTime { get; set; }
        public decimal? income { get; set; }
        public decimal? outcome { get; set; }
        public string project { get; set; }
        public string user { get; set; }
        public string remark { get; set; }
    }

    public partial class bond_transact_timeline : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("bond_transact_timeline", DTEnums.ActionEnum.View.ToString()); //检查权限
                var startTime = DTRequest.GetQueryString("startTime");
                if (!string.IsNullOrEmpty(startTime))
                    txtStartTime.Text = startTime;
                var endTime = DTRequest.GetQueryString("endTime");
                if (!string.IsNullOrEmpty(endTime))
                    txtEndTime.Text = endTime;
                var keywords = DTRequest.GetQueryString("keywords");  //关键字查询
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            var wallets = QueryProjectTransactions(out totalCount);
            rptList.DataSource = wallets.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("bond_transact_timeline.aspx", "keywords={0}&page={1}&startTime={2}&endTime={3}",
                txtKeywords.Text, "__id__", txtStartTime.Text, txtEndTime.Text);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IEnumerable<BondTransaction> QueryProjectTransactions(out int count)
        {
            var context = new Agp2pDataContext();

            IQueryable<li_project_transactions> query = context.li_project_transactions;
            
            if (!string.IsNullOrWhiteSpace(txtKeywords.Text))
            { 
                query = query.Where(b => b.dt_users.user_name.Contains(txtKeywords.Text) || b.dt_users.real_name.Contains(txtKeywords.Text)); 
            }

            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(h => Convert.ToDateTime(txtStartTime.Text) <= h.create_time);
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(h => h.create_time <= Convert.ToDateTime(txtEndTime.Text));

            count = query.Count();

            return query.OrderByDescending(ptr => ptr.id).AsEnumerable()
                .Where(ptr => ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.BondFee)
                .Select(ptr => new BondTransaction
                {
                    income = ptr.principal,
                    occurTime = ptr.create_time,
                    remark = ptr.remark,
                    user = ptr.dt_users.user_name,
                    project = ptr.li_projects.title
                });
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
            Response.Redirect(Utils.CombUrlTxt("bond_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text));
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
            Response.Redirect(Utils.CombUrlTxt("bond_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text));
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var data = QueryProjectTransactions(out totalCount);
            var xlsData = data.Skip(pageSize*(page - 1)).Take(pageSize);

            var titles = new[] { "序号", "时间", "收入", "支出", "操作类型", "关联人员", "备注"};
            Utils.ExportXls("风险保证金明细", titles, xlsData, Response);
        }
    }
}