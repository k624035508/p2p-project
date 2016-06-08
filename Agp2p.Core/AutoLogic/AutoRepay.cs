using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Transactions;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;
using System.Web.UI.WebControls;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Core.PayApiLogic;
using Agp2p.Model;

namespace Agp2p.Core.AutoLogic
{
    public class AutoRepay
    {
        /* 公司账号，购买后的债权为内部债权，只能被中间户买入、定期债权转出不收取转让手续费，可在持有债权 24 小时后转出 */
        public const string CompanyAccount = "VIP08";
        /* 中间户买入后的债权能被用于活期投资 */
        public const string AgentGroup = "中间户";

        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => GenerateHuoqiRepaymentTask(m.TimerType, m.OnTime)); // 每日定时生成活期项目的还款计划（注意：这个任务需要放在每日定时还款任务之前）
            MessageBus.Main.Subscribe<TimerMsg>(m => DoGainLoanerRepayment(m.TimerType, m.OnTime)); // 回款前取得借款人还的款
            MessageBus.Main.Subscribe<TimerMsg>(m => CheckStaticProjectWithdrawOvertime(m.TimerType, m.OnTime)); // 检查定期项目债权转让是否超时
            MessageBus.Main.Subscribe<TimerMsg>(m => DoRepay(m.TimerType, m.OnTime)); // 每日定时还款
        }

        public static void DoGainLoanerRepayment(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.LoanerRepayTimer) return;

            // 找出今天需要回款，并且没有收取还款的计划
            var context = new Agp2pDataContext();
            var shouldRepayTask = context.li_repayment_tasks.Where(t =>
                    t.li_projects.li_risks.li_loaners != null &&
                    t.status == (int) Agp2pEnums.RepaymentStatusEnum.Unpaid &&
                    t.should_repay_time.Date <= DateTime.Today)
                .AsEnumerable()
                .Where(t => !t.li_projects.IsNewbieProject1())
                .Where(t =>
                {
                    var loaner = t.li_projects.li_risks.li_loaners;
                    return !loaner.dt_users.li_bank_transactions.Any(
                        btr =>
                            btr.type == (int) Agp2pEnums.BankTransactionTypeEnum.GainLoanerRepay &&
                            btr.status == (int)Agp2pEnums.BankTransactionStatusEnum.Confirm && btr.remarks == t.id.ToString());
                })
                .ToList();
            if (!shouldRepayTask.Any()) return;

            shouldRepayTask.ForEach(t =>
            {
                var project = context.li_projects.SingleOrDefault(p => p.id == t.project);
                if (project != null)
                {
                    var loaner = project.li_risks.li_loaners.dt_users;
                    try
                    {
                        if (project.IsHuoqiProject() || (project.autoRepay != null && (bool)project.autoRepay))
                        {
                            //创建自动还款托管接口请求
                            var autoRepayReqMsg = loaner.dt_user_groups.title.Equals("融资合作组") ? 
                            new CompanyAutoRepayReqMsg(loaner.id, t.project, (t.repay_principal + t.repay_interest).ToString("f")) : 
                            new AutoRepayReqMsg(loaner.id, t.project, (t.repay_principal + t.repay_interest).ToString("f"));
                            autoRepayReqMsg.Remarks = $"isEarly=false&repayTaskId={t.id}";
                            //发送请求
                            MessageBus.Main.PublishAsync(autoRepayReqMsg, msg =>
                            {
                                //处理请求同步返回结果
                                var repayRespMsg = BaseRespMsg.NewInstance<RepayRespMsg>(msg.SynResult);
                                repayRespMsg.AutoRepay = true;
                                MessageBus.Main.PublishAsync(repayRespMsg);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        context.AppendAdminLog("GainLoanerRepayment",
                            ex.Message == "借款人的余额不足"
                                ? $"借款人 {loaner.GetFriendlyUserName()} 的余额小于还款计划需要收取的金额 {t.repay_principal + t.repay_interest}"
                                : ex.GetSimpleCrashInfo());
                    }
                }
            });
        }

        public static void CheckStaticProjectWithdrawOvertime(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.AutoRepayTimer) return;

            var context = new Agp2pDataContext();
            // 申请时间超过 2 日 / 到达还款日，则取消债权转让申请
            var withdrawOvertimeClaims = context.li_claims
                .Where(c => c.status == (int) Agp2pEnums.ClaimStatusEnum.NeedTransfer && c.projectId == c.profitingProjectId &&
                        !c.Children.Any())
                .AsEnumerable().Where(c =>
                {
                    var nextRepayDate = c.li_projects.li_repayment_tasks.FirstOrDefault(ta => ta.IsUnpaid())?.should_repay_time.Date;
                    if (nextRepayDate != null && nextRepayDate.Value <= DateTime.Today)
                    {
                        return true;
                    }
                    return c.createTime < DateTime.Now.AddDays(-2);
                }).ToList();
            if (!withdrawOvertimeClaims.Any()) return;

            withdrawOvertimeClaims.ForEach(c => TransactionFacade.StaticClaimWithdrawCancel(context, c.id, false));
            context.AppendAdminLog("StaticWithdraw", "取消超时的定期债权转让申请：" + withdrawOvertimeClaims.Count);
            context.SubmitChanges();
        }

        public static void GenerateHuoqiRepaymentTask(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.LoanerRepayTimer) return;

            var context = new Agp2pDataContext();
            var today = DateTime.Today;

            var huoqiProjects = context.li_projects.Where(p =>
                        p.status == (int) Agp2pEnums.ProjectStatusEnum.Financing &&
                        p.dt_article_category.call_index == "huoqi").ToList();

            var dailyRepayments = huoqiProjects.SelectMany(p =>
            {
                // 检查今日是否已经生成过还款计划
                var lastRepaymentTask = context.li_repayment_tasks.Where(ta => ta.project == p.id)
                    .OrderByDescending(ta => ta.should_repay_time)
                    .FirstOrDefault();
                if (lastRepaymentTask != null && today < lastRepaymentTask.should_repay_time)
                {
                    return Enumerable.Empty<li_repayment_tasks>();
                }

                // 如果是今天/昨日才投的活期标，则不返利
                // 如果前日有 不可转让/可转让 的债权，则会产生收益（提现后不再产生收益）
                var shouldRepayTo = p.li_claims_profiting.Where(c => c.IsProfiting()).ToList();
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
                        repay_interest = Math.Round(1m/TransactionFacade.HuoqiProjectProfitingDay*p.profit_rate_year/100*shouldRepayTo.Sum(c => c.principal), 2),
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

        public static void DoRepay(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.AutoRepayTimer) return;
            if (ConfigLoader.loadSiteConfig().enableAutoRepay == 0) return;

            var context = new Agp2pDataContext();
            var shouldRepayTask = context.li_repayment_tasks.Where(
                t =>
                    !t.li_projects.IsNewbieProject1() &&
                    t.status == (int) Agp2pEnums.RepaymentStatusEnum.Unpaid &&
                    t.should_repay_time.Date <= DateTime.Today).ToList();
            if (!shouldRepayTask.Any()) return;


            // 优先进行特殊项目的回款
            shouldRepayTask.OrderByDescending(t => t.li_projects.dt_article_category.sort_id).ForEach(ta =>
            {
                //TODO 特殊项目回款处理
                //if (ta.li_projects.IsNewbieProject())
                //{
                //    context.ExecuteRepaymentTask(ta.id);
                //}
                //else
                    //调用托管本息到账接口,在本息到账异步响应中执行还款计划
                    RequestApiHandle.SendReturnPrinInte(ta.project, (ta.repay_interest + ta.repay_principal).ToString("f"), ta.id, false, ta.li_projects.IsHuoqiProject());
            });

            context.AppendAdminLogAndSave("AutoRepay", "今日待还款项目自动还款：" + shouldRepayTask.Count);

            // 活期项目不发兑付公告
            SendRepayNotice(shouldRepayTask.Where(t => !t.li_projects.IsHuoqiProject()).ToList(), context);
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
                var tableTemplate = "<table class='table table-bordered'><tbody><tr><th>序号</th><th>项目类别</th><th>项目名称</th><th>返回金额(元)</th><th>返回本金(元)</th><th>返回收益(元)</th></tr>{tr}</tbody></table>";
                var trTemplate = "<tr><td>{no}</td><td>{project_type}</td><td>{project_name}</td><td>{amount}</td><td>{principal}</td><td>{interest}</td></tr>";

                var trAll = Enumerable.Range(0, tasks.Count).Select(i =>
                {
                    var t = tasks[i];
                    return trTemplate.Replace("{no}", (i + 1).ToString())
                        .Replace("{project_type}", t.li_projects.dt_article_category.title)
                        .Replace("{project_name}", t.li_projects.title)
                        .Replace("{amount}", (t.repay_principal + t.repay_interest).ToString("N"))
                        .Replace("{principal}", t.repay_principal.ToString("N"))
                        .Replace("{interest}", t.repay_interest.ToString("N"));
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
