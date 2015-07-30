using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Lip2p.BLL;
using Lip2p.Common;
using Lip2p.Core;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.transact
{
    public partial class project_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        private Lip2pDataContext context = new Lip2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");

            pageSize = GetPageSize(10); //每页数量
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("manage_project_transaction", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
           //txtKeywords.Text = keywords;
            var query = context.li_projects
                .Where(p => (int)Lip2pEnums.ProjectStatusEnum.FaBiao <= p.status && p.title.Contains(txtKeywords.Text));

            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.add_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("project_list.aspx", "keywords={0}&page={1}", txtKeywords.Text, "__id__");
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
            Response.Redirect(Utils.CombUrlTxt("project_list.aspx", "keywords={0}", txtKeywords.Text));
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
            Response.Redirect(Utils.CombUrlTxt("project_list.aspx", "keywords={0}", txtKeywords.Text));
        }

        protected string calcProjectProgress(li_projects project)
        {
            return (project.investment_amount/project.financing_amount).ToString("P2");
        }

        protected string QueryRepaymentProgress(li_projects pro)
        {
            if (pro.status <= (int) Lip2pEnums.ProjectStatusEnum.FaBiao)
                return "未满标";
            var repayments = pro.li_repayment_tasks.Select(r => r.status).ToList();
            return string.Format("{0}/{1}", repayments.Count(r => r != (int) Lip2pEnums.RepaymentStatusEnum.Unpaid), repayments.Count);
        }

        protected void btnFinishInvestment_OnClick(object sender, EventArgs e)
        {
            try
            {
                int projectId = Convert.ToInt32(((Button)sender).CommandArgument);
                var pro = context.FinishInvestment(projectId);
                AddAdminLog(DTEnums.ActionEnum.Cancel.ToString(), "截标成功：" + pro.title); //记录日志
                JscriptMsg("截标成功：" + pro.title, Utils.CombUrlTxt("project_list.aspx", "page={0}&keywords={1}", page.ToString(), txtKeywords.Text), "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("截标失败！" + ex.Message, Utils.CombUrlTxt("project_list.aspx", "page={0}&keywords={1}", page.ToString(), txtKeywords.Text), "Failure");
            }
        }
    }
}