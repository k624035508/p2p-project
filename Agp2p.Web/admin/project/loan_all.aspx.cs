using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Core;

namespace Agp2p.Web.admin.project
{
    public partial class loan_all : Web.UI.ManagePage
    {
        protected int ChannelId;
        protected int TotalCount;
        protected int PageIndex;
        protected int PageSize;

        protected int CategoryId;
        protected int ProjectStatus;
        protected string Keywords = string.Empty;

        private Agp2pDataContext context = new Agp2pDataContext();
            

        protected void Page_Load(object sender, EventArgs e)
        {
            this.ChannelId = DTRequest.GetQueryInt("channel_id");
            this.CategoryId = DTRequest.GetQueryInt("category_id");
            this.ProjectStatus = DTRequest.GetQueryInt("project_status");
            this.Keywords = DTRequest.GetQueryString("keywords");

            if (ChannelId == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_all", DTEnums.ActionEnum.View.ToString()); //检查权限
                txtKeywords.Text = Keywords;
                txtStartTime.Text = DTRequest.GetQueryString("startTime");
                txtEndTime.Text = DTRequest.GetQueryString("endTime");
                TreeBind(); //绑定类别
                RptBind();
            }
        }

        protected void TreeBind()
        {
            //产品
            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            var categoryIdTitleMap = context.dt_article_category.Where(c => c.channel_id == this.ChannelId).OrderBy(c => c.sort_id).ToDictionary(c => c.id, c => c.title);
            this.ddlCategoryId.Items.AddRange(categoryIdTitleMap.Select(c => new ListItem(c.Value, c.Key.ToString())).ToArray());

            //状态
            this.ddlStatus.Items.Clear();
            this.ddlStatus.Items.Add(new ListItem("所有状态", ""));
            this.ddlStatus.Items.AddRange(
                        Utils.GetEnumValues<Agp2pEnums.ProjectStatusEnum>()
                            .Select(te => new ListItem(Utils.GetAgp2pEnumDes(te), ((int)te).ToString()))
                            .ToArray());
        }


        #region 数据绑定=================================
        protected void RptBind()
        {
            this.PageIndex = DTRequest.GetQueryInt("page", 1);
            if (this.CategoryId > 0)
            {
                this.ddlCategoryId.SelectedValue = this.CategoryId.ToString();
            }
            if (this.ProjectStatus > 0)
            {
                this.ddlStatus.SelectedValue = this.ProjectStatus.ToString();
            }

            //绑定列表
            this.rptList1.DataSource = GetList();
            this.rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = this.PageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("loan_all.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&page={4}&startTime={5}&endTime={6}",
                this.ChannelId.ToString(), CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString(), "__id__", txtStartTime.Text, txtEndTime.Text);
            PageContent.InnerHtml = Utils.OutPageList(this.PageSize, this.PageIndex, this.TotalCount, pageUrl, 8);
        }

        /// <summary>
        /// 获取项目列表
        /// </summary>
        /// <param name="_channel_name"></param>
        /// <param name="_category_id"></param>
        /// <param name="_pageIndex"></param>
        /// <param name="_keyword"></param>
        /// <param name="_project_status"></param>
        /// <returns></returns>
        private List<li_projects> GetList()
        {
            PageSize = GetPageSize(GetType().Name + "_page_size");
            var query = (from p in context.li_projects
                from m in context.li_risk_mortgage
                where p.risk_id == m.risk && (string.IsNullOrEmpty(this.Keywords) || p.title.Contains(this.Keywords) ||
                                              p.no.Contains(this.Keywords) ||
                                              m.li_mortgages.name.Contains(this.Keywords))
                select p).AsQueryable();

            if (this.CategoryId > 0)
                query = query.Where(q => q.category_id == this.CategoryId);
            if (this.ProjectStatus > 0)
                query = query.Where(q => q.status == this.ProjectStatus);
            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(h => Convert.ToDateTime(txtStartTime.Text) <= h.add_time.Date);
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(h => h.add_time.Date <= Convert.ToDateTime(txtEndTime.Text));

            this.TotalCount = query.Count();
            return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                .Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_all.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&startTime={4}&endTime={5}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString(), txtStartTime.Text, txtEndTime.Text));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("loan_all.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}}&startTime={4}&endTime={5}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString(), txtStartTime.Text, txtEndTime.Text));
        }

        protected string GetTagString(object tag)
        {
            return tag == null ? "无" : Utils.GetAgp2pEnumDes((Agp2p.Common.Agp2pEnums.ProjectTagEnum)Utils.StrToInt(tag.ToString(), 0));
        }

        protected string QueryLoaner(int projectId)
        {
            return context.GetLonerName(projectId);
        }

        protected void ddlCategoryId_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_all.aspx",
                "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&startTime={4}&endTime={5}", this.ChannelId.ToString(),
                ddlCategoryId.SelectedValue, txtKeywords.Text, ddlStatus.SelectedValue, txtStartTime.Text, txtEndTime.Text));
        }

        protected void ddlStatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_all.aspx",
                "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&startTime={4}&endTime={5}", this.ChannelId.ToString(),
                ddlCategoryId.SelectedValue, txtKeywords.Text, ddlStatus.SelectedValue, txtStartTime.Text, txtEndTime.Text));
        }
    }
}