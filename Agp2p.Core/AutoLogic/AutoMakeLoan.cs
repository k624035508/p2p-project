using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.AutoLogic
{
    /// <summary>
    /// 每日自动放款（活期）
    /// </summary>
    class AutoMakeLoan
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<TimerMsg>(m => DoAutoMakeLoan(m.TimerType, m.OnTime)); // 每日定时检测逾期还款，设置为逾期状态
        }

        private static void DoAutoMakeLoan(TimerMsg.Type timerType, bool onTime)
        {
            if (timerType != TimerMsg.Type.AutoMakeLoanTimer) return;

            var db = new Agp2pDataContext();
            
        }
    }
}
