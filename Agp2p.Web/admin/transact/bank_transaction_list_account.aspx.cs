using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.transact
{
    public partial class bank_transaction_list_account : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string account_id; // 根据 url 参数来保持当前选择的用户与银行账户
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            account_id = DTRequest.GetQueryString("account_id");

            pageSize = GetPageSize(GetType().Name + "_page_size");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("manage_users_charge_withdraw", DTEnums.ActionEnum.View.ToString()); //检查权限
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            int accountId = Convert.ToInt32(account_id);
            var query = context.li_bank_transactions.Where(b => b.withdraw_account == accountId);
            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.id).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("bank_transaction_list_account.aspx", "page={0}&account_id={1}", "__id__", account_id);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("bank_transaction_list_account.aspx", "account_id={0}", account_id));
        }

        protected string GetUserId()
        {
            int accountId = Convert.ToInt32(account_id);
            return context.li_bank_accounts.First(b => b.id == accountId).owner.ToString();
        }

        protected void btnConfirm_OnClick(object sender, EventArgs e)
        {
            try
            {
                int bankTransactionId = Convert.ToInt32(((Button)sender).CommandArgument);
                var bt = context.ConfirmBankTransaction(bankTransactionId, GetAdminInfo().id);
                var bankTransactionType = (Agp2pEnums.BankTransactionTypeEnum) Convert.ToByte(bt.type);
                var remark = (bankTransactionType == Agp2pEnums.BankTransactionTypeEnum.Withdraw
                    ? ("确认银行账户 " + bt.li_bank_accounts.account)
                    : ("确认用户 " + bt.dt_users.user_name)) + " " + Utils.GetAgp2pEnumDes(bankTransactionType) + "成功, 涉及金额: " + bt.value;

                AddAdminLog(DTEnums.ActionEnum.Confirm.ToString(), remark); //记录日志
                JscriptMsg(remark, Utils.CombUrlTxt("bank_transaction_list_account.aspx", "account_id={0}", account_id), "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("确认银行账户交易失败！" + ex.Message, Utils.CombUrlTxt("bank_transaction_list_account.aspx", "account_id={0}", account_id), "Failure");
            }
        }

        protected void btnCancel_OnClick(object sender, EventArgs e)
        {
            try
            {
                int bankTransactionId = Convert.ToInt32(((Button)sender).CommandArgument);
                var bt = context.CancelBankTransaction(bankTransactionId, GetAdminInfo().id);
                var bankTransactionType = (Agp2pEnums.BankTransactionTypeEnum)Convert.ToByte(bt.type);
                var remark = (bankTransactionType == Agp2pEnums.BankTransactionTypeEnum.Withdraw
                    ? ("取消银行账户 " + bt.li_bank_accounts.account)
                    : ("取消用户 " + bt.dt_users.user_name)) + " " + Utils.GetAgp2pEnumDes(bankTransactionType) + "成功, 涉及金额: " + bt.value;

                AddAdminLog(DTEnums.ActionEnum.Cancel.ToString(), remark); //记录日志
                JscriptMsg(remark, Utils.CombUrlTxt("bank_transaction_list_account.aspx", "account_id={0}", account_id), "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("取消银行账户交易失败！" + ex.Message, Utils.CombUrlTxt("bank_transaction_list_account.aspx", "account_id={0}", account_id), "Failure");
            }
        }

        protected decimal GetHandlingFee(li_bank_transactions tr)
        {
            return tr.handling_fee_type == (int) Agp2pEnums.BankTransactionHandlingFeeTypeEnum.NoHandlingFee
                ? 0
                : Math.Max(TransactionFacade.DefaultHandlingFee, Convert.ToDecimal(Eval("handling_fee")));
        }

        protected string GetUserName()
        {
            return context.li_bank_accounts.First(b => b.id == Convert.ToInt32(account_id)).dt_users.user_name;
        }

        protected string GetBankName()
        {
            return context.li_bank_accounts.First(b => b.id == Convert.ToInt32(account_id)).bank;
        }
    }
}