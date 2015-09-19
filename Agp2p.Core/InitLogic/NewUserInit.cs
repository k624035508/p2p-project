using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.InitLogic
{
    class NewUserInit
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<NewUserCreatedMsg>(m => NewUser(m.UserId, m.RegTime)); // 新用户创建钱包
        }

        /// <summary>
        /// 有新用户注册回调
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="regTime"></param>
        private static void NewUser(int userId, DateTime regTime)
        {
            var context = new Agp2pDataContext();
            var newUser = context.dt_users.Single(u => u.id == userId);

            // 创建钱包
            var wallet = new li_wallets { user_id = userId, last_update_time = regTime };
            context.li_wallets.InsertOnSubmit(wallet);

            // 创建自己的邀请码
            var codeModel = new dt_user_code
            {
                dt_users = newUser,
                user_name = newUser.user_name,
                type = DTEnums.CodeEnum.Register.ToString(),
                str_code = Utils.GetCheckCode(8), //获取邀请码
                eff_time = newUser.reg_time.Value,
                add_time = newUser.reg_time.Value
            };
            context.dt_user_code.InsertOnSubmit(codeModel);

            // 注册送积分
            var dtUserPointLog = new dt_user_point_log
            {
                user_id = newUser.id,
                user_name = newUser.user_name,
                add_time = newUser.reg_time,
                value = newUser.dt_user_groups.point,
                remark = "注册赠送积分"
            };
            context.dt_user_point_log.InsertOnSubmit(dtUserPointLog);

            // 发站内信
            var dtUserMessage = new dt_user_message
            {
                type = 1,
                accept_user_name = newUser.user_name,
                title = "欢迎您成为本站会员",
                content = ConfigLoader.loadUserConfig().regmsgtxt,
                post_user_name = "",
                post_time = newUser.reg_time.Value
            };
            context.dt_user_message.InsertOnSubmit(dtUserMessage);

            context.SubmitChanges();
        }

    }
}
