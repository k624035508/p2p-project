using System;
using TinyMessenger;

namespace Lip2p.Core.Message
{
    class UserInvestedMsg : ITinyMessage
    {
        public int ProjectTransactionId { get; protected set; }
        public DateTime InvestTime { get; protected set; }

        public UserInvestedMsg(int projectTransactionId, DateTime investTime)
        {
            ProjectTransactionId = projectTransactionId;
            InvestTime = investTime;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
