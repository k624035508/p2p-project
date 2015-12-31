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
            if (!shouldRepayTask.Any()) return;

            shouldRepayTask.ForEach(ta => context.ExecuteRepaymentTask(ta.id));
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
