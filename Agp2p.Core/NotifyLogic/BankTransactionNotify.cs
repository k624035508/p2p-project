using System;
using System.Collections.Generic;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.NotifyLogic
{
    class BankTransactionNotify
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<BankTransactionFinishedMsg>(t => HandleBankTransactionFinishedMsg(t.TransactionId)); // 发送电子合同
        }

        private static void HandleBankTransactionFinishedMsg(int trId)
        {
            var context = new Agp2pDataContext();
            var tr = context.li_bank_transactions.Single(t => t.id == trId);
            if (tr.status == (int) Agp2pEnums.BankTransactionStatusEnum.Acting)
                throw new Exception("交易尚未完成，不应该触发此事件");

            switch ((Agp2pEnums.BankTransactionTypeEnum)tr.type)
            {
                case Agp2pEnums.BankTransactionTypeEnum.Charge:
                    var notificationTypesForCharger =
                        context.li_notification_settings.Where(n => n.user_id == tr.charger).AsEnumerable()
                            .Select(n => (Agp2pEnums.NotificationTypeEnum) n.type)
                            .ToList();
                    if (tr.status == (int) Agp2pEnums.BankTransactionStatusEnum.Confirm)
                    {
                        var sendChargeSuccessSMS = notificationTypesForCharger.Contains(Agp2pEnums.NotificationTypeEnum.ChargeSuccessForSms);
                        var sendChargeSuccessUserMsg = notificationTypesForCharger.Contains(Agp2pEnums.NotificationTypeEnum.ChargeSuccessForUserMsg);
                        if (sendChargeSuccessSMS)
                        {
                            SendChargeSuccessSms(context, tr);
                        }
                    }
                    else if (tr.status == (int) Agp2pEnums.BankTransactionStatusEnum.Cancel)
                    {
                        // 没有充值失败的通知类型
                    }
                    else throw new Exception("设置了不存在的银行交易状态");
                    break;
                case Agp2pEnums.BankTransactionTypeEnum.Withdraw:
                    var notificationTypesForWithdrawer =
                        context.li_notification_settings.Where(n => n.user_id == tr.li_bank_accounts.owner).AsEnumerable()
                            .Select(n => (Agp2pEnums.NotificationTypeEnum) n.type)
                            .ToList();
                    if (tr.status == (int)Agp2pEnums.BankTransactionStatusEnum.Confirm)
                    {
                        var sendWithdrawSuccessSMS = notificationTypesForWithdrawer.Contains(Agp2pEnums.NotificationTypeEnum.WithdrawSuccessForSms);
                        var sendWithdrawSuccessUserMsg = notificationTypesForWithdrawer.Contains(Agp2pEnums.NotificationTypeEnum.WithdrawSuccessForUserMsg);
                    }
                    else if (tr.status == (int)Agp2pEnums.BankTransactionStatusEnum.Cancel)
                    {
                        var sendWithdrawFailureSMS = notificationTypesForWithdrawer.Contains(Agp2pEnums.NotificationTypeEnum.WithdrawFailureForSms);
                        var sendWithdrawFailureUserMsg = notificationTypesForWithdrawer.Contains(Agp2pEnums.NotificationTypeEnum.WithdrawFailureForUserMsg);
                    }
                    else throw new Exception("设置了不存在的银行交易状态");
                    break;
                default:
                    throw new Exception("设置了不存在的银行交易类型");
            }
        }

        private static void SendChargeSuccessSms(Agp2pDataContext context, li_bank_transactions tr)
        {
            //短信通知
            var charger = tr.dt_users;

            //找出模板
            var dtSmsTemplate = context.dt_sms_template.FirstOrDefault(t => t.call_index == "user_recharge_info");
            if (dtSmsTemplate == null) return;

            //替换模板内容
            var msgContent = dtSmsTemplate.content.Replace("{amount}", tr.value.ToString());

            string errorMsg;
            if (!SMSHelper.SendTemplateSms(charger.mobile, msgContent, out errorMsg))
            {
                context.AppendAdminLogAndSave("ReChargeSms", "发送充值提醒失败：" + errorMsg + "（客户ID：" + charger.user_name + "，订单号：" + tr.no_order + "）");
            }
        }
    }
}
