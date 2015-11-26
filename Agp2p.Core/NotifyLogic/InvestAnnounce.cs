using System;
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
            MessageBus.Main.Subscribe<ProjectInvestCompletedMsg>(m => HandleProjectInvestCompletedMsg(m.ProjectId)); // 计算每个投资者账单收益
            MessageBus.Main.Subscribe<UserInvestedMsg>(m => HandleProjectInvestMsg(m.ProjectTransactionId, m.InvestTime)); // 发送投资成功消息
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
                if (!string.IsNullOrWhiteSpace(investment.dt_users.email))
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
                    .Select(n => n.type).Cast<Agp2pEnums.NotificationTypeEnum>();

                if (!sendNotificationSettings.Contains(Agp2pEnums.NotificationTypeEnum.InvestSuccessForUserMsg))
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
        /// 满标
        /// </summary>
        /// <param name="projectId"></param>
        private static void HandleProjectInvestCompletedMsg(int projectId)
        {
            //找出所有投资记录，计算收益（收益以还款记录为准，投资记录收益作为账单参考）
            var context = new Agp2pDataContext();
            var project = context.li_projects.SingleOrDefault(p => p.id == projectId);
            Debug.Assert(project != null, "project != null");
            project.li_project_transactions.ForEach(pt =>
            {
                pt.interest = pt.principal*project.GetFinalProfitRate(DateTime.Now);
            });
            context.SubmitChanges();

            //TODO 是否发送放款通知
        }
    }
}
