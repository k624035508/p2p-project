using System;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Agp2p.Core;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;

namespace Agp2p.Web.admin.transact
{
    public partial class all_bank_account_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            keywords = DTRequest.GetQueryString("keywords");

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
            txtKeywords2.Text = keywords;
            var query = context.li_bank_accounts.Where(b => (b.bank.Contains(keywords) || b.account.Contains(keywords) || b.dt_users.real_name.Contains(keywords)));
            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.last_access_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum2.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("all_bank_account_list.aspx", "keywords={0}&page={1}", keywords, "__id__");
            PageContent2.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click2(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("all_bank_account_list.aspx", "keywords={0}", txtKeywords2.Text));
        }

        //银行卡状态管理
        protected string GetTypeName(string typeId)
        {
            switch (typeId)
            {
                case "1":
                    return "未知";
                case "2":
                    return "快捷支付";
                case "3":
                    return "网银支付";
                default:
                    return "其它";
            }
        }




        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum2.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("all_bank_account_list.aspx", "keywords={0}", keywords));
        }

    }
}