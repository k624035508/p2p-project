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
        public int? index { get; set; }
        public string occurTime { get; set; }
        public decimal? income { get; set; }
        public decimal? outcome { get; set; }
        public string project { get; set; }
        public string user { get; set; }
        public string remark { get; set; }
        public string category { get; set; }
    }

    public partial class bond_transact_timeline : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        protected int categoryId;

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(GetType().Name + "_page_size");
            page = DTRequest.GetQueryInt("page", 1);
            categoryId = DTRequest.GetQueryInt("category_id");
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
                TreeBind();
                RptBind();
            }
        }

        #region 数据绑定=================================
        protected void TreeBind()
        {
            var categoryIdTitleMap = new Agp2pDataContext().dt_article_category.Where(c => c.channel_id == 6).OrderBy(c => c.sort_id).ToDictionary(c => c.id, c => c.title);
            ddlCategoryId.Items.Clear();
            ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            ddlCategoryId.Items.AddRange(categoryIdTitleMap.Select(c => new ListItem(c.Value, c.Key.ToString())).ToArray());
            if (categoryId > 0)
            {
                ddlCategoryId.SelectedValue = categoryId.ToString();
            }
        }

        private void RptBind()
        {
            var transactions = QueryProjectTransactions();
            if (rblType.SelectedValue == "0")
            {
                var pageData = transactions.Skip(pageSize*(page - 1)).Take(pageSize).ToList();
                rptList.DataSource = pageData.Concat(Enumerable.Range(0, 1).Select(i => new BondTransaction
                {
                    index = null,
                    occurTime = "总计",
                    income = pageData.Aggregate(0m, (sum, tr) => sum + tr.income.GetValueOrDefault()),
                    outcome = pageData.Aggregate(0m, (sum, tr) => sum + tr.outcome.GetValueOrDefault()),
                }));
                rptList.DataBind();

                //绑定页码
                txtPageNum.Text = pageSize.ToString();
                string pageUrl = Utils.CombUrlTxt("bond_transact_timeline.aspx",
                    "keywords={0}&page={1}&startTime={2}&endTime={3}&category_id={4}",
                    txtKeywords.Text, "__id__", txtStartTime.Text, txtEndTime.Text, ddlCategoryId.SelectedValue);
                PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
            }
            else
            {
                rptList_summary.DataSource = QueryGroupData(transactions);
                rptList_summary.DataBind();
            }
        }

        private List<BondTransaction> QueryGroupData(IEnumerable<BondTransaction> query)
        {
            var summaryData =
                    query.GroupBy(tr => tr.category)
                        .Zip(Utils.Infinite(1), (trg, no) => new { trg = trg, no })
                        .Select(tr => new BondTransaction
                        {
                            index = tr.no,
                            category = tr.trg.Key,
                            income = tr.trg.Sum(t => t.income)
                        })
                        .ToList();
            summaryData.Add(new BondTransaction()
            {
                index = null,
                category = "总计",
                income = summaryData.Aggregate(0m, (sum, tr) => sum + tr.income.GetValueOrDefault())
            });
            return summaryData;
        }

        private IEnumerable<BondTransaction> QueryProjectTransactions()
        {
            var context = new Agp2pDataContext();

            IQueryable<li_company_inoutcome> query =
                context.li_company_inoutcome.Where(
                    ptr => ptr.type == (int) Agp2pEnums.OfflineTransactionTypeEnum.BondFee);

            if (categoryId > 0)
                query = query.Where(q => q.li_projects.category_id == categoryId);
            if (!string.IsNullOrWhiteSpace(txtKeywords.Text))
            { 
                query = query.Where(b => b.dt_users.user_name.Contains(txtKeywords.Text) || b.dt_users.real_name.Contains(txtKeywords.Text)); 
            }

            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(h => Convert.ToDateTime(txtStartTime.Text) <= h.create_time);
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(h => h.create_time <= Convert.ToDateTime(txtEndTime.Text));

            totalCount = query.Count();

            return query.OrderByDescending(ptr => ptr.create_time)
                .AsEnumerable()
                .Zip(Utils.Infinite(1), (prt, no) => new {prt, no}).Select(pprt => new BondTransaction
                {
                    index = pprt.no,
                    income = pprt.prt.income,
                    occurTime = pprt.prt.create_time.ToString("yyyy-MM-dd HH:mm"),
                    remark = pprt.prt.remark,
                    user = string.IsNullOrEmpty(pprt.prt.dt_users.real_name) ? pprt.prt.dt_users.user_name : pprt.prt.dt_users.real_name,
                    project = pprt.prt.li_projects.title,
                    category = pprt.prt.li_projects.dt_article_category.title
                }).AsQueryable();
        }

        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("bond_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&category_id={3}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, ddlCategoryId.SelectedValue));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("bond_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&category_id={3}",
                txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, ddlCategoryId.SelectedValue));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("bond_transact_timeline.aspx", "keywords={0}&startTime={1}&endTime={2}&category_id={3}",
               txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, ddlCategoryId.SelectedValue));
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var data = QueryProjectTransactions();
            if (rblType.SelectedValue == "0")
            {
                var xlsData = data.Skip(pageSize*(page - 1)).Take(pageSize);
                var titles = new[] {"序号", "时间", "收入", "支出", "操作类型", "关联人员", "备注"};
                Utils.ExportXls("风险保障金明细", titles, xlsData, Response);
            }
            else
            {
                var xlsData = QueryGroupData(data).Select(d => new
                {
                    d.index,
                    d.category,
                    d.income
                });
                var titles = new[] { "序号", "产品", "风险保障金" };
                Utils.ExportXls("风险保障金汇总", titles, xlsData, Response);
            }
        }

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