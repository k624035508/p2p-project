using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class ProjectRepaidMsg : ITinyMessage
    {
        public int RepaymentTaskId { get; protected set; }
        public bool IsProjectNeedComplete { get; protected set; }

        public ProjectRepaidMsg(int repaymentTaskId, bool projectNeedComplete)
        {
            RepaymentTaskId = repaymentTaskId;
            IsProjectNeedComplete = projectNeedComplete;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
