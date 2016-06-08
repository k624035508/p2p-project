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
            public DateTime? InvestCompleteTime { get; set; } // “我的投资”的图表需要
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
            var myRepayingProjects = user.li_claims.Where(c =>
                        c.projectId == c.profitingProjectId && c.IsLeafClaim() &&
                        c.status < (int)Agp2pEnums.ClaimStatusEnum.Transferred).ToLookup(cg => cg.li_projects);

            Model.siteconfig config = new siteconfig().loadConfig();

            var unsorted = myRepayingProjects.Select(p =>
            {
                var ratio = p.Sum(c => c.principal)/p.Key.investment_amount;
                var query = p.Key.li_repayment_tasks.Where(t => t.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid)
                    .Where(task => !p.Key.IsNewbieProject1() || task.only_repay_to == user.id);

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
                    InvestValue = p.Sum(c => c.principal),
                    ProfitRateYear = p.Key.GetProfitRateYearly(),
                    InvestCompleteTime = p.Key.invest_complete_time
                };

                return reps1;
            }).ToList();

            return unsorted.Where(ts => ts.Any()).OrderBy(ts => ts.First().ShouldRepayDay).SelectMany(x => x).ToList();
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
        public static string AjaxQueryInvestedProject(short projectStatus, short pageIndex, short pageSize) // 微信端用到到此 api
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var context = new Agp2pDataContext();

            var stat = (Agp2pEnums.MyInvestRadioBtnTypeEnum) projectStatus;
            // 查出投资过的项目
            var query = context.li_claims.Where(c => c.userId == userInfo.id && c.status < (int) Agp2pEnums.ClaimStatusEnum.Transferred && !c.Children.Any());
            if (stat == Agp2pEnums.MyInvestRadioBtnTypeEnum.RepayComplete)
            {
                query = query.Where(c => (int) Agp2pEnums.ClaimStatusEnum.Completed <= c.status);
            }
            else if (stat == Agp2pEnums.MyInvestRadioBtnTypeEnum.Repaying)
            {
                query = query.Where(c => c.status < (int) Agp2pEnums.ClaimStatusEnum.Completed && c.li_projects.make_loan_time != null);
            }
            else if (stat == Agp2pEnums.MyInvestRadioBtnTypeEnum.Investing)
            {
                query = query.Where(c => c.li_projects.make_loan_time == null && !c.li_projects.IsNewbieProject1());
            }

            var groupBy = query.ToLookup(c => c.li_projects);
            var projInvestments = groupBy.ToDictionary(g => g.Key, g => g.Sum(c => c.li_project_transactions_invest.principal));
            var projInvestTime = groupBy.ToDictionary(g => g.Key, g => g.Max(c => c.createTime));

            var result = projInvestments
                    .OrderByDescending(p => projInvestTime[p.Key])
                    .Skip(pageSize * pageIndex)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        InvestTime = projInvestTime[p.Key].ToString("yyyy-MM-dd HH:mm"),
                        ProjectName = p.Key.title,
                        InvestValue = p.Value,
                        ProjectId = p.Key.id
                    });

            return JsonConvert.SerializeObject(result);
        }

        [WebMethod]
        public static string AjaxQueryProjectRepaymentDetail(short projectId) // 微信端用到此 api
        {
            var context = new Agp2pDataContext();
            var userInfo = GetUserInfoByLinq(context);
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var project = context.li_projects.Single(p => p.id == projectId);
            var investAmount = context.li_claims.Where(c =>
                c.userId == userInfo.id && c.projectId == projectId &&
                c.status < (int) Agp2pEnums.ClaimStatusEnum.Transferred && !c.Children.Any())
                .ToList()
                .Aggregate(0m, (sum, c) => sum + c.li_project_transactions_invest.principal);

            var claims = userInfo.li_claims.Where(c => c.profitingProjectId == projectId).ToList();
            var investRatio = claims.Sum(c => c.principal) / project.investment_amount;

            var profitAmount = project.IsNewbieProject1()
                ? (project.li_repayment_tasks.Single(ta => ta.only_repay_to == userInfo.id).repay_interest + investAmount).ToString("c")
                : (int)Agp2pEnums.ProjectStatusEnum.Financing < project.status
                    ? project.li_repayment_tasks.Sum(
                        ta =>
                            Math.Round(investRatio * ta.repay_principal, 2) +
                            Math.Round(investRatio * ta.repay_interest, 2)).ToString("c") // 模拟放款累计
                    : "(未满标)";
            var result = new
            {
                Title = project.title,
                ProfitingAmount = profitAmount,
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
    }
}
