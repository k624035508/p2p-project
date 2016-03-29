using System;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.AutoLogic;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.claims
{
    public class BuyedClaim
    {
        public int ClaimId { get; set; }
        public string OriginalOwner { get; set; }
        public decimal Principal { get; set; }
        public string ProjectName  { get; set; }
        public DateTime? WithdrawTime  { get; set; }
        public DateTime? BuyTime  { get; set; }
    }

    public partial class agent_buyed_claims : UI.ManagePage
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
                ChkAdminLevel("agent_buyed_claims", DTEnums.ActionEnum.View.ToString()); //检查权限
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
                selectedAgent = Convert.ToInt32(ddlAgent.Items[0].Value);
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
            var query =
                context.li_claims.Where(
                    c =>
                        c.userId == selectedAgent &&
                        c.Parent != null && c.Parent.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                        c.profitingProjectId == c.projectId && c.status == (int) Agp2pEnums.ClaimStatusEnum.Transferable &&
                        !c.Children.Any());

            totalCount = query.Count();
            var thisPageClaims = query.OrderByDescending(c => c.createTime).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataSource = thisPageClaims.Select(cl =>
                new BuyedClaim
                {
                    ClaimId = cl.id,
                    Principal = cl.principal,
                    WithdrawTime = cl.Parent.createTime, /* 实际上等于 cl.createTime */
                    OriginalOwner = cl.Parent.dt_users.GetFriendlyUserName(),
                    BuyTime = cl.li_project_transactions_invest.create_time,
                    ProjectName = cl.li_projects.title,
                }).Concat(Enumerable.Repeat(new BuyedClaim
                {
                    ClaimId = 0,
                    OriginalOwner = "总计",
                    Principal = thisPageClaims.Aggregate(0m, (sum, c) => sum + c.principal)
                }, 1)).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("agent_buyed_claims.aspx", "keywords={0}&page={1}&selectedAgent={2}", txtKeywords.Text, "__id__", selectedAgent.ToString());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("agent_buyed_claims.aspx", "keywords={0}&selectedAgent={1}", txtKeywords.Text, selectedAgent.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("agent_buyed_claims.aspx", "keywords={0}&selectedAgent={1}", txtKeywords.Text, selectedAgent.ToString()));
        }

        protected void ddlAgent_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("agent_buyed_claims.aspx", "keywords={0}&selectedAgent={1}", txtKeywords.Text, ddlAgent.SelectedValue));
        }

        protected void btnBuy_OnClick(object sender, EventArgs e)
        {
            var buyAmount = Request["__EVENTARGUMENT"];
            int claimId = Convert.ToInt32(((LinkButton)sender).CommandArgument);
            selectedAgent = Convert.ToInt32(ddlAgent.SelectedValue);

            try
            {
                TransactionFacade.BuyClaim(context, claimId, selectedAgent, Convert.ToDecimal(buyAmount));
                context = new Agp2pDataContext();
                RptBind();
                JscriptMsg("买入债权成功", "", "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("买入债权失败：" + ex.Message, "", "Success");
            }
        }
    }
}