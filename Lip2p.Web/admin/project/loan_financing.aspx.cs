using Lip2p.Common;
using Lip2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Core;
using Lip2p.Web.UI;

namespace Lip2p.Web.admin.project
{
    public partial class loan_financing : Web.UI.ManagePage
    {
        protected int ChannelId;
        protected int TotalCount;
        protected int PageIndex;
        protected int PageSize;
        protected int CategoryId;
        protected int ProjectStatus;
        protected string ChannelName = string.Empty;
        protected string Keywords = string.Empty;

        private readonly Lip2pDataContext context = new Lip2pDataContext();

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

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_apply", DTEnums.ActionEnum.View.ToString()); //检查权限
                ShowProject();
                TreeBind(); //绑定类别
                RptBind();
            }
        }

        private void ShowProject()
        {
            var keywords = DTRequest.GetQueryString("keywords");
            if (!string.IsNullOrEmpty(keywords))
                txtKeywords.Text = keywords;
            
            rblStatus.Items.Add(
                new ListItem(Utils.GetLip2pEnumDes(Lip2pEnums.ProjectStatusEnum.FinancingApplicationSuccess),
                    ((int) Lip2pEnums.ProjectStatusEnum.FinancingApplicationSuccess).ToString()));
            rblStatus.Items.Add(
                new ListItem(Utils.GetLip2pEnumDes(Lip2pEnums.ProjectStatusEnum.Financing),
                    ((int)Lip2pEnums.ProjectStatusEnum.Financing).ToString()));
            rblStatus.Items.Add(
                new ListItem(Utils.GetLip2pEnumDes(Lip2pEnums.ProjectStatusEnum.FinancingTimeout),
                    ((int)Lip2pEnums.ProjectStatusEnum.FinancingTimeout).ToString()));
            rblStatus.Items.Add(
                new ListItem(Utils.GetLip2pEnumDes(Lip2pEnums.ProjectStatusEnum.FinancingFail),
                    ((int)Lip2pEnums.ProjectStatusEnum.FinancingFail).ToString()));

            if (ProjectStatus == 0)
            {
                ProjectStatus = (int) Lip2pEnums.ProjectStatusEnum.FinancingApplicationSuccess;
                rblStatus.SelectedIndex = 0;
            }
            else
            {
                rblStatus.SelectedValue = ProjectStatus.ToString();
            }
        }

        protected void TreeBind()
        {
            BLL.article_category bll = new BLL.article_category();
            DataTable dt = bll.GetList(0, this.CategoryId);

            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            foreach (DataRow dr in dt.Rows)
            {
                string Id = dr["id"].ToString();
                int ClassLayer = int.Parse(dr["class_layer"].ToString());
                string Title = dr["title"].ToString().Trim();

                if (ClassLayer == 1)
                {
                    this.ddlCategoryId.Items.Add(new ListItem(Title, Id));
                }
                else
                {
                    Title = "├ " + Title;
                    Title = Utils.StringOfChar(ClassLayer - 1, "　") + Title;
                    this.ddlCategoryId.Items.Add(new ListItem(Title, Id));
                }
            }
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
            string pageUrl = Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&page={4}",
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
            PageSize = new BLL.channel().GetPageSize(ChannelName);
            var query = context.li_projects.Where(p => p.title.Contains(Keywords) || p.no.Contains(Keywords));
            if (ProjectStatus == (int) Lip2pEnums.ProjectStatusEnum.FinancingApplicationSuccess)
            {
                //如果是待发标状态则需要查询出定时发标的状态
                query = query.Where(
                        p => p.status == ProjectStatus || p.status == (int) Lip2pEnums.ProjectStatusEnum.FinancingAtTime);
            }
            else
            {
                query = query.Where(p => p.status == ProjectStatus);
            }
            if (CategoryId > 0)
                query = query.Where(q => q.category_id == CategoryId);
            
            this.TotalCount = query.Count();
            return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                .Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {            
            Response.Redirect(Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie("article_page_size", _pagesize.ToString(), 43200);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        /// <summary>
        /// 获取标识描述
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected string getTagString(object tag)
        {
            return tag == null ? "无" : Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectTagEnum)Utils.StrToInt(tag.ToString(), 0));
        }

        /// <summary>
        /// 获取募集进度
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        protected string getInvestmentProgress(int projectId)
        {
            return
                context.GetInvestmentProgress(projectId,
                    (total, projectAmount) =>
                        new BasePage.ProjectInvestmentProgress {total = total, projectAmount = projectAmount})
                    .GetInvestmentProgress() + "%";
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
    }
}