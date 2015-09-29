using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
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
            txt_earlier_pay.Text = Costconfig.earlier_pay.ToString("N1");
            txt_overtime_pay.Text = Costconfig.overtime_pay.ToString("N1");
            txt_recharge_lowest.Text = Costconfig.recharge_lowest.ToString("N0");
            txt_withdraw.Text = Costconfig.withdraw.ToString("N1");
        }

        protected void btnSave_OnClick(object sender, EventArgs e)
        {
            try
            {
                Model.costconfig model = new Model.costconfig();
                model.earlier_pay = Utils.StrToFloat(txt_earlier_pay.Text.Trim(), 0);
                model.overtime_pay = Utils.StrToFloat(txt_overtime_pay.Text.Trim(), 0);
                model.recharge_lowest = Utils.StrToFloat(txt_recharge_lowest.Text.Trim(), 0);
                model.withdraw = Utils.StrToFloat(txt_withdraw.Text.Trim(), 0);

                new BLL.cost_config().saveConifg(model);
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改费用配置信息"); //记录日志
                JscriptMsg("修改费用配置成功！", "cost_config.aspx", "Success");
            }
            catch
            {
                JscriptMsg("文件写入失败，请检查是否有权限！", "", "Error");
            }
        }
    }
}