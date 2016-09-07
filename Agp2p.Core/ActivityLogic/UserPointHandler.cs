using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.UserPointMsg;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.ActivityLogic
{
    internal class UserPointHandler
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<UserPointMsg>(HandleUserSignMsg);
        }

        /// <summary>
        /// 会员积分处理
        /// </summary>
        private static void HandleUserSignMsg(UserPointMsg userPointMsg)
        {
            switch (userPointMsg.Type)
            {
                case (int)Agp2pEnums.PointEnum.Register:
                    userPointMsg.Point = 38;
                    userPointMsg.Remark = "注册送积分";
                    break;
                case (int)Agp2pEnums.PointEnum.RealNameAuth:
                    userPointMsg.Point = 38;
                    userPointMsg.Remark = "实名认证";
                    break;
                case (int)Agp2pEnums.PointEnum.BindingEmail:
                    userPointMsg.Point = 28;
                    userPointMsg.Remark = "绑定邮箱";
                    break;
                case (int)Agp2pEnums.PointEnum.BindingBank:
                    userPointMsg.Point = 38;
                    userPointMsg.Remark = "绑定银行卡";
                    break;
                case (int)Agp2pEnums.PointEnum.Recharge:
                    userPointMsg.Point = 50;
                    break;
                case (int)Agp2pEnums.PointEnum.Invest:
                    
                    userPointMsg.Remark = "投资";
                    break;
                case (int)Agp2pEnums.PointEnum.FirstInvest:
                    userPointMsg.Point = 368;
                    userPointMsg.Remark = "首次投资";
                    break;
                case (int)Agp2pEnums.PointEnum.InviteRegister:
                    userPointMsg.Point = 888;
                    break;
                case (int)Agp2pEnums.PointEnum.InviteInvest:
                    userPointMsg.Point = 50;
                    break;
                case (int)Agp2pEnums.PointEnum.LotteryGet:
                    userPointMsg.Remark = "积分抽奖获取积分";
                    break;
                case (int)Agp2pEnums.PointEnum.Lottery:
                    userPointMsg.Point = -80;
                    userPointMsg.Remark = "积分抽奖消耗";
                    break;
            }

            var context  = new Agp2pDataContext();
            
            var user = context.dt_users.SingleOrDefault(u => u.id == userPointMsg.UserId);
            if (user != null)
            {
                var dtUserPointLog = new dt_user_point_log
                {
                    user_id = userPointMsg.UserId,
                    user_name = userPointMsg.UserName,
                    add_time = DateTime.Now,
                    value = userPointMsg.Point,
                    type = (short) userPointMsg.Type,
                    remark = userPointMsg.Remark
                };
                context.dt_user_point_log.InsertOnSubmit(dtUserPointLog);
                user.point += userPointMsg.Point;
                context.SubmitChanges();
            }
            
        }
    }
}
