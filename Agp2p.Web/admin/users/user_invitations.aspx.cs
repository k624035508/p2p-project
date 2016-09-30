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
            keywords = DTRequest.GetQueryString("keywords");
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
            txtKeywords2.Text = keywords;
            var query = context.li_invitations.ToList();
            totalCount = query.Count();
            rptList.DataSource = query.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("user_invitations.aspx", "keywords={0}&page={1}", keywords, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
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
    }
}