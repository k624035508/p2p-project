using System.Data;
using Agp2p.Linq2SQL;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Agp2p.Common;
using Agp2p.Core;

namespace Agp2p.Web.UI
{
    public partial class BasePage : System.Web.UI.Page
    {
        /// <summary>
        /// 返回平台实时数据
        /// </summary>
        /// <returns>DataTable</returns>
        protected DataTable get_statistics_data()
        {
            DataTable dt = new DataTable();
            Agp2pDataContext context = new Agp2pDataContext();
            //累计注册
            int userCount = context.dt_users.Count();
            //运营天数
            int totalDays = DateTime.Now.Subtract(DateTime.Parse("2015-11-1")).Days;
            //累计赚取
            decimal totalProfit = context.QueryTotalProfit();
            //累计投资
            decimal totalInvested = context.QueryTotalInvested();
            //累计待收
            decimal totalInvesting = context.QueryTotalInvesting();
            //每日成交量
            decimal tradingVolume = context.QueryTradingVolume(1);

            DataRow dr = dt.NewRow();
            dt.Columns.Add("userCount", Type.GetType("System.Int32"));
            dt.Columns.Add("totalDays", Type.GetType("System.Int32"));
            dt.Columns.Add("totalProfit", Type.GetType("System.Decimal"));
            dt.Columns.Add("totalInvested", Type.GetType("System.Decimal"));
            dt.Columns.Add("totalInvesting", Type.GetType("System.Decimal"));
            dt.Columns.Add("tradingVolume", Type.GetType("System.Decimal"));
            
            dr["userCount"] = userCount;
            dr["totalDays"] = totalDays;
            dr["totalProfit"] = totalProfit / 10000;
            dr["totalInvested"] = totalInvested / 10000;
            dr["totalInvesting"] = totalInvesting / 10000;
            dr["tradingVolume"] = tradingVolume / 10000;
            dt.Rows.Add(dr);
            return dt;
        }

        protected List<Dictionary<string, string>> get_inviter_invitation_ranking_list()  // 当月被推荐人投资数的排名
        {
            var context = new Agp2pDataContext();
            return
                context.li_invitations.Where(
                    inv =>
                        inv.first_invest_transaction != null && inv.dt_users.reg_time.Value.Year == DateTime.Now.Year &&
                        inv.dt_users.reg_time.Value.Month == DateTime.Now.Month).ToLookup(i => i.dt_users1)
                    .OrderByDescending(l => l.Count()).Take(10)
                    .Select(i => new Dictionary<string, string>
                    {
                        {"inviter", Utils.GetUserNameHidden(i.Key.user_name)},
                        {"invitationCount", i.Count().ToString()}
                    }).ToList();
        }

        protected List<Dictionary<string, string>> get_inviter_bonus_ranking_list() // 当月被推荐人的投资总额排名
        {
            var context = new Agp2pDataContext();
            var now = DateTime.Now;

            return context.li_invitations.GroupBy(inv => inv.dt_users1, inv => inv.dt_users)
                .ToDictionary(g => g.Key, g => g.SelectMany(u => u.li_project_transactions.Where(
                    ptr =>
                        ptr.create_time.Year == now.Year && ptr.create_time.Month == now.Month &&
                        ptr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                    .Select(ptr => ptr.principal))
                    .Sum())
                .OrderByDescending(d => d.Value)
                .Take(10)
                .Select(pair => new Dictionary<string, string>
                {
                    {"inviter", Utils.GetUserNameHidden(pair.Key.user_name)},
                    {"inviteesInvestmentSum", pair.Value.ToString("c")}
                }).ToList();
        }
    }
}
