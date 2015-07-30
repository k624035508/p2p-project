using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.Job.admin
{
    public partial class edit : Lip2p.Web.UI.ManagePage
    {
        private string action = "Add"; //操作类型
        protected int id;

        protected void Page_Load(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_job", DTEnums.ActionEnum.Show.ToString()); //检查权限
            string _action = DTRequest.GetQueryString("action");
            if (!string.IsNullOrEmpty(_action) && _action == "Edit")
            {
                this.action = "Edit"; //修改类型
                this.id = DTRequest.GetQueryInt("id");
                if (this.id < 1)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                if (!new BLL.job().Exists(this.id))
                {
                    JscriptMsg("信息不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                if (action == "Edit") //修改
                {
                    ShowInfo(this.id);
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int _id)
        {
            BLL.job bll = new BLL.job();
            Model.job model = bll.GetModel(_id);

            txtTitle.Text = model.title;
            rblIsLock.SelectedValue = model.is_lock.ToString();
            txtDepart.Text = model.depart;
            txtNumber.Text = model.number.ToString();
            ddlSex.SelectedValue = model.sex;
            txtExperience.Text = model.experience;
            txtEducation.Text = model.education;
            txtAge.Text = model.age;
            txtProfession.Text = model.profession;
            txtWork_area.Text = model.work_area;
            txtStart_time.Text = model.start_time;
            txtEnd_time.Text = model.end_time;
            txtContent.Value = model.Content;
            txtClick.Text = model.click.ToString();
            txtSortId.Text = model.sort_id.ToString();


            txtContactName.Text = model.ContactName;
            txtPhone.Text = model.Phone;
            txtFax.Text = model.Fax;
            txtEmail.Text = model.Fax;
            txtAddress.Text = model.Address;
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            ChkAdminLevel("plugin_job", DTEnums.ActionEnum.Add.ToString()); //检查权限
            bool result = true;
            Model.job model = new Model.job();
            BLL.job bll = new BLL.job();

            model.title = txtTitle.Text.Trim();
            model.depart = txtDepart.Text.Trim();
            model.sex = ddlSex.SelectedValue;
            model.number = int.Parse(txtNumber.Text.Trim());
            model.education = txtEducation.Text.Trim();
            model.age = txtAge.Text.Trim();
            model.profession = txtProfession.Text.Trim();
            model.experience = txtExperience.Text.Trim();
            model.work_area = txtWork_area.Text.Trim();
            model.start_time = txtStart_time.Text.Trim();
            model.end_time = txtEnd_time.Text.Trim();
            model.Content = txtContent.Value.Trim();
            model.ContactName = txtContactName.Text.Trim();
            model.Phone = txtPhone.Text.Trim();
            model.Fax = txtFax.Text.Trim();
            model.Email = txtEmail.Text.Trim();
            model.Address = txtAddress.Text.Trim();
            model.click =int.Parse(txtClick.Text.Trim());
            model.sort_id = int.Parse(txtSortId.Text.Trim());
            model.is_lock = int.Parse(rblIsLock.SelectedValue);
            model.add_time = DateTime.Now;

            if (!bll.Add(model))
            {
                result = false;
            }
            return result;
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int _id)
        {
            ChkAdminLevel("plugin_job", DTEnums.ActionEnum.Edit.ToString()); //检查权限
            bool result = true;
            BLL.job bll = new BLL.job();
            Model.job model = bll.GetModel(_id);

            model.title = txtTitle.Text.Trim();
            model.depart = txtDepart.Text.Trim();
            model.sex = ddlSex.SelectedValue;
            model.number = int.Parse(txtNumber.Text.Trim());
            model.education = txtEducation.Text.Trim();
            model.age = txtAge.Text.Trim();
            model.profession = txtProfession.Text.Trim();
            model.experience = txtExperience.Text.Trim();
            model.work_area = txtWork_area.Text.Trim();
            model.start_time = txtStart_time.Text.Trim();
            model.end_time = txtEnd_time.Text.Trim();
            model.Content = txtContent.Value.Trim();
            model.ContactName = txtContactName.Text.Trim();
            model.Phone = txtPhone.Text.Trim();
            model.Fax = txtFax.Text.Trim();
            model.Email = txtEmail.Text.Trim();
            model.Address = txtAddress.Text.Trim();
            model.click = int.Parse(txtClick.Text.Trim());
            model.sort_id = int.Parse(txtSortId.Text.Trim());
            model.is_lock = int.Parse(rblIsLock.SelectedValue);
            model.add_time = DateTime.Now;

            if (!bll.Update(model))
            {
                result = false;
            }

            return result;
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == "Edit") //修改
            {
                if (!DoEdit(this.id))
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("编辑成功啦！", "index.aspx", "Success");
            }
            else //添加
            {
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("添加成功啦！", "index.aspx", "Success");
            }
        }

    }
}