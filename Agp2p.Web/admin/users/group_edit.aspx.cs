using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.users
{
    /// <summary>
    /// 会员组别（增加或编辑）
    /// </summary>
    public partial class group_edit : UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int id = 0;
        private Agp2pDataContext context = new Agp2pDataContext();

        // 页面加载操作
        protected void Page_Load(object sender, EventArgs e)
        {
            //取到操作类型
            string _action = DTRequest.GetQueryString("action");
            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                id = DTRequest.GetQueryInt("id");
                if (id == 0)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                if (!new BLL.user_groups().Exists(id))
                {
                    JscriptMsg("用户组不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("user_group", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind();
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(id);
                }
            }

        }

        #region 绑定导航菜单=============================
        private void TreeBind()
        {
            BLL.department bll = new BLL.department();
            DataTable dt = bll.GetList(0);

            this.ddlDepartments.Items.Clear();
            this.ddlDepartments.Items.Add(new ListItem("无加入部门", ""));
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
            }
        }

        #endregion

        #region 赋值操作=================================
        private void ShowInfo(int _id)
        {
            BLL.user_groups bll = new BLL.user_groups();
            Model.user_groups model = bll.GetModel(_id);
            var department = context.li_user_group_departments.FirstOrDefault(f => f.user_group_id == model.id);
            txtTitle.Text = model.title;
            if (department != null)
            {
                ddlDepartments.SelectedValue = department.department_id.ToString();
            }
            
            if (model.is_lock == 1)
            {
                rblIsLock.Checked = true;
            }
            /*if (model.is_default == 1)
            {
                rblIsDefault.Checked = true;
            }
            if (model.is_upgrade == 1)
            {
                rblIsUpgrade.Checked = true;
            }
            txtGrade.Text = model.grade.ToString();
            txtUpgradeExp.Text = model.upgrade_exp.ToString();
            txtAmount.Text = model.amount.ToString();
            txtPoint.Text = model.point.ToString();
            txtDiscount.Text = model.discount.ToString();*/
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            bool result = false;
            Model.user_groups model = new Model.user_groups();
            BLL.user_groups bll = new BLL.user_groups();

            model.title = txtTitle.Text.Trim();
            model.is_lock = 0;
            if (rblIsLock.Checked)
            {
                model.is_lock = 1;
            }
            model.is_default = 0;            
            model.is_upgrade = 0;

            /*if (rblIsDefault.Checked)
            {
                model.is_default = 1;
            }
            if (rblIsUpgrade.Checked)
            {
                model.is_upgrade = 1;
            }
            model.grade = int.Parse(txtGrade.Text.Trim());
            model.upgrade_exp = int.Parse(txtUpgradeExp.Text.Trim());
            model.amount = decimal.Parse(txtAmount.Text.Trim());
            model.point = int.Parse(txtPoint.Text.Trim());
            model.discount = int.Parse(txtDiscount.Text.Trim());*/
            int groupId = bll.Add(model);
            if (0 < groupId)
            {
                if (ddlDepartments.SelectedValue != "")
                {
                    var selectedGroups = context.li_user_group_departments.Where(k => k.user_group_id == groupId).ToList();
                    context.li_user_group_departments.DeleteAllOnSubmit(selectedGroups);

                    var tr = new li_user_group_departments
                    {
                        user_group_id = groupId,
                        department_id = Convert.ToInt32(ddlDepartments.SelectedValue)
                    };
                    context.li_user_group_departments.InsertOnSubmit(tr);
                    context.SubmitChanges();
                }
                
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加用户组:" + model.title); //记录日志
                result = true;
            }
            return result;
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int _id)
        {
            bool result = false;
            BLL.user_groups bll = new BLL.user_groups();
            Model.user_groups model = bll.GetModel(_id);

            model.title = txtTitle.Text.Trim();
            model.is_lock = 0;
            if (rblIsLock.Checked)
            {
                model.is_lock = 1;
            }
            model.is_default = 0;            
            model.is_upgrade = 0;

            /*if (rblIsDefault.Checked)
            {
                model.is_default = 1;
            }
            if (rblIsUpgrade.Checked)
            {
                model.is_upgrade = 1;
            }
            model.grade = int.Parse(txtGrade.Text.Trim());
            model.upgrade_exp = int.Parse(txtUpgradeExp.Text.Trim());
            model.amount = decimal.Parse(txtAmount.Text.Trim());
            model.point = int.Parse(txtPoint.Text.Trim());
            model.discount = int.Parse(txtDiscount.Text.Trim());*/
            if (bll.Update(model))
            {
                if (ddlDepartments.SelectedValue != "")
                {
                    var selectedGroups = context.li_user_group_departments.Where(k => k.user_group_id == _id).ToList();
                    context.li_user_group_departments.DeleteAllOnSubmit(selectedGroups);
                    var tr = new li_user_group_departments
                    {
                        user_group_id = _id,
                        department_id = Convert.ToInt32(ddlDepartments.SelectedValue)
                    };
                    context.li_user_group_departments.InsertOnSubmit(tr);
                }
                else
                {
                    var selectedGroups = context.li_user_group_departments.Where(k => k.user_group_id == _id).ToList();
                    context.li_user_group_departments.DeleteAllOnSubmit(selectedGroups);
                }
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改用户组:" + model.title); //记录日志
                result = true;
            }

            return result;
        }
        #endregion


        // 提交保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("user_group", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改用户组成功！", "group_list.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("user_group", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加用户组成功！", "group_list.aspx", "Success");
            }
        }
    }
}