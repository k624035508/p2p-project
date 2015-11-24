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
            public decimal ProfitRateYear { get; set; }
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
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
        public static List<MyRepayment> QueryProjectRepayments(int userId, Agp2pEnums.MyRepaymentQueryTypeEnum type, string startTime = "", string endTime = "")
        {
            var context = new Agp2pDataContext();
            var investedProjectValueMap = context.li_project_transactions.Where(
                tr =>
                    tr.investor == userId &&
                    tr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    tr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .GroupBy(inv => inv.li_projects)
                .ToDictionary(g => g.Key, g => g.Sum(tr => tr.principal));

            return investedProjectValueMap.SelectMany(p =>
            {
                var ratio1 = investedProjectValueMap[p.Key]/p.Key.investment_amount;
                var query1 = p.Key.li_repayment_tasks.Where(t => t.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid);

                if (!string.IsNullOrWhiteSpace(startTime))
                {
                    query1 = query1.Where(tr => Convert.ToDateTime(startTime) <= tr.should_repay_time);
                }
                if (!string.IsNullOrWhiteSpace(endTime))
                {
                    query1 = query1.Where(tr => tr.should_repay_time <= Convert.ToDateTime(endTime));
                }

                var validRepaymentTaskCount = query1.Count();

                var reps1 =
                    query1.Where(tr =>
                        type == Agp2pEnums.MyRepaymentQueryTypeEnum.Unpaid
                            ? (int) Agp2pEnums.RepaymentStatusEnum.Unpaid == tr.status
                            : (int) Agp2pEnums.RepaymentStatusEnum.ManualPaid <= tr.status)
                            .Select(task => new MyRepayment
                            {
                                RepaymentId = task.id,
                                Project = null,
                                RepayInterest = Math.Round(task.repay_interest*ratio1, 2),
                                RepayPrincipal = Math.Round(task.repay_principal*ratio1, 2),
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
                    InvestValue = investedProjectValueMap[p.Key],
                    ProfitRateYear = p.Key.profit_rate_year
                };

                return reps1;
            }).ToList();
        }
    }
}
