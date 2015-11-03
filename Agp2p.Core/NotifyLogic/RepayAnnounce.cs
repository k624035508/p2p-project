using System;
using System.Diagnostics;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.NotifyLogic
{
    /// <summary>
    /// 返回收益后发送短信提醒
    /// </summary>
    class RepayAnnounce
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<ProjectRepaidMsg>(m => HandleProjectRepaidMsg(m.RepaymentTaskId)); // 放款计划执行后发送还款提醒
        }

        private static void HandleProjectRepaidMsg(int repaymentTaskId)
        {
            if (ConfigLoader.loadSiteConfig().sendShortMsgAfterRepay != 1) return;

            var context = new Agp2pDataContext();
            var repay = context.li_repayment_tasks.Single(r => r.id == repaymentTaskId);
            var transactions =
                context.li_project_transactions.Where(
                    t => t.project == repay.project && t.type != (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest
                         && t.create_time == repay.repay_at).ToList();

            transactions.ForEach(t =>
            {
                // 检测用户是否接收放款的通知
                var sendNotificationSettings = context.li_notification_settings.Where(n => n.user_id == t.investor)
                    .Select(n => n.type).Cast<Agp2pEnums.NotificationTypeEnum>();

                try
                {
                    //找出模板
                    var smsModel = 0 < t.principal
                        ? context.dt_sms_template.SingleOrDefault(te => te.call_index == "user_repay_all_info")
                        : context.dt_sms_template.SingleOrDefault(te => te.call_index == "user_repay_info");
                    if (smsModel == null) throw new InvalidOperationException("找不到模板");

                    //发送短信
                    var msgContent = smsModel.content
                        .Replace("{user_name}", t.dt_users.user_name)
                        .Replace("{project_name}", t.li_projects.title)
                        .Replace("{amount}", (t.principal + t.interest).ToString());

                    if (sendNotificationSettings.Contains(Agp2pEnums.NotificationTypeEnum.ProjectRepaidForUserMsg))
                    {
                        //发送站内消息
                        var userMsg = new dt_user_message
                        {
                            type = 1,
                            post_user_name = "",
                            accept_user_name = t.dt_users.user_name,
                            title = smsModel.title,
                            content = msgContent,
                            post_time = t.create_time,
                            receiver = t.investor
                        };
                        context.dt_user_message.InsertOnSubmit(userMsg);
                        context.SubmitChanges();
                    }
                    if (sendNotificationSettings.Contains(Agp2pEnums.NotificationTypeEnum.ProjectRepaidForSms))
                    {
                        var errorMsg = string.Empty;
                        if (!SMSHelper.SendTemplateSms(t.dt_users.mobile, msgContent, out errorMsg))
                        {
                            context.AppendAdminLogAndSave("RepaymentSms",
                                string.Format("发送还款提醒失败：{0}（客户ID：{1}，项目名称：{2}）", errorMsg, t.dt_users.user_name, t.li_projects.title));
                        }
                    }
                }
                catch (Exception e)
                {
                    context.AppendAdminLogAndSave("RepaymentSms",
                        string.Format("发送还款提醒失败：{0}（客户ID：{1}，项目名称：{2}）", e.Message, t.dt_users.user_name, t.li_projects.title));
                }
            });
        }
    }
}
