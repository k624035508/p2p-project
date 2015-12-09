using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Agp2p.Common
{
    public enum CallBackEnum
    {
        OnTime,
        OverTime
    }

    public class DailyTimer
    {
        private Timer aTimer;
        public DateTime nextTriggerTime { get; private set; }
        public bool Running { get; private set; }

        public DailyTimer(int hours, int minutes, int seconds, Action<CallBackEnum> callback, Action<Exception> crashAction)
        {
            Action<CallBackEnum> callBackWrapper = b =>
            {
                try
                {
                    callback(b);
                }
                catch (Exception ex)
                {
                    crashAction(ex);
                }
            };

            nextTriggerTime = DateTime.Today.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);
            if (nextTriggerTime < DateTime.Now)
            {
                nextTriggerTime = nextTriggerTime.AddDays(1);
                callBackWrapper(CallBackEnum.OverTime);
            }

            aTimer = new Timer(60*1000) { Enabled = true };
            Running = true;

            aTimer.Elapsed += (sender, args) =>
            {
                if (nextTriggerTime < args.SignalTime)
                {
                    nextTriggerTime = nextTriggerTime.AddDays(1);
                    callBackWrapper(CallBackEnum.OnTime);
                }
            };
        }

        public void Release()
        {
            Running = false;
            aTimer.Stop();
            aTimer.Dispose();
        }
    }
}
