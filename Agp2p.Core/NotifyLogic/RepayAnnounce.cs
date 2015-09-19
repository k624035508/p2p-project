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
            try
            {
                var transactions =
                    context.li_project_transactions.Where(
                        t => t.project == repay.project && t.type != (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest
                             && t.create_time == repay.repay_at).ToList();
                transactions.ForEach(t =>
                {
                    try
                    {
                        //找出模板
                        var smsModel = 0 < t.value
                            ? context.dt_sms_template.SingleOrDefault(te => te.call_index == "user_repay_all_info")
                            : context.dt_sms_template.SingleOrDefault(te => te.call_index == "user_repay_info");
                        if (smsModel == null) throw new InvalidOperationException("找不到模板");

                        //发送短信
                        var msgContent = smsModel.content
                            .Replace("{user_name}", t.dt_users.user_name)
                            .Replace("{project_name}", t.li_projects.title)
                            .Replace("{amount}", (t.value + t.repay_interest).ToString());
                        var errorMsg = string.Empty;
                        if (Utils.IsDebugging())
                        {
                            Debug.WriteLine("发送还款提醒到{0}, 内容: {1}", t.dt_users.mobile, msgContent);
                            return;
                        }
                        else if (!SMSHelper.SendTemplateSms(t.dt_users.mobile, msgContent, out errorMsg))
                        {
                            context.AppendAdminLogAndSave("RepaymentSms",
                                string.Format("发送还款提醒失败：{0}（客户ID：{1}，项目名称：{2}）", errorMsg, t.dt_users.user_name, t.li_projects.title));
                        }

                        //发送站内消息
                        var userMsg = new dt_user_message
                        {
                            type = 1,
                            post_user_name = "",
                            accept_user_name = t.dt_users.user_name,
                            title = "项目还款",
                            content = string.Format("您的投标“{0}”已还款，共{1}元，详情请到“我的投资”中查询，欢迎续投！",
                                t.li_projects.title,
                                (t.value + t.repay_interest).ToString()),
                            post_time = t.create_time
                        };
                        context.dt_user_message.InsertOnSubmit(userMsg);
                        context.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        context.AppendAdminLogAndSave("RepaymentSms",
                            string.Format("发送还款提醒失败：{0}（客户ID：{1}，项目名称：{2}）", e.Message, t.dt_users.user_name, t.li_projects.title));
                    }
                });
            }
            catch (Exception ex)
            {
                context.AppendAdminLogAndSave("RepaymentSms", "发送还款提醒失败：" + ex.Message);
            }
        }
    }
}
