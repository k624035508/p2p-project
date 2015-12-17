using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class ProjectStartRepaymentMsg : ITinyMessage
    {
        public int ProjectId { get; protected set; }

        public ProjectStartRepaymentMsg(int projectId)
        {
            ProjectId = projectId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
