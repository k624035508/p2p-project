using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class ProjectSchedulePublishMsg: ITinyMessage
    {
        public int ProjectId { get; protected set; }

        public ProjectSchedulePublishMsg(int projectId)
        {
            ProjectId = projectId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
