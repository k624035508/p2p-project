using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.AutoLogic
{
    class CheckOverTimePaid
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => DoCheckOverTimePaid(m.OnTime)); // 每日定时检测逾期还款，设置为逾期状态
        }

        private static void DoCheckOverTimePaid(bool onTime)
        {
            var db = new Agp2pDataContext();
            var overTime =
                db.li_repayment_tasks.Where(
                    r => r.status == (int) Agp2pEnums.RepaymentStatusEnum.Unpaid && DateTime.Now > r.should_repay_time)
                    .ToList();

            overTime.ForEach(o => o.status = (int)Agp2pEnums.RepaymentStatusEnum.OverTime);
            db.SubmitChanges();
        }
    }
}
