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
using Agp2p.Web.UI;

namespace Agp2p.Web.admin.project
{
    public partial class loan_financing_success : Web.UI.ManagePage
    {
        protected int ChannelId;
        protected int TotalCount;
        protected int PageIndex;
        protected int PageSize;
        protected int CategoryId;
        protected int ProjectStatus;
        protected string ChannelName = string.Empty;
        protected string Keywords = string.Empty;
        protected Dictionary<int, string> CategoryIdTitleMap;

        private readonly Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            this.ChannelId = DTRequest.GetQueryInt("channel_id");
            this.CategoryId = DTRequest.GetQueryInt("category_id");
            this.ProjectStatus = DTRequest.GetQueryInt("status");

            if (ChannelId == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
            this.ChannelName = new BLL.channel().GetChannelName(this.ChannelId); //取得频道名称
            CategoryIdTitleMap = context.dt_article_category.Where(c => c.channel_id == this.ChannelId).OrderBy(c => c.sort_id).ToDictionary(c => c.id, c => c.title);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_success", DTEnums.ActionEnum.View.ToString()); //检查权限
                ShowProjectStatus();
                TreeBind(); //绑定类别
                RptBind();
            }
        }

        private void ShowProjectStatus()
        {
            var keywords = DTRequest.GetQueryString("keywords");
            if (!string.IsNullOrEmpty(keywords))
                txtKeywords.Text = keywords;
            
            //满标
            rblStatus.Items.Add(
                new ListItem(Utils.GetAgp2pEnumDes(Agp2pEnums.ProjectStatusEnum.FinancingSuccess),
                    ((int) Agp2pEnums.ProjectStatusEnum.FinancingSuccess).ToString()));
            //还款中
            rblStatus.Items.Add(
                new ListItem(Utils.GetAgp2pEnumDes(Agp2pEnums.ProjectStatusEnum.ProjectRepaying),
                    ((int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying).ToString()));
            //提前还款
            //rblStatus.Items.Add(
            //    new ListItem(Utils.GetAgp2pEnumDes(Agp2pEnums.ProjectStatusEnum.RepayCompleteEarlier),
            //        ((int)Agp2pEnums.ProjectStatusEnum.RepayCompleteEarlier).ToString()));
            //已完成
            rblStatus.Items.Add(
                new ListItem(Utils.GetAgp2pEnumDes(Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime),
                    ((int)Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime).ToString()));

            if (ProjectStatus == 0)
            {
                ProjectStatus = (int) Agp2pEnums.ProjectStatusEnum.FinancingSuccess;
                rblStatus.SelectedIndex = 0;
            }
            else
            {
                rblStatus.SelectedValue = ProjectStatus.ToString();
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

            //绑定列表
            this.rptList1.DataSource = GetList();
            this.rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = this.PageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("loan_financing_success.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&page={4}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString(), "__id__");
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
            var query = context.li_projects.Where(p => p.title.Contains(Keywords) || p.no.Contains(Keywords));
            if (CategoryId > 0)
                query = query.Where(q => q.category_id == CategoryId);
            query = ProjectStatus == (int) Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime ? query.Where(p => p.status >= ProjectStatus) : query.Where(p => p.status == ProjectStatus);

            this.TotalCount = query.Count();
            return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                .Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_financing_success.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {            
            Response.Redirect(Utils.CombUrlTxt("loan_financing_success.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("loan_financing_success.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        /// <summary>
        /// 选择状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rblStatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            this.ProjectStatus = Utils.StrToInt(rblStatus.SelectedValue, 0);
            RptBind();
        }

        protected string QueryLoaner(int projectId)
        {
            return context.GetLonerName(projectId);
        }
    }
}