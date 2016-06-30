using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.AutoLogic
{
    class FinancingTimeout
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => CheckTimeoutProject(m.TimerType, m.OnTime)); // 每日定时检测募集超时项目，设置为超时状态
        }

        private static void CheckTimeoutProject(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.AutoRepayTimer) return;

            var db = new Agp2pDataContext();
            // 活期项目不会过时
            var timeoutProjects = db.li_projects.Where(
                p => p.dt_article_category.call_index != "huoqi" &&
                     p.status == (int) Agp2pEnums.ProjectStatusEnum.Financing && p.publish_time != null)
                .AsEnumerable()
                .Where(p => p.publish_time.Value.AddDays(p.financing_day).Date <= DateTime.Today).ToList();
            if (!timeoutProjects.Any()) return;

            timeoutProjects.ForEach(p =>
            {
                p.status = (int) Agp2pEnums.ProjectStatusEnum.FinancingTimeout;
                db.AppendAdminLog("AutoSetProjectTimeout", string.Format("项目 {0} 募集已超时", p.title));
            });
            db.SubmitChanges();

            timeoutProjects.ForEach(p =>
            {
                MessageBus.Main.Publish(new ProjectFinancingTimeoutMsg(p.id)); // 广播项目融资超时的消息
            });
        }
    }
}
