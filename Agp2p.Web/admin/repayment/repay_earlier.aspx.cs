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

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_over_time", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //绑定类别
                RptBind();
            }
        }

        protected void TreeBind()
        {
            BLL.article_category bll = new BLL.article_category();
            DataTable dt = bll.GetList(0, this.ChannelId);

            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            foreach (DataRow dr in dt.Rows)
            {
                string Id = dr["id"].ToString();
                int ClassLayer = int.Parse(dr["class_layer"].ToString());
                string Title = dr["title"].ToString().Trim();

                if (ClassLayer == 1)
                {
                    this.ddlCategoryId.Items.Add(new ListItem(Title, Id));
                }
                else
                {
                    Title = "├ " + Title;
                    Title = Utils.StringOfChar(ClassLayer - 1, "　") + Title;
                    this.ddlCategoryId.Items.Add(new ListItem(Title, Id));
                }
            }
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
            string pageUrl = Utils.CombUrlTxt("loan_over_time.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&page={4}",
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
            PageSize = new BLL.channel().GetPageSize(ChannelName);
            var query =
                context.li_repayment_tasks.Where(
                    r =>
                        r.status == (int)Agp2pEnums.RepaymentStatusEnum.EarlierPaid &&
                        (r.li_projects.title.Contains(Keywords) || r.li_projects.no.Contains(Keywords)));

            if (CategoryId > 0)
                query = query.Where(q => q.li_projects.category_id == CategoryId);

            var repayList = query.AsEnumerable().Select(r =>
            {
                var repay = new RepayEarlier();
                var loaner = r.li_projects.li_risks.li_loaners.dt_users;
                repay.Loaner = $"{loaner.real_name}({loaner.user_name})";
                //计算原来的还款计划应还总额
                var repayInvalid =
                    r.li_projects.li_repayment_tasks.Where(t => t.status == (int) Agp2pEnums.RepaymentStatusEnum.Invalid).OrderBy(t => t.should_repay_time);//作废的计划
                repay.Amount = repayInvalid.Sum(t => t.repay_interest + t.repay_principal);//应还款
                repay.FactAmount = r.repay_interest + r.repay_principal;//实还款
                repay.DayCount = r.should_repay_time.DayOfYear - repayInvalid.Last().should_repay_time.DayOfYear;//提前天数
                repay.ShouldRepayTime = r.should_repay_time.ToString("yyyy-MM-dd hh:mm");//应还时间
                repay.RepayTime = r.repay_at?.ToString("yyyy-MM-dd hh:mm") ?? "";//实还时间
                //TODO 逾期罚息计算
                repay.Cost = 0;

                repay.Category = r.li_projects.category_id;
                repay.ProfitRate = r.li_projects.profit_rate_year;
                repay.RepaymentType =
                    Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTypeEnum)r.li_projects.repayment_type);

                repay.ProjectId = r.li_projects.id;
                repay.ProjectTitle = r.li_projects.title;

                return repay;
            });

            this.TotalCount = repayList.Count();
            return repayList.OrderBy(q => q.ShouldRepayTime).Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("loan_over_time.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {            
            Response.Redirect(Utils.CombUrlTxt("loan_over_time.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                this.ChannelId.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie("article_page_size", _pagesize.ToString(), 43200);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("loan_over_time.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
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
            public int ProfitRate { get; set; }//年化利率
            public string RepaymentType { get; set; }//年化利率
        }
    }
}