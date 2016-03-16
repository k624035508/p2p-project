using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Transactions;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.AutoLogic
{
    public class AutoRepay
    {
        public const string ClaimTakeOverGroupName = "公司账号";

        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => GenerateHuoqiRepaymentTask(m.OnTime)); // 每日定时生成活期项目的还款计划（注意：这个任务需要放在每日定时还款任务之前）
            MessageBus.Main.Subscribe<TimerMsg>(m => DoRepay(m.OnTime)); // 每日定时还款

            MessageBus.Main.Subscribe<TimerMsg>(m => HuoqiClaimTransferToCompanyWhenNeeded(m.OnTime)); // 活期项目提现后，由公司账号接手
            MessageBus.Main.Subscribe<TimerMsg>(m => DoHuoqiProjectWithdraw(m.OnTime, DateTime.Now)); // 活期项目提现的执行
        }

        public static void HuoqiClaimTransferToCompanyWhenNeeded(bool onTime)
        {
            using (var ts = new TransactionScope())
            {
                // 将需要转让的债权由公司账号购买，转手之后设置为 TransferredUnpaid
                var context = new Agp2pDataContext();
                var companyUsers = context.dt_users.Where(u => u.dt_user_groups.title == ClaimTakeOverGroupName).ToList();
                if (!companyUsers.Any())
                    throw new InvalidOperationException("请先往“公司账号”的会员组添加会员");

                // 接手昨日/更早的提现
                var needTransferClaims = context.li_claims.Where(
                    c => c.projectId != c.profitingProjectId && c.status == (int)Agp2pEnums.ClaimStatusEnum.NeedTransfer && !c.li_claims2.Any() && c.createTime.Date < DateTime.Today)
                    .ToList();

                if (!needTransferClaims.Any()) return;
                if (companyUsers.Sum(u => u.li_wallets.idle_money) < needTransferClaims.Sum(c => c.principal))
                    throw new InvalidOperationException("警告：公司账号的金额不足以接手需要转让的债权");

                context.RecaptureHuoqiClaim(needTransferClaims, DateTime.Now);
                ts.Complete();
            }
        }

        public static void DoHuoqiProjectWithdraw(bool onTime, DateTime withdrawAt)
        {
            var context = new Agp2pDataContext();
            var repayTime = DateTime.Now;

            // 对 withdrawDay 的前一天的 Unpaid 债权进行回款（方便在脚本中使用）
            var checkDay = withdrawAt.Date.AddDays(-1);

            // 执行未回款债权的回款，减少项目的在投金额（必须要是今日之前的提现）
            var claims = context.li_claims.Where(
                c =>
                    (c.status == (int) Agp2pEnums.ClaimStatusEnum.CompletedUnpaid ||
                     c.status == (int) Agp2pEnums.ClaimStatusEnum.TransferredUnpaid) &&
                    c.li_claims1.createTime.Date == checkDay && !c.li_claims2.Any()).ToList();
            if (!claims.Any()) return;

            // 查询出昨日的全部提现及其是第几次的提现
            var yesterdayWithdraws = context.li_claims.Where(c =>
                c.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer &&
                c.li_claims1.status == (int) Agp2pEnums.ClaimStatusEnum.Nontransferable &&
                c.createTime.Date == checkDay)
                .GroupBy(c => c.dt_users)
                .ToDictionary(g => g.Key, g =>
                        g.Zip(Utils.Infinite(), (claim, index) => new {claim, index})
                            .ToDictionary(e => e.claim, e => e.index));

            claims.ToLookup(c => c.li_projects1).ForEach(pcs =>
            {
                var huoqiProject = pcs.Key;
                pcs.ToLookup(c => c.dt_users).ForEach(ucs =>
                {
                    var investor = ucs.Key;
                    var wallet = investor.li_wallets;

                    ucs.ForEach(c =>
                    {
                        Agp2pEnums.ClaimStatusEnum newStatus;
                        if (c.status == (int) Agp2pEnums.ClaimStatusEnum.CompletedUnpaid)
                            newStatus = Agp2pEnums.ClaimStatusEnum.Completed;
                        else if (c.status == (int)Agp2pEnums.ClaimStatusEnum.TransferredUnpaid)
                            newStatus = Agp2pEnums.ClaimStatusEnum.Transferred;
                        else
                            throw new InvalidOperationException("活期项目 T+1 提款出错：未知的债权状态");

                        var newStatusChild = c.NewStatusChild(repayTime, newStatus);
                        context.li_claims.InsertOnSubmit(newStatusChild);

                        // 提现大于 3 次后每次提现都扣除手续费 0.25%
                        var userYesterdayWithdraws = yesterdayWithdraws[c.dt_users];
                        var parentClaim = userYesterdayWithdraws.Keys.Single(c.IsChildOf);
                        var withdrawIndex = userYesterdayWithdraws[parentClaim];
                        var handlingFee = withdrawIndex <= 2 ? 0 : c.principal*0.25m/100;

                        var withdrawTransact = new li_project_transactions
                        {
                            principal = c.principal - handlingFee,
                            project = huoqiProject.id,
                            create_time = repayTime,
                            investor = investor.id,
                            type = (byte) Agp2pEnums.ProjectTransactionTypeEnum.HuoqiProjectWithdraw,
                            status = (byte) Agp2pEnums.ProjectTransactionStatusEnum.Success,
                            gainFromClaim = c.id,
                            remark = $"活期项目【{huoqiProject.title}】债权赎回成功：债权金额 {c.principal.ToString("c")}，赎回费 {handlingFee.ToString("c")}"
                        };
                        context.li_project_transactions.InsertOnSubmit(withdrawTransact);

                        wallet.idle_money += withdrawTransact.principal;
                        wallet.investing_money -= c.principal;
                        wallet.last_update_time = repayTime;

                        var his = TransactionFacade.CloneFromWallet(wallet, Agp2pEnums.WalletHistoryTypeEnum.HuoqiProjectWithdrawSuccess);
                        his.li_project_transactions = withdrawTransact;
                        context.li_wallet_histories.InsertOnSubmit(his);
                    });
                });
            });
            context.AppendAdminLog("HuoqiWithdraw", "今日活期项目提现成功: " + claims.Sum(c => c.principal).ToString("c"));

            context.SubmitChanges();
        }

        public static void GenerateHuoqiRepaymentTask(bool onTime)
        {
            var context = new Agp2pDataContext();
            var today = DateTime.Today;

            // 第三日开始返息：如果存在需要回款的活期项目债权，并且今天没有该项目的回款计划，则生成
            var huoqiProjects = context.li_projects
                .Where(p => p.status == (int)Agp2pEnums.ProjectStatusEnum.Financing && p.dt_article_category.call_index == "huoqi")
                .Where(p => p.li_repayment_tasks.All(ta => ta.should_repay_time.Date != today)).ToList();

            var dailyRepayments = huoqiProjects.SelectMany(p =>
            {
                // 如果是今天/昨日才投的活期标，则不返利
                // 如果前日有 不可转让/可转让 的债权，则会产生收益（提现后不再产生收益）
                var shouldRepayTo = p.li_claims1.Where(c => !c.li_claims2.Any()).AsEnumerable().Where(c => c.IsProfiting()).ToList();
                if (!shouldRepayTo.Any())
                {
                    return Enumerable.Empty<li_repayment_tasks>();
                }
                return new[]
                {
                    new li_repayment_tasks
                    {
                        should_repay_time = today.AddHours(15),
                        repay_principal = 0,
                        repay_interest = Math.Round(1m/365*p.profit_rate_year/100*shouldRepayTo.Sum(c => c.principal), 2),
                        project = p.id,
                        status = (byte) Agp2pEnums.RepaymentStatusEnum.Unpaid,
                        term = (short) ((p.li_repayment_tasks.LastOrDefault()?.term ?? 0) + 1)
                    }
                };
            }).ToList();
            if (!dailyRepayments.Any()) return;

            context.li_repayment_tasks.InsertAllOnSubmit(dailyRepayments);

            context.AppendAdminLog("Huoqi",
                "自动生成今天 " + dailyRepayments.Count + " 个活期项目的还款计划，利润总计：" +
                dailyRepayments.Aggregate(0m, (sum, tasks) => sum + tasks.repay_interest).ToString("c"));
            context.SubmitChanges();
        }

        public static void DoRepay(bool onTime)
        {
            if (ConfigLoader.loadSiteConfig().enableAutoRepay == 0) return;

            var context = new Agp2pDataContext();
            var shouldRepayTask = context.li_repayment_tasks.Where(
                t =>
                    t.li_projects.dt_article_category.call_index != "newbie" &&
                    t.status == (int) Agp2pEnums.RepaymentStatusEnum.Unpaid &&
                    t.should_repay_time.Date <= DateTime.Today).ToList();
            if (!shouldRepayTask.Any()) return;

            // 优先进行特殊项目的放款
            shouldRepayTask.OrderByDescending(t => t.li_projects.dt_article_category.sort_id)
                .ForEach(ta => context.ExecuteRepaymentTask(ta.id));
            context.AppendAdminLogAndSave("AutoRepay", "今日待还款项目自动还款：" + shouldRepayTask.Count);
            SendRepayNotice(shouldRepayTask, context);
        }

        /// <summary>
        /// 发布当天的兑付公告
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="context"></param>
        public static void SendRepayNotice(List<li_repayment_tasks> tasks, Agp2pDataContext context)
        {
            //找到兑付公告模板
            var temp = context.dt_mail_template.SingleOrDefault(te => te.call_index == "project_repay");
            if (temp != null)
            {
                //构造兑付项目表格
                var tableTemplate = "<table class='table table-bordered'><tbody><tr><th>序号</th><th>项目类别</th><th>项目名称</th><th>返回金额</th><th>返回本金</th><th>返回收益</th></tr>{tr}</tbody></table>";
                var trTemplate = "<tr><td>{no}</td><td>{project_type}</td><td>{project_name}</td><td>{amount}</td><td>{principal}</td><td>{interest}</td></tr>";

                var trAll = Enumerable.Range(0, tasks.Count).Select(i =>
                {
                    var t = tasks[i];
                    return trTemplate.Replace("{no}", (i + 1).ToString())
                        .Replace("{project_type}", t.li_projects.dt_article_category.title)
                        .Replace("{project_name}", t.li_projects.title)
                        .Replace("{amount}", (t.repay_principal + t.repay_interest).ToString("c"))
                        .Replace("{principal}", t.repay_principal.ToString("c"))
                        .Replace("{interest}", t.repay_interest.ToString("c"));
                });

                var tableContent = tableTemplate.Replace("{tr}", string.Join("", trAll));

                var siteConfig = ConfigLoader.loadSiteConfig();
                var addTime = DateTime.Now;
                var content = temp.content.Replace("{today}", addTime.ToString("yyyy-MM-dd"))
                    .Replace("{count}", tasks.Count.ToString())
                    .Replace("{table}", tableContent)
                    .Replace("{webtel}", siteConfig == null ? "" : siteConfig.webtel);

                //创建公告
                try
                {
                    var title = addTime.ToString("yyyy年M月d日") + "项目兑付公告";
                    var notice = new dt_article
                    {
                        add_time = addTime,
                        category_id = 43,
                        channel_id = 5,
                        title = title,
                        seo_title = title,
                        seo_keywords = "安广融合p2p,项目兑付公告",
                        content = content
                    };

                    var noticeAttr = new dt_article_attribute_value
                    {
                        dt_article = notice,
                        author = "安广融合",
                        source = "安广融合理财平台"
                    };

                    context.dt_article.InsertOnSubmit(notice);
                    context.dt_article_attribute_value.InsertOnSubmit(noticeAttr);
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    context.AppendAdminLogAndSave("AutoRepay", "发送兑付公告失败：" + ex.Message);
                }
            }
        }
    }
}
