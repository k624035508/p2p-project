using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using System.Web;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using System.Data.Linq;
using System.Net;
using System.Web.Services;
using Lip2p.BLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Lip2p.Web.UI.Page
{
    public partial class myreceiveplan : usercenter
    {
        protected static readonly short PageSize = 8;
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

        protected class MyInvestProject
        {
            public string Name { get; set; }
            public int ProfitRateYear { get; set; }
            public decimal InvestValue { get; set; }
        }

        protected class MyRepayment
        {
            public MyInvestProject Project { get; set; }
            public string ShouldRepayDay { get; set; }
            public decimal RepayPrincipal { get; set; }
            public decimal RepayInterest { get; set; }
            public string Term { get; set; }
        }

        protected List<MyRepayment> query_investment(int userId, Lip2pEnums.RepaymentStatusEnum type, int pageIndex, long startTick, long endTick, short pageSize, out int count)
        {
            return QueryMyRepayments(userId, type, startTick, endTick,out count).Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

        /// <summary>
        /// 查询新手标、天标与普通标的回款记录
        /// </summary>
        /// <param name="userId">当前用户</param>
        /// <param name="type">还款类型</param>
        /// <param name="startTick">回款开始日期</param>
        /// <param name="endTick">回款结束日期</param>
        /// <param name="count">总数量</param>
        /// <returns></returns>
        protected static List<MyRepayment> QueryMyRepayments(int userId, Lip2pEnums.RepaymentStatusEnum type, long startTick, long endTick, out int count)
        {
            var context = new Lip2pDataContext();

            //查询新手标和天标的回款记录
            var trialsAndDaily = QueryTrialsAndDailyRepayments(context, userId, type, startTick, endTick);
            var investedProject = QueryProjectRepayments(context, userId, type, startTick, endTick);

            List<MyRepayment> repayments = trialsAndDaily.Concat(investedProject).ToList();
            count = repayments.Count();
            return repayments;
        }

        private static List<MyRepayment> QueryTrialsAndDailyRepayments(Lip2pDataContext context, int userId, Lip2pEnums.RepaymentStatusEnum type, long startTick, long endTick)
        {
            IEnumerable<li_activity_transactions> query = context.li_activity_transactions.Where(
                a =>
                    a.user_id == userId && (a.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.Trial ||
                                            a.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject) &&
                    a.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm);

            query = (type == Lip2pEnums.RepaymentStatusEnum.Unpaid
                ? query.Where(w => w.transact_time == null)
                : query.Where(w => w.transact_time != null)).AsEnumerable();

            if (startTick != 0)
            {
                query = query.Where(tr => new DateTime(startTick) <= ((JObject)JsonConvert.DeserializeObject(tr.details)).Value<DateTime>("RepayTime").Date);
            }
            if (endTick != 0)
            {
                query = query.Where(tr => ((JObject)JsonConvert.DeserializeObject(tr.details)).Value<DateTime>("RepayTime").Date <= new DateTime(endTick));
            }

            var trialsAndDaily = query.Select(atr =>
            {
                var projectId = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<int>("ProjectId");
                var project = context.li_projects.Single(a => a.id == projectId);
                var mr = new MyRepayment
                {
                    Project = null,
                    RepayInterest = atr.value,
                    RepayPrincipal = 0,
                    ShouldRepayDay = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<string>("RepayTime"),
                    Term = "1/1", //project.repayment_term_span_count,
                };

                mr.Project = new MyInvestProject
                {
                    Name = project.title,
                    InvestValue = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value"),
                    ProfitRateYear = project.profit_rate_year
                };
                return mr;
            }).ToList();
            return trialsAndDaily;
        }

        /// <summary>
        /// 查询普通项目的回款记录
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
        private static List<MyRepayment> QueryProjectRepayments(Lip2pDataContext context, int userId, Lip2pEnums.RepaymentStatusEnum type, long startTick, long endTick)
        {
            var investedProjectValueMap = context.li_project_transactions.Where(
                tr =>
                    tr.investor == userId &&
                    tr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                    tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                .GroupBy(inv => inv.li_projects)
                .ToDictionary(g => g.Key, g => g.Sum(tr => tr.value));

            return investedProjectValueMap.SelectMany(p =>
            {
                var ratio1 = investedProjectValueMap[p.Key]/p.Key.investment_amount;
                IEnumerable<li_repayment_tasks> query1 = p.Key.li_repayment_tasks;

                if (startTick != 0)
                {
                    query1 = query1.Where(tr => new DateTime(startTick) <= tr.should_repay_time);
                }
                if (endTick != 0)
                {
                    query1 = query1.Where(tr => tr.should_repay_time <= new DateTime(endTick));
                }

                var reps1 =
                    query1.Where(tr =>
                        type != Lip2pEnums.RepaymentStatusEnum.Unpaid
                            ? (int) Lip2pEnums.RepaymentStatusEnum.ManualPaid <= tr.status
                            : (int) type == tr.status).Select(task => new MyRepayment
                            {
                                Project = null,
                                RepayInterest = task.repay_interest*ratio1,
                                RepayPrincipal = task.repay_principal*ratio1,
                                ShouldRepayDay = task.should_repay_time.ToString(),
                                Term = task.term.ToString() + "/" + p.Key.repayment_term_span_count.ToString(),
                            }).ToList();

                if (!reps1.Any())
                {
                    return Enumerable.Empty<MyRepayment>();
                }
                reps1.First().Project = new MyInvestProject
                {
                    Name = p.Key.title,
                    InvestValue = investedProjectValueMap[p.Key],
                    ProfitRateYear = p.Key.profit_rate_year
                };

                return reps1;
            }).ToList();
        }

        /// <summary>
        /// 查询用户已回款与待回款的笔数
        /// </summary>
        /// <param name="status">回款状态</param>
        /// <returns></returns>
        protected int QueryMyRepaymentsCount(Lip2pEnums.RepaymentStatusEnum status)
        {
            var context = new Lip2pDataContext();
            //普通标回款数量
            var tasks = context.li_project_transactions.Where(
                tr =>
                    tr.investor == userModel.id &&
                    tr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                    tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                .AsEnumerable().GroupBy(inv => inv.li_projects)
                .Select(g => g.Key)
                .SelectMany(
                    p =>
                        p.li_repayment_tasks.Where(
                            rt =>
                                status == Lip2pEnums.RepaymentStatusEnum.Unpaid
                                    ? rt.status == (int) status
                                    : (int) Lip2pEnums.RepaymentStatusEnum.ManualPaid <= rt.status));


            //新手标+天标回款记录总数
            var query = context.li_activity_transactions.Where(
               a =>
                   a.user_id == userModel.id && 
                  (a.activity_type == (int)Lip2pEnums.ActivityTransactionActivityTypeEnum.Trial ||
                   a.activity_type == (int)Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject) && 
                   a.status == (int)Lip2pEnums.ActivityTransactionStatusEnum.Confirm);

            query = status == Lip2pEnums.RepaymentStatusEnum.Unpaid ? query.Where(w => w.transact_time == null) : query.Where(w => w.transact_time != null);

            //普通标+新手标+天标回款记录总数
            return tasks.Count()+query.Count();
        }

        /// <summary>
        /// 查询用户总投资项目数量
        /// </summary>
        /// <returns></returns>
        protected int QueryTotalProject()
        {
            var context = new Lip2pDataContext();
            var totalProject =
                context.li_project_transactions.Where(
                    tr =>
                        tr.investor == userModel.id && tr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                        tr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                    .Select(r => r.project)
                    .Distinct()
                    .Count();
            var query = context.li_activity_transactions.Where(
                   w =>
                   w.user_id == userModel.id &&
                  (w.activity_type == (int)Lip2pEnums.ActivityTransactionActivityTypeEnum.Trial ||
                   w.activity_type == (int)Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject) &&
                   w.status == (int)Lip2pEnums.ActivityTransactionStatusEnum.Confirm); // TODO 有 bug，应该判断是不是同一个项目
            return totalProject + query.Count();
        }

        [WebMethod]
        public static string AjaxQueryInvestedProject(short projectStatus, short pageIndex, short pageSize)
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            var context = new Lip2pDataContext();
            // 查出用户用过的 新手券/天标券
            var usedTickets = context.li_activity_transactions.Where(atr =>
                atr.user_id == userInfo.id &&
                atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Confirm &&
                (atr.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.Trial ||
                 atr.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject))
                .Where(atr =>
                    projectStatus == (int) Lip2pEnums.ProjectStatusEnum.WanCheng
                        ? atr.transact_time != null
                        : atr.transact_time == null)
                .ToDictionary(atr => atr.li_wallet_histories.First().create_time, atr =>
                    context.li_projects.Single(
                        p => p.id == ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<int>("ProjectId")));

            // 查出投资过的项目
            var investedProjects = context.li_project_transactions.Where(ptr =>
                    ptr.investor == userInfo.id && ptr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                    ptr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                    .Where(ptr => ptr.li_projects.status == projectStatus ||
                        projectStatus == (int) Lip2pEnums.ProjectStatusEnum.ManBiao && ptr.li_projects.status == (int) Lip2pEnums.ProjectStatusEnum.JieBiao) // 查满标时包括截标
                .GroupBy(ptr => ptr.li_projects).ToDictionary(g => g.Last().create_time, g => g.Key);

            var result = usedTickets.Concat(investedProjects)
                    .OrderByDescending(p => p.Key)
                    .Skip(pageSize*pageIndex)
                    .Take(pageSize)
                    .Select(p =>
                    {
                        decimal investedValue;
                        int? ticketId = null;
                        if (investedProjects.ContainsKey(p.Key))
                        {
                            investedValue = QueryInvestAmount(investedProjects[p.Key], userInfo.id);
                        }
                        else
                        {
                            var atr = context.li_wallet_histories.Single(h => h.create_time == p.Key && h.user_id == userInfo.id).li_activity_transactions;
                            ticketId = atr.id;
                            investedValue = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value");
                        }
                        return new
                        {
                            InvestTime = p.Key.ToString("yyyy-MM-dd HH:mm"),
                            ProjectTitle = p.Value.title,
                            InvestValue = investedValue.ToString("c"),
                            ProjectId = p.Value.id,
                            TicketId = ticketId
                        };
                    });

            return JsonConvert.SerializeObject(result);
        }

        private static decimal QueryInvestAmount(li_projects proj, int userId)
        {
            return proj.li_project_transactions.Where(
                ptr =>
                    ptr.investor == userId &&
                    ptr.type == (int) Lip2pEnums.ProjectTransactionTypeEnum.Invest &&
                    ptr.status == (int) Lip2pEnums.ProjectTransactionStatusEnum.Success)
                .Sum(ptr => ptr.value);
        }

        [WebMethod]
        public static string AjaxQueryProjectRepaymentDetail(short projectId, short? ticketId)
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            var context = new Lip2pDataContext();
            if (ticketId == null)
            {
                var project = context.li_projects.Single(p => p.id == projectId);
                var investAmount = QueryInvestAmount(project, userInfo.id);
                var investRatio = investAmount/project.financing_amount;
                var result = new
                {
                    Title = project.title,
                    ProfitingAmount = (int) Lip2pEnums.ProjectStatusEnum.FaBiao < project.status
                        ? project.li_repayment_tasks.Sum(ta => Math.Round(investRatio * ta.repay_principal, 2) + Math.Round(investRatio * ta.repay_interest, 2)).ToString("c") // 模拟放款累计
                        : "(未满标)",
                    ProfitRateYear = project.profit_rate_year,
                    InvestedValue = investAmount.ToString("c"),
                    TermsData = project.li_repayment_tasks.Select(ta => new
                    {
                        RepayInterest = Math.Round(investRatio * ta.repay_interest, 2).ToString("c"),
                        RepayPrincipal = Math.Round(investRatio * ta.repay_principal, 2).ToString("c"),
                        ShouldRepayTime = ta.should_repay_time.ToString("yyyy-MM-dd"),
                        RepayTotal = (Math.Round(investRatio * ta.repay_interest, 2) + Math.Round(investRatio * ta.repay_principal, 2)).ToString("c")
                    })
                };
                return JsonConvert.SerializeObject(result);
            }
            else
            {
                var project = context.li_projects.Single(p => p.id == projectId);
                var atr = context.li_activity_transactions.Single(tr => tr.id == ticketId);
                var result = new
                {
                    Title = project.title,
                    ProfitingAmount = atr.value.ToString("c"),
                    ProfitRateYear = project.profit_rate_year,
                    InvestedValue = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value").ToString("c"),
                    TermsData = new[]
                    {
                        new
                        {
                            RepayInterest = atr.value.ToString("c"),
                            RepayPrincipal = 0.ToString("c"),
                            ShouldRepayTime = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<DateTime>("RepayTime").ToString("yyyy-MM-dd"),
                            RepayTotal = atr.value.ToString("c")
                        }
                    }
                };
                return JsonConvert.SerializeObject(result);
            }
        }
    }
}
