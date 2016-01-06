using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using System.Web;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Data.Linq;
using System.Net;
using System.Web.Services;
using Agp2p.BLL;
using Agp2p.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Agp2p.Web.UI.Page
{
    public partial class myreceiveplan : usercenter
    {

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
        }

        public class MyInvestProject
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Link { get; set; }
            public string ProfitRateYear { get; set; }
            public decimal InvestValue { get; set; }
        }

        public class MyRepayment
        {
            public int RepaymentId { get; set; }
            public MyInvestProject Project { get; set; }
            public string ShouldRepayDay { get; set; }
            public decimal RepayPrincipal { get; set; }
            public decimal RepayInterest { get; set; }
            public string Term { get; set; }
        }

        /// <summary>
        /// 查询普通项目的回款记录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="type"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static List<MyRepayment> QueryProjectRepayments(dt_users user, Agp2pEnums.MyRepaymentQueryTypeEnum type, string startTime = "", string endTime = "")
        {
            var investedProjectValueMap = user.li_project_transactions.Where(
                tr =>
                    tr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    tr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .GroupBy(inv => inv.li_projects)
                .ToDictionary(g => g.Key, g => g.Sum(tr => tr.principal));

            Model.siteconfig config = new BLL.siteconfig().loadConfig();
            return investedProjectValueMap.SelectMany(p =>
            {
                var ratio = TransactionFacade.GetInvestRatio(p.Key)[user];
                var query = p.Key.li_repayment_tasks.Where(t => t.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid)
                    .Where(task => p.Key.dt_article_category.call_index != "newbie" || task.only_repay_to == user.id);

                if (!string.IsNullOrWhiteSpace(startTime))
                {
                    query = query.Where(tr => Convert.ToDateTime(startTime) <= tr.should_repay_time);
                }
                if (!string.IsNullOrWhiteSpace(endTime))
                {
                    query = query.Where(tr => tr.should_repay_time <= Convert.ToDateTime(endTime));
                }

                var validRepaymentTaskCount = query.Count();

                var reps1 =
                    query.Where(tr =>
                        type == Agp2pEnums.MyRepaymentQueryTypeEnum.Unpaid
                            ? tr.status < (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid
                            : (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid <= tr.status)
                            .Select(task => new MyRepayment
                            {
                                RepaymentId = task.id,
                                Project = null,
                                RepayInterest = Math.Round(task.repay_interest*ratio, 2),
                                RepayPrincipal = Math.Round(task.repay_principal*ratio, 2),
                                ShouldRepayDay = task.should_repay_time.ToString("yyyy/MM/dd"),
                                Term = task.term.ToString() + "/" + validRepaymentTaskCount
                            }).ToList();

                if (!reps1.Any())
                {
                    return Enumerable.Empty<MyRepayment>();
                }
                reps1.First().Project = new MyInvestProject
                {
                    Id = p.Key.id,
                    Name = p.Key.title,
                    Link = linkurl(config, "project", p.Key.id),
                    InvestValue = investedProjectValueMap[p.Key],
                    ProfitRateYear = p.Key.GetProfitRateYearly()
                };

                return reps1;
            }).ToList();
        }

        private static decimal QueryInvestAmount(li_projects proj, int userId)
        {
            return proj.li_project_transactions.Where(
                ptr =>
                    ptr.investor == userId &&
                    ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    ptr.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .Sum(ptr => ptr.principal);
        }

        [WebMethod]
        public static string AjaxQueryInvestedProject(bool projectFinish, short pageIndex, short pageSize) // 微信端用到到此 api
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            var context = new Agp2pDataContext();

            // 查出投资过的项目
            var investedProjects = context.li_project_transactions.Where(ptr =>
                ptr.investor == userInfo.id && ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .Where(ptr => projectFinish
                            ? ptr.li_projects.status == (int) Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime
                            : ptr.li_projects.status != (int) Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime)
                .GroupBy(ptr => ptr.li_projects).ToDictionary(g => g.Last().create_time, g => g.Key);

            var result = investedProjects
                    .OrderByDescending(p => p.Key)
                    .Skip(pageSize * pageIndex)
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
                            investedValue = ((JObject)JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value");
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

        [WebMethod]
        public static string AjaxQueryProjectRepaymentDetail(short projectId, short? ticketId) // 微信端用到此 api
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            if (ticketId == null)
            {
                var project = context.li_projects.Single(p => p.id == projectId);
                var investAmount = QueryInvestAmount(project, userInfo.id);
                var investRatio = TransactionFacade.GetInvestRatio(project)[userInfo];
                var result = new
                {
                    Title = project.title,
                    ProfitingAmount = (int)Agp2pEnums.ProjectStatusEnum.Financing < project.status
                        ? project.li_repayment_tasks.Sum(ta => Math.Round(investRatio * ta.repay_principal, 2) + Math.Round(investRatio * ta.repay_interest, 2)).ToString("c") // 模拟放款累计
                        : "(未满标)",
                    ProfitRateYear = project.GetProfitRateYearly(),
                    InvestedValue = investAmount.ToString("c"),
                    TermsData = project.li_repayment_tasks.Where(t => t.only_repay_to == null || t.only_repay_to == userInfo.id).Select(ta => new
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
                    ProfitRateYear = project.GetProfitRateYearly(),
                    InvestedValue = ((JObject)JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value").ToString("c"),
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
