using System;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.transact
{
    public class HuoqiProjectInvestor
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal NonWithdrawingMoney { get; set; }
        public decimal WithdrawingMoney { get; set; }
        public decimal WithdrawSuccessMoney { get; set; }
    }

    public partial class huoqi_withdraw : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        protected int huoqiProject;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");
            huoqiProject = DTRequest.GetQueryInt("huoqiProject");

            pageSize = GetPageSize(GetType().Name + "_page_size");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("huoqi_withdraw", DTEnums.ActionEnum.View.ToString()); //检查权限
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
            ddlHuoqiProjects.Items.Clear();
            ddlHuoqiProjects.Items.AddRange(
                context.li_projects.Where(
                    p =>
                        p.dt_article_category.call_index == "huoqi" &&
                        p.status == (int) Agp2pEnums.ProjectStatusEnum.Financing)
                    .AsEnumerable()
                    .Select(p => new ListItem(p.title, p.id.ToString()))
                    .ToArray());
            if (ddlHuoqiProjects.Items.Count != 0)
            {
                huoqiProject = Convert.ToInt32(ddlHuoqiProjects.Items[0].Value);
            }
            else
            {
                ddlHuoqiProjects.Items.Add(new ListItem("暂无活期项目"));
            }
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            //txtKeywords.Text = keywords;
            ddlHuoqiProjects.SelectedValue = huoqiProject.ToString();
            var query = context.li_claims.Where(c => c.profitingProjectId == huoqiProject && !c.Children.Any()).ToLookup(c => c.dt_users).Select(
                g =>
                {
                    return new HuoqiProjectInvestor
                    {
                        UserId = g.Key.id,
                        NonWithdrawingMoney = g.Where(c => c.status < (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer).Sum(c => c.principal),
                        UserName =
                            string.IsNullOrWhiteSpace(g.Key.real_name)
                                ? g.Key.user_name
                                : string.Format("{0}({1})", g.Key.user_name, g.Key.real_name),
                        WithdrawingMoney =
                            g.Where(c => c.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer)
                                .Aggregate(0m, (sum, c) => sum + c.principal),
                        WithdrawSuccessMoney =
                            g.Where(
                                c =>
                                    c.status == (int) Agp2pEnums.ClaimStatusEnum.TransferredUnpaid ||
                                    c.status == (int) Agp2pEnums.ClaimStatusEnum.CompletedUnpaid)
                                .Aggregate(0m, (sum, c) => sum + c.principal),
                    };
                }).Where(e => e.NonWithdrawingMoney != 0 || e.WithdrawingMoney != 0 || e.WithdrawSuccessMoney != 0).ToList();

            totalCount = query.Count;
            rptList.DataSource = query.OrderBy(q => q.UserName).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("huoqi_withdraw.aspx", "keywords={0}&page={1}&huoqiProject={2}", txtKeywords.Text, "__id__", huoqiProject.ToString());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("huoqi_withdraw.aspx", "keywords={0}&huoqiProject={1}", txtKeywords.Text,huoqiProject.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("huoqi_withdraw.aspx", "keywords={0}&huoqiProject={1}", txtKeywords.Text, huoqiProject.ToString()));
        }

        protected void ddlHuoqiProjects_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("huoqi_withdraw.aspx", "keywords={0}&huoqiProject={1}", txtKeywords.Text, ddlHuoqiProjects.SelectedValue));
        }

        protected void btnWithdraw_OnClick(object sender, EventArgs e)
        {
            var withdrawAmount = Request["__EVENTARGUMENT"];
            int userId = Convert.ToInt32(((LinkButton)sender).CommandArgument);
            huoqiProject = Convert.ToInt32(ddlHuoqiProjects.SelectedValue);

            try
            {
                context.HuoqiProjectWithdraw(userId, huoqiProject, Convert.ToDecimal(withdrawAmount));
                RptBind();
                JscriptMsg("提现成功", "", "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("提现失败：" + ex.Message, "", "Success");
            }
        }
    }
}