using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.users
{
    public partial class user_invitations : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        protected Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            ChkAdminLevel("user_invitations", DTEnums.ActionEnum.View.ToString()); //检查权限            
            pageSize = GetPageSize(GetType().Name + "_page_size");
            if (!Page.IsPostBack)
            {
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            keywords = DTRequest.GetQueryString("keywords");
            if (!string.IsNullOrEmpty(keywords))
            {
                txtKeywords2.Text = keywords;
            }

            var query = QueryInvitations();
            totalCount = query.Count();
            rptList.DataSource = query.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("user_invitations.aspx", "keywords={0}&page={1}", txtKeywords2.Text, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IQueryable<li_invitations> QueryInvitations()
        {
            IQueryable<li_invitations> query = context.li_invitations;
            if (!string.IsNullOrWhiteSpace(txtKeywords2.Text))
            {
                query = query.Where(q => q.dt_users.user_name.Contains(txtKeywords2.Text) || q.dt_users.mobile.Contains(txtKeywords2.Text)
                 || context.dt_users.Single(d => d.id == q.inviter).user_name.Contains(txtKeywords2.Text)
                 || context.dt_users.Single(d => d.id == q.inviter).mobile.Contains(txtKeywords2.Text));
            }
            return query;
        }
        #endregion

        //关键字查询
        protected void btnSearch_Click(object sender,EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("user_invitations.aspx", "keywords={0}", txtKeywords2.Text));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender,EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("user_invitations.aspx", "keywords={0}", txtKeywords2.Text));
        }

        //获取邀请人信息
        protected dt_users queryInviter(int inviterId)
        {
            var inviter = context.dt_users.Single(u => u.id == inviterId);
            return inviter;
        }
    }
}