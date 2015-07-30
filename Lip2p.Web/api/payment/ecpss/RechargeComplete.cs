using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lip2p.Linq2SQL;
using Lip2p.BLL;
using Lip2p.Core;

namespace Lip2p.Web.api.payment.ecpss
{
    public class RechargeComplete
    {
        public static void DoRecharge(Lip2pDataContext context, li_bank_transactions order)
        {
            context.ConfirmBankTransaction(order.id, null);            
            //短信通知
            var user = context.dt_users.FirstOrDefault(u => u.id == order.charger);
            if (user != null)
            {
                //找出模板
                var smsModel = new sms_template().GetModel("user_recharge_info");
                if (smsModel != null)
                {
                    //替换模板内容
                    //var siteConfig = new BLL.siteconfig().loadConfig();
                    var msgContent = smsModel.content;
                    msgContent = msgContent.Replace("{amount}", order.value.ToString());

                    string errorMsg = string.Empty;
                    if (!SMSHelper.SendTemplateSms(user.mobile, msgContent, out errorMsg))
                    {
                        new manager_log().Add(1, "admin", "ReChargeSms", "发送充值提醒失败：" + errorMsg + "（客户ID：" + user.user_name + "，订单号：" + order.no_order + "）");
                    }
                }
            }
        }
    }
}