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
            MessageBus.Main.Subscribe<BankTransactionCreatedMsg>(t => HandleBankTransactionCreatedMsg(t.Transaction));
            MessageBus.Main.Subscribe<BankTransactionFinishedMsg>(t => HandleBankTransactionFinishedMsg(t.Transaction));
        }

        private static void HandleBankTransactionCreatedMsg(li_bank_transactions tr)
        {
            if (tr.status != (int)Agp2pEnums.BankTransactionStatusEnum.Acting)
                throw new Exception("交易已经完成，不应该触发此事件");

            switch ((Agp2pEnums.BankTransactionTypeEnum)tr.type)
            {
                case Agp2pEnums.BankTransactionTypeEnum.Charge:
                    // 没有充值申请的通知类型
                    break;
                case Agp2pEnums.BankTransactionTypeEnum.Withdraw:
                    var notificationTypesForWithdrawer =
                        tr.li_bank_accounts.dt_users.li_notification_settings.AsEnumerable()
                            .Select(n => (Agp2pEnums.NotificationTypeEnum)n.type)
                            .ToList();
                    var sendWithdrawApplySms = notificationTypesForWithdrawer.Contains(Agp2pEnums.NotificationTypeEnum.WithdrawApplyForSms);
                    var sendWithdrawApplyUserMsg = notificationTypesForWithdrawer.Contains(Agp2pEnums.NotificationTypeEnum.WithdrawApplyForUserMsg);

                    if (sendWithdrawApplySms || sendWithdrawApplyUserMsg)
                    {
                        sendWithdrawApplyNotification(tr, sendWithdrawApplySms, sendWithdrawApplyUserMsg);
                    }
                    break;
                default:
                    throw new Exception("设置了不存在的银行交易类型");
            }
        }

        private static void sendWithdrawApplyNotification(li_bank_transactions tr, bool sendSms, bool sendUserMsg)
        {
            var context = new Agp2pDataContext();
            //短信通知
            var withdrawer = tr.li_bank_accounts.dt_users;

            //找出模板
            var dtSmsTemplate = context.dt_sms_template.FirstOrDefault(t => t.call_index == "user_withdraw_info");
            if (dtSmsTemplate == null) return;

            //替换模板内容
            var msgContent = dtSmsTemplate.content.Replace("{amount}", tr.value.ToString())
                .Replace("{user_name}", withdrawer.user_name)
                .Replace("{date}", DateTime.Today.ToString("yyyy-MM-dd HH:mm"));

            if (sendSms)
            {
                try
                {
                    string errorMsg;
                    if (!SMSHelper.SendTemplateSms(withdrawer.mobile, msgContent, out errorMsg))
                    {
                        context.AppendAdminLogAndSave("WithdrawSms", "发送提现提醒失败：" + errorMsg + "（客户ID：" + withdrawer.user_name + "，订单号：" + tr.no_order + "）");
                    }
                }
                catch (Exception ex)
                {
                    context.AppendAdminLogAndSave("WithdrawSms", "发送充值提醒失败：" + ex.Message + "（客户ID：" + withdrawer.user_name + "，订单号：" + tr.no_order + "）");
                }
            }
            if (sendUserMsg)
            {
                var message = new dt_user_message
                {
                    type = 1,
                    accept_user_name = withdrawer.user_name,
                    content = msgContent,
                    receiver = withdrawer.id,
                    title = dtSmsTemplate.title,
                    post_time = tr.transact_time.GetValueOrDefault(tr.create_time)
                };
                context.dt_user_message.InsertOnSubmit(message);
                context.SubmitChanges();
            }
        }

        private static void HandleBankTransactionFinishedMsg(li_bank_transactions tr)
        {
            if (tr.status == (int) Agp2pEnums.BankTransactionStatusEnum.Acting)
                throw new Exception("交易尚未完成，不应该触发此事件");

            switch ((Agp2pEnums.BankTransactionTypeEnum)tr.type)
            {
                case Agp2pEnums.BankTransactionTypeEnum.Charge:
                    if (tr.status == (int) Agp2pEnums.BankTransactionStatusEnum.Confirm)
                    {
                        var notificationTypesForCharger =
                            tr.dt_users.li_notification_settings.AsEnumerable()
                                .Select(n => (Agp2pEnums.NotificationTypeEnum)n.type)
                                .ToList();
                        var sendChargeSuccessSMS = notificationTypesForCharger.Contains(Agp2pEnums.NotificationTypeEnum.ChargeSuccessForSms);
                        var sendChargeSuccessUserMsg = notificationTypesForCharger.Contains(Agp2pEnums.NotificationTypeEnum.ChargeSuccessForUserMsg);

                        if (sendChargeSuccessSMS || sendChargeSuccessUserMsg)
                            SendChargeSuccessNotification(tr, sendChargeSuccessUserMsg, sendChargeSuccessSMS);
                    }
                    else if (tr.status == (int) Agp2pEnums.BankTransactionStatusEnum.Cancel)
                    {
                        // 没有充值失败的通知类型
                    }
                    else throw new Exception("设置了不存在的银行交易状态");
                    break;
                case Agp2pEnums.BankTransactionTypeEnum.Withdraw:
                    /*var notificationTypesForWithdrawer =
                        tr.li_bank_accounts.dt_users.li_notification_settings.AsEnumerable()
                            .Select(n => (Agp2pEnums.NotificationTypeEnum)n.type)
                            .ToList();*/
                    if (tr.status == (int)Agp2pEnums.BankTransactionStatusEnum.Confirm)
                    {
                        // 没有提现成功的通知类型
                    }
                    else if (tr.status == (int)Agp2pEnums.BankTransactionStatusEnum.Cancel)
                    {
                        // 没有提现失败的通知类型
                    }
                    else throw new Exception("设置了不存在的银行交易状态");
                    break;
                default:
                    throw new Exception("设置了不存在的银行交易类型");
            }
        }

        private static void SendChargeSuccessNotification(li_bank_transactions tr, bool sendUserMsg, bool sendSms)
        {
            var context = new Agp2pDataContext();
            //短信通知
            var charger = tr.dt_users;

            //找出模板
            var dtSmsTemplate = context.dt_sms_template.FirstOrDefault(t => t.call_index == "user_recharge_info");
            if (dtSmsTemplate == null) return;

            var siteConfig = ConfigLoader.loadSiteConfig(false);

            //替换模板内容
            var msgContent = dtSmsTemplate.content.Replace("{amount}", tr.value.ToString())
                .Replace("{webtel}", siteConfig.webtel);

            if (sendSms)
            {
                try
                {
                    string errorMsg;
                    if (!SMSHelper.SendTemplateSms(charger.mobile, msgContent, out errorMsg))
                    {
                        context.AppendAdminLogAndSave("ReChargeSms", "发送充值提醒失败：" + errorMsg + "（客户ID：" + charger.user_name + "，订单号：" + tr.no_order + "）");
                    }
                }
                catch (Exception ex)
                {
                    context.AppendAdminLogAndSave("ReChargeSms", "发送充值提醒失败：" + ex.Message + "（客户ID：" + charger.user_name + "，订单号：" + tr.no_order + "）");
                }
            }
            if (sendUserMsg)
            {
                var message = new dt_user_message
                {
                    type = 1,
                    accept_user_name = charger.user_name,
                    content = msgContent,
                    receiver = charger.id,
                    title = dtSmsTemplate.title,
                    post_time = tr.transact_time.GetValueOrDefault(tr.create_time)
                };
                context.dt_user_message.InsertOnSubmit(message);
                context.SubmitChanges();
            }
        }
    }
}
