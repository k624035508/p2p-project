using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.AutoLogic;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.claims
{
    public class WithdrawClaim
    {
        public string OriginalOwner { get; set; }
        public DateTime? WithdrawTime  { get; set; }
        public string WithdrawClaimCompleteDay  { get; set; }
    }

    public class BuyedClaim
    {
        public int ClaimId { get; set; }
        public WithdrawClaim WithdrawClaim { get; set; }
        public string Number  { get; set; }
        public decimal Principal { get; set; }
        public string ProjectName  { get; set; }
        public string HuoqiInvestor  { get; set; }
        public DateTime? HuoqiInvestTime  { get; set; }
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
            // 被投了活期也要显示
            var query =
                context.li_claims.Where(
                    c =>
                        (c.userId == selectedAgent && c.profitingProjectId == c.projectId && c.status == (int) Agp2pEnums.ClaimStatusEnum.Transferable
                         || c.agent == selectedAgent && c.status < (int) Agp2pEnums.ClaimStatusEnum.Completed) && !c.Children.Any());

            totalCount = query.Count();
            var thisPageClaims = query.OrderByDescending(c => c.createTime).Skip(pageSize * (page - 1)).Take(pageSize).ToList();

            var emptyWithdrawClaim = new WithdrawClaim();
            rptList.DataSource = thisPageClaims.Select(cl => new {firstHistoryClaimByAgent = cl.GetFirstHistoryClaimByOwner(selectedAgent), cl})
                    .GroupBy(g => g.firstHistoryClaimByAgent, g => g.cl)
                    .SelectMany(pairs =>
                    {
                        var firstHistoryClaimByAgent = pairs.Key;
                        var rootWithdrawClaim = firstHistoryClaimByAgent.Parent;
                        var buyedClaims = pairs.Select(cl =>
                        {
                            if (cl.agent != null)
                            {
                                return new BuyedClaim
                                {
                                    ClaimId = cl.id,
                                    WithdrawClaim = emptyWithdrawClaim,
                                    Number = cl.number,
                                    Principal = cl.principal,
                                    BuyTime = firstHistoryClaimByAgent.li_project_transactions_invest.create_time,
                                    ProjectName = cl.li_projects.title,
                                    HuoqiInvestor = cl.dt_users.GetFriendlyUserName(),
                                    HuoqiInvestTime = cl.li_project_transactions_invest.create_time
                                };
                            }
                            return new BuyedClaim
                            {
                                ClaimId = cl.id,
                                WithdrawClaim = emptyWithdrawClaim,
                                Number = cl.number,
                                Principal = cl.principal,
                                BuyTime = firstHistoryClaimByAgent.li_project_transactions_invest.create_time,
                                ProjectName = cl.li_projects.title,
                            };
                        }).ToList();
                        if (buyedClaims.Any())
                        {
                            buyedClaims.First().WithdrawClaim = new WithdrawClaim
                            {
                                OriginalOwner = rootWithdrawClaim.dt_users.GetFriendlyUserName(),
                                WithdrawTime = rootWithdrawClaim.createTime,
                                WithdrawClaimCompleteDay = rootWithdrawClaim.li_projects.li_repayment_tasks.FirstOrDefault(t => t.IsUnpaid())?.should_repay_time.ToString("yyyy-MM-dd")
                            };
                            return buyedClaims.Concat(Enumerable.Repeat(new BuyedClaim
                            {
                                ClaimId = 0,
                                WithdrawClaim = new WithdrawClaim {OriginalOwner = "原债权本金"},
                                Principal = rootWithdrawClaim.principal,
                                ProjectName = "已买入活期",
                                HuoqiInvestor = buyedClaims.Where(c => !string.IsNullOrWhiteSpace(c.HuoqiInvestor)).Aggregate(0m, (sum, c) => sum + c.Principal).ToString("f"),
                            }, 1));
                        }
                        return buyedClaims;
                    }).Concat(Enumerable.Repeat(new BuyedClaim
                    {
                        ClaimId = 0,
                        WithdrawClaim = new WithdrawClaim {OriginalOwner = "总计"},
                        Principal = thisPageClaims.Aggregate(0m, (sum, c) => sum + c.principal),
                        ProjectName = "已买入活期",
                        HuoqiInvestor = thisPageClaims.Where(c => c.agent != null).Aggregate(0m, (sum, c) => sum + c.principal).ToString("f"),
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