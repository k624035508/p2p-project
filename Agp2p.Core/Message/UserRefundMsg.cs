using System;
using Agp2p.Linq2SQL;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class UserRefundMsg : ITinyMessage
    {
        public int ProjectTransactionId { get; protected set; }

        public UserRefundMsg(int projectTransactionId)
        {
            ProjectTransactionId = projectTransactionId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
