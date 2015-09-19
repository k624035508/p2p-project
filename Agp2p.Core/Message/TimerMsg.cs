using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class TimerMsg : ITinyMessage
    {
        public bool OnTime { get; protected set; }

        public TimerMsg(bool onTime)
        {
            OnTime = onTime;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
