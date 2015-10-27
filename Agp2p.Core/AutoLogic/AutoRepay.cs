using System;
using System.Collections.Generic;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;
using System.Web.UI.WebControls;

namespace Agp2p.Core.AutoLogic
{
    class AutoRepay
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
                context.AppendAdminLogAndSave("AutoRepay", "今日待放款项目自动放款：" + shouldRepayTask.Count);
                SendRepayNotice(shouldRepayTask, context);
            }
        }

        /// <summary>
        /// 发布当天的兑付公告
        /// </summary>
        /// <param name="task"></param>
        /// <param name="context"></param>
        private static void SendRepayNotice(List<li_repayment_tasks> task, Agp2pDataContext context)
        {
            //找到兑付公告模板
            var temp = context.dt_mail_template.SingleOrDefault(te => te.call_index == "project_repay");
            if (temp != null)
            {
                //构造兑付项目表格
                string table = "<table style=\"color:#000000;font-family:'Microsoft YaHei', 'Heiti SC', simhei, 'Lucida Sans Unicode', 'Myriad Pro', 'Hiragino Sans GB', Verdana;font-size:13px;background-color:#FFFFFF;\"><tbody><tr><th><p style=\"text-indent:2em;\">序号</p></th><th><p style=\"text-indent:2em;\">项目名称</p></th><th><p style=\"text-align:left;text-indent:2em;\">返回金额</p></th><th><p style=\"text-indent:2em;\">返回本金</p></th><th><p style=\"text-indent:2em;\">返回收益</p></th></tr></tbody><tbody>{tr}</tbody></table>";
                string tr = "<tr><td><p style=\"text-indent:2em;\">{no}</p></td><td><p style=\"text-indent:2em;\">{project_name}</p></td><td><p style=\"text-indent:2em;\">{amount}</p></td><td><p style=\"text-indent:2em;\">{principal}</p></td><td><p style=\"text-indent:2em;\">{interest}</p></td></tr>";

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

                var add_time = DateTime.Now;
                var content = temp.content.Replace("{today}", add_time.ToString("yyyy-MM-dd"))
                    .Replace("{count}", no.ToString())
                    .Replace("{table}", table);

                //创建公告
                try
                {
                    var notice = new dt_article();
                    notice.add_time = add_time;
                    notice.category_id = 43;
                    notice.channel_id = 5;
                    notice.title = add_time.ToString("yyyy年M月d日") + "项目兑付公告";
                    notice.seo_title = notice.title;
                    notice.seo_keywords = "丰车赚,金汇丰p2p,项目兑付公告";
                    notice.content = content;
                    
                    var notice_attr = new dt_article_attribute_value();
                    notice_attr.dt_article = notice;
                    notice_attr.author = "金汇丰";
                    notice_attr.source = "金汇丰理财平台";

                    context.dt_article.InsertOnSubmit(notice);
                    context.dt_article_attribute_value.InsertOnSubmit(notice_attr);
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
