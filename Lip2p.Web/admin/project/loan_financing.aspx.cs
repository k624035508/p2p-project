using Lip2p.Common;
using Lip2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Lip2p.Web.admin.project
{
    public partial class loan_financing : Web.UI.ManagePage
    {
        protected int channel_id;
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected int category_id;
        protected string project_status = string.Empty;
        protected string channel_name = string.Empty;
        protected string keywords = string.Empty;

        private Lip2pDataContext context = new Lip2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            this.channel_id = DTRequest.GetQueryInt("channel_id");
            this.category_id = DTRequest.GetQueryInt("category_id");
            this.project_status = DTRequest.GetQueryString("project_status");

            if (channel_id == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
            this.channel_name = new BLL.channel().GetChannelName(this.channel_id); //取得频道名称

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_apply", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                TreeBind(this.channel_id); //绑定类别
                RptBind(this.channel_id, this.category_id, txtKeywords.Text, Utils.StrToInt(this.project_status, 0));
            }
        }

        protected void TreeBind(int _channel_id)
        {
            BLL.article_category bll = new BLL.article_category();
            DataTable dt = bll.GetList(0, _channel_id);

            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有类型", ""));
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
        protected void RptBind(int _channel_id, int _category_id, string _keyworkds, int _project_status)
        {
            this.page = DTRequest.GetQueryInt("page", 1);
            if (this.category_id > 0)
            {
                this.ddlCategoryId.SelectedValue = _category_id.ToString();
            }

            //绑定列表
            this.rptList1.DataSource = GetList(this.channel_name, _category_id, this.page, _keyworkds, _project_status);
            this.rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = this.pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&page={4}",
                _channel_id.ToString(), _category_id.ToString(), txtKeywords.Text, this.project_status.ToString(), "__id__");
            PageContent.InnerHtml = Utils.OutPageList(this.pageSize, this.page, this.totalCount, pageUrl, 8);
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
        private List<li_projects> GetList(string _channel_name, int _category_id, int _pageIndex, string _keyword, int _project_status)
        {
            pageSize = new BLL.channel().GetPageSize(_channel_name);
            var query = context.li_projects.Where(p => (p.status >= (int)Lip2pEnums.ProjectStatusEnum.FinancingFail && p.status < (int)Lip2pEnums.ProjectStatusEnum.FinancingSuccess) 
            && (p.title.Contains(_keyword) || p.no.Contains(_keyword)));
            if (_category_id > 0)
                query = query.Where(q => q.category_id == _category_id);
            
            this.totalCount = query.Count();
            return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                .Skip(pageSize * (page - 1)).Take(pageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {            
            Response.Redirect(Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.project_status));
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
            Response.Redirect(Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status));
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("loan_apply", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0; //成功数量
            int errorCount = 0; //失败数量

            Lip2pDataContext context = new Lip2pDataContext();
            for (int i = 0; i < rptList1.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList1.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList1.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    var project = context.li_projects.FirstOrDefault(p => p.id == id);
                    if (project != null)
                    {
                        context.li_projects.DeleteOnSubmit(project);
                        context.li_risks.DeleteOnSubmit(project.li_risks);
                        sucCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
            }
            context.SubmitChanges();
            AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "删除" + this.channel_name + "频道内容成功" + sucCount + "条，失败" + errorCount + "条"); //记录日志
            JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！", Utils.CombUrlTxt("loan_apply.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status), "Success");
        }
    }
}