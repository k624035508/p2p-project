using System.Linq;
using Lip2p.Common;
using Lip2p.Core.Message;
using Lip2p.Linq2SQL;

namespace Lip2p.Core.AutoLogic
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
            var context = new Lip2pDataContext();
            var project = context.li_projects.FirstOrDefault(p => p.id == projectId);
            if (project != null)
            {
                project.status = (int)Lip2pEnums.ProjectStatusEnum.Financing;
                context.SubmitChanges();
            }
        }

    }
}
