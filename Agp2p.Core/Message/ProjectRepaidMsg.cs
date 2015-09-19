using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class ProjectRepaidMsg : ITinyMessage
    {
        public int RepaymentTaskId { get; protected set; }

        public ProjectRepaidMsg(int repaymentTaskId)
        {
            RepaymentTaskId = repaymentTaskId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
