using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Core;

namespace Agp2p.Web.admin.repayment
{
    public partial class pay_manage : Web.UI.ManagePage
    {
        protected int ChannelId;
        protected int TotalCount;
        protected int PageIndex;
        protected int PageSize;
        protected int CategoryId;
        protected int RePayStatus;
        protected string ChannelName = string.Empty;
        protected string Keywords = string.Empty;
        protected Dictionary<int, string> CategoryIdTitleMap;

        private readonly Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            this.ChannelId = DTRequest.GetQueryInt("channel_id");
            this.CategoryId = DTRequest.GetQueryInt("category_id");
            this.RePayStatus = DTRequest.GetQueryInt("status");
            this.PageSize = GetPageSize(GetType().Name + "_page_size"); //每页数量
            this.Keywords = DTRequest.GetQueryString("keywords");

            if (ChannelId == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
            this.ChannelName = new BLL.channel().GetChannelName(this.ChannelId); //取得频道名称
            CategoryIdTitleMap = context.dt_article_category.Where(c => c.channel_id == this.ChannelId).OrderBy(c => c.sort_id).ToDictionary(c => c.id, c => c.title);

            if (!Page.IsPostBack)
            {
                txtKeywords.Text = Keywords;
                txtStartTime.Text = DTRequest.GetQueryString("startTime");
                txtEndTime.Text = DTRequest.GetQueryString("endTime");

                ChkAdminLevel("pay_manage", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //绑定类别
                RptBind();
            }
        }

        protected void TreeBind()
        {
            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            this.ddlCategoryId.Items.AddRange(CategoryIdTitleMap.Select(c => new ListItem(c.Value, c.Key.ToString())).ToArray());
        }


        #region 数据绑定=================================
        protected void RptBind()
        {
            this.PageIndex = DTRequest.GetQueryInt("page", 1);
            if (this.CategoryId > 0)
            {
                this.ddlCategoryId.SelectedValue = this.CategoryId.ToString();
            }
            if (this.RePayStatus > 0)
            {
                rblStatus.SelectedValue = this.RePayStatus.ToString();
            }

            //绑定列表
            this.rptList1.DataSource = GetList();
            this.rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = this.PageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("pay_manage.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&page={4}&startTime={5}&endTime={6}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.RePayStatus.ToString(), "__id__", txtStartTime.Text, txtEndTime.Text);
            PageContent.InnerHtml = Utils.OutPageList(this.PageSize, this.PageIndex, this.TotalCount, pageUrl, 8);
        }

        /// <summary>
        /// 获取账单列表
        /// </summary>
        /// <param name="_channel_name"></param>
        /// <param name="_category_id"></param>
        /// <param name="_pageIndex"></param>
        /// <param name="_keyword"></param>
        /// <param name="_project_status"></param>
        /// <returns></returns>
        private List<PayBill> GetList()
        {
            //查询还款计划
            var query =
                context.li_repayment_tasks.Where(
                    r =>
                        r.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid && r.only_repay_to == null &&
                        r.li_projects.title.Contains(txtKeywords.Text));
            if (CategoryId > 0)
                query = query.Where(q => q.li_projects.category_id == CategoryId);
            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(h => Convert.ToDateTime(txtStartTime.Text) <= h.should_repay_time.Date);
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(h => h.should_repay_time.Date <= Convert.ToDateTime(txtEndTime.Text));

            query = rblStatus.SelectedValue == "2"
                ? query.Where(r => (int)Agp2pEnums.RepaymentStatusEnum.ManualPaid <= r.status)//已还款
                : query.Where(
                    r =>
                        Convert.ToByte(rblStatus.SelectedValue) == r.status ||
                        r.status == (int)Agp2pEnums.RepaymentStatusEnum.OverTime);

            var repayList = query.OrderBy(r => r.should_repay_time).AsEnumerable()
                .SelectMany(r =>
                {
                    var pro = r.li_projects;
                    // 查询所有收益记录
                    List<li_project_transactions> profiting = pro.li_project_transactions.Where(
                        t =>
                            t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                            t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success).ToList();

                    return profiting.Select(p => new PayBill
                    {
                        Invester = $"{p.dt_users.real_name}({p.dt_users.user_name})",
                        Principal = p.principal,
                        Interest = p.interest??0,
                        ShouldPayTime = r.should_repay_time.ToString("yyyy-MM-dd"),
                        PayTime = r.repay_at?.ToString("yyyy-MM-dd") ?? "",
                        Category = pro.category_id,
                        ProfitRate = pro.profit_rate_year,
                        RepaymentType =
                            Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTypeEnum)pro.repayment_type),
                        ProjectId = pro.id,
                        ProjectTitle = pro.title,
                        ProjectStatus = pro.status,
                        RepayStatus = r.status,
                        PayId = p.id,
                        //投资协议
                        AgreeNo =
                            $"<a href='/tools/submit_ajax.ashx?action=generate_user_invest_contract&id={p.id}&user_id={p.investor}' target='_blank'>{p.agree_no}</a>"
                    });
                }).AsQueryable();

            this.TotalCount = repayList.Count();

            return repayList.Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("pay_manage.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&startTime={4}&endTime={5}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.RePayStatus.ToString(), txtStartTime.Text, txtEndTime.Text));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("pay_manage.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&startTime={4}&endTime={5}",
                this.ChannelId.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.RePayStatus.ToString(), txtStartTime.Text, txtEndTime.Text));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("pay_manage.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&startTime={4}&endTime={5}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.RePayStatus.ToString(), txtStartTime.Text, txtEndTime.Text));
        }

        class PayBill
        {
            public int PayId { get; set; }
            public int ProjectId { get; set; }
            public string ProjectTitle { get; set; }
            public string Invester { get; set; }
            public decimal Principal { get; set; }//应兑付本金
            public decimal Interest { get; set; }//应兑付利息
            public string ShouldPayTime { get; set; }//应兑付时间
            public string PayTime { get; set; }//实兑付时间
            public int Category { get; set; }//产品
            public decimal ProfitRate { get; set; }//年化利率
            public string RepaymentType { get; set; }//年化利率
            public int ProjectStatus { get; set; }//项目状态
            public int RepayStatus { get; set; }//还款状态
            public string AgreeNo { get; set; }//投资协议
        }

        protected void rblStatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RePayStatus = Utils.StrToInt(rblStatus.SelectedValue, 0);
            RptBind();
        }
    }
}