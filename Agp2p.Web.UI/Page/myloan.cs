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
    public partial class myloan : usercenter
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

        /// <summary>
        /// 查询借款列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
        protected static List<li_projects> QueryProjectsByLoaner(int userId, Agp2pEnums.MyLoanQueryTypeEnum type,
            int pageIndex, string startTime, string endTime, short pageSize, out int count)
        {
            var context = new Agp2pDataContext();

            var query = context.li_projects.Where(p => p.li_risks.li_loaners.user_id == userId && p.dt_article_category.call_index != "newbie")
                .Where(p => p.status != (int) Agp2pEnums.ProjectStatusEnum.FinancingApplicationCancel &&
                            p.status != (int) Agp2pEnums.ProjectStatusEnum.FinancingFail &&
                            p.status != (int) Agp2pEnums.ProjectStatusEnum.FinancingApplicationFail);

            if (!string.IsNullOrWhiteSpace(startTime))
                query = query.Where(c => Convert.ToDateTime(startTime) <= c.make_loan_time);
            if (!string.IsNullOrWhiteSpace(endTime))
                query = query.Where(c => c.make_loan_time <= Convert.ToDateTime(endTime));

            if (type == Agp2pEnums.MyLoanQueryTypeEnum.Applying)
                query = query.Where(p => p.status < (int) Agp2pEnums.ProjectStatusEnum.Financing);
            else if (type == Agp2pEnums.MyLoanQueryTypeEnum.Loaning)
                query = query.Where(p => (int)Agp2pEnums.ProjectStatusEnum.Financing <= p.status && p.status < (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying);
            else if (type == Agp2pEnums.MyLoanQueryTypeEnum.Repaying)
                query = query.Where(p => (int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying <= p.status && p.status < (int) Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime);
            else if (type == Agp2pEnums.MyLoanQueryTypeEnum.Repaid)
                query = query.Where(p => p.status == (int) Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime);

            count = query.Count();
            return query.OrderByDescending(h => h.id).Skip(pageSize*pageIndex).Take(pageSize).ToList();
        }

        [WebMethod]
        public new static string AjaxQueryLoan(short type, short pageIndex, short pageSize, string startTime = "", string endTime = "")
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            int count;
            var projects = QueryProjectsByLoaner(userInfo.id, (Agp2pEnums.MyLoanQueryTypeEnum) type, pageIndex, startTime, endTime, pageSize, out count);

            var now = DateTime.Now;
            var config = new BLL.siteconfig().loadConfig();

            var result = projects.Select(proj =>
            {
                var validTask = proj.li_repayment_tasks.Where(ta => ta.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid).ToList();
                return new
                {
                    proj.id,
                    url = linkurl(config, "project", proj.id),
                    name = proj.title,
                    profitRateYearly = proj.GetProfitRateYearly(),
                    nextRepayTime = validTask.FirstOrDefault(ta => ta.IsUnpaid())?.should_repay_time.ToString("yyyy-MM-dd"),
                    financingAmount = proj.financing_amount,
                    totalProfit = validTask.Any()
                            ? validTask.Aggregate(0m, (sum, task) => sum + task.repay_interest)
                            : Math.Round(proj.financing_amount*proj.GetFinalProfitRate(now), 2),
                    status = proj.GetProjectStatusDesc(),
                    isAutoRepay = proj.autoRepay,
                    repayLimit = validTask.Where(ta => ta.IsUnpaid())
                            .Aggregate(0m, (sum, ta) => sum + ta.repay_interest + ta.repay_principal),
                };
            });
            return JsonConvert.SerializeObject(new {totalCount = count, data = result});
        }

        [WebMethod]
        public static string ManualRepay(int projectId)
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            //最近的还款计划
            var context = new Agp2pDataContext();
            var project = context.li_projects.SingleOrDefault(p => p.id == projectId);
            if (project != null)
            {
                var repayTask = project.li_repayment_tasks.FirstOrDefault(
                        r => r.IsUnpaid() && r.should_repay_time.Date == DateTime.Today);
                if (repayTask != null)
                {
                    var url = $"/api/payment/sumapay/index.aspx?api={Agp2pEnums.SumapayApiEnum.McRep}&userId={userInfo.id}" +
                        $"&projectCode={projectId}&sum={(repayTask.repay_principal + repayTask.repay_interest)}&repayTaskId={repayTask.id}";
                    return JsonHelper.ObjectToJSON(new { status = 1, url});
                }
                else
                {
                    return JsonHelper.ObjectToJSON(new { status = 0, msg = "今天没有还款任务！" });
                }
            }
            else
            {
                return JsonHelper.ObjectToJSON(new {status = 0, msg = "没有找到项目！"});
            }
        }
    }
    
}