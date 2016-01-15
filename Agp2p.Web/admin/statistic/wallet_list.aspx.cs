using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Text;
using Senparc.Weixin.MP.AdvancedAPIs;

namespace Agp2p.Web.admin.statistic
{
    public partial class wallet_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;
        protected int UserGroud;

        protected string keywords = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");

            pageSize = GetPageSize(GetType().Name + "_page_size");
            page = DTRequest.GetQueryInt("page", 1);
            UserGroud = DTRequest.GetQueryInt("UserGroud");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("statistics_wallet", DTEnums.ActionEnum.View.ToString()); //检查权限
                txtKeywords.Text = DTRequest.GetQueryString("keywords");
                txtStartTime.Text = DTRequest.GetQueryString("startTime");
                txtEndTime.Text = DTRequest.GetQueryString("endTime");
                TreeBind();
                RptBind();
            }
        }

        #region 绑定用户分组=================================
        protected void TreeBind()
        {
            var context = new Agp2pDataContext();
            ddlUserGroud.Items.Clear();
            ddlUserGroud.Items.Add(new ListItem("所有会员组", ""));

            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();

            ddlUserGroud.Items.AddRange(context.dt_user_groups.Where(g => g.is_lock == 0 && (!canAccessGroups.Any() || canAccessGroups.Contains(g.id)))
                .OrderByDescending(g => g.id)
                .Select(g => new ListItem(g.title, g.id.ToString()))
                .ToArray());
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind()
        {
            //txtKeywords.Text = keywords;

            var wallets = QueryWallets(out totalCount);
            if (rblTableType.SelectedValue == "0")
            {
                div_pagination.Visible = true;
                rptList.DataSource = wallets.Skip(pageSize * (page - 1)).Take(pageSize).ToList();
                rptList.DataBind();
            }
            else
            {
                div_pagination.Visible = false;
                rptList.DataSource = QueryGroupData(wallets);
                rptList.DataBind();
            }

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("wallet_list.aspx", "keywords={0}&page={1}&startTime={2}&endTime={3}&UserGroud={4}", txtKeywords.Text, "__id__", txtStartTime.Text, txtEndTime.Text, UserGroud.ToString());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private List<li_wallets> QueryGroupData(IEnumerable<li_wallets> wallets)
        {
            var grouped = wallets.Where(w => w.user_id != 0).GroupBy(w => w.dt_users.dt_user_groups).Select(g => new li_wallets
            {
                dt_users = new dt_users { id = -1, user_name = g.Key.title },
                idle_money = g.Aggregate(0m, (sum, wa) => sum + wa.idle_money),
                locked_money = g.Aggregate(0m, (sum, wa) => sum + wa.locked_money),
                investing_money = g.Aggregate(0m, (sum, wa) => sum + wa.investing_money),
                total_investment = g.Aggregate(0m, (sum, wa) => sum + wa.total_investment),
                profiting_money = g.Aggregate(0m, (sum, wa) => sum + wa.profiting_money),
                total_profit = g.Aggregate(0m, (sum, wa) => sum + wa.total_profit),
                total_charge = g.Aggregate(0m, (sum, wa) => sum + wa.total_charge),
                total_withdraw = g.Aggregate(0m, (sum, wa) => sum + wa.total_withdraw),
            }).ToList();
            grouped.Add(new li_wallets
            {
                idle_money = grouped.Aggregate(0m, (sum, wa) => sum + wa.idle_money),
                locked_money = grouped.Aggregate(0m, (sum, wa) => sum + wa.locked_money),
                investing_money = grouped.Aggregate(0m, (sum, wa) => sum + wa.investing_money),
                total_investment = grouped.Aggregate(0m, (sum, wa) => sum + wa.total_investment),
                profiting_money = grouped.Aggregate(0m, (sum, wa) => sum + wa.profiting_money),
                total_profit = grouped.Aggregate(0m, (sum, wa) => sum + wa.total_profit),
                total_charge = grouped.Aggregate(0m, (sum, wa) => sum + wa.total_charge),
                total_withdraw = grouped.Aggregate(0m, (sum, wa) => sum + wa.total_withdraw),
            });
            return grouped;
        }

        private IEnumerable<li_wallets> QueryWallets(out int count)
        {
            var context = new Agp2pDataContext();
            var options = new DataLoadOptions();
            options.LoadWith<li_wallets>(w => w.dt_users); // 类似 EF 的 Include
            options.LoadWith<dt_users>(w => w.li_wallet_histories);
            context.LoadOptions = options;

            IQueryable<li_wallets> query = context.li_wallets;
            //用户分组查询
            if (0 < UserGroud) // 选择了某一组
            {
                ddlUserGroud.SelectedValue = UserGroud.ToString();
                query = query.Where(w => w.dt_users.group_id == UserGroud);
            }
            else
            {
                // 限制当前管理员对会员的查询
                var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();
                query = query.Where(w => !canAccessGroups.Any() || canAccessGroups.Contains(w.dt_users.group_id));
            }
            if (!string.IsNullOrWhiteSpace(txtKeywords.Text))
            { 
                query = query.Where(b => b.dt_users.user_name.Contains(txtKeywords.Text) || b.dt_users.real_name.Contains(txtKeywords.Text)); 
            }

            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(h => Convert.ToDateTime(txtStartTime.Text) <= h.last_update_time.Date);
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(h => h.last_update_time.Date <= Convert.ToDateTime(txtEndTime.Text));

            count = query.Count() + 1;

            var wallets = query.AsEnumerable().OrderByDescending(
                w =>
                    w.dt_users.li_wallet_histories.Select(h => h.create_time)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max()).ToLazyList();

            return wallets.Concat(Enumerable.Range(0, 1).Select(i => new li_wallets // 总计条目
            {
                idle_money = wallets.Where(w => !w.dt_users.dt_user_groups.title.Contains("刘嫦娣")).Sum(w => w.idle_money),
                locked_money = wallets.Sum(w => w.locked_money),
                investing_money = wallets.Sum(w => w.investing_money),
                profiting_money = wallets.Sum(w => w.profiting_money),
                total_investment = wallets.Sum(w => w.total_investment),
                total_profit = wallets.Sum(w => w.total_profit),
                total_charge = wallets.Sum(w => w.total_charge),
                total_withdraw = wallets.Sum(w => w.total_withdraw),
                last_update_time = DateTime.Now
            }));
        }

        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("wallet_list.aspx", "keywords={0}&startTime={1}&endTime={2}&UserGroud={3}", txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, UserGroud.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("wallet_list.aspx", "keywords={0}&startTime={1}&endTime={2}&UserGroud={3}", txtKeywords.Text, txtStartTime.Text, txtEndTime.Text, UserGroud.ToString()));
        }

        //筛选类别
        protected void ddlUserGroud_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("wallet_list.aspx", "keywords={0}&startTime={1}&endTime={2}&UserGroud={3}", txtKeywords.Text, txtStartTime.Text, txtEndTime.Text,ddlUserGroud.SelectedValue));
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            var wallets = QueryWallets(out totalCount);
            if (rblTableType.SelectedValue == "0")
            {
                var xlsData = wallets.Skip(pageSize*(page - 1)).Take(pageSize)
                    .Select(
                        w => new
                        {
                            user = w.dt_users == null ? "" : w.dt_users.user_name,
                            name = w.dt_users == null ? "" : w.dt_users.real_name,
                            w.idle_money,
                            w.locked_money,
                            w.investing_money,
                            invested = w.total_investment - w.investing_money,
                            w.total_investment,
                            w.profiting_money,
                            w.total_profit,
                            allProfit = w.total_profit + w.profiting_money,
                            w.total_charge,
                            w.total_withdraw,
                            w.last_update_time
                        });

                var titles = new[]
                {"用户", "姓名", "可用余额", "冻结金额", "在投金额", "已还本金", "累计投资", "待收益", "已收益", "累计收益", "累计充值", "累计提现", "更新时间"};
                Utils.ExportXls("用户资金", titles, xlsData, Response);
            }
            else
            {
                var xlsData = QueryGroupData(wallets).Select(w => new
                {
                    groupName = w.dt_users != null ? w.dt_users.user_name : "总计",
                    w.idle_money,
                    w.locked_money,
                    w.investing_money,
                    invested = w.total_investment - w.investing_money,
                    w.total_investment,
                    w.profiting_money,
                    w.total_profit,
                    allProfit = w.total_profit + w.profiting_money,
                    w.total_charge,
                    w.total_withdraw,
                    w.last_update_time
                });

                var titles = new[]
                {"会员组", "可用余额", "冻结金额", "在投金额", "已还本金", "累计投资", "待收益", "已收益", "累计收益", "累计充值", "累计提现", "更新时间"};
                Utils.ExportXls("用户资金", titles, xlsData, Response);
            }
        }

        protected void rblTableType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
        }
    }
}