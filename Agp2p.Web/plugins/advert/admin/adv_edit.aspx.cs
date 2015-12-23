using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;

namespace Agp2p.Web.Plugin.Advert.admin
{
    public partial class adv_edit : UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int id = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");

            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                this.action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                this.id = DTRequest.GetQueryInt("id", 0);
                if (this.id < 1)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                if (!new BLL.advert().Exists(this.id))
                {
                    JscriptMsg("信息不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("plugin_advert_adv", DTEnums.ActionEnum.View.ToString()); //检查权限
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(this.id);
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int _id)
        {
            BLL.advert bll = new BLL.advert();
            Model.advert model = bll.GetModel(_id);

            txtTitle.Text = model.title;
            rblType.SelectedValue = model.type.ToString();
            txtRemark.Text = model.remark;
            txtViewNum.Text = model.view_num.ToString();
            txtPrice.Text = model.price.ToString();
            txtViewWidth.Text = model.view_width.ToString();
            txtViewHeight.Text = model.view_height.ToString();
            rblTarget.SelectedValue = model.target;
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            bool result = false;
            Model.advert model = new Model.advert();
            BLL.advert bll = new BLL.advert();

            model.title = txtTitle.Text.Trim();
            model.type = int.Parse(rblType.SelectedValue);
            model.price = decimal.Parse(txtPrice.Text.Trim());
            model.remark = txtRemark.Text.Trim();
            model.view_num = int.Parse(txtViewNum.Text.Trim());
            model.view_width = int.Parse(txtViewWidth.Text.Trim());
            model.view_height = int.Parse(txtViewHeight.Text.Trim());
            model.target = rblTarget.SelectedValue;

            if (bll.Add(model) >0)
            {
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加广告位：" + model.title); //记录日志
                result = true;
            }
            return result;
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int _id)
        {
            bool result = false;
            BLL.advert bll = new BLL.advert();
            Model.advert model = bll.GetModel(_id);

            model.title = txtTitle.Text.Trim();
            model.type = int.Parse(rblType.SelectedValue);
            model.price = decimal.Parse(txtPrice.Text.Trim());
            model.remark = txtRemark.Text.Trim();
            model.view_num = int.Parse(txtViewNum.Text.Trim());
            model.view_width = int.Parse(txtViewWidth.Text.Trim());
            model.view_height = int.Parse(txtViewHeight.Text.Trim());
            model.target = rblTarget.SelectedValue;

            if (bll.Update(model))
            {
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改广告位：" + model.title); //记录日志
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
                ChkAdminLevel("plugin_advert_adv", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(this.id))
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("修改成功！", "index.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("plugin_advert_adv", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("添加成功！", "index.aspx", "Success");
            }
        }

    }
}