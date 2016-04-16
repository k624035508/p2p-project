using System.Data;
using System.Linq;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using Agp2p.Core;
using Agp2p.Core.AutoLogic;

namespace Agp2p.Web.UI
{
    public class ProjectDetail
    {
        public DateTime add_time { get; set; }
        public decimal amount { get; set; }
        public int category_id { get; set; }
        public int id { get; set; }
        public string img_url { get; set; }
        public string no { get; set; }
        public decimal profit_rate_year { get; set; }
        public string project_amount_str { get; set; }
        public string project_investment_balance { get; set; }
        public int project_investment_count { get; set; }
        public string project_investment_progress { get; set; }
        public DateTime? publish_time { get; set; }
        public int repayment_number { get; set; }
        public string repayment_term { get; set; }
        public string repayment_type { get; set; }
        public int? sort_id { get; set; }
        public int status { get; set; }
        public int tag { get; set; }
        public string title { get; set; }
        public string categoryTitle { get; set; }
        public string categoryCallIndex { get; set; }
        public string conversionBank { get; set; }
        public string linkurl { get; set; }
    }

    public partial class BasePage : System.Web.UI.Page
    {
        protected DataTable get_project_list(int top, int category_id, int profit_rate_index, int repayment_index, int status_index)
        {
            int total = 0;
            return get_project_list(top, 1, out total, category_id, profit_rate_index, repayment_index, status_index);
        }

        protected static DataTable get_project_list(int pageSize, int pageNum, out int total, int category_id, int profit_rate_index, int repayment_index, int status_index)
        {
            Model.siteconfig config = new BLL.siteconfig().loadConfig();
            var articleCategory = new Agp2pDataContext().dt_article_category.Single(ca => ca.title == "债权转让");
            var queryToNewObj = QueryInvestables(pageSize, pageNum - 1, out total, category_id, profit_rate_index, repayment_index, status_index).Select(inv =>
            {
                var p = inv.Project;
                return new ProjectDetail
                {
                    id = p.id,
                    img_url = GetProjectImageUrl(p.img_url, p.category_id),
                    no = p.no,
                    title = inv.Title,
                    status = (int) inv.Status,
                    sort_id = p.sort_id,
                    repayment_type = p.GetProjectRepaymentTypeDesc(),
                    repayment_term = p.GetProjectTermSpanEnumDesc(),
                    repayment_number = p.repayment_term_span_count,
                    profit_rate_year = p.profit_rate_year,
                    /*category_id = inv.CategoryId,*/
                    categoryTitle = inv.CategoryName,
                    categoryCallIndex = inv.CategoryCallIndex,
                    amount = p.financing_amount,
                    add_time = p.publish_time ?? p.add_time,
                    publish_time = p.publish_time,
                    tag = p.tag.GetValueOrDefault(),
                    //category_img = get_category_icon_by_categoryid(categoryList, p.category_id),//类别图标路径
                    //project_repayment = p.GetProjectTermSpanEnumDesc(),//项目还款期限单位
                    project_amount_str = p.financing_amount.ToString("n0"),//项目金额字符
                    project_investment_progress = inv.InvestmentProgress.ToString("p1").Split('%')[0],//项目进度
                    project_investment_balance = inv.InvestmentBalance.ToString("n0") + "元",//项目投资剩余金额
                    project_investment_count = inv.InvesterCount,//项目投资人数
                    linkurl = inv.Linkurl(config)
                };
            });

            var dt = queryToNewObj.ToDataTable(p => new object[] { queryToNewObj });
            return dt ?? new DataTable();
        }

        public static li_projects GetFirstNewbieProject()
        {
            var context = new Agp2pDataContext();
            return context.li_projects.OrderByDescending(p => p.id)
                .FirstOrDefault(
                    p =>
                        (int) Agp2pEnums.ProjectStatusEnum.Financing <= p.status &&
                        p.dt_article_category.call_index == "newbie");
        }

        public class Investable
        {
            public li_projects Project { get; set; }
            public li_claims NeedTransferClaim { get; set; }

            private Agp2pDataContext _context;

            private Agp2pDataContext Context => _context ?? (_context = new Agp2pDataContext());

            public bool IsClaimTransferProject => NeedTransferClaim != null;

            public string Title => NeedTransferClaim == null ? Project.title : NeedTransferClaim.number;

            public string CategoryName => NeedTransferClaim == null
                ? Project.dt_article_category.title
                : Context.dt_article_category.Single(ca => ca.call_index == "claims").title;

            public string CategoryCallIndex => NeedTransferClaim == null ? "claims" : Project.dt_article_category.call_index;

            public int CategoryId => NeedTransferClaim == null
                ? Context.dt_article_category.Single(ca => ca.call_index == "claims").id
                : Project.dt_article_category.id;

            public decimal InvestmentAmount => NeedTransferClaim == null
                ? Project.investment_amount
                : NeedTransferClaim.li_project_transactions_profiting.Where(
                    ptr =>
                        ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn &&
                        ptr.status != (int) Agp2pEnums.ProjectTransactionStatusEnum.Rollback)
                    .Aggregate(0m, (sum, tr) => sum + tr.principal);

            public decimal FinancingAmount => NeedTransferClaim == null
                ? Project.financing_amount
                : NeedTransferClaim.principal + NeedTransferClaim.keepInterest.GetValueOrDefault();

            public decimal InvestmentProgress => NeedTransferClaim == null
                ? Project.GetInvestmentProgress(
                    (investedAmount, financingAmount) => investedAmount/financingAmount)
                : (InvestmentAmount/FinancingAmount);

            public decimal ProfitRateYearly => NeedTransferClaim == null
                ? Project.profit_rate_year/100
                : FinancingAmount*
                  (Project.IsHuoqiProject()
                      ? TransactionFacade.HuoqiProjectProfitingDay
                      : TransactionFacade.NormalProjectProfitingDay)/NeedTransferClaim.principal/RemainDays;

            private int RemainDays
            {
                get
                {
                    var task = Project.li_repayment_tasks.Last(t => t.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid);
                    return (int) (task.should_repay_time.Date - NeedTransferClaim.createTime.Date).TotalDays;
                }
            }

            public decimal InvestmentBalance => NeedTransferClaim == null
                ? Project.financing_amount - Project.investment_amount
                : FinancingAmount - InvestmentAmount;

            public int InvesterCount => NeedTransferClaim == null
                ? Project.GetInvestedUserCount()
                : NeedTransferClaim.li_project_transactions_profiting.Where(
                    ptr =>
                        ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn &&
                        ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Pending)
                    .GroupBy(ptr => ptr.investor)
                    .Count();

            public Agp2pEnums.ProjectStatusEnum Status => NeedTransferClaim == null || Project.status == (int) Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime
                    ? (Agp2pEnums.ProjectStatusEnum) Project.status
                    : NeedTransferClaim.li_project_transactions_profiting.Any(
                        ptr => ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.ClaimTransferredIn &&
                               ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Rollback)
                        ? Agp2pEnums.ProjectStatusEnum.FinancingTimeout
                        : NeedTransferClaim.Children.Any()
                            ? (Agp2pEnums.ProjectStatusEnum) Project.status
                            : Agp2pEnums.ProjectStatusEnum.Financing;

            public DateTime StatusChangeTime => NeedTransferClaim == null
                ? Project.GetStatusChangingTime()
                : Status == Agp2pEnums.ProjectStatusEnum.Financing
                    ? NeedTransferClaim.createTime
                    : Project.GetStatusChangingTime();

            public string Linkurl(Model.siteconfig config)
            {
                return NeedTransferClaim == null
                    ? linkurl(config, "project", Project.id)
                    : linkurl(config, "project", Project.id, NeedTransferClaim.id);
            }
        }

        public static IQueryable<li_projects> QueryingProjects(Agp2pDataContext context, int categoryId = 0, int profitRateIndex = 0, int repaymentIndex = 0, int statusIndex = 0)
        {
            //查出所以项目类别
            //var categoryList = get_category_list(channel_name, 0);

            var query = context.li_projects.Where(p => (int)Agp2pEnums.ProjectStatusEnum.FinancingAtTime <= p.status);
            if (categoryId != 0)
            {
                var category = context.dt_article_category.Single(ca => ca.id == categoryId);
                if (category.parent_id.GetValueOrDefault() == 0) // 大类
                    query = query.Where(p => p.dt_article_category.parent_id == categoryId || p.category_id == categoryId);
                else
                    query = query.Where(p => p.category_id == categoryId);
            }

            //项目筛选暂写死逻辑在此
            if (0 < profitRateIndex)//年化利率条件
            {
                switch ((Agp2pEnums.InterestRateTypeEnum)profitRateIndex)
                {
                    case Agp2pEnums.InterestRateTypeEnum.LessThanSix:
                        query = query.Where(p => p.profit_rate_year < 6);
                        break;
                    case Agp2pEnums.InterestRateTypeEnum.SixToTen:
                        query = query.Where(p => 6 <= p.profit_rate_year && p.profit_rate_year < 10);
                        break;
                    case Agp2pEnums.InterestRateTypeEnum.TenToFifteen:
                        query = query.Where(p => 10 <= p.profit_rate_year && p.profit_rate_year <= 15);
                        break;
                }
            }

            if (0 < repaymentIndex)//借款期限条件
            {
                switch ((Agp2pEnums.RepaymentTermEnum)repaymentIndex)
                {
                    case Agp2pEnums.RepaymentTermEnum.LessThanOneMonth:
                        query = query.Where(p => p.repayment_term_span_count < 30 && p.repayment_term_span == (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Day);
                        break;
                    case Agp2pEnums.RepaymentTermEnum.OneToThreeMonth:
                        query = query.Where(p => p.repayment_term_span_count <= 1 && p.repayment_term_span_count < 3 && p.repayment_term_span == (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Month
                            || 30 <= p.repayment_term_span_count && p.repayment_term_span_count < 30 * 3 && p.repayment_term_span == (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Day);
                        break;
                    case Agp2pEnums.RepaymentTermEnum.ThreeToSixMonth:
                        query = query.Where(p => p.repayment_term_span_count <= 3 && p.repayment_term_span_count <= 6 && p.repayment_term_span == (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Month
                            || 30 * 3 <= p.repayment_term_span_count && p.repayment_term_span_count <= 30 * 6 && p.repayment_term_span == (int)Agp2pEnums.ProjectRepaymentTermSpanEnum.Day);
                        break;
                }
            }

            if (0 < statusIndex)// 状态条件
            {
                switch ((Agp2pEnums.ProjectStatusQueryTypeEnum)statusIndex)
                {
                    case Agp2pEnums.ProjectStatusQueryTypeEnum.Financing:
                        query = query.Where(p => (int)Agp2pEnums.ProjectStatusEnum.Financing <= p.status && p.status < (int)Agp2pEnums.ProjectStatusEnum.FinancingSuccess);
                        break;
                    case Agp2pEnums.ProjectStatusQueryTypeEnum.FinancingSuccess:
                        query = query.Where(p => p.status == (int)Agp2pEnums.ProjectStatusEnum.FinancingSuccess);
                        break;
                    case Agp2pEnums.ProjectStatusQueryTypeEnum.ProjectRepaying:
                        query = query.Where(p => (int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying <= p.status && p.status < (int)Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime);
                        break;
                    case Agp2pEnums.ProjectStatusQueryTypeEnum.ProjectRepayComplete:
                        query = query.Where(p => (int)Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime <= p.status);
                        break;
                }
            }
            return query.OrderBy(q => q.status)
                .ThenByDescending(q => q.sort_id)
                .ThenByDescending(q => q.add_time)
                .ThenByDescending(q => q.id);
        }

        public static IEnumerable<Investable> QueryInvestables(int pageSize, int pageIndex, out int total,
            int categoryId = 0, int profitRateIndex = 0, int repaymentIndex = 0, int statusIndex = 0)
        {
            var context = new Agp2pDataContext();
            var projectQuerying = QueryingProjects(context, categoryId, profitRateIndex, repaymentIndex, statusIndex);

            var claimsCategory = context.dt_article_category.Single(ca => ca.call_index == "claims");

            // 普通用户不能买公司账号转出的债权
            var claimQuerying = categoryId != 0 && categoryId != claimsCategory.id
                ? Enumerable.Empty<li_claims>().AsQueryable()
                : context.li_claims.Where(c =>
                    c.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                    c.Parent.dt_users.dt_user_groups.title != AutoRepay.CompanyAccount)
                    .OrderByDescending(c => c.createTime);

            int projectTotalCount = 0, claimTotalCount = 0;
            var investables = projectQuerying.AsEnumerableAutoPartialQuery(out projectTotalCount, pageSize).Select(p => new Investable {Project = p})
                .Concat(
                    claimQuerying.AsEnumerableAutoPartialQuery(out claimTotalCount, pageSize)
                        .Select(c => new Investable {NeedTransferClaim = c, Project = c.li_projects}));

            total = projectTotalCount + claimTotalCount;

            var queryToNewObj =
                investables.OrderBy(inv => inv.Status)
                    .ThenByDescending(inv => inv.StatusChangeTime)
                    .Skip(pageSize*pageIndex)
                    .Take(pageSize);
            return queryToNewObj;
        }

        protected static string GetProjectImageUrl(string url, int category_id)
        {
            if (string.IsNullOrEmpty(url))
            {
                return category_id == 33 ? "/templates/Agp2p/images/img_noting_house.png" :
                    "/templates/Agp2p/images/img_noting_car.png";
            }
            return url;
        }

        /// <summary>
        /// 获取项目进度参数
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        protected static ProjectInvestmentProgress GetProjectInvestmentProgress(li_projects pro)
        {
            return pro.GetInvestmentProgress((total, projectAmount) => new ProjectInvestmentProgress{ total = total, projectAmount = projectAmount });
        }

        public class ProjectInvestmentProgress
        {
            public decimal total;
            public decimal projectAmount;

            public string GetInvestmentProgress()
            {
                return (total / projectAmount).ToString("P1").Split('%')[0];
            }

            public string GetInvestmentBalance()
            {
                return (projectAmount - total).ToString("N0") + "元";
            }
        }

        /// <summary>
        /// 获取年化利率筛选枚举
        /// </summary>
        /// <returns></returns>
        protected DataTable get_project_rate_enum()
        {
            DataTable dt = new DataTable();            
            dt.Columns.Add("key", Type.GetType("System.String"));
            dt.Columns.Add("value", Type.GetType("System.String"));
            dt.Columns.Add("text", Type.GetType("System.String"));         

            foreach (Agp2pEnums.InterestRateTypeEnum e in Enum.GetValues(typeof(Agp2pEnums.InterestRateTypeEnum)))
            {
                DataRow dr = dt.NewRow();
                dr["key"] = e.ToString();
                dr["value"] = (int)e;
                dr["text"] = Utils.GetAgp2pEnumDes(e);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 获取还款期限筛选枚举
        /// </summary>
        /// <returns></returns>
        protected DataTable get_project_term_enum()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("key", Type.GetType("System.String"));
            dt.Columns.Add("value", Type.GetType("System.String"));
            dt.Columns.Add("text", Type.GetType("System.String"));

            foreach (Agp2pEnums.RepaymentTermEnum e in Enum.GetValues(typeof(Agp2pEnums.RepaymentTermEnum)))
            {
                DataRow dr = dt.NewRow();
                dr["key"] = e.ToString();
                dr["value"] = (int)e;
                dr["text"] = Utils.GetAgp2pEnumDes(e);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 获取金额筛选枚举
        /// </summary>
        /// <returns></returns>
        protected DataTable get_project_amount_enum()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("key", Type.GetType("System.String"));
            dt.Columns.Add("value", Type.GetType("System.String"));
            dt.Columns.Add("text", Type.GetType("System.String"));

            foreach (Agp2pEnums.AmountTypeEnum e in Enum.GetValues(typeof(Agp2pEnums.AmountTypeEnum)))
            {
                DataRow dr = dt.NewRow();
                dr["key"] = e.ToString();
                dr["value"] = (int)e;
                dr["text"] = Utils.GetAgp2pEnumDes(e);
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
    
}
