using System;
using System.Linq;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.transact
{
    public partial class bank_users_charge_withdraw : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        protected int UserGroud;
        private Lip2pDataContext context = new Lip2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");
            UserGroud = DTRequest.GetQueryInt("UserGroud");

            pageSize = GetPageSize(10); //每页数量
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("manage_users_charge_withdraw", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                TreeBind();
                RptBind();
            }
        }

        #region 绑定用户分组=================================
        protected void TreeBind()
        {
            ddlUserGroud.Items.Clear();
            ddlUserGroud.Items.Add(new ListItem("所有会员组", ""));

            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();

            ddlUserGroud.Items.AddRange(context.dt_user_groups.Where(g => g.is_lock == 0 && (!canAccessGroups.Any() || canAccessGroups.Contains(g.id)))
                .OrderByDescending(g => g.id)
                .Select(g => new ListItem(g.title, g.id.ToString()))
                .ToArray());

        }
        #endregion

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            //txtKeywords.Text = keywords;
            var query = context.dt_users.Where(b => b.user_name.Contains(txtKeywords.Text));
            //用户分组查询
            if (0 < UserGroud) // 选择了某一组
            {
                ddlUserGroud.SelectedValue = UserGroud.ToString();
                query = query.Where(b => b.group_id == UserGroud);
            }
            else
            {
                // 限制当前管理员对会员的查询
                var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();
                query = query.Where(u => !canAccessGroups.Any() || canAccessGroups.Contains(u.group_id));
            }

            totalCount = query.Count();
            rptList.DataSource = query.OrderBy(q => q.user_name).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("bank_users_charge_withdraw.aspx", "keywords={0}&page={1}&UserGroud={2}", txtKeywords.Text, "__id__", UserGroud.ToString());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        #region 返回每页数量=============================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie(GetType().Name + "_page_size"), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    return _pagesize;
                }
            }
            return _default_size;
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("bank_users_charge_withdraw.aspx", "keywords={0}&UserGroud={1}", txtKeywords.Text,UserGroud.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie(GetType().Name + "_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("bank_users_charge_withdraw.aspx", "keywords={0}&UserGroud={1}", txtKeywords.Text, UserGroud.ToString()));
        }

        //筛选类别
        protected void ddlUserGroud_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("bank_users_charge_withdraw.aspx", "keywords={0}&UserGroud={1}", txtKeywords.Text, ddlUserGroud.SelectedValue));
        }

        protected decimal QueryChargingMoney(dt_users user)
        {
            return user.li_bank_transactions.Where(
                t =>
                    t.status == (int) Lip2pEnums.BankTransactionStatusEnum.Acting &&
                    t.type == (int) Lip2pEnums.BankTransactionTypeEnum.Charge).Select(t => t.value).AsEnumerable().Sum();
        }

        protected decimal QueryWithdrawingMoney(dt_users user)
        {
            return user.li_bank_transactions.Where(
                t =>
                    t.status == (int) Lip2pEnums.BankTransactionStatusEnum.Acting &&
                    t.type == (int) Lip2pEnums.BankTransactionTypeEnum.Withdraw).Select(t => t.value).AsEnumerable().Sum();
        }

        protected int GetBankTransactionCount(dt_users user)
        {
            return user.li_bank_transactions.Count + user.li_bank_accounts.Sum(a => a.li_bank_transactions.Count);
        }
    }
}