using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class UserInvestedMsg : ITinyMessage
    {
        public int ProjectTransactionId { get; protected set; }
        public DateTime InvestTime { get; protected set; }
        public bool IsDelayInvest { get; protected set; }

        public UserInvestedMsg(int projectTransactionId, DateTime investTime, bool isDelayInvest = false)
        {
            ProjectTransactionId = projectTransactionId;
            InvestTime = investTime;
            IsDelayInvest = isDelayInvest;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
