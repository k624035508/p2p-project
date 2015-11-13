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
    public partial class project_investment_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string project_id; // 根据 url 参数来保持当前选择的用户
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            project_id = DTRequest.GetQueryString("project_id");

            pageSize = GetPageSize(GetType().Name + "_page_size");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("manage_project_transaction", DTEnums.ActionEnum.View.ToString()); //检查权限
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            int projectId = Convert.ToInt32(project_id);
            var query =
                context.li_project_transactions.Where(
                    t => t.project == projectId && t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest);

            totalCount = query.Count();
            rptList.DataSource = query.OrderBy(q => q.status).ThenByDescending(q => q.create_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("project_investment_list.aspx", "page={0}&project_id={1}", "__id__", project_id);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("project_investment_list.aspx", "project_id={0}", project_id));
        }

        protected string queryProfitStatus(li_project_transactions tr)
        {
            var profits = context.li_project_transactions.Where(
                a =>
                    a.project == tr.project && a.investor == tr.investor &&
                    (a.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.RepayToInvestor && 0 < a.interest))
                .Select(t => t.interest.Value)
                .ToList();
            return string.Format("收益 {0} 次，共 {1} 元", profits.Count, profits.Sum().ToString("c"));
        }

        protected void btnRefund_OnClick(object sender, EventArgs e)
        {
            try
            {
                int projectTransactionId = Convert.ToInt32(((Button)sender).CommandArgument);
                var bt = context.Refund(projectTransactionId);
                var remark = "撤销投资成功, 涉及金额: " + bt.principal;
                AddAdminLog(DTEnums.ActionEnum.Cancel.ToString(), remark); //记录日志
                JscriptMsg(remark, Utils.CombUrlTxt("project_investment_list.aspx", "project_id={0}", project_id), "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("撤销投资失败！" + ex.Message, Utils.CombUrlTxt("project_investment_list.aspx", "project_id={0}", project_id), "Failure");
            }
        }
    }
}