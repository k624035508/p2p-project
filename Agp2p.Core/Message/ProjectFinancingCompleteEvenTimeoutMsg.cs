using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class ProjectFinancingCompleteEvenTimeoutMsg : ITinyMessage
    {
        public int ProjectId { get; protected set; }

        public ProjectFinancingCompleteEvenTimeoutMsg(int projectId)
        {
            ProjectId = projectId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
