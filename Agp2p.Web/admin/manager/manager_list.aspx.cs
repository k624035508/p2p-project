using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.manager
{
    public partial class manager_list : Web.UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;
        private string department_id = string.Empty;
        private List<int> idstrList = new List<int>();
        private List<int> manageridstrList = new List<int>();

        protected string keywords = string.Empty;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            //this.keywords = DTRequest.GetQueryString("keywords");

            this.pageSize = GetPageSize(10); //每页数量
            this.page = DTRequest.GetQueryInt("page", 1);
            department_id = DTRequest.GetQueryString("department_id");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("manager_list", DTEnums.ActionEnum.View.ToString()); //检查权限

                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;

                DdlDepartmentsBind();   //绑定管理员部门
                RptBind();
            }
        }

        /// <summary>
        /// 绑定管理员部门
        /// </summary>
        private void DdlDepartmentsBind()
        {
            var manager = context.dt_manager.FirstOrDefault(w => w.id == GetAdminInfo().id);
            var department = context.li_departments.FirstOrDefault(w => w.id == manager.department_id);
            BLL.department bll = new BLL.department();
            var dt = new DataTable();
            if (department == null)
            {
                dt = bll.GetList(0);
            }
            else if (department.parent_id == 0)
            {
                dt = bll.GetListByOkParentId(department.id, 0);
            }
            else
            {
                dt = bll.GetListByOkId(department.id, 0);
            }

            this.ddlDepartments.Items.Clear();
            this.ddlDepartments.Items.Add(new ListItem("不限", ""));
            foreach (DataRow dr in dt.Rows)
            {
                string Id = dr["id"].ToString();
                int ClassLayer = int.Parse(dr["class_layer"].ToString());
                string Title = dr["department_name"].ToString().Trim();

                if (ClassLayer == 1)
                {
                    this.ddlDepartments.Items.Add(new ListItem(Title, Id));
                }
                else
                {
                    Title = "├ " + Title;
                    Title = Utils.StringOfChar(ClassLayer - 1, "　") + Title;
                    this.ddlDepartments.Items.Add(new ListItem(Title, Id));
                }
                var g = context.dt_manager.Where(w => w.department_id == Convert.ToInt32(Id))
                       .Select(s => s.id)
                       .ToList();
                if (idstrList.Count == 0)
                {
                    idstrList = g;
                }
                else
                {
                    idstrList = idstrList.Concat(g).ToList();
                }
            }
            ddlDepartments.SelectedValue = department_id;
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            //BLL.manager bll = new BLL.manager();
            //this.rptList.DataSource = bll.GetList(this.pageSize, this.page, _strWhere, _orderby, out this.totalCount);
            //this.rptList.DataBind();

            IQueryable<dt_manager> query = context.dt_manager;
            BLL.department departmentbll = new BLL.department();

            if (string.IsNullOrWhiteSpace(ddlDepartments.SelectedValue)) // 不限：查自己的组权限 + 下属的组权限，都无选权限的话能看全部
            {
                var manager = context.dt_manager.Single(m => m.id == GetAdminInfo().id);
                if (manager.department_id >0)
                {
                    query = query.Where(w => idstrList.Contains(w.id));
                }
            }
            else // 选了下属，看下属的组权限，无选的话不能看全部
            {
                var department = context.li_departments.FirstOrDefault(w => w.id == Convert.ToInt32(ddlDepartments.SelectedValue));
                if (department.class_layer == 1)
                {
                    DataTable dt = departmentbll.GetListByOkParentId(department.id, 0);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string Id = dr["id"].ToString();
                        var g = context.dt_manager.Where(w => w.department_id == Convert.ToInt32(Id))
                       .Select(s => s.id)
                       .ToList();
                        if (manageridstrList.Count == 0)
                        {
                            manageridstrList = g;
                        }
                        else
                        {
                            manageridstrList = manageridstrList.Concat(g).ToList();
                        }
                    }
                    query = query.Where(w => manageridstrList.Contains(w.id));  // 不限：查自己的组权限 + 下属的组权限，都无选权限的话能看全部
                }
                else   // 选了下属，看下属的组权限，无选的话不能看全部
                {
                    var canAccessGroups = context.dt_manager.Where(w => w.department_id == Convert.ToInt32(ddlDepartments.SelectedValue)).Select(s => s.id).ToArray();
                    query = query.Where(w => canAccessGroups.Contains(w.id));
                }
            }

            if (!string.IsNullOrWhiteSpace(txtKeywords.Text))
            {
                query = query.Where(w => (w.user_name.Contains(txtKeywords.Text) || w.real_name.Contains(txtKeywords.Text) || w.email.Contains(txtKeywords.Text)));
            }

            totalCount = query.Count();

            rptList.DataSource = query
                    .OrderBy(u => u.add_time)
                    .ThenByDescending(u => u.id)
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToList();

            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("manager_list.aspx", "keywords={0}&page={1}&department_id={2}", txtKeywords.Text, "__id__", ddlDepartments.SelectedValue);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        #region 组合SQL查询语句==========================
        protected string CombSqlTxt(string _keywords)
        {
            StringBuilder strTemp = new StringBuilder();
            _keywords = _keywords.Replace("'", "");
            if (!string.IsNullOrEmpty(_keywords))
            {
                strTemp.Append(" and (user_name like  '%" + _keywords + "%' or real_name like '%" + _keywords + "%' or email like '%" + _keywords + "%')");
            }

            return strTemp.ToString();
        }
        #endregion

        #region 返回每页数量=============================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie("manager_page_size"), out _pagesize))
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
            Response.Redirect(Utils.CombUrlTxt("manager_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text,ddlDepartments.SelectedValue));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie("manager_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("manager_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text, ddlDepartments.SelectedValue));
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("manager_list", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            BLL.manager bll = new BLL.manager();
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    if (bll.Delete(id))
                    {
                        sucCount += 1;
                    }
                    else
                    {
                        errorCount += 1;
                    }
                }
            }
            AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除管理员" + sucCount + "条，失败" + errorCount + "条"); //记录日志
            JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！", Utils.CombUrlTxt("manager_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text, ddlDepartments.SelectedValue), "Success");
        }

        protected void ddlDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("manager_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text, ddlDepartments.SelectedValue));
        }
    }
}