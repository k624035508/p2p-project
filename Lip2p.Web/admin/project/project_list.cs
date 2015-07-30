using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using Lip2p.Core;
using System.Linq;

namespace Lip2p.Web.admin.project
{
    public partial class project_list : Web.UI.ManagePage
    {
        protected int channel_id;
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected int category_id;
        protected string project_status = string.Empty;
        protected string channel_name = string.Empty;
        protected string keywords = string.Empty;
        protected string prolistview = string.Empty;

        protected string page_name = string.Empty;//页面名称

        private Lip2pDataContext context = new Lip2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            this.channel_id = DTRequest.GetQueryInt("channel_id");
            this.category_id = DTRequest.GetQueryInt("category_id");
            //this.keywords = DTRequest.GetQueryString("keywords");
            this.project_status = DTRequest.GetQueryString("project_status");
           
            if (channel_id == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
            this.channel_name = new BLL.channel().GetChannelName(this.channel_id); //取得频道名称
            this.pageSize = GetPageSize(10); //每页数量
            this.prolistview = Utils.GetCookie("article_list_view"); //显示方式
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("project_"+page_name, DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                TreeBind(this.channel_id); //绑定类别
                RptBind(this.channel_id, this.category_id, txtKeywords.Text, Utils.StrToInt(this.project_status, 0));
            }
        }

        #region 绑定类别=================================
        protected void TreeBind(int _channel_id)
        {
            BLL.article_category bll = new BLL.article_category();
            DataTable dt = bll.GetList(0, _channel_id);

            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有类别", ""));
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
        #endregion

        #region 数据绑定=================================
        protected void RptBind(int _channel_id, int _category_id, string _keyworkds, int _project_status)
        {
            this.page = DTRequest.GetQueryInt("page", 1);
            if (this.category_id > 0)
            {
                this.ddlCategoryId.SelectedValue = _category_id.ToString();
            }
            this.ddlProperty.SelectedValue = this.project_status.ToString();
            //this.txtKeywords.Text = this.keywords;

            //图表或列表显示
            switch (this.prolistview)
            {
                case "Txt":
                    this.rptList2.Visible = false;
                    this.rptList1.DataSource = GetList(this.channel_name, _category_id, this.page, _keyworkds, _project_status);
                    this.rptList1.DataBind();
                    break;
                default:
                    this.rptList1.Visible = false;
                    this.rptList2.DataSource = GetList(this.channel_name, _category_id, this.page, _keyworkds, _project_status);
                    this.rptList2.DataBind();
                    break;
            }
            //绑定页码
            txtPageNum.Text = this.pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&page={4}",
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
            //var pageSize = new BLL.channel().GetPageSize(_channel_name);
            var query = context.li_projects.Where(p => 
                p.title.Contains(_keyword) || p.no.Contains(_keyword));
            if (_category_id > 0)
                query = query.Where(q => q.category_id == _category_id);

            if (_project_status > 0)
                query = query.Where(q => q.status == _project_status);
            else
            {                
                if (this.page_name.Equals("approval"))
                    query = query.Where(q => q.status < (int)Lip2pEnums.ProjectStatusEnum.QianYue);
                else if (this.page_name.Equals("publish"))
                    query = query.Where(q => q.status >= (int)Lip2pEnums.ProjectStatusEnum.QianYue);
                else if (this.page_name.Equals("audit"))
                {
                    query = query.Where(q => q.status == (int)Lip2pEnums.ProjectStatusEnum.LiXiang 
                        || q.status == (int)Lip2pEnums.ProjectStatusEnum.LiBiao);
                }
            }

            this.totalCount = query.Count();
            if (this.page_name.Equals("publish"))
                return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.publish_time).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                .Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            else
                return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                    .Skip(pageSize * (page - 1)).Take(pageSize).ToList();            
        }

        protected void rptList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(((HiddenField)e.Item.FindControl("hidId")).Value);
            var project = context.li_projects.FirstOrDefault(p => p.id == id);
            if (project != null)
            {
                switch (e.CommandName)
                {
                    case "IsOrdered":
                        if (project.tag == (int)Lip2pEnums.ProjectTagEnum.Ordered)
                            project.tag = 0;
                        else
                            project.tag = (int)Lip2pEnums.ProjectTagEnum.Ordered;
                        break;
                    case "IsHot":
                        if (project.tag == (int)Lip2pEnums.ProjectTagEnum.Hot)
                            project.tag = 0;
                        else
                            project.tag = (int)Lip2pEnums.ProjectTagEnum.Hot;
                        break;
                    case "IsTrial":
                        if (0 < project.investment_amount)
                        {
                            ShowJsAlert("已经有人投资过的项目不能更改此标识");
                        }
                        else
                        {
                            if (project.tag == (int) Lip2pEnums.ProjectTagEnum.Trial)
                                project.tag = 0;
                            else
                            {
                                if (project.repayment_term_span != (int) Lip2pEnums.ProjectRepaymentTermSpanEnum.Day)
                                    ShowJsAlert("新手体验标只能设置于日标");
                                else
                                    project.tag = (int)Lip2pEnums.ProjectTagEnum.Trial;
                            }
                        }
                        break;
                    case "IsDailyProject":
                        if (0 < project.investment_amount)
                        {
                            ShowJsAlert("已经有人投资过的项目不能更改此标识");
                        }
                        else
                        {
                            if (project.tag == (int) Lip2pEnums.ProjectTagEnum.DailyProject)
                                project.tag = 0;
                            else
                            {
                                if (project.repayment_term_span != (int) Lip2pEnums.ProjectRepaymentTermSpanEnum.Day)
                                    ShowJsAlert("天标只能设置于日标");
                                else
                                    project.tag = (int)Lip2pEnums.ProjectTagEnum.DailyProject;
                            }
                        }
                        break;
                }
                context.SubmitChanges();
                RptBind(this.channel_id, this.category_id, txtKeywords.Text, Utils.StrToInt(this.project_status, 0));
            }
        }
        #endregion

        #region 返回图文每页数量=========================
        protected int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie("article_page_size"), out _pagesize))
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
            Response.Redirect(Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.project_status));
        }

        //筛选项目状态
        protected void ddlProperty_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
               this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, ddlProperty.SelectedValue));
        }

        //设置文字列表显示
        protected void lbtnViewTxt_Click(object sender, EventArgs e)
        {
            Utils.WriteCookie("article_list_view", "Txt", 14400);
            Response.Redirect(Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&page={4}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status, this.page.ToString()));
        }

        //设置图文列表显示
        protected void lbtnViewImg_Click(object sender, EventArgs e)
        {
            Utils.WriteCookie("article_list_view", "Img", 14400);
            Response.Redirect(Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&page={4}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status, this.page.ToString()));
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
            Response.Redirect(Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status));
        }

        //保存排序
        protected void btnSave_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("project_" + page_name, DTEnums.ActionEnum.Edit.ToString()); //检查权限
            BLL.article bll = new BLL.article();
            Repeater rptList = new Repeater();
            switch (this.prolistview)
            {
                case "Txt":
                    rptList = this.rptList1;
                    break;
                default:
                    rptList = this.rptList2;
                    break;
            }
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                int sortId;
                if (!int.TryParse(((TextBox)rptList.Items[i].FindControl("txtSortId")).Text.Trim(), out sortId))
                {
                    sortId = 99;
                }
                bll.UpdateField(id, "sort_id=" + sortId.ToString());
            }
            AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "保存" + this.channel_name + "频道内容排序"); //记录日志
            JscriptMsg("保存排序成功啦！", Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status), "Success");
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("project_" + page_name, DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0; //成功数量
            int errorCount = 0; //失败数量
            Repeater rptList = new Repeater();
            switch (this.prolistview)
            {
                case "Txt":
                    rptList = this.rptList1;
                    break;
                default:
                    rptList = this.rptList2;
                    break;
            }

            Lip2pDataContext context = new Lip2pDataContext();
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
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
            JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！", Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), this.category_id.ToString(), txtKeywords.Text, this.project_status), "Success");
        }

    }
}