using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Web.UI;

namespace Agp2p.Web.admin.settings
{
    public partial class cost_config : ManagePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("cost_config", DTEnums.ActionEnum.View.ToString()); //检查权限
                ShowInfo();
            }
        }

        private void ShowInfo()
        {
            txt_loan_fee_rate.Text = (Costconfig.loan_fee_rate * 100).ToString("N1");
            txt_loan_fee_rate_bank.Text = (Costconfig.loan_fee_rate_bank * 100).ToString("N2");
            txt_bond_fee_rate.Text = (Costconfig.bond_fee_rate * 100).ToString("N1");
            txt_bond_fee_rate_bank.Text = (Costconfig.bond_fee_rate_bank * 100).ToString("N2");
            txt_earlier_pay.Text = (Costconfig.earlier_pay*100).ToString("N1");
            txt_overtime_pay.Text = (Costconfig.overtime_pay*100).ToString("N1");
            txt_overtime_cost.Text = (Costconfig.overtime_cost*1000).ToString("N1");
            txt_overtime_cost2.Text = (Costconfig.overtime_cost2*1000).ToString("N1");
            txt_overtime_cost_bank.Text = (Costconfig.overtime_cost_bank * 1000).ToString("N1");
            txt_recharge_lowest.Text = Costconfig.recharge_lowest.ToString("N0");
            txt_withdraw.Text = Costconfig.withdraw.ToString("N1");
            txt_static_withdraw.Text = (Costconfig.static_withdraw * 100).ToString("N1");
            txt_recharge_fee_rate.Text = (Costconfig.recharge_fee_rate * 1000).ToString("N1");
            txt_recharge_fee_rate_quick.Text = (Costconfig.recharge_fee_rate_quick * 1000).ToString("N1");
        }

        protected void btnSave_OnClick(object sender, EventArgs e)
        {
            try
            {
                Model.costconfig model = new Model.costconfig();
                model.loan_fee_rate = Utils.StrToDecimal(txt_loan_fee_rate.Text.Trim(), 0) / 100;
                model.loan_fee_rate_bank = Utils.StrToDecimal(txt_loan_fee_rate_bank.Text.Trim(), 0) / 100;
                model.bond_fee_rate = Utils.StrToDecimal(txt_bond_fee_rate.Text.Trim(), 0) / 100;
                model.bond_fee_rate_bank = Utils.StrToDecimal(txt_bond_fee_rate_bank.Text.Trim(), 0) / 100;
                model.earlier_pay = Utils.StrToDecimal(txt_earlier_pay.Text.Trim(), 0)/100;
                model.overtime_pay = Utils.StrToDecimal(txt_overtime_pay.Text.Trim(), 0)/100;
                model.overtime_cost = Utils.StrToDecimal(txt_overtime_cost.Text.Trim(), 0)/1000;
                model.overtime_cost2 = Utils.StrToDecimal(txt_overtime_cost2.Text.Trim(), 0)/1000;
                model.overtime_cost_bank = Utils.StrToDecimal(txt_overtime_cost_bank.Text.Trim(), 0) / 1000;
                model.recharge_lowest = Utils.StrToDecimal(txt_recharge_lowest.Text.Trim(), 0);
                model.withdraw = Utils.StrToDecimal(txt_withdraw.Text.Trim(), 0) ;
                model.static_withdraw = Utils.StrToDecimal(txt_static_withdraw.Text.Trim(), 0) / 100;
                model.recharge_fee_rate = Utils.StrToDecimal(txt_recharge_fee_rate.Text.Trim(), 0) / 1000;
                model.recharge_fee_rate_quick = Utils.StrToDecimal(txt_recharge_fee_rate_quick.Text.Trim(), 0) / 1000;

                new BLL.cost_config().saveConifg(model);
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改费用配置信息"); //记录日志
                JscriptMsg("修改费用配置成功！", "cost_config.aspx", "Success");
                ConfigLoader.CleanCache();
            }
            catch
            {
                JscriptMsg("文件写入失败，请检查是否有权限！", "", "Error");
            }
        }
    }
}