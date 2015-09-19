using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.transact
{
    public partial class project_investment_edit : UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        protected int id = 0, projectId;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            if (DTEnums.ActionEnum.Add.ToString() == _action)
            {
                if (!int.TryParse(Request.QueryString["project_id"], out projectId))
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("manage_project_transaction", DTEnums.ActionEnum.View.ToString()); //检查权限
                InitSelectUserDDL();
                if (action == DTEnums.ActionEnum.Add.ToString())
                {
                    var article = context.li_projects.First(u => u.id == projectId);
                    lblProject.Text = article.title;
                    lblRemainCredits.Text = context.GetInvestmentProgress(projectId, (invested, total) => total - invested).ToString("c");
                }
            }
        }

        private void InitSelectUserDDL()
        {
            var users = context.dt_users.OrderByDescending(u => u.id);
            ddlInvestor.Items.Clear();
            ddlInvestor.Items.AddRange(users.Select(u => new ListItem(u.user_name, u.id.ToString())).ToArray());

            var userId = Convert.ToInt32(ddlInvestor.SelectedValue);
            var wallet = context.li_wallets.First(u => u.user_id == userId);
            lblUserIdleMoney.Text = wallet.idle_money.ToString("c");
        }

        #region 增加操作=================================
        private bool DoAdd()
        {
            try
            {
                var investingMoney = Math.Abs(Convert.ToDecimal(txtValue.Text.Trim()));
                context.Invest(Convert.ToInt32(ddlInvestor.SelectedValue), projectId, investingMoney);
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), ddlInvestor.SelectedItem.Text + " 被添加投资信息: " + investingMoney); //记录日志
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
            if (action == DTEnums.ActionEnum.Add.ToString())
            {
                ChkAdminLevel("manage_project_transaction", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加投资信息成功！", Utils.CombUrlTxt("project_investment_list.aspx", "project_id={0}", projectId.ToString()), "Success");
            }
        }

        protected void ddlInvestor_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var userId = Convert.ToInt32(ddlInvestor.SelectedValue);
            var wallet = context.li_wallets.First(u => u.user_id == userId);
            lblUserIdleMoney.Text = wallet.idle_money.ToString("c");
        }
    }
}