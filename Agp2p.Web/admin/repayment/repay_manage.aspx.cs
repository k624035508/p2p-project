using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Core;

namespace Agp2p.Web.admin.repayment
{
    public partial class repay_manage : Web.UI.ManagePage
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
                ChkAdminLevel("repay_manage", DTEnums.ActionEnum.View.ToString()); //检查权限
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
            string pageUrl = Utils.CombUrlTxt("repay_manage.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&page={4}",
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
        private List<RepayOverTime> GetList()
        {
            PageSize = GetPageSize(GetType().Name + "_page_size");
            var query =
                context.li_repayment_tasks.Where(
                    r => r.status != (int)Agp2pEnums.RepaymentStatusEnum.Invalid && (r.li_projects.title.Contains(Keywords) || r.li_projects.no.Contains(Keywords)))
                    .OrderBy(r => r.should_repay_time).AsQueryable();

            if (CategoryId > 0)
                query = query.Where(q => q.li_projects.category_id == CategoryId);
            //待还
            if (rblStatus.SelectedValue == "0")
                query = query.Where(r => r.status < (int)Agp2pEnums.RepaymentStatusEnum.ManualPaid);
            //已还
            else if (rblStatus.SelectedValue == "1")
                query = query.Where(r => r.status >= (int)Agp2pEnums.RepaymentStatusEnum.ManualPaid);

            var repayList = query.AsEnumerable().Select(r => new RepayOverTime
            {
                Loaner = context.GetLonerName(r.project),
                Principal = r.repay_principal,
                Interest = r.repay_interest,
                TimeTerm =
                    r.li_projects.repayment_term_span == (int) Agp2pEnums.ProjectRepaymentTermSpanEnum.Day
                        ? "1/1"
                        : $"{r.term.ToString()}/{r.li_projects.repayment_term_span_count}",
                ShouldRepayTime = r.should_repay_time.ToString("yyyy-MM-dd HH:mm"),
                RepayTime = r.repay_at?.ToString("yyyy-MM-dd HH:mm") ?? "",
                Category = r.li_projects.category_id,
                ProfitRate = r.li_projects.profit_rate_year,
                RepaymentType =
                    Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTypeEnum) r.li_projects.repayment_type),
                ProjectId = r.li_projects.id,
                ProjectTitle = r.li_projects.title,
                ProjectStatus = r.li_projects.status,
                RepayStatus = r.status,
                RepayId = r.id
            }).AsQueryable();
            
            this.TotalCount = repayList.Count();
            return repayList.Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("repay_manage.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {            
            Response.Redirect(Utils.CombUrlTxt("repay_manage.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("repay_manage.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        class RepayOverTime
        {
            public int RepayId { get; set; }
            public int ProjectId { get; set; }
            public string ProjectTitle { get; set; }
            public string Loaner { get; set; }
            public decimal Principal { get; set; }//应还本金
            public decimal Interest { get; set; }//应还利息
            public string TimeTerm { get; set; }//还款期数
            public string ShouldRepayTime { get; set; }//应还时间
            public string RepayTime { get; set; }//实还时间
            public int Category { get; set; }//产品
            public decimal ProfitRate { get; set; }//年化利率
            public string RepaymentType { get; set; }//年化利率
            public int ProjectStatus { get; set; }//项目状态
            public int RepayStatus { get; set; }//还款状态
        }

        protected void rblStatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
        }

        /// <summary>
        /// 还款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbt_repay_OnClick(object sender, EventArgs e)
        {
            try
            {
                ChkAdminLevel("repay_manage", DTEnums.ActionEnum.Add.ToString());
                int repayId = Utils.StrToInt(((LinkButton) sender).CommandArgument, 0);
                //TODO 扣除借款人托管账户钱
                //根据时间判断是否提前还款
                var repay = context.li_repayment_tasks.SingleOrDefault(r => r.id == repayId);
                Debug.Assert(repay != null, "repay != null");
                if (repay.should_repay_time >= DateTime.Now)
                {
                    decimal cost = (decimal) Costconfig.earlier_pay;
                    context.EarlierRepayAll(repay.project, cost);
                }
                else
                    context.ExecuteRepaymentTask(repayId, Agp2pEnums.RepaymentStatusEnum.ManualPaid);

                JscriptMsg("还款成功！",
                    Utils.CombUrlTxt("repay_manage.aspx", "channel_id={0}&category_id={1}&status={2}",
                        this.ChannelId.ToString(), this.CategoryId.ToString(), this.ProjectStatus.ToString()));
            }
            catch (Exception)
            {
                JscriptMsg("还款失败！", "back", "Error");
            }
        }
    }
}