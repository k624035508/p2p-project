using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.BLL;
using Lip2p.Common;
using Lip2p.Core;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.transact
{
    public partial class bank_transaction_charge : UI.ManagePage
    {
        protected int id = 0, userId;
        private Lip2pDataContext context = new Lip2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["user_id"], out userId))
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            var user = context.dt_users.First(b => b.id == userId);
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("manage_users_charge_withdraw", DTEnums.ActionEnum.View.ToString()); //检查权限
                lblUser.Text = user.user_name;
                lblIdleMoney.Text = user.li_wallets.idle_money.ToString("c");
            }
        }

        #region 增加操作=================================
        private bool DoAdd()
        {
            try
            {
                var bankTransaction = context.Charge(userId, Math.Abs(Convert.ToDecimal(txtValue.Text)),
                    Lip2pEnums.PayApiTypeEnum.ManualAppend,
                    string.IsNullOrWhiteSpace(txtRemarks.Text) ? null : txtRemarks.Text.Trim());
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), string.Format("{0} 添加用户 {1} 充值记录: {2} 备注: {3}", lblUser.Text, bankTransaction.dt_users.user_name, txtValue.Text, txtRemarks.Text)); //记录日志
                return true;
            }
            catch (Exception ex)
            {
                JscriptMsg(ex.Message, "", "Error");
                return false;
            }
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("manage_users_charge_withdraw", DTEnums.ActionEnum.Add.ToString()); //检查权限
            if (!DoAdd())
            {
                JscriptMsg("保存过程中发生错误！", "", "Error");
                return;
            }
            JscriptMsg("添加银行账户交易记录成功！", Utils.CombUrlTxt("bank_users_charge_withdraw.aspx", "user_id={0}", userId.ToString()), "Success");
        }
    }
}