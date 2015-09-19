using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class ProjectRepayCompletedMsg : ITinyMessage
    {
        public int ProjectId { get; protected set; }
        public DateTime ProjectCompleteTime { get; protected set; }

        public ProjectRepayCompletedMsg(int projectId, DateTime completeTime)
        {
            ProjectId = projectId;
            ProjectCompleteTime = completeTime;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
