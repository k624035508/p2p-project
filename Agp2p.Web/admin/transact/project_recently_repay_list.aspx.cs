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
    public partial class project_recently_repay_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");

            pageSize = GetPageSize(GetType().Name + "_page_size");
            if (!Page.IsPostBack)
            {
                var showType = DTRequest.GetQueryString("showType");
                if (!string.IsNullOrEmpty(showType))
                    rblProjectShowType.SelectedValue = showType;
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                var startTime = DTRequest.GetQueryString("startTime");
                if (!string.IsNullOrEmpty(startTime))
                    txtStartTime.Text = startTime;
                var endTime = DTRequest.GetQueryString("endTime");
                if (!string.IsNullOrEmpty(endTime))
                    txtEndTime.Text = endTime;
                ChkAdminLevel("manage_project_transaction_recently", DTEnums.ActionEnum.View.ToString()); //检查权限
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            //txtKeywords.Text = keywords;
            var query = context.li_projects
                .Where(
                    p =>
                        (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying <= p.status &&
                        p.status < (int)Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime && p.title.Contains(txtKeywords.Text));

            if (rblProjectShowType.SelectedValue == "1")
            {
                query = query.Where(p =>
                            p.li_repayment_tasks.Any(r =>
                                    r.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid &&
                                    r.should_repay_time.Date == DateTime.Today));
            }

            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(p => p.li_repayment_tasks.Any(r => Convert.ToDateTime(txtStartTime.Text) <= r.should_repay_time.Date));
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(p => p.li_repayment_tasks.Any(r => r.should_repay_time.Date <= Convert.ToDateTime(txtEndTime.Text)));

            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.add_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("project_recently_repay_list.aspx", "keywords={0}&page={1}&showType={2}&startTime={3}&endTime={4}", txtKeywords.Text, "__id__", rblProjectShowType.SelectedValue, txtStartTime.Text, txtEndTime.Text);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project_recently_repay_list.aspx", "keywords={0}&showType={1}&startTime={2}&endTime={3}", txtKeywords.Text, rblProjectShowType.SelectedValue, txtStartTime.Text, txtEndTime.Text));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("project_recently_repay_list.aspx", "keywords={0}&showType={1}&startTime={2}&endTime={3}", txtKeywords.Text, rblProjectShowType.SelectedValue, txtStartTime.Text, txtEndTime.Text));
        }

        protected string calcProjectProgress(li_projects project)
        {
            return (project.investment_amount/project.financing_amount).ToString("P2");
        }

        protected string QueryRepaymentProgress(li_projects pro)
        {
            var repayments = pro.li_repayment_tasks.Select(r => r.status).ToList();
            if (repayments.Count == 0) return "未满标";
            return string.Format("{0}/{1}", repayments.Count(r => r != (int) Agp2pEnums.RepaymentStatusEnum.Unpaid && r != (int)Agp2pEnums.RepaymentStatusEnum.OverTime), repayments.Count);
        }

        protected void btnRepayNow_OnClick(object sender, EventArgs e)
        {
            int projectId = Convert.ToInt32(((Button)sender).CommandArgument);
            try
            {
                var repaymentTaskId = context.li_projects.Single(p => p.id == projectId)
                    .li_repayment_tasks.First(t => t.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid && t.should_repay_time.Date <= DateTime.Today).id;
                var repayment = context.ExecuteRepaymentTask(repaymentTaskId, Agp2pEnums.RepaymentStatusEnum.ManualPaid);
                RptBind();
                var remark = "执行还款计划成功, 利息: " + repayment.repay_interest + " 返还本金: " + repayment.repay_principal;
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), remark); //记录日志
                JscriptMsg(remark, Utils.CombUrlTxt("project_recently_repay_list.aspx", "page={0}&keywords={1}&showType={2}&startTime={3}&endTime={4}", page.ToString(), txtKeywords.Text, rblProjectShowType.SelectedValue, txtStartTime.Text, txtEndTime.Text), "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("执行还款计划失败！" + ex.Message,
                    Utils.CombUrlTxt("project_recently_repay_list.aspx", "page={0}&keywords={1}&showType={2}&startTime={3}&endTime={4}",
                        page.ToString(), txtKeywords.Text, rblProjectShowType.SelectedValue, txtStartTime.Text, txtEndTime.Text), "Failure");
            }
        }

        protected void rblProjectShowType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
        }

        protected bool IsProjectCanRepayToday(li_projects pro)
        {
            return
                pro.li_repayment_tasks.Any(
                    r =>
                        (r.status == (int) Agp2pEnums.RepaymentStatusEnum.Unpaid ||
                         r.status == (int) Agp2pEnums.RepaymentStatusEnum.OverTime) &&
                        r.should_repay_time.Date <= DateTime.Today);
        }
    }
}