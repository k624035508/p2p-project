using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class ProjectFinancingTimeoutMsg : ITinyMessage
    {
        public int ProjectId { get; protected set; }

        public ProjectFinancingTimeoutMsg(int projectId)
        {
            ProjectId = projectId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
