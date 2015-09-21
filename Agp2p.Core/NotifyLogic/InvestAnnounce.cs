using System;
using System.Linq;
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
            MessageBus.Main.Subscribe<ProjectInvestCompletedMsg>(m => HandleProjectInvestCompletedMsg(m.ProjectId)); // 发送电子合同
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
            //找出项目投资信息
            var investment = context.li_project_transactions.Single(p => p.id == projectTransactionId);
            //发送投资站内信息
            var userMsg = new dt_user_message
            {
                type = 1,
                post_user_name = "",
                accept_user_name = investment.dt_users.user_name,
                title = "投标成功",
                content = string.Format("您于{0}投资了“{1}”，投资金额为{2}元。详情请到“投资列表”中查询。",
                    DateTime.Now.ToString("yyyy年MM月dd日HH时mm分"),
                    investment.li_projects.title,
                    investment.principal.ToString("N")),
                post_time = investTime,
            };
            context.dt_user_message.InsertOnSubmit(userMsg);
            context.SubmitChanges();
        }

        /// <summary>
        /// 满标后发送电子合同
        /// </summary>
        /// <param name="projectId"></param>
        private static void HandleProjectInvestCompletedMsg(int projectId)
        {
            siteconfig siteConfig = ConfigLoader.loadSiteConfig();
            var context = new Agp2pDataContext();
            //找出项目信息
            var project = context.li_projects.Single(p => p.id == projectId);
            try
            {
                //获得投资协议邮件内容
                var mailModel = context.dt_mail_template.SingleOrDefault(te => te.call_index == "user_invest_agree");
                if (mailModel == null) throw new InvalidOperationException("发送投资协议时没有找到电子邮件模版：user_invest_agree");
                //找出投资了该项目的所有会员
                var userInvestList = context.li_project_transactions.Where(t => t.project == projectId).ToList();
                userInvestList.ForEach(ui =>
                {
                    //替换模板内容
                    string titletxt = mailModel.maill_title;
                    string bodytxt = mailModel.content;
                    //甲方信息
                    bodytxt = bodytxt.Replace("{jiafang_name}", project.li_risks != null && project.li_risks.li_creditors != null ? project.li_risks.li_creditors.real_name : "");
                    bodytxt = bodytxt.Replace("{jiafang_id_card}", project.li_risks != null && project.li_risks.li_creditors != null ? project.li_risks.li_creditors.id_card_number : "");
                    //项目信息
                    bodytxt = bodytxt.Replace("{project_name}", project.title);
                    bodytxt = bodytxt.Replace("{project_rate}", project.profit_rate_year.ToString());
                    bodytxt = bodytxt.Replace("{project_date_manbiao}", project.invest_complete_time != null ? string.Format("{0:yyyy年MM月dd日}", project.invest_complete_time) : "");
                    bodytxt = bodytxt.Replace("{project_date_wancheng}", string.Format("{0:yyyy年MM月dd日}", project.li_repayment_tasks.Max(t => t.should_repay_time)));
                    //乙方信息
                    bodytxt = bodytxt.Replace("{yifang_name}", ui.dt_users.real_name);
                    bodytxt = bodytxt.Replace("{yifang_user_name}", ui.dt_users.user_name);
                    bodytxt = bodytxt.Replace("{yifang_id_card}", ui.dt_users.id_card_number);
                    bodytxt = bodytxt.Replace("{yifang_email}", ui.dt_users.email);                               
                    //投资信息
                    var agree_no = Utils.GetOrderNumber();
                    bodytxt = bodytxt.Replace("{agree_no}", agree_no);
                    bodytxt = bodytxt.Replace("{project_amount}", ui.principal.ToString("N"));
                    //增加协议号到投资记录中
                    ui.agree_no = agree_no;             
                    //发送邮件
                    if (!string.IsNullOrWhiteSpace(ui.dt_users.email))
                    {
                        DTMail.sendMail(siteConfig.emailsmtp,
                            siteConfig.emailusername,
                            DESEncrypt.Decrypt(siteConfig.emailpassword),
                            siteConfig.emailnickname,
                            siteConfig.emailfrom,
                            ui.dt_users.email,
                            titletxt, bodytxt);
                    }
                });
                context.SubmitChanges();
            }
            catch(Exception ex)
            {
                context.AppendAdminLog("Invest", "满标发送电子协议错误：" + ex.Message);
                context.SubmitChanges();
            }
        }
    }
}
