using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class ProjectStartRepaymentMsg : ITinyMessage
    {
        public int ProjectId { get; protected set; }
        public DateTime MakeLoanTime { get; protected set; }

        public ProjectStartRepaymentMsg(int projectId, DateTime makeLoanTime)
        {
            ProjectId = projectId;
            MakeLoanTime = makeLoanTime;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
