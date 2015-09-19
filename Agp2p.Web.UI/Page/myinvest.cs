using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using Agp2p.Common;
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
                    Agp2pEnums.MyInvestRadioBtnTypeEnum.Repaying, new[] { Agp2pEnums.ProjectStatusEnum.ProjectRepaying,
                        Agp2pEnums.ProjectStatusEnum.NotRepayCompleteIntime, Agp2pEnums.ProjectStatusEnum.BadDebt }
                },
                {
                    Agp2pEnums.MyInvestRadioBtnTypeEnum.RepayComplete, new[] { Agp2pEnums.ProjectStatusEnum.RepayCompleteEarlier,
                        Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime, Agp2pEnums.ProjectStatusEnum.RepayCompleteDelay,
                        Agp2pEnums.ProjectStatusEnum.AdvancePayForFinancer, Agp2pEnums.ProjectStatusEnum.BadDebtRepayComplete }
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
        protected static List<li_project_transactions> query_investment(int userId, Agp2pEnums.MyInvestRadioBtnTypeEnum type,
            int pageIndex, long startTick, long endTick, short pageSize, out int count)
        {
            var context = new Agp2pDataContext();
            var query =
                context.li_project_transactions.Where(
                    tr =>
                        tr.investor == userId && tr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        tr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success);
            if (MyTradeTypeMapHistoryEnum.Keys.Contains(type))
                query = query.Where(tr => MyTradeTypeMapHistoryEnum[type].Cast<int>().Contains(tr.li_projects.status));
            if (startTick != 0)
                query = query.Where(tr => new DateTime(startTick) <= tr.create_time);
            if (endTick != 0)
                query = query.Where(tr => tr.create_time <= new DateTime(endTick));

            count = query.Count();
            return query.OrderByDescending(h => h.id).Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

        [WebMethod]
        public static string AjaxQueryInvestment(byte type, short pageIndex, short pageSize)
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            int count;
            var ptrs = query_investment(userInfo.id, (Agp2pEnums.MyInvestRadioBtnTypeEnum) type, pageIndex, 0, 0, pageSize, out count);
            var result = ptrs.Select(ptr => new
            {
                projectName = ptr.li_projects.title,
                investTime = ptr.create_time.ToString("yyyy-MM-dd HH:mm"),
                investValue = ptr.value.ToString("c")
            });
            return JsonConvert.SerializeObject(result);
        }
    }
}
