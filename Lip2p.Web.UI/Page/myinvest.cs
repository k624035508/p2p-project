using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using Newtonsoft.Json;

namespace Lip2p.Web.UI.Page
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

        private static readonly Dictionary<Lip2pEnums.MyInvestRadioBtnTypeEnum, Lip2pEnums.ProjectStatusEnum[]>
            MyTradeTypeMapHistoryEnum = new Dictionary<Lip2pEnums.MyInvestRadioBtnTypeEnum, Lip2pEnums.ProjectStatusEnum[]>
            {
                {
                    Lip2pEnums.MyInvestRadioBtnTypeEnum.Investing, new[] { Lip2pEnums.ProjectStatusEnum.Financing }
                },
                {
                    Lip2pEnums.MyInvestRadioBtnTypeEnum.InvestEndding, new[] { Lip2pEnums.ProjectStatusEnum.FinancingSuccess, Lip2pEnums.ProjectStatusEnum.FinancingTimeout }
                },
                {
                    Lip2pEnums.MyInvestRadioBtnTypeEnum.Repaying, new[] { Lip2pEnums.ProjectStatusEnum.ProjectRepaying,
                        Lip2pEnums.ProjectStatusEnum.NotRepayCompleteIntime, Lip2pEnums.ProjectStatusEnum.BadDebt }
                },
                {
                    Lip2pEnums.MyInvestRadioBtnTypeEnum.RepayComplete, new[] { Lip2pEnums.ProjectStatusEnum.RepayCompleteEarlier,
                        Lip2pEnums.ProjectStatusEnum.RepayCompleteIntime, Lip2pEnums.ProjectStatusEnum.RepayCompleteDelay,
                        Lip2pEnums.ProjectStatusEnum.AdvancePayForFinancer, Lip2pEnums.ProjectStatusEnum.BadDebtRepayComplete }
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
        protected static List<li_project_transactions> query_investment(int userId, Lip2pEnums.MyInvestRadioBtnTypeEnum type,
            int pageIndex, long startTick, long endTick, short pageSize, out int count)
        {
            var context = new Lip2pDataContext();
            var query =
                context.li_project_transactions.Where(
                    tr =>
                        tr.investor == userId && tr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                        tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success);
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
            var ptrs = query_investment(userInfo.id, (Lip2pEnums.MyInvestRadioBtnTypeEnum) type, pageIndex, 0, 0, pageSize, out count);
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
