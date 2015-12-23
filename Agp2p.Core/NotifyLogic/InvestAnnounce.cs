using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;
using Agp2p.Model;
using DESEncrypt = Agp2p.Common.DESEncrypt;
using DTMail = Agp2p.Common.DTMail;
using Utils = Agp2p.Common.Utils;

namespace Agp2p.Core.NotifyLogic
{
    class InvestAnnounce
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<ProjectStartRepaymentMsg>(m => HandleProjectInvestCompletedMsg(m.ProjectId)); // 计算每个投资者账单收益
            MessageBus.Main.Subscribe<UserInvestedMsg>(m => HandleProjectInvestMsg(m.ProjectTransactionId, m.InvestTime)); // 发送投资成功消息
            MessageBus.Main.Subscribe<ProjectFinancingFailMsg>(m => HandleProjectFinancingFailMsg(m.ProjectId)); // 发送流标的通知
        }

        private static void HandleProjectFinancingFailMsg(int projectId)
        {
            var context = new Agp2pDataContext();
            var project = context.li_projects.Single(p => p.id == projectId);

            // 查出所有已经退款的投资者 TODO 排除自己退款的投资者
            var investors = project.li_project_transactions.Where(
                ptr =>
                    ptr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                    ptr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Rollback)
                .GroupBy(ptr => ptr.dt_users).Select(g => g.Key);

            var sendTime = DateTime.Now;

            //找出模板
            var smsModel = context.dt_sms_template.SingleOrDefault(te => te.call_index == "project_financing_fail");
            if (smsModel == null) throw new InvalidOperationException("找不到流标提醒模板: project_financing_fail");

            var msgContent = smsModel.content
                .Replace("{project}", project.title)
                .Replace("{date}", sendTime.ToString("yyyy-MM-dd HH:mm"));


            // 通知投资者项目流标
            investors.ForEach(investor =>
            {
                try
                {
                    //发送站内消息
                    var userMsg = new dt_user_message
                    {
                        type = 1,
                        post_user_name = "",
                        accept_user_name = investor.user_name,
                        title = smsModel.title,
                        content = msgContent,
                        post_time = sendTime,
                        receiver = investor.id
                    };
                    context.dt_user_message.InsertOnSubmit(userMsg);
                    context.SubmitChanges();
                    var errorMsg = string.Empty;
                    if (!SMSHelper.SendTemplateSms(investor.mobile, msgContent, out errorMsg))
                    {
                        context.AppendAdminLogAndSave("ProjectFinancingFailSms",
                            string.Format("发送项目流标提醒失败：{0}（客户ID：{1}，项目名称：{2}）", errorMsg, investor.user_name, project.title));
                    }
                }
                catch (Exception ex)
                {
                    context.AppendAdminLogAndSave("ProjectFinancingFailSms",
                        string.Format("发送项目流标提醒失败：{0}（客户ID：{1}，项目名称：{2}）", ex.Message, investor.user_name, project.title));
                }
            });
        }

        /// <summary>
        /// 发送投资成功消息
        /// </summary>
        /// <param name="projectTransactionId"></param>
        /// <param name="investTime"></param>
        private static void HandleProjectInvestMsg(int projectTransactionId, DateTime investTime)
        {
            var context = new Agp2pDataContext();
            try
            {
                //找出项目投资信息
                var investment = context.li_project_transactions.Single(p => p.id == projectTransactionId);

                //发送电子合同
                siteconfig siteConfig = ConfigLoader.loadSiteConfig();
                //增加协议号到投资记录中
                investment.agree_no = investment.li_projects.dt_article_category.call_index.ToUpper() + Utils.GetOrderNumber();
                //获得投资协议邮件内容
                var bodytxt = context.GetInvestContractContext(investment, AppDomain.CurrentDomain.BaseDirectory + "\\tools\\invest-agreement.html");
                //发送投资协议邮件
                // TODO 新手体验标 不发投资协议到邮箱
                if (!string.IsNullOrWhiteSpace(investment.dt_users.email) && investment.li_projects.dt_article_category.call_index != "newbie")
                {
                    DTMail.sendMail(siteConfig.emailsmtp,
                        siteConfig.emailusername,
                        DESEncrypt.Decrypt(siteConfig.emailpassword),
                        siteConfig.emailnickname,
                        siteConfig.emailfrom,
                        investment.dt_users.email,
                        "安广融合投资协议", bodytxt);
                }

                // 检测用户是否接收放款的通知
                var sendNotificationSettings = context.li_notification_settings.Where(n => n.user_id == investment.investor)
                    .Select(n => n.type).Cast<Agp2pEnums.DisabledNotificationTypeEnum>();

                if (sendNotificationSettings.Contains(Agp2pEnums.DisabledNotificationTypeEnum.InvestSuccessForUserMsg))
                    return;
                else
                    context.SubmitChanges();

                //发送投资站内信息
                var dtSmsTemplate = context.dt_sms_template.FirstOrDefault(t => t.call_index == "invest_success");
                if (dtSmsTemplate == null) return;

                var content = dtSmsTemplate.content.Replace("{date}", investment.create_time.ToString("yyyy年MM月dd日HH时mm分"))
                    .Replace("{projectName}", investment.li_projects.title)
                    .Replace("{amount}", investment.principal.ToString("N"));

                var userMsg = new dt_user_message
                {
                    type = 1,
                    post_user_name = "",
                    accept_user_name = investment.dt_users.user_name,
                    title = dtSmsTemplate.title,
                    content = content,
                    post_time = investTime,
                    receiver = investment.investor
                };
                context.dt_user_message.InsertOnSubmit(userMsg);
                context.SubmitChanges();
            }
            catch (Exception ex)
            {
                context.AppendAdminLog("Invest", "发送投资成功消息时错误：" + ex.Message);
                context.SubmitChanges();
            }
        }

        /// <summary>
        /// 放款
        /// </summary>
        /// <param name="projectId"></param>
        private static void HandleProjectInvestCompletedMsg(int projectId)
        {
            //找出所有投资记录，计算收益（收益以还款记录为准，投资记录收益作为账单参考）
            var context = new Agp2pDataContext();
            var project = context.li_projects.SingleOrDefault(p => p.id == projectId);
            Debug.Assert(project != null, "project != null");

            //查询所有投资记录
            var investTrans =
                project.li_project_transactions.Where(
                    t =>
                        t.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        t.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success).ToList();

            var finalProfitRate = project.GetFinalProfitRate(DateTime.Now);
            investTrans.ForEach(pt =>
            {
                pt.interest = pt.principal * finalProfitRate;
            });
            context.SubmitChanges();

            //满标则发满标通知，否则发截标通知
            var dtSmsTemplate = project.financing_amount == project.investment_amount
                ? context.dt_sms_template.FirstOrDefault(t => t.call_index == "project_financing_success")
                : context.dt_sms_template.FirstOrDefault(t => t.call_index == "project_financing_success_cut");
            if (dtSmsTemplate == null) return;

            //发送通知给每个投资者
            investTrans.Select(ptr => ptr.dt_users).Distinct().ForEach(user =>
            {
                var msgContent = dtSmsTemplate.content.Replace("{date}", DateTime.Now.ToString("yyyy年MM月dd日"))
                    .Replace("{project}", project.title);
                try
                {
                    string errorMsg;
                    if (!SMSHelper.SendTemplateSms(user.mobile, msgContent, out errorMsg))
                    {
                        context.AppendAdminLogAndSave("WithdrawSms", "发送放款/截标通知失败：" + errorMsg + "（客户ID：" + user.user_name + "）");
                    }
                }
                catch (Exception ex)
                {
                    context.AppendAdminLogAndSave("WithdrawSms", "发送放款/截标通知失败：" + ex.Message + "（客户ID：" + user.user_name + "）");
                }
            });
        }
    }
}
