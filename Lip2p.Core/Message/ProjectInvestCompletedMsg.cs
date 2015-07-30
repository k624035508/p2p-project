using System;
using TinyMessenger;

namespace Lip2p.Core.Message
{
    class ProjectInvestCompletedMsg : ITinyMessage
    {
        public int ProjectId { get; protected set; }

        public ProjectInvestCompletedMsg(int projectId)
        {
            ProjectId = projectId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
