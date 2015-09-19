using System;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.transact
{
    public partial class bank_account_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty, user_id; // 根据 url 参数来保持当前选择的用户
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            keywords = DTRequest.GetQueryString("keywords");
            user_id = DTRequest.GetQueryString("user_id");

            if (user_id == "")
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }

            pageSize = GetPageSize(10); //每页数量
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
            txtKeywords.Text = keywords;
            var query = context.li_bank_accounts.Where(b => b.owner == Convert.ToInt32(user_id)).Where(b => b.bank.Contains(keywords) || b.account.Contains(keywords));
            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.last_access_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("bank_account_list.aspx", "keywords={0}&page={1}&user_id={2}", keywords, "__id__", user_id);
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
            Response.Redirect(Utils.CombUrlTxt("bank_account_list.aspx", "keywords={0}&user_id={1}", txtKeywords.Text, user_id));
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
            Response.Redirect(Utils.CombUrlTxt("bank_account_list.aspx", "keywords={0}&user_id={1}", keywords, user_id));
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("manage_users_charge_withdraw", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    var preDel = context.li_bank_accounts.FirstOrDefault(q => q.id == id);
                    if (preDel != null)
                    {
                        sucCount += 1;
                        context.li_bank_accounts.DeleteOnSubmit(preDel);
                    }
                    else errorCount += 1;
                }
            }
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除银行账户 " + sucCount + " 条，失败 " + errorCount + " 条"); //记录日志
                JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！", Utils.CombUrlTxt("bank_account_list.aspx", "keywords={0}&user_id={1}", keywords, user_id), "Success");
            }
            catch (Exception)
            {
                JscriptMsg("删除失败！", Utils.CombUrlTxt("bank_account_list.aspx", "keywords={0}&user_id={1}", keywords, user_id), "Failure");
            }
        }

        protected string GetUserName()
        {
            return context.dt_users.First(u => u.id == Convert.ToInt32(user_id)).user_name;
        }
    }
}