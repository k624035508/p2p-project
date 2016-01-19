using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.UI.Page
{
    public partial class myinvest : usercenter
    {
        static readonly protected short PageSize = 8;
        protected int investment_type;
        protected int page;
        protected long time_span_start;
        protected long time_span_end;

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
            investment_type = DTRequest.GetQueryInt("investment_type");
            page = Math.Max(1, DTRequest.GetQueryInt("page"));
            time_span_start = DTRequest.GetQueryLong("time_span_start");
            time_span_end = DTRequest.GetQueryLong("time_span_end");
        }

        private static readonly Dictionary<Agp2pEnums.MyInvestRadioBtnTypeEnum, Agp2pEnums.ProjectStatusEnum[]>
            MyTradeTypeMapHistoryEnum = new Dictionary<Agp2pEnums.MyInvestRadioBtnTypeEnum, Agp2pEnums.ProjectStatusEnum[]>
            {
                {
                    Agp2pEnums.MyInvestRadioBtnTypeEnum.Investing, new[] { Agp2pEnums.ProjectStatusEnum.Financing }
                },
                {
                    Agp2pEnums.MyInvestRadioBtnTypeEnum.InvestEndding, new[] { Agp2pEnums.ProjectStatusEnum.FinancingSuccess, Agp2pEnums.ProjectStatusEnum.FinancingTimeout }
                },
                {
                    Agp2pEnums.MyInvestRadioBtnTypeEnum.Repaying, new[] { Agp2pEnums.ProjectStatusEnum.ProjectRepaying}
                },
                {
                    Agp2pEnums.MyInvestRadioBtnTypeEnum.RepayComplete, new[] { Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime, Agp2pEnums.ProjectStatusEnum.BadDebt }
                },
            };

        /// <summary>
        /// 查询投资列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
        protected static List<li_project_transactions> QueryInvestment(int userId, Agp2pEnums.MyInvestRadioBtnTypeEnum type,
            int pageIndex, string startTime, string endTime, short pageSize, out int count)
        {
            var context = new Agp2pDataContext();
            var query =
                context.li_project_transactions.Where(
                    tr =>
                        tr.investor == userId && tr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        tr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success);
            if (MyTradeTypeMapHistoryEnum.Keys.Contains(type))
                query = query.Where(tr => MyTradeTypeMapHistoryEnum[type].Cast<int>().Contains(tr.li_projects.status));
            if (!string.IsNullOrWhiteSpace(startTime))
                query = query.Where(tr => Convert.ToDateTime(startTime) <= tr.create_time);
            if (!string.IsNullOrWhiteSpace(endTime))
                query = query.Where(tr => tr.create_time <= Convert.ToDateTime(endTime));

            count = query.Count();
            return query.OrderByDescending(h => h.id).Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

        [WebMethod]
        public new static string AjaxQueryInvestment(short type, short pageIndex, short pageSize, string startTime = "", string endTime = "")
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            int count;
            var ptrs = QueryInvestment(userInfo.id, (Agp2pEnums.MyInvestRadioBtnTypeEnum) type, pageIndex, startTime, endTime, pageSize, out count);

            var now = DateTime.Now;
            Model.siteconfig config = new BLL.siteconfig().loadConfig();
            var result = ptrs.Select(ptr =>
            {
                var proj = ptr.li_projects;
                var profit = proj.dt_article_category.call_index == "newbie"
                    ? 10
                    : proj.profit_rate == 0
                        ? Math.Round(ptr.principal*proj.GetFinalProfitRate(now), 2)
                        : Math.Round(proj.profit_rate*ptr.principal, 2);
                return new
                {
                    ptrId = ptr.id,
                    projectId = proj.id,
                    projectUrl = linkurl(config, "project", proj.id),
                    projectName = proj.title,
                    projectProfitRateYearly = proj.GetProfitRateYearly(),
                    term = proj.repayment_term_span_count + proj.GetProjectTermSpanEnumDesc(),
                    investTime = ptr.create_time.ToString("yy/MM/dd HH:mm"),
                    investValue = ptr.principal,
                    profit,
                    status = proj.GetProjectStatusDesc(),
                    isNewbieProject = proj.dt_article_category.call_index == "newbie"
                };
            });
            return JsonConvert.SerializeObject(new {totalCount = count, data = result});
        }
    }
}
