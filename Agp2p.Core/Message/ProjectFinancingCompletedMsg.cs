using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class ProjectFinancingCompletedMsg : ITinyMessage
    {
        public int ProjectId { get; protected set; }

        public ProjectFinancingCompletedMsg(int projectId)
        {
            ProjectId = projectId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
