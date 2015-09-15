using System;
using System.Collections.Generic;
using System.Text;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using System.Linq;
using Lip2p.BLL;
using System.Web;
using Lip2p.Core;
using Lip2p.Core.ActivityLogic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lip2p.Web.UI.Page
{
    /// <summary>
    /// 项目详细页面继承类
    /// </summary>
    public partial class project : Web.UI.BasePage
    {
        Lip2pDataContext context = new Lip2pDataContext();
        protected int project_id;         //项目id
        protected li_projects projectModel = new li_projects();//项目
        protected int invsetorCount = 0;//投资人数
        protected string investmentProgress = string.Empty;//投资进度
        protected string investmentBalance = string.Empty;//剩余金额
        protected li_loaners loaner = new li_loaners();//借款人
        protected li_loaner_companies loaner_company;//借款人企业
        protected li_risks risk = new li_risks();//风险信息
        protected List<li_mortgages> mortgages = new List<li_mortgages>();//抵押物
        protected List<ProjectTransactions> project_transactions = new List<ProjectTransactions>();//投标记录
        protected List<li_repayment_tasks> repayment_tasks = new List<li_repayment_tasks>();//还款计划
        protected List<li_albums> albums_pictures = new List<li_albums>();//现场图片
        protected List<li_albums> albums_credit = new List<li_albums>();//债权信息图片
        protected List<li_albums> albums_mortgage = new List<li_albums>();//抵押图片
        protected decimal idle_money = 0;//客户可用余额
        protected bool has_email = false;

        protected static readonly short PageSize = 20;
        protected int page;

        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            Init += Project_Init; //加入IInit事件

            project_id = DTRequest.GetQueryInt("project_id");
            page = Math.Max(1, DTRequest.GetQueryInt("page"));

            if (project_id <= 0) return;
            projectModel = context.li_projects.FirstOrDefault(p => p.id == project_id);
            if (projectModel == null)
            {
                HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您要浏览的页面不存在或已删除啦！")));
            }
            else
            {
                var pr = GetProjectInvestmentProgress(context, project_id);
                //投资进度
                investmentProgress = pr.GetInvestmentProgress();
                //剩余金额
                investmentBalance = pr.GetInvestmentBalance();
                //风控信息
                risk = projectModel.li_risks;
                //借款人
                loaner = risk.li_loaners;
                //借款人企业
                loaner_company = risk.li_loaners.li_loaner_companies;
                //抵押物
                mortgages = (from rm in risk.li_risk_mortgage
                    from m in context.li_mortgages
                    where rm.risk == risk.id && rm.mortgage == m.id
                    select m).ToList();

                //现场图片
                mortgages.ForEach(
                    m =>
                    {
                        albums_pictures.AddRange(
                            m.li_albums.Where(
                                a => a.mortgage == m.id && a.type == (int) Lip2pEnums.AlbumTypeEnum.Pictures));
                    });
                //债权图片
                mortgages.ForEach(
                    m =>
                    {
                        albums_credit.AddRange(
                            m.li_albums.Where(
                                a => a.mortgage == m.id && a.type == (int) Lip2pEnums.AlbumTypeEnum.PropertyCertificate));
                    });
                albums_credit.AddRange(
                    loaner.li_albums.Where(a => a.loaner == loaner.id && a.type == (int) Lip2pEnums.AlbumTypeEnum.IdCard));
                albums_credit.AddRange(
                    risk.li_creditors.dt_users.li_albums.Where(a => a.type == (int) Lip2pEnums.AlbumTypeEnum.IdCard));
                albums_credit.AddRange(
                    risk.li_albums.Where(
                        a => a.risk == risk.id && a.type == (int) Lip2pEnums.AlbumTypeEnum.LienCertificate));
                //抵押图片
                albums_mortgage.AddRange(
                    risk.li_albums.Where(
                        a => a.risk == risk.id && (a.type == (int) Lip2pEnums.AlbumTypeEnum.LoanAgreement
                                                   || a.type == (int) Lip2pEnums.AlbumTypeEnum.MortgageContract)));

                //投标记录
                //if (projectModel.tag == (int)Lip2pEnums.ProjectTagEnum.Trial || projectModel.tag == (int)Lip2pEnums.ProjectTagEnum.DailyProject)
                //{
                //    project_transactions =
                //        context.li_activity_transactions.Where(atr => atr.details.Contains("\"ProjectId\":" + projectModel.id + ",")) // 防止跨项目查询(ProjectId 不能是最后一个属性)
                //            .OrderByDescending(atr => atr.create_time)
                //            .AsEnumerable()
                //            .Select(atr => new ProjectTransactions
                //            {
                //                id = atr.id,
                //                user_name = Utils.GetUserNameHidden(atr.dt_users.user_name),
                //                user_id = atr.dt_users.id,
                //                create_time = atr.li_wallet_histories.First().create_time.ToString("yyyy-MM-dd HH:mm:ss"),
                //                value = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value").ToString("c")
                //            }).ToList();
                //}
                //else
                //{
                //    project_transactions = projectModel.li_project_transactions
                //        .Where(pt => pt.type == (int)Lip2pEnums.ProjectTransactionTypeEnum.Invest && pt.status == (int)Lip2pEnums.ProjectTransactionStatusEnum.Success)
                //        .OrderByDescending(pt => pt.create_time)
                //        .AsEnumerable()
                //        .Select(pt => new ProjectTransactions
                //        {
                //            id = pt.id,
                //            user_name = Utils.GetUserNameHidden(pt.dt_users.user_name),
                //            user_id = pt.dt_users.id,
                //            create_time = pt.create_time.ToString("yyyy-MM-dd HH:mm:ss"),
                //            value = pt.value.ToString("c")
                //        }).ToList();
                //}
                //投资人数
                int count = 0;
                invsetorCount = query_projecttransactions(projectModel, out count).GroupBy(pt => pt.user_id).Count();

                //还款计划
                repayment_tasks = context.li_repayment_tasks
                    .OrderBy(rt => rt.should_repay_time)
                    .Where(rt => rt.project == project_id)
                    .ToList();
            }
        }

        void Project_Init(object sender, EventArgs e)
        {
            //客户余额
            if (IsUserLogin())
            {
                var user = GetUserInfo();
                has_email = !string.IsNullOrEmpty(user.email);

                if (projectModel.tag == (int)Lip2pEnums.ProjectTagEnum.Trial)
                {
                    var trial = context.li_activity_transactions.SingleOrDefault(
                        a =>
                            a.user_id == user.id &&
                            a.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.Trial);
                    if (trial != null)
                    {
                        var ticket = new TrialActivity.TrialTicket(trial);
                        if (!ticket.IsUsed())
                        {
                            idle_money = ticket.GetTicketValue();
                        }
                    }
                    return;
                }
                else if (projectModel.tag == (int)Lip2pEnums.ProjectTagEnum.DailyProject)
                {
                    idle_money = context.li_activity_transactions.Where(t =>
                        t.user_id == user.id &&
                        t.activity_type == (int) Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject &&
                        t.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting)
                        .AsEnumerable()
                        .Select(atr => ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value"))
                        .DefaultIfEmpty(0)
                        .Sum();
                    return;
                }

                // 普通项目
                var wallet = context.li_wallets.FirstOrDefault(w => w.user_id == user.id);
                if (wallet != null)
                {
                    idle_money = wallet.idle_money;
                }
            }
        }

        //投标记录
        protected List<ProjectTransactions> query_investment( li_projects projectModel, int pageIndex, short pageSize, out int count)
        {
            return query_projecttransactions(projectModel, out count).Skip(pageSize * pageIndex).Take(pageSize).ToList(); 
        }

        //投标记录
        protected List<ProjectTransactions> query_projecttransactions(li_projects projectModel, out int count)
        {
            //投标记录
            if (projectModel.tag == (int)Lip2pEnums.ProjectTagEnum.Trial || projectModel.tag == (int)Lip2pEnums.ProjectTagEnum.DailyProject)
            {
                project_transactions =
                    context.li_activity_transactions.Where(atr => atr.details.Contains("\"ProjectId\":" + projectModel.id + ",")) // 防止跨项目查询(ProjectId 不能是最后一个属性)
                        .OrderByDescending(atr => atr.create_time)
                        .AsEnumerable()
                        .Select(atr => new ProjectTransactions
                        {
                            id = atr.id,
                            user_name = Utils.GetUserNameHidden(atr.dt_users.user_name),
                            user_id = atr.dt_users.id,
                            create_time = atr.li_wallet_histories.First().create_time.ToString("yyyy-MM-dd HH:mm:ss"),
                            value = ((JObject)JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value").ToString("c")
                        }).ToList();
            }
            else
            {
                project_transactions = projectModel.li_project_transactions
                    .Where(pt => pt.type == (int)Lip2pEnums.ProjectTransactionTypeEnum.Invest && pt.status == (int)Lip2pEnums.ProjectTransactionStatusEnum.Success)
                    .OrderByDescending(pt => pt.create_time)
                    .AsEnumerable()
                    .Select(pt => new ProjectTransactions
                    {
                        id = pt.id,
                        user_name = Utils.GetUserNameHidden(pt.dt_users.user_name),
                        user_id = pt.dt_users.id,
                        create_time = pt.create_time.ToString("yyyy-MM-dd HH:mm:ss"),
                        value = pt.value.ToString("c")
                    }).ToList();
            }
            count = project_transactions.Count;
            return project_transactions;
        }

        protected decimal get_calc_final_profit_rate()
        {
            return TransactionFacade.CalcFinalProfitRate(DateTime.Now, projectModel.profit_rate_year, (Lip2pEnums.ProjectRepaymentTermSpanEnum)projectModel.repayment_term_span, projectModel.repayment_term_span_count);
        }

        protected bool has_pay_password()
        {
            var user = GetUserInfoByLinq();
            return user != null && !string.IsNullOrEmpty(user.pay_password);
        }

        protected class ProjectTransactions
        {
            public int id;
            public string user_name;
            public int user_id;
            public string create_time;
            public string value;
        }

        private static readonly Dictionary<Lip2pEnums.ProjectTagEnum, Lip2pEnums.ActivityTransactionActivityTypeEnum>
            EnumMapping = new Dictionary<Lip2pEnums.ProjectTagEnum, Lip2pEnums.ActivityTransactionActivityTypeEnum>
            {
                {Lip2pEnums.ProjectTagEnum.Trial, Lip2pEnums.ActivityTransactionActivityTypeEnum.Trial},
                {Lip2pEnums.ProjectTagEnum.DailyProject, Lip2pEnums.ActivityTransactionActivityTypeEnum.DailyProject},
            };
        protected List<Dictionary<string, string>> QueryTickets()
        {
            var user = GetUserInfoByLinq();
            if (user != null)
            {
                return user.li_activity_transactions.Where(
                    atr =>
                        atr.activity_type == (int) EnumMapping[(Lip2pEnums.ProjectTagEnum) projectModel.tag] &&
                        atr.status == (int) Lip2pEnums.ActivityTransactionStatusEnum.Acting).Select(
                            atr =>
                            {
                                var ticketVal = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value");
                                return new Dictionary<string, string>
                                       {
                                           { "ticketName", Utils.GetLip2pEnumDes((Lip2pEnums.ActivityTransactionActivityTypeEnum) atr.activity_type) + " " + ticketVal.ToString("c") },
                                           { "ticketValue", ticketVal.ToString("F") },
                                       };
                            }).ToList();
            }
            return Enumerable.Empty<Dictionary<string, string>>().ToList();
        }
    }
}
