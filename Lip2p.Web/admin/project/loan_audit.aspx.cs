using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Lip2p.BLL;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using Lip2p.Web.UI;

namespace Lip2p.Web.admin.project
{
    public partial class loan_audit : ManagePage
    {
        private readonly Lip2pDataContext context = new Lip2pDataContext();
        protected int category_id;
        protected int channel_id;
        protected string channel_name = string.Empty;
        protected string keywords = string.Empty;
        protected int page;
        protected int pageSize;
        protected int totalCount;

        protected void Page_Load(object sender, EventArgs e)
        {
            channel_id = DTRequest.GetQueryInt("channel_id");
            category_id = DTRequest.GetQueryInt("category_id");

            if (channel_id == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
            channel_name = new BLL.channel().GetChannelName(channel_id); //取得频道名称

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
                    var keywords = DTRequest.GetQueryString("keywords");
                    if (!string.IsNullOrEmpty(keywords))
                        txtKeywords.Text = keywords;
                    TreeBind(channel_id); //绑定类别
                    RptBind(channel_id, category_id, txtKeywords.Text);
                }
            }
        }

        protected void TreeBind(int _channel_id)
        {
            var bll = new article_category();
            var dt = bll.GetList(0, _channel_id);

            ddlCategoryId.Items.Clear();
            ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            foreach (DataRow dr in dt.Rows)
            {
                var Id = dr["id"].ToString();
                var ClassLayer = int.Parse(dr["class_layer"].ToString());
                var Title = dr["title"].ToString().Trim();

                if (ClassLayer == 1)
                {
                    ddlCategoryId.Items.Add(new ListItem(Title, Id));
                }
                else
                {
                    Title = "├ " + Title;
                    Title = Utils.StringOfChar(ClassLayer - 1, "　") + Title;
                    ddlCategoryId.Items.Add(new ListItem(Title, Id));
                }
            }
        }

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_audit.aspx",
                "channel_id={0}&category_id={1}&keywords={2}",
                channel_id.ToString(), category_id.ToString(), txtKeywords.Text));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_audit.aspx",
                "channel_id={0}&category_id={1}&keywords={2}",
                channel_id.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text));
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
            Response.Redirect(Utils.CombUrlTxt("loan_audit.aspx",
                "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                channel_id.ToString(), category_id.ToString(), txtKeywords.Text));
        }

        #region 数据绑定=================================

        protected void RptBind(int _channel_id, int _category_id, string _keyworkds)
        {
            page = DTRequest.GetQueryInt("page", 1);
            if (category_id > 0)
            {
                ddlCategoryId.SelectedValue = _category_id.ToString();
            }

            //绑定列表
            rptList1.DataSource = GetList(channel_name, _category_id, page, _keyworkds);
            rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("loan_audit.aspx",
                "channel_id={0}&category_id={1}&keywords={2}&page={3}",
                _channel_id.ToString(), _category_id.ToString(), txtKeywords.Text, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        /// <summary>
        ///     获取项目列表
        /// </summary>
        /// <param name="_channel_name"></param>
        /// <param name="_category_id"></param>
        /// <param name="_pageIndex"></param>
        /// <param name="_keyword"></param>
        /// <returns></returns>
        private List<li_projects> GetList(string _channel_name, int _category_id, int _pageIndex, string _keyword)
        {
            pageSize = new BLL.channel().GetPageSize(_channel_name);
            var query =
                context.li_projects.Where(
                    p => (p.status == (int)Lip2pEnums.ProjectStatusEnum.FinancingApplicationChecking) && (p.title.Contains(_keyword) || p.no.Contains(_keyword)));
            if (_category_id > 0)
                query = query.Where(q => q.category_id == _category_id);

            totalCount = query.Count();
            return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                .Skip(pageSize*(page - 1)).Take(pageSize).ToList();
        }

        #endregion

        protected string QueryLoaner(int projectId)
        {
            var project = context.li_projects.SingleOrDefault(p => p.id == projectId);
            if (project != null)
            {
                var user = project.li_risks.li_loaners.dt_users;
                return $"{user.real_name}({user.user_name})";
            }
            return "";
        }

        protected void btnAudit_OnClick(object sender, EventArgs e)
        {
            ChkAdminLevel("loan_audit", DTEnums.ActionEnum.Audit.ToString()); //检查权限
            var context = new Lip2p.Linq2SQL.Lip2pDataContext();
            for (int i = 0; i < rptList1.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList1.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList1.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    var project = context.li_projects.FirstOrDefault(p => p.id == id);
                    if (project != null)
                    {
                        project.status = (int)Lip2pEnums.ProjectStatusEnum.FinancingApplicationSuccess;
                    }
                }
            }
            context.SubmitChanges();
            AddAdminLog(DTEnums.ActionEnum.Audit.ToString(), "审核" + this.channel_name + "频道内容信息"); //记录日志
            JscriptMsg("批量审核成功！", Utils.CombUrlTxt("loan_audit.aspx", "channel_id={0}&category_id={1}&keywords={2}",
                this.channel_id.ToString(), this.category_id.ToString(), this.keywords), "Success");
        }

        private void do_loan_audit(bool auditSuccess, int projectId)
        {
            ChkAdminLevel("loan_audit", DTEnums.ActionEnum.Audit.ToString()); //检查权限
            var project = context.li_projects.FirstOrDefault(p => p.id == projectId);
            if (project != null)
            {
                project.status = auditSuccess
                    ? (int)Lip2pEnums.ProjectStatusEnum.FinancingApplicationSuccess
                    : (int)Lip2pEnums.ProjectStatusEnum.FinancingApplicationFail;
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Audit.ToString(), "审核操作成功！"); //记录日志
                JscriptMsg("审核操作成功！", Utils.CombUrlTxt("loan_audit.aspx", "channel_id={0}", this.channel_id.ToString()));
            }
            else
            {
                JscriptMsg("项目不存在或已被删除！", "back", "Error");
            }
        }
    }
}