using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.transact
{
    public partial class bank_account_edit : UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int id = 0, userId;
        private Lip2pDataContext context = new Lip2pDataContext();

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
                var model = context.li_bank_accounts.FirstOrDefault(q => q.id == id);
                if (model == null)
                {
                    JscriptMsg("记录不存在或已被删除！", "back", "Error");
                    return;
                }
                userId = model.owner;
            }
            if (_action == DTEnums.ActionEnum.Add.ToString())
            {
                if (!int.TryParse(Request.QueryString["user_id"], out userId))
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                var user = context.dt_users.First(u => u.id == userId);
                lblUser.Text = user.user_name;
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("manage_users_charge_withdraw", DTEnums.ActionEnum.View.ToString()); //检查权限
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(id);
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int id)
        {
            var model = context.li_bank_accounts.First(q => q.id == id);
            var user = model.dt_users;
            lblUser.Text = user.user_name;
            txtBank.Text = model.bank;
            txtOpeningBank.Text = model.opening_bank;
            txtAccount.Text = model.account;
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            var check = context.li_bank_accounts.FirstOrDefault(q => q.owner == userId && q.account == txtAccount.Text.Trim()); //检测账户是否重复
            if (check != null)
            {
                JscriptMsg("银行账户重复！", "", "Error");
                return false;
            }
            var model = new li_bank_accounts
            {
                owner = userId,
                bank = txtBank.Text.Trim(),
                opening_bank = txtOpeningBank.Text.Trim(),
                account = txtAccount.Text.Trim(),
                last_access_time = DateTime.Now
            };
            context.li_bank_accounts.InsertOnSubmit(model);
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), lblUser.Text + " 添加银行账户: " + model.account); //记录日志
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
            var model = context.li_bank_accounts.First(q => q.id == id);

            model.bank = txtBank.Text.Trim();
            model.opening_bank = txtOpeningBank.Text.Trim();
            model.account = txtAccount.Text.Trim();
            model.last_access_time = DateTime.Now;

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), lblUser.Text + " 修改银行账户:" + model.account); //记录日志
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
                ChkAdminLevel("manage_users_charge_withdraw", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改银行账户成功！", Utils.CombUrlTxt("bank_account_list.aspx", "user_id={0}", userId.ToString()), "Success");
            }
            else //添加
            {
                ChkAdminLevel("manage_users_charge_withdraw", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加银行账户成功！", Utils.CombUrlTxt("bank_account_list.aspx", "user_id={0}", userId.ToString()), "Success");
            }
        }

        protected string GetUserName()
        {
            return context.dt_users.First(u => u.id == userId).user_name;
        }
    }
}