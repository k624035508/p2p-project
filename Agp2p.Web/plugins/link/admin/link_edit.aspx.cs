using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.Link.admin
{
    public partial class link_edit : Lip2p.Web.UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int id = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");

            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                this.action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                this.id = DTRequest.GetQueryInt("id");
                if (this.id == 0)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                if (!new BLL.link().Exists(this.id))
                {
                    JscriptMsg("信息不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("plugin_link", DTEnums.ActionEnum.View.ToString()); //检查权限
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(this.id);
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int _id)
        {
            BLL.link bll = new BLL.link();
            Model.link model = bll.GetModel(_id);

            txtTitle.Text = model.title;
            if (model.is_red == 1)
            {
                cbIsRed.Checked = true;
            }
            else
            {
                cbIsRed.Checked = false;
            }
            rblIsLock.SelectedValue = model.is_lock.ToString();
            txtSortId.Text = model.sort_id.ToString();
            txtUserName.Text = model.user_name;
            txtUserTel.Text = model.user_tel;
            txtEmail.Text = model.email;
            txtSiteUrl.Text = model.site_url;
            txtImgUrl.Text = model.img_url;
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            bool result = false;
            Model.link model = new Model.link();
            BLL.link bll = new BLL.link();

            model.title = txtTitle.Text.Trim();
            model.is_lock = Utils.StrToInt(rblIsLock.SelectedValue, 0);
            if (cbIsRed.Checked == true)
            {
                model.is_red = 1;
            }
            else
            {
                model.is_red = 0;
            }
            model.sort_id = Utils.StrToInt(txtSortId.Text.Trim(), 99);
            model.user_name = txtUserName.Text.Trim();
            model.user_tel = txtUserTel.Text.Trim();
            model.email = txtEmail.Text.Trim();
            model.site_url = txtSiteUrl.Text.Trim();
            model.img_url = txtImgUrl.Text.Trim();
            model.is_image = 1;
            if (string.IsNullOrEmpty(model.img_url))
            {
                model.is_image = 0;
            }
            if (bll.Add(model) > 0)
            {
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加友情链接：" + model.title); //记录日志
                result = true;
            }
            return result;
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int _id)
        {
            bool result = false;
            BLL.link bll = new BLL.link();
            Model.link model = bll.GetModel(_id);

            model.title = txtTitle.Text.Trim();
            model.is_lock = Utils.StrToInt(rblIsLock.SelectedValue, 0);
            if (cbIsRed.Checked == true)
            {
                model.is_red = 1;
            }
            else
            {
                model.is_red = 0;
            }
            model.sort_id = Utils.StrToInt(txtSortId.Text.Trim(), 99);
            model.user_name = txtUserName.Text.Trim();
            model.user_tel = txtUserTel.Text.Trim();
            model.email = txtEmail.Text.Trim();
            model.site_url = txtSiteUrl.Text.Trim();
            model.img_url = txtImgUrl.Text.Trim();
            model.is_image = 1;
            if (string.IsNullOrEmpty(model.img_url))
            {
                model.is_image = 0;
            }
            if (bll.Update(model))
            {
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改友情链接：" + model.title); //记录日志
                result = true;
            }

            return result;
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("plugin_link", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(this.id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改链接成功！", "index.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("plugin_link", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加链接成功！", "index.aspx", "Success");
            }
        }

    }
}