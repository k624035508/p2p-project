using System;
using Agp2p.Linq2SQL;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class ProjectFinancingFailMsg : ITinyMessage
    {
        public int ProjectId { get; protected set; }

        public ProjectFinancingFailMsg(int projectId)
        {
            ProjectId = projectId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
