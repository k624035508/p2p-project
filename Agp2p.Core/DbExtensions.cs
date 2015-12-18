using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Core
{
    public static class DbExtensions
    {
        public static void AppendAdminLog(this Agp2pDataContext context, string actionType, string remark, bool sysLog = true, int userId = 1, string userName = "admin")
        {
            var dtManagerLog = new dt_manager_log
            {
                user_id = userId,
                user_name = userName,
                action_type = actionType,
                remark = remark,
                user_ip = sysLog ? "" : DTRequest.GetIP(),
                add_time = DateTime.Now
            };
            context.dt_manager_log.InsertOnSubmit(dtManagerLog);
        }

        public static void AppendAdminLogAndSave(this Agp2pDataContext context, string actionType, string remark, bool sysLog = true, int userId = 1, string userName = "admin")
        {
            context.AppendAdminLog(actionType, remark, sysLog, userId, userName);
            context.SubmitChanges();
        }

        public static void AppendAdminMessage(this Agp2pDataContext context, dt_manager receiver,
            Agp2pEnums.ManagerMessageSourceEnum source, string title, string body, DateTime createTime)
        {
            var newManagerMsg = new li_manager_messages
            {
                body = body,
                creationTime = createTime,
                title = title,
                source = (int)source,
                receiver = receiver.id
            };
            context.li_manager_messages.InsertOnSubmit(newManagerMsg);
        }

        public static void AppendAdminMessageAndSave(this Agp2pDataContext context, dt_manager receiver,
            Agp2pEnums.ManagerMessageSourceEnum source, string title, string body, DateTime createTime)
        {
            context.AppendAdminMessage(receiver, source, title, body, createTime);
            context.SubmitChanges();
        }

        public static string GetFriendlyUserName(this dt_users user)
        {
            if (!string.IsNullOrWhiteSpace(user.real_name))
            {
                return $"{user.user_name}（{user.real_name}）";
            }
            return !string.IsNullOrWhiteSpace(user.nick_name) ? $"{user.user_name}（{user.nick_name}）" : user.user_name;
        }
    }
}
