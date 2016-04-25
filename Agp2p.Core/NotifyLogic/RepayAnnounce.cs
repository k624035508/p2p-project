using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.NotifyLogic
{
    /// <summary>
    /// 发送收益相关的提醒
    /// </summary>
    class RepayAnnounce
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => HandleTimerMessage(m.TimerType, m.OnTime)); // 还款前 3 日发送还款提醒给借款人
            MessageBus.Main.Subscribe<ProjectRepaidMsg>(m => HandleProjectRepaidMsg(m.RepaymentTaskId)); // 放款计划执行后发送还款提醒
        }

        private static void HandleTimerMessage(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.AutoRepayTimer) return;

            // 安广融合借款人还款提现：你 3 天后将要返还还项目【{project}】的第 {termNumber} 期借款，本金 {principal} 加利息 {interest} 共计 {total}。
            var context = new Agp2pDataContext();
            var willRepayTasks =
                context.li_repayment_tasks.Where(
                    t =>
                        t.li_projects.dt_article_category.call_index != "newbie" &&
                        t.should_repay_time.Date == DateTime.Today.AddDays(3) &&
                        t.status == (int)Agp2pEnums.RepaymentStatusEnum.Unpaid).ToList();
            if (!willRepayTasks.Any()) return;

            var smsTemplate = context.dt_sms_template.SingleOrDefault(te => te.call_index == "loaner_repay_hint");

            if (smsTemplate == null)
            {
                context.AppendAdminLogAndSave("LoanerRepayHint", "找不到还款提现模板: loaner_repay_hint");
                return;
            }

            willRepayTasks.ForEach(task =>
            {
                var loaner = task.li_projects.li_risks.li_loaners.dt_users;
                if(string.IsNullOrEmpty(loaner.mobile)) return;
                var logTag = string.Format("LoanerRepayHint_{0}_{1}", loaner.id, task.id);

                // 判断有没有发送过短信
                var alreadySend = context.dt_manager_log.Any(log => log.action_type == logTag);
                if (alreadySend) return;

                var smsContent = smsTemplate.content
                    .Replace("{project}", task.li_projects.title)
                    .Replace("{termNumber}", task.term.ToString())
                    .Replace("{principal}", task.repay_principal.ToString("c"))
                    .Replace("{interest}", task.repay_interest.ToString("c"))
                    .Replace("{total}", (task.repay_principal + task.repay_interest).ToString("c"));

                try
                {
                    var errorMsg = string.Empty;
                    if (!SMSHelper.SendTemplateSms(loaner.mobile, smsContent, out errorMsg))
                    {
                        context.AppendAdminLogAndSave("LoanerRepayHint",
                            string.Format("发送还款提醒失败：{0}（借款人ID：{1}，项目名称：{2}）", errorMsg, loaner.user_name, task.li_projects.title));
                    }
                    context.AppendAdminLogAndSave(logTag, string.Format("发送还款提醒成功：{0}", smsContent));
                }
                catch (Exception ex)
                {
                    context.AppendAdminLogAndSave("LoanerRepayHint",
                        string.Format("发送还款提醒失败：{0}（借款人ID：{1}，项目名称：{2}）", ex.GetSimpleCrashInfo(), loaner.user_name, task.li_projects.title));
                }
            });
        }

        private static void HandleProjectRepaidMsg(int repaymentTaskId)
        {
            if (ConfigLoader.loadSiteConfig().sendShortMsgAfterRepay != 1) return;

            var context = new Agp2pDataContext();
            var repayTask = context.li_repayment_tasks.Single(r => r.id == repaymentTaskId);
            var transactions = context.li_project_transactions.Where(t =>
                    t.project == repayTask.project &&
                    t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.RepayToInvestor
                    && t.create_time == repayTask.repay_at).ToLookup(ptr => ptr.dt_users);

            transactions.ForEach(group =>
            {
                var repayToUser = group.Key;
                var repayPrincipal = group.Sum(tr => tr.principal);
                var repayInterest = group.Sum(tr => tr.interest.GetValueOrDefault());
                var now = DateTime.Now;

                // 检测用户是否接收放款的通知
                var sendNotificationSettings = context.li_notification_settings.Where(n => n.user_id == repayToUser.id)
                    .Select(n => n.type).Cast<Agp2pEnums.DisabledNotificationTypeEnum>();

                try
                {
                    //找出模板
                    var smsModel = 0 < repayPrincipal
                        ? context.dt_sms_template.SingleOrDefault(te => te.call_index == "user_repay_all_info")
                        : context.dt_sms_template.SingleOrDefault(te => te.call_index == "user_repay_info");
                    if (smsModel == null) throw new InvalidOperationException("找不到放款提醒模板: user_repay_all_info 或 user_repay_info");

                    //发送短信
                    var msgContent = smsModel.content
                        //.Replace("{user_name}", t.dt_users.user_name)
                        .Replace("{project_name}", repayTask.li_projects.title)
                        .Replace("{amount}", (repayPrincipal + repayInterest).ToString());

                    if (!sendNotificationSettings.Contains(Agp2pEnums.DisabledNotificationTypeEnum.ProjectRepaidForUserMsg))
                    {
                        //发送站内消息
                        var userMsg = new dt_user_message
                        {
                            type = 1,
                            post_user_name = "",
                            accept_user_name = repayToUser.user_name,
                            title = smsModel.title,
                            content = msgContent,
                            post_time = now,
                            receiver = repayToUser.id
                        };
                        context.dt_user_message.InsertOnSubmit(userMsg);
                        context.SubmitChanges();
                    }
                    if (!sendNotificationSettings.Contains(Agp2pEnums.DisabledNotificationTypeEnum.ProjectRepaidForSms))
                    {
                        var errorMsg = string.Empty;
                        if (!SMSHelper.SendTemplateSms(repayToUser.mobile, msgContent, out errorMsg))
                        {
                            context.AppendAdminLogAndSave("RepaymentSms",
                                string.Format("发送放款提醒失败：{0}（客户ID：{1}，项目名称：{2}）", errorMsg, repayToUser.user_name, repayTask.li_projects.title));
                        }
                    }
                }
                catch (Exception e)
                {
                    context.AppendAdminLogAndSave("RepaymentSms",
                        string.Format("发送放款提醒失败：{0}（客户ID：{1}，项目名称：{2}）", e.GetSimpleCrashInfo(), repayToUser.user_name, repayTask.li_projects.title));
                }
            });
        }
    }
}
