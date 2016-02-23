using System;
using System.Collections.Generic;
using System.Text;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Text.RegularExpressions;
using Agp2p.BLL;
using System.Web;
using Agp2p.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agp2p.Web.UI.Page
{
    /// <summary>
    /// 项目详细页面继承类
    /// </summary>
    public partial class project : Web.UI.BasePage
    {
        Agp2pDataContext context = new Agp2pDataContext();
        protected int project_id;         //项目id
        protected li_projects projectModel = new li_projects();//项目
        protected int invsetorCount = 0;//投资人数
        protected string investmentBalance = string.Empty;//剩余金额
        protected li_loaners loaner = new li_loaners();//借款人
        protected li_loaner_companies loaner_company;//借款人企业
        protected li_risks risk = new li_risks();//风险信息
        protected List<li_mortgages> mortgages = new List<li_mortgages>();//抵押物
        protected List<ProjectTransactions> project_transactions = new List<ProjectTransactions>();//投标记录
        protected List<li_repayment_tasks> repayment_tasks = new List<li_repayment_tasks>();//还款计划
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
                return;
            }
            // 浏览次数 + 1
            projectModel.click += 1;
            context.SubmitChanges();

            var pr = GetProjectInvestmentProgress(projectModel);
            //剩余金额
            investmentBalance = pr.GetInvestmentBalance();
            //风控信息
            risk = projectModel.li_risks;
            //借款人
            loaner = risk.li_loaners;
            //借款人企业
            loaner_company = risk.li_loaners?.li_loaner_companies;
            //抵押物
            mortgages = projectModel.li_risks.li_risk_mortgage.Select(rm => rm.li_mortgages).ToList();

            invsetorCount = projectModel.GetInvestedUserCount();

            //还款计划
            repayment_tasks = projectModel.li_repayment_tasks
                .Where(task => task.only_repay_to == null)
                .OrderBy(rt => rt.should_repay_time)
                .ToList();
        }

        protected IEnumerable<li_albums> QueryAlbums()
        {
            //现场图片
            var pictures = mortgages.SelectMany(m => m.li_albums.Where(a => a.type == (int)Agp2pEnums.AlbumTypeEnum.Pictures));

            //债权图片
            var certificates = mortgages.SelectMany(m =>
                m.li_albums.Where(a =>
                    a.type == (int) Agp2pEnums.AlbumTypeEnum.PropertyCertificate ||
                    a.type == (int) Agp2pEnums.AlbumTypeEnum.LienCertificate));

            var loanerIdCard = loaner?.li_albums.Where(a => a.type == (int)Agp2pEnums.AlbumTypeEnum.IdCard) ?? Enumerable.Empty<li_albums>();

            var creditorIdCard = Enumerable.Empty<li_albums>();
            if (risk.li_creditors?.dt_users != null)
            {
                creditorIdCard = risk.li_creditors.dt_users.li_albums.Where(a => a.type == (int)Agp2pEnums.AlbumTypeEnum.IdCard);
            }

            //票理财项目不显示企业图片资料
            var companyPics = Enumerable.Empty<li_albums>();
            if (projectModel.dt_article_category.call_index != "ypl")
            {
                if (risk.li_loaners?.li_loaner_companies != null)
                {
                    companyPics =
                        risk.li_loaners.li_loaner_companies.li_albums.Where(
                            a => a.type == (int) Agp2pEnums.AlbumTypeEnum.Pictures);
                }
            }

            // 借款合同图片
            var mortgagePics = risk.li_albums.Where(a =>
                a.type == (int)Agp2pEnums.AlbumTypeEnum.LoanAgreement ||
                a.type == (int)Agp2pEnums.AlbumTypeEnum.MortgageContract ||
                a.type == (int) Agp2pEnums.AlbumTypeEnum.LienCertificate);

            // 机构
            var guarantorPics = projectModel.li_risks?.li_guarantors?.li_albums ?? Enumerable.Empty<li_albums>();

            return
                new[] { companyPics, pictures, certificates, loanerIdCard, creditorIdCard, mortgagePics, guarantorPics}.SelectMany(s => s);
        } 

        void Project_Init(object sender, EventArgs e)
        {
            //客户余额
            if (!IsUserLogin()) return;

            var user = GetUserInfoByLinq();
            has_email = !string.IsNullOrEmpty(user.email);
            idle_money = user.li_wallets.idle_money;
        }

        //投标记录
        protected List<ProjectTransactions> QueryProjectTransactions()
        {
            project_transactions = projectModel.li_project_transactions
                .Where(pt => pt.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest && pt.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success)
                .OrderByDescending(pt => pt.create_time)
                .AsEnumerable()
                .Select(pt => new ProjectTransactions
                {
                    id = pt.id,
                    user_name = Utils.GetUserNameHidden(pt.dt_users.user_name),
                    user_id = pt.dt_users.id,
                    create_time = pt.create_time.ToString("yyyy-MM-dd HH:mm:ss"),
                    value = pt.principal.ToString("c0")
                }).ToList();
            return project_transactions;
        }

        protected bool HasPayPassword()
        {
            var user = GetUserInfoByLinq();
            return user != null && !string.IsNullOrWhiteSpace(user.pay_password);
        }

        protected bool HasBindedEmail()
        {
            var user = GetUserInfoByLinq();
            return user != null && !string.IsNullOrWhiteSpace(user.email);
        }

        protected bool HasBindedIdCard()
        {
            var user = GetUserInfoByLinq();
            return user != null && !string.IsNullOrWhiteSpace(user.id_card_number);
        }

        protected class ProjectTransactions
        {
            public int id;
            public string user_name;
            public int user_id;
            public string create_time;
            public string value;
        }

        private static readonly Dictionary<Agp2pEnums.ProjectTagEnum, Agp2pEnums.ActivityTransactionActivityTypeEnum>
            EnumMapping = new Dictionary<Agp2pEnums.ProjectTagEnum, Agp2pEnums.ActivityTransactionActivityTypeEnum>
            {
                {Agp2pEnums.ProjectTagEnum.Trial, Agp2pEnums.ActivityTransactionActivityTypeEnum.Trial},
            };
        protected List<Dictionary<string, string>> QueryTickets()
        {
            var user = GetUserInfoByLinq();
            if (user != null)
            {
                return user.li_activity_transactions.Where(
                    atr =>
                        atr.activity_type == (int) EnumMapping[(Agp2pEnums.ProjectTagEnum) projectModel.tag] &&
                        atr.status == (int) Agp2pEnums.ActivityTransactionStatusEnum.Acting).Select(
                            atr =>
                            {
                                var ticketVal = ((JObject) JsonConvert.DeserializeObject(atr.details)).Value<decimal>("Value");
                                return new Dictionary<string, string>
                                       {
                                           { "ticketName", Utils.GetAgp2pEnumDes((Agp2pEnums.ActivityTransactionActivityTypeEnum) atr.activity_type) + " " + ticketVal.ToString("c") },
                                           { "ticketValue", ticketVal.ToString("F") },
                                       };
                            }).ToList();
            }
            return Enumerable.Empty<Dictionary<string, string>>().ToList();
        }

        protected static string GetRemainTime(li_projects proj)
        {
            var deadlineDay = proj.publish_time.Value.AddDays(proj.financing_day);

            var match = new Regex(@"^(\d{1,2}):(\d{2}):(\d{2})$").Match(ConfigLoader.loadSiteConfig().systemTimerTriggerTime);
            TimeSpan remainTimeSpan;
            if (!match.Success)
            {
                remainTimeSpan = deadlineDay.Subtract(DateTime.Now);
            }
            else
            {
                remainTimeSpan =
                    deadlineDay.Date.AddHours(Convert.ToInt32(match.Groups[1].Value))
                        .AddMinutes(Convert.ToInt32(match.Groups[2].Value))
                        .AddSeconds(Convert.ToInt32(match.Groups[3].Value))
                        .Subtract(DateTime.Now);
            }
            var timeSpanNotNeg = new[] {remainTimeSpan, TimeSpan.Zero}.Max();
            return $"{timeSpanNotNeg.Days}天{timeSpanNotNeg.Hours}时{timeSpanNotNeg.Minutes}分";
        }

        protected IEnumerable<Tuple<string, string>> GetAllMortgagesInfo(bool showValue = true)
        {
            return mortgages.SelectMany(m =>
            {
                var properties = GetMortgageProperties(m);
                return showValue
                    ? properties.Concat(new[] { new Tuple<string, string>("市场价值", m.valuation.ToString("c")) }).ToList()
                    : properties;
            });
        }

        protected static IEnumerable<Tuple<string, string>> GetMortgageProperties(li_mortgages m)
        {
            var schemeObj = (JObject)JsonConvert.DeserializeObject(m.li_mortgage_types.scheme);
            var kv = (JObject)JsonConvert.DeserializeObject(m.properties);

            return schemeObj.Cast<KeyValuePair<string, JToken>>()
                .Select(p => new Tuple<string, string>(p.Value.ToString(), kv[p.Key]?.ToString() ?? ""));
        }
    }
}
