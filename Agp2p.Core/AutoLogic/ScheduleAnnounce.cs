using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.AutoLogic
{
    class ScheduleAnnounce
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<ProjectSchedulePublishMsg>(m => HandleProjectSchedulePublishMsg(m.ProjectId)); // 改变项目成为发标状态
        }

        /// <summary>
        /// 改变项目成为发标状态
        /// </summary>
        /// <param name="projectId"></param>
        private static void HandleProjectSchedulePublishMsg(int projectId)
        {
            var context = new Agp2pDataContext();
            var project = context.li_projects.FirstOrDefault(p => p.id == projectId);
            if (project != null)
            {
                project.status = (int)Agp2pEnums.ProjectStatusEnum.Financing;
                context.SubmitChanges();
            }
        }

    }
}
