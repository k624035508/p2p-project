using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Agp2p.Web.UI;

namespace Agp2p.Web.admin.project
{
    public partial class loan_audit : ManagePage
    {
        private readonly Agp2pDataContext context = new Agp2pDataContext();
        protected int CategoryId;
        protected int ChannelId;
        protected string ChannelName = string.Empty;
        protected string Keywords = string.Empty;
        protected int PageIndex;
        protected int PageSize;
        protected int TotalCount;
        protected Dictionary<int, string> CategoryIdTitleMap;

        protected void Page_Load(object sender, EventArgs e)
        {
            ChannelId = DTRequest.GetQueryInt("channel_id");
            CategoryId = DTRequest.GetQueryInt("category_id");
            Keywords = DTRequest.GetQueryString("keywords");

            if (ChannelId == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
            ChannelName = new BLL.channel().GetChannelName(ChannelId); //取得频道名称
            CategoryIdTitleMap = context.dt_article_category.Where(c => c.channel_id == this.ChannelId).OrderBy(c => c.sort_id).ToDictionary(c => c.id, c => c.title);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_audit", DTEnums.ActionEnum.View.ToString()); //检查权限
                var action = DTRequest.GetQueryString("action");
                if (!string.IsNullOrEmpty(action) && action.Equals("audit_success"))
                {
                    do_loan_audit(true, DTRequest.GetQueryInt("id"));
                }
                else if (!string.IsNullOrEmpty(action) && action.Equals("audit_fail"))
                {
                    do_loan_audit(false, DTRequest.GetQueryInt("id"));
                }
                else
                {
                    if (!string.IsNullOrEmpty(Keywords))
                        txtKeywords.Text = Keywords;
                    TreeBind(); //绑定类别
                    RptBind();
                }
            }
        }

        protected void TreeBind()
        {
            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            this.ddlCategoryId.Items.AddRange(CategoryIdTitleMap.Select(c => new ListItem(c.Value, c.Key.ToString())).ToArray());
        }

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_audit.aspx",
                "channel_id={0}&category_id={1}&keywords={2}",
                ChannelId.ToString(), CategoryId.ToString(), txtKeywords.Text));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_audit.aspx",
                "channel_id={0}&category_id={1}&keywords={2}",
                ChannelId.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("loan_audit.aspx",
                "channel_id={0}&category_id={1}&keywords={2}",
                ChannelId.ToString(), CategoryId.ToString(), txtKeywords.Text));
        }

        #region 数据绑定=================================

        protected void RptBind()
        {
            PageIndex = DTRequest.GetQueryInt("page", 1);
            if (CategoryId > 0)
            {
                ddlCategoryId.SelectedValue = this.CategoryId.ToString();
            }

            //绑定列表
            rptList1.DataSource = GetList();
            rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = PageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("loan_audit.aspx",
                "channel_id={0}&category_id={1}&keywords={2}&page={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(PageSize, PageIndex, TotalCount, pageUrl, 8);
        }

        /// <summary>
        ///     获取项目列表
        /// </summary>
        /// <param name="_channel_name"></param>
        /// <param name="_category_id"></param>
        /// <param name="_pageIndex"></param>
        /// <param name="_keyword"></param>
        /// <returns></returns>
        private List<li_projects> GetList()
        {
            PageSize = GetPageSize(GetType().Name + "_page_size");
            var query =
                context.li_projects.Where(
                    p => (p.status == (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationChecking) && (p.title.Contains(Keywords) || p.no.Contains(Keywords)));
            if (CategoryId > 0)
                query = query.Where(q => q.category_id == CategoryId);

            TotalCount = query.Count();
            return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                .Skip(PageSize*(PageIndex - 1)).Take(PageSize).ToList();
        }

        #endregion

        protected string QueryLoaner(int projectId)
        {
            return context.GetLonerName(projectId);
        }

        protected void btnAudit_OnClick(object sender, EventArgs e)
        {
            ChkAdminLevel("loan_audit", DTEnums.ActionEnum.Audit.ToString()); //检查权限
            var context = new Agp2pDataContext();
            for (int i = 0; i < rptList1.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList1.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList1.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    var project = context.li_projects.FirstOrDefault(p => p.id == id);
                    if (project != null)
                    {
                        project.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationSuccess;
                    }
                }
            }
            context.SubmitChanges();
            AddAdminLog(DTEnums.ActionEnum.Audit.ToString(), "审核" + this.ChannelName + "频道内容信息"); //记录日志
            JscriptMsg("批量审核成功！", Utils.CombUrlTxt("loan_audit.aspx", "channel_id={0}&category_id={1}&keywords={2}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), this.Keywords), "Success");
        }

        private void do_loan_audit(bool auditSuccess, int projectId)
        {
            ChkAdminLevel("loan_audit", DTEnums.ActionEnum.Audit.ToString()); //检查权限
            var project = context.li_projects.FirstOrDefault(p => p.id == projectId);
            if (project != null)
            {
                project.status = auditSuccess
                    ? (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationSuccess
                    : (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationFail;
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Audit.ToString(), "审核操作成功！"); //记录日志
                JscriptMsg("审核操作成功！", Utils.CombUrlTxt("loan_audit.aspx", "channel_id={0}", this.ChannelId.ToString()));
            }
            else
            {
                JscriptMsg("项目不存在或已被删除！", "back", "Error");
            }
        }
    }
}