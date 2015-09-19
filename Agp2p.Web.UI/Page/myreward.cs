using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Linq2SQL;
using Agp2p.Common;
using LitJson;

namespace Agp2p.Web.UI.Page
{
    public class myreward : usercenter
    {
        private Agp2pDataContext context = new Agp2pDataContext();
        protected List<li_invitations> invitations;
        protected Dictionary<int, li_activity_transactions> myReward;

        protected int totalCount;
        protected int page;
        protected int pageSize = 10;

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
            totalCount = DTRequest.GetQueryInt("totalCount", 0);
            page = DTRequest.GetQueryInt("page", 1);

            // 查询自己的奖励
            myReward = context.li_activity_transactions.Where(tr => tr.activity_type == (int)Agp2pEnums.ActivityTransactionActivityTypeEnum.RefereeFirstTimeProfitBonus)
                .ToDictionary(t => (int)JsonMapper.ToObject(t.details)["Invitee"]);

            //查询成功邀请的人
            var query = context.li_invitations.Where(i => i.inviter == userModel.id);
            totalCount = query.Count();
            invitations = query.OrderByDescending(i => i.dt_users.reg_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
        }

        /// <summary>
        /// 获取用户名
        /// </summary>
        /// <param name="inv"></param>
        /// <returns></returns>
        protected string get_be_inviter_name(li_invitations inv)
        {
            var user = inv.dt_users;
            return string.IsNullOrEmpty(user.real_name) ? user.user_name : string.Format("{0}（{1}）", user.user_name, user.real_name);
        }

        protected string getFirstInvestAmount(li_invitations inv)
        {
            if (inv.li_project_transactions != null)
            {
                return inv.li_project_transactions.value.ToString("c");
            }
            return "";
        }

        protected string getMyReward(li_invitations inv)
        {
            if (myReward.ContainsKey(inv.user_id))
            {
                var atr = myReward[inv.user_id];
                return atr.value.ToString("c") + (atr.status == (int)Agp2pEnums.ActivityTransactionStatusEnum.Confirm ? "（已发放）" : "（待发放）");
            }
            return "";
        }

        protected string get_invite_code()
        {
            var userCode = context.dt_user_code.FirstOrDefault(u => u.user_id == userModel.id && u.type == DTEnums.CodeEnum.Register.ToString());
            //新增一个邀请码
            if (userCode == null)
            {
                var codeBll = new BLL.user_code();
                var strCode = Utils.GetCheckCode(8);
                var codeModel = new Model.user_code
                {
                    user_id = userModel.id,
                    user_name = userModel.user_name,
                    type = DTEnums.CodeEnum.Register.ToString(),
                    str_code = strCode,
                    eff_time = DateTime.Now.AddDays(1)
                };

                codeBll.Add(codeModel);
                return strCode;
            }
            return userCode.str_code;
        }
    }
}
