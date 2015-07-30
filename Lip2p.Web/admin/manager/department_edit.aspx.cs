using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.BLL;
using Lip2p.Common;

namespace Lip2p.Web.admin.manager
{
    public partial class department_edit : Web.UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int id = 0;
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            this.id = DTRequest.GetQueryInt("id");
            this.pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                this.action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                if (this.id == 0)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                if (!new BLL.department().Exists(this.id))
                {
                    JscriptMsg("部门不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("department_list", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //绑定导航菜单
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(this.id);
                }
                else if (!string.IsNullOrEmpty(_action) && _action == "pagedepartment")
                {
                    id = DTRequest.GetQueryInt("id");
                    ShowInfo(id);
                }
                else
                {
                    if (this.id > 0)
                    {
                        this.ddlParentId.SelectedValue = this.id.ToString();
                    }
                    txtTitle.Attributes.Add("ajaxurl", "../../tools/admin_ajax.ashx?action=department_validate");
                }
            }
        }

        #region 绑定导航菜单=============================
        private void TreeBind()
        {
            BLL.department bll = new BLL.department();
            DataTable dt = bll.GetList(0);

            this.ddlParentId.Items.Clear();
            this.ddlParentId.Items.Add(new ListItem("无父级部门", "0"));
            foreach (DataRow dr in dt.Rows)
            {
                string Id = dr["id"].ToString();
                int ClassLayer = int.Parse(dr["class_layer"].ToString());
                string Title = dr["department_name"].ToString().Trim();

                if (ClassLayer == 1)
                {
                    this.ddlParentId.Items.Add(new ListItem(Title, Id));
                }
                else
                {
                    Title = "├ " + Title;
                    Title = Utils.StringOfChar(ClassLayer - 1, "　") + Title;
                    this.ddlParentId.Items.Add(new ListItem(Title, Id));
                }
            }
        }
        #endregion

        #region 赋值操作=================================
        private void ShowInfo(int _id)
        {
            BLL.department bll = new BLL.department();
            Model.department model = bll.GetModel(_id);

            ddlParentId.SelectedValue = model.parent_id.ToString();
            txtSortId.Text = model.sort_id.ToString();
            if (model.is_lock == 1)
            {
                cbIsLock.Checked = true;
            }

            txtTitle.Text = model.department_name;
            txtTitle.Attributes.Add("ajaxurl", "../../tools/admin_ajax.ashx?action=department_validate&old_name=" + Utils.UrlEncode(model.department_name));
            txtTitle.Focus(); //设置焦点，防止JS无法提交
            if (model.is_sys == 1)
            {
                ddlParentId.Enabled = false;
                txtTitle.ReadOnly = true;
            }
            txtRemark.Text = model.remark;

            this.page = DTRequest.GetQueryInt("page", 1);
            //txtKeywords.Text = this.keywords;
            BLL.manager bllManager = new BLL.manager();
            this.rptList.DataSource = bllManager.GetList(this.pageSize, this.page, "department_id=" + _id + "", "add_time asc,id desc", out this.totalCount);
            this.rptList.DataBind();

            //绑定页码
            txtPageNum.Text = this.pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("department_edit.aspx", "page={0}&action={1}&id={2}", "__id__","pagedepartment",_id.ToString());
            PageContent.InnerHtml = Utils.OutPageList(this.pageSize, this.page, this.totalCount, pageUrl, 8);
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            try
            {
                Model.department model = new Model.department();
                BLL.department bll = new BLL.department();

                model.department_name = txtTitle.Text.Trim();
                model.sort_id = int.Parse(txtSortId.Text.Trim());
                model.is_lock = 0;
                if (cbIsLock.Checked == true)
                {
                    model.is_lock = 1;
                }
                model.remark = txtRemark.Text.Trim();
                model.parent_id = int.Parse(ddlParentId.SelectedValue);

                if (bll.Add(model) > 0)
                {
                    AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加部门信息:" + model.department_name); //记录日志
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int _id)
        {
            try
            {
                BLL.department bll = new BLL.department();
                Model.department model = bll.GetModel(_id);

                model.department_name = txtTitle.Text.Trim();
                model.sort_id = int.Parse(txtSortId.Text.Trim());
                model.is_lock = 0;
                if (cbIsLock.Checked == true)
                {
                    model.is_lock = 1;
                }
                model.remark = txtRemark.Text.Trim();
                if (model.is_sys == 0)
                {
                    int parentId = int.Parse(ddlParentId.SelectedValue);
                    //如果选择的父ID不是自己,则更改
                    if (parentId != model.id)
                    {
                        model.parent_id = parentId;
                    }
                }

                if (bll.Update(model))
                {
                    AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "修改部门信息:" + model.department_name); //记录日志
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        #endregion

        #region 返回每页数量=============================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie("department_page_size"), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    return _pagesize;
                }
            }
            return _default_size;
        }
        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie("department_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("department_edit.aspx", "action={0}&id={1}", "pagedepartment", id.ToString()));
        }

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("department_list", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(this.id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改部门信息成功！", "department_list.aspx", "Success", "parent.loadMenuTree");
            }
            else //添加
            {
                ChkAdminLevel("department_list", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加部门信息成功！", "department_list.aspx", "Success", "parent.loadMenuTree");
            }
        }

    }
}