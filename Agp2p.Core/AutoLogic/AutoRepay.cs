using System;
using System.Collections.Generic;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;
using System.Web.UI.WebControls;
using Agp2p.Model;

namespace Agp2p.Core.AutoLogic
{
    public class AutoRepay
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => DoRepay(m.OnTime)); // 每日定时还款
        }

        private static void DoRepay(bool onTime)
        {
            if (ConfigLoader.loadSiteConfig().enableAutoRepay == 0) return;

            var context = new Agp2pDataContext();
            var shouldRepayTask = context.li_repayment_tasks.Where(
                t =>
                    t.status == (int) Agp2pEnums.RepaymentStatusEnum.Unpaid &&
                    t.should_repay_time.Date <= DateTime.Today).ToList();
            shouldRepayTask.ForEach(ta => context.ExecuteRepaymentTask(ta.id));
            if (shouldRepayTask.Any())
            {
                context.AppendAdminLogAndSave("AutoRepay", "今日待还款项目自动还款：" + shouldRepayTask.Count);
                SendRepayNotice(shouldRepayTask, context);
            }
        }

        /// <summary>
        /// 发布当天的兑付公告
        /// </summary>
        /// <param name="task"></param>
        /// <param name="context"></param>
        public static void SendRepayNotice(List<li_repayment_tasks> task, Agp2pDataContext context)
        {
            //找到兑付公告模板
            var temp = context.dt_mail_template.SingleOrDefault(te => te.call_index == "project_repay");
            if (temp != null)
            {
                //构造兑付项目表格
                string table = "<table class='table table-bordered repayment-tb'><tbody><tr><th>序号</th><th>项目名称</th><th>返回金额</th><th>返回本金</th><th>返回收益</th></tr></tbody><tbody>{tr}</tbody></table>";
                string tr = "<tr><td>{no}</td><td>{project_name}</td><td>{amount}</td><td>{principal}</td><td>{interest}</td></tr>";

                int no = 0;
                string tr_all = string.Empty;
                task.ForEach(t =>
                {
                    no++;
                    tr_all += tr.Replace("{no}", no.ToString())
                        .Replace("{project_name}", t.li_projects.title)
                        .Replace("{amount}", (t.repay_principal + t.repay_interest).ToString("c"))
                        .Replace("{principal}", t.repay_principal.ToString("c"))
                        .Replace("{interest}", t.repay_interest.ToString("c"));
                });
                table = table.Replace("{tr}", tr_all);

                var siteConfig = ConfigLoader.loadSiteConfig();
                var addTime = DateTime.Now;
                var content = temp.content.Replace("{today}", addTime.ToString("yyyy-MM-dd"))
                    .Replace("{count}", no.ToString())
                    .Replace("{table}", table)
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
