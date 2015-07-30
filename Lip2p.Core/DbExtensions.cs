using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Core
{
    public static class DbExtensions
    {
        public static void AppendAdminLog(this Lip2pDataContext context, string actionType, string remark, int userId = 1, string userName = "admin")
        {
            var dtManagerLog = new dt_manager_log
            {
                user_id = userId,
                user_name = userName,
                action_type = actionType,
                remark = remark,
                user_ip = DTRequest.GetIP(),
                add_time = DateTime.Now
            };
            context.dt_manager_log.InsertOnSubmit(dtManagerLog);
        }

        public static void AppendAdminLogAndSave(this Lip2pDataContext context, string actionType, string remark, int userId = 1, string userName = "admin")
        {
            context.AppendAdminLog(actionType, remark, userId, userName);
            context.SubmitChanges();
        }
    }
}
