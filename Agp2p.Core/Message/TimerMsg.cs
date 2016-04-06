using System;
using System.ComponentModel;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class TimerMsg : ITinyMessage
    {
        public enum Type
        {
            [Description("自动回款")]
            AutoRepayTimer = 1,
            [Description("借款人还款")]
            LoanerRepayTimer = 2
        }

        public bool OnTime { get; protected set; }
        public Type TimerType { get; protected set; }

        public TimerMsg(Type timerType, bool onTime)
        {
            TimerType = timerType;
            OnTime = onTime;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
