using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Core;
using Agp2p.Web.UI;

namespace Agp2p.Web.admin.repayment
{
    public partial class repay_earlier : Web.UI.ManagePage
    {
        protected int ChannelId;
        protected int TotalCount;
        protected int PageIndex;
        protected int PageSize;
        protected int CategoryId;
        protected int ProjectStatus;
        protected string ChannelName = string.Empty;
        protected string Keywords = string.Empty;
        protected Dictionary<int, string> CategoryIdTitleMap;

        private readonly Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            this.ChannelId = DTRequest.GetQueryInt("channel_id");
            this.CategoryId = DTRequest.GetQueryInt("category_id");
            this.ProjectStatus = DTRequest.GetQueryInt("status");

            if (ChannelId == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
            this.ChannelName = new BLL.channel().GetChannelName(this.ChannelId); //取得频道名称
            CategoryIdTitleMap = context.dt_article_category.Where(c => c.channel_id == this.ChannelId).OrderBy(c => c.sort_id).ToDictionary(c => c.id, c => c.title);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("repay_earlier", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //绑定类别
                RptBind();
            }
        }

        protected void TreeBind()
        {
            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            this.ddlCategoryId.Items.AddRange(CategoryIdTitleMap.Select(c => new ListItem(c.Value, c.Key.ToString())).ToArray());
        }


        #region 数据绑定=================================
        protected void RptBind()
        {
            this.PageIndex = DTRequest.GetQueryInt("page", 1);
            if (this.CategoryId > 0)
            {
                this.ddlCategoryId.SelectedValue = this.CategoryId.ToString();
            }

            //绑定列表
            this.rptList1.DataSource = GetList();
            this.rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = this.PageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("repay_earlier.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&page={4}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString(), "__id__");
            PageContent.InnerHtml = Utils.OutPageList(this.PageSize, this.PageIndex, this.TotalCount, pageUrl, 8);
        }

        /// <summary>
        /// 获取项目列表
        /// </summary>
        /// <param name="_channel_name"></param>
        /// <param name="_category_id"></param>
        /// <param name="_pageIndex"></param>
        /// <param name="_keyword"></param>
        /// <param name="_project_status"></param>
        /// <returns></returns>
        private List<RepayEarlier> GetList()
        {
            PageSize = GetPageSize(GetType().Name + "_page_size");
            var query =
                context.li_repayment_tasks.Where(
                    r =>
                        r.status == (int)Agp2pEnums.RepaymentStatusEnum.EarlierPaid &&
                        (r.li_projects.title.Contains(Keywords) || r.li_projects.no.Contains(Keywords))).OrderBy(r => r.should_repay_time).AsQueryable();

            if (CategoryId > 0)
                query = query.Where(q => q.li_projects.category_id == CategoryId);

            var repayList = query.AsEnumerable().Select(r =>
            {
                var repay = new RepayEarlier();
                var loaner = r.li_projects.li_risks.li_loaners.dt_users;
                repay.Loaner = $"{loaner.real_name}({loaner.user_name})";
                //计算原来的还款计划应还总额
                var repayOld =
                    r.li_projects.li_repayment_tasks.Where(t => t.project == r.project && t.status != (int) Agp2pEnums.RepaymentStatusEnum.EarlierPaid).OrderBy(t => t.should_repay_time);//旧计划
                //只有一期
                if (!repayOld.Any())
                {
                    repay.Amount = r.repay_interest + r.repay_principal;
                    repay.FactAmount = repay.Amount;
                }
                else
                {
                    repay.Amount = repayOld.Sum(t => t.repay_interest + t.repay_principal); //应还款
                    repay.FactAmount = (r.repay_interest + r.repay_principal) +
                                       (repayOld.Where(t => t.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid)
                                           .Sum(t => t.repay_interest + t.repay_principal)); //实还款
                }

                repay.Cost = r.cost??0;
                repay.DayCount = (r.should_repay_time.Subtract((DateTime)r.repay_at)).Days;//提前天数
                repay.ShouldRepayTime = r.should_repay_time.ToString("yyyy-MM-dd HH:mm");//应还时间
                repay.RepayTime = r.repay_at?.ToString("yyyy-MM-dd HH:mm") ?? "";//实还时间
                repay.Category = r.li_projects.category_id;
                repay.ProfitRate = r.li_projects.profit_rate_year;
                repay.RepaymentType =
                    Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTypeEnum)r.li_projects.repayment_type);
                repay.ProjectId = r.li_projects.id;
                repay.ProjectTitle = r.li_projects.title;
                repay.ProjectStatus = r.li_projects.status;
                repay.RepayStatus = r.status;

                repay.TimeTerm = r.li_projects.repayment_term_span == (int) Agp2pEnums.ProjectRepaymentTermSpanEnum.Day
                    ? "1/1"
                    : $"{r.term.ToString()}/{r.li_projects.repayment_term_span_count}";

                return repay;
            }).AsQueryable();

            this.TotalCount = repayList.Count();
            return repayList.Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("repay_earlier.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {            
            Response.Redirect(Utils.CombUrlTxt("repay_earlier.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("repay_earlier.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        class RepayEarlier
        {
            public int ProjectId { get; set; }
            public string ProjectTitle { get; set; }
            public string Loaner { get; set; }
            public decimal Amount { get; set; }//应还
            public decimal FactAmount { get; set; }//实际还款
            public decimal Cost { get; set; }//罚息
            public string ShouldRepayTime { get; set; }//应还时间
            public string RepayTime { get; set; }//实还时间
            public int DayCount { get; set; }//提前天数
            public int Category { get; set; }//产品
            public decimal ProfitRate { get; set; }//年化利率
            public string RepaymentType { get; set; }//年化利率
            public int ProjectStatus { get; set; }//项目状态
            public int RepayStatus { get; set; }//还款状态
            public string TimeTerm { get; set; }//期数
        }
    }
}