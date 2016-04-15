using System;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.AutoLogic;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.claims
{
    public class BuyableClaim
    {
        public int ClaimId { get; set; }
        public string Owner { get; set; }
        public decimal Principal { get; set; }
        public decimal BuyableAmount { get; set; }
        public string ProjectName  { get; set; }
        public DateTime WithdrawTime  { get; set; }
    }

    public partial class agent_buying_claims : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        protected int selectedAgent;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");
            selectedAgent = DTRequest.GetQueryInt("selectedAgent");

            pageSize = GetPageSize(GetType().Name + "_page_size");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("agent_buying_claims", DTEnums.ActionEnum.View.ToString()); //检查权限
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
            ddlAgent.Items.Clear();
            var agentGroup = context.dt_user_groups.SingleOrDefault(g => g.title == AutoRepay.AgentGroup);
            if (agentGroup == null)
                throw new InvalidOperationException("请先设置中间人到“中间户”会员组");

            ddlAgent.Items.AddRange(
                agentGroup.dt_users
                    .AsEnumerable()
                    .Select(agent => new ListItem(agent.GetFriendlyUserName(), agent.id.ToString()))
                    .ToArray());
            if (ddlAgent.Items.Count != 0)
            {
                if (selectedAgent == 0)
                    selectedAgent = Convert.ToInt32(ddlAgent.Items[0].Value);
                else
                    ddlAgent.SelectedValue = selectedAgent.ToString();
            }
            else
            {
                ddlAgent.Items.Add(new ListItem("请先设置中间人到“中间户”会员组"));
            }
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            //txtKeywords.Text = keywords;
            // 中间人在后台只能买公司账号转出的债权
            var query =
                context.li_claims.Where(
                    c => c.profitingProjectId == c.projectId && c.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                        c.Parent.dt_users.dt_user_groups.title == AutoRepay.CompanyAccount &&
                        !c.Children.Any())
                    .AsEnumerable()
                    .Select(cl =>
                        new BuyableClaim
                        {
                            ClaimId = cl.id,
                            Principal = cl.principal,
                            BuyableAmount =
                                cl.principal - cl.li_project_transactions_profiting.Where(ptr =>
                                    ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn &&
                                    ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Pending)
                                    .Aggregate(0m, (sum, tr) => sum + tr.principal),
                            WithdrawTime = cl.createTime,
                            Owner = cl.dt_users.GetFriendlyUserName(),
                            ProjectName = cl.li_projects.title,
                        }).ToList();

            totalCount = query.Count;
            rptList.DataSource = query.OrderBy(q => q.Owner).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("agent_buying_claims.aspx", "keywords={0}&page={1}&selectedAgent={2}", txtKeywords.Text, "__id__", selectedAgent.ToString());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("agent_buying_claims.aspx", "keywords={0}&selectedAgent={1}", txtKeywords.Text, selectedAgent.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("agent_buying_claims.aspx", "keywords={0}&selectedAgent={1}", txtKeywords.Text, selectedAgent.ToString()));
        }

        protected void ddlAgent_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("agent_buying_claims.aspx", "keywords={0}&selectedAgent={1}", txtKeywords.Text, ddlAgent.SelectedValue));
        }

        protected void btnBuy_OnClick(object sender, EventArgs e)
        {
            var buyAmount = Request["__EVENTARGUMENT"];
            int claimId = Convert.ToInt32(((LinkButton)sender).CommandArgument);
            selectedAgent = Convert.ToInt32(ddlAgent.SelectedValue);

            try
            {
                context = new Agp2pDataContext();
                var claim = context.li_claims.SingleOrDefault(c => c.id == claimId);
                Response.Write("<script>window.open('" + "/api/payment/sumapay/index.aspx?api=" + (int)Agp2pEnums.SumapayApiEnum.CreAs
                                           + "&userId=" + selectedAgent + "&claimId=" + claimId + "&undertakeSum=" + buyAmount +"','_blank')</script>");

                //TransactionFacade.BuyClaim(context, claimId, selectedAgent, Convert.ToDecimal(buyAmount));
                //RptBind();
                //JscriptMsg("买入债权成功", "", "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("买入债权失败：" + ex.Message, "", "Success");
            }
        }
    }
}