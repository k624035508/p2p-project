using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.LineService.admin
{
    public partial class edit : Lip2p.Web.UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        protected int id = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                this.action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                this.id = DTRequest.GetQueryInt("id");
                if (this.id < 1)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                if (!new BLL.online_service().Exists(this.id))
                {
                    JscriptMsg("信息不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("plugin_lineservice_list", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //绑定分组
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(this.id);
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int _id)
        {
            BLL.online_service bll = new BLL.online_service();
            Model.online_service model = bll.GetModel(_id);

            ddlGroupId.SelectedValue = model.group_id.ToString();
            txtTitle.Text = model.title;
            rblIsLock.SelectedValue = model.is_lock.ToString();
            txtSortId.Text = model.sort_id.ToString();
            txtLinkUrl.Text = model.link_url;
            txtImgUrl.Text = model.img_url;
        }
        #endregion

        #region 绑定类别=================================
        private void TreeBind()
        {
            BLL.online_service_group bll = new BLL.online_service_group();
            DataSet ds = bll.GetList(0, "", "sort_id asc,id desc");

            this.ddlGroupId.Items.Clear();
            this.ddlGroupId.Items.Add(new ListItem("请选择分组...", ""));
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                this.ddlGroupId.Items.Add(new ListItem(dr["title"].ToString().Trim(), dr["id"].ToString()));
            }
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            bool result = false;
            Model.online_service model = new Model.online_service();
            BLL.online_service bll = new BLL.online_service();

            model.group_id = int.Parse(ddlGroupId.SelectedValue);
            model.title = txtTitle.Text.Trim();
            model.img_url = txtImgUrl.Text.Trim();
            model.link_url = txtLinkUrl.Text.Trim();
            model.sort_id = int.Parse(txtSortId.Text.Trim());
            model.is_lock = int.Parse(rblIsLock.SelectedValue);
            model.add_time = DateTime.Now;

            if (bll.Add(model) > 0)
            {
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加在线客服：" + model.title); //记录日志
                result = true;
            }
            return result;
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int _id)
        {
            bool result = false;
            BLL.online_service bll = new BLL.online_service();
            Model.online_service model = bll.GetModel(_id);

            model.group_id = int.Parse(ddlGroupId.SelectedValue);
            model.title = txtTitle.Text.Trim();
            model.img_url = txtImgUrl.Text.Trim();
            model.link_url = txtLinkUrl.Text.Trim();
            model.sort_id = int.Parse(txtSortId.Text.Trim());
            model.is_lock = int.Parse(rblIsLock.SelectedValue);

            if (bll.Update(model))
            {
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改在线客服：" + model.title); //记录日志
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
                ChkAdminLevel("plugin_lineservice_list", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(this.id))
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("编辑成功啦！", "index.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("plugin_lineservice_list", DTEnums.ActionEnum.Add.ToString()); //检查权限
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