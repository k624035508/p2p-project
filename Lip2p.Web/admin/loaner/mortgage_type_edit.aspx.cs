using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.loaner
{
    public partial class mortgage_type_edit : UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int id = 0;
        Lip2pDataContext context = new Lip2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                if (!int.TryParse(Request.QueryString["id"], out id))
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                var model = context.li_mortgage_types.FirstOrDefault(q => q.id == id);
                if (model == null)
                {
                    JscriptMsg("记录不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_mortgage_types", DTEnums.ActionEnum.View.ToString()); //检查权限
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(id);
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int id)
        {
            var model = context.li_mortgage_types.First(q => q.id == id);
            txtTypeName.Text = model.name;
            txtScheme.Value = model.scheme;
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            var check = context.li_mortgage_types.FirstOrDefault(q => q.name == txtTypeName.Text.Trim()); //检测用户名是否重复
            if (check != null)
            {
                JscriptMsg("类型名称重复！", "", "Error");
                return false;
            }
            var model = new li_mortgage_types
            {
                name = txtTypeName.Text.Trim(),
                scheme = txtScheme.Value,
                last_update_time = DateTime.Now
            };
            context.li_mortgage_types.InsertOnSubmit(model);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加抵押物类型:" + model.name); //记录日志
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int id)
        {
            var model = context.li_mortgage_types.First(q => q.id == id);

            model.name = txtTypeName.Text.Trim();
            model.scheme = txtScheme.Value;
            model.last_update_time = DateTime.Now;

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改抵押物类型:" + model.name); //记录日志
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("loan_mortgage_types", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改抵押物类型成功！", "mortgage_list.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("loan_mortgage_types", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加抵押物类型成功！", "mortgage_list.aspx", "Success");
            }
        }
    }
}