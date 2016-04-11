﻿using System;
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

        /// <summary>
        /// 查询投资列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
        protected static List<li_claims> QueryInvestment(int userId, Agp2pEnums.MyInvestRadioBtnTypeEnum type,
            int pageIndex, string startTime, string endTime, short pageSize, out int count)
        {
            var context = new Agp2pDataContext();

            var query = context.li_claims.Where(c =>
                        c.userId == userId && c.projectId == c.profitingProjectId &&
                        c.status < (int)Agp2pEnums.ClaimStatusEnum.Transferred && !c.Children.Any());

            if (!string.IsNullOrWhiteSpace(startTime))
                query = query.Where(c => Convert.ToDateTime(startTime) <= c.createTime);
            if (!string.IsNullOrWhiteSpace(endTime))
                query = query.Where(c => c.createTime <= Convert.ToDateTime(endTime));

            if (type == Agp2pEnums.MyInvestRadioBtnTypeEnum.Investing)
                query = query.Where(c => c.li_projects.status < (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying);
            else if (type == Agp2pEnums.MyInvestRadioBtnTypeEnum.Repaying)
                query = query.Where(c => c.li_projects.status == (int) Agp2pEnums.ProjectStatusEnum.ProjectRepaying);
            else if (type == Agp2pEnums.MyInvestRadioBtnTypeEnum.RepayComplete)
                query = query.Where(c => c.li_projects.status == (int) Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime);

            count = query.Count();
            return query.OrderByDescending(h => h.id).Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

        /// <summary>
        /// 查询借款列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
       protected static List<li_projects> QueryLoan(int userId, Agp2pEnums.MyLoanQueryTypeEnum type,
           int pageIndex, string startTime, string endTime, short pageSize, out int count)
        {
            var context = new Agp2pDataContext();

            var query = context.li_projects.Where(c =>
                    c.li_risks.li_loaners.user_id == userId
                     );

            if (!string.IsNullOrWhiteSpace(startTime))
                query = query.Where(c => Convert.ToDateTime(startTime) <= c.make_loan_time);
            if (!string.IsNullOrWhiteSpace(endTime))
                query = query.Where(c => c.make_loan_time <= Convert.ToDateTime(endTime));

            if (type == Agp2pEnums.MyLoanQueryTypeEnum.Applying)
                query = query.Where(c => c.status == (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationChecking);
            else if (type == Agp2pEnums.MyLoanQueryTypeEnum.Loaning)
                query = query.Where(c => c.status < (int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying);
            else if (type == Agp2pEnums.MyLoanQueryTypeEnum.Repaying)
                query = query.Where(c => c.status == (int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying);
            else if (type == Agp2pEnums.MyLoanQueryTypeEnum.Repaid)
                query = query.Where(c => c.status == (int)Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime);

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
            var claims = QueryInvestment(userInfo.id, (Agp2pEnums.MyInvestRadioBtnTypeEnum) type, pageIndex, startTime, endTime, pageSize, out count);

            var now = DateTime.Now;
            var config = new BLL.siteconfig().loadConfig();

            var result = claims.Select(c =>
            {
                var proj = c.li_projects;
                decimal profit;
                if (proj.dt_article_category.call_index == "newbie") profit = 10;
                else
                {
                    profit = proj.profit_rate == 0
                        ? Math.Round(c.principal*proj.GetFinalProfitRate(now), 2)
                        : Math.Round(proj.profit_rate*c.principal, 2);
                }
                return new
                {
                    ptrId = c.id,
                    projectId = proj.id,
                    projectUrl = linkurl(config, "project", proj.id),
                    projectName = proj.title,
                    projectProfitRateYearly = proj.GetProfitRateYearly(),
                    term = proj.repayment_term_span_count + proj.GetProjectTermSpanEnumDesc(),
                    investTime = c.createTime.ToString("yy/MM/dd HH:mm"),
                    investValue = c.principal,
                    profit,
                    status = proj.GetProjectStatusDesc(),
                    isNewbieProject = proj.dt_article_category.call_index == "newbie"
                };
            });
            return JsonConvert.SerializeObject(new {totalCount = count, data = result});
        }
      [WebMethod]
        public new static string AjaxQueryLoan(short type, short pageIndex, short pageSize, string startTime = "", string endTime = "")
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            int count;
            var projects = QueryLoan(userInfo.id, (Agp2pEnums.MyLoanQueryTypeEnum)type, pageIndex, startTime, endTime, pageSize, out count);

            var now = DateTime.Now;
            var config = new BLL.siteconfig().loadConfig();

            var result = projects.Select(c =>
            {
                var risk = c.li_risks.li_loaners;
                decimal profit=10;
         
                return new
                {
                    ptrId = c.id,
                    projectId = risk.id,
                    projectUrl = linkurl(config, "project", c.id),
                    projectName = c.title,
                    projectProfitRateYearly = c.GetProfitRateYearly(),
                    term =c.repayment_term_span_count + c.GetProjectTermSpanEnumDesc(),
                    investTime = Convert.ToDateTime(c.make_loan_time).ToString("yy/MM/dd"),
                    investValue = risk.income,
                    profit,
                    status = c.GetProjectStatusDesc(),
                    isNewbieProject = c.dt_article_category.call_index == "newbie"
                };
            });
            return JsonConvert.SerializeObject(new { totalCount = count, data = result });
        } 
    }
}
