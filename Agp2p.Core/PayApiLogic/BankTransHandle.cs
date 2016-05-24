using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Agp2p.Core.Message.PayApiMsg.Transaction;
using Agp2p.Linq2SQL;

namespace Agp2p.Core.PayApiLogic
{
    /// <summary>
    /// 个人资金账户接口响应
    /// </summary>
    internal class BankTransHandle
    {
        internal static void DoSubscribe()
        {
            MessageBus.Main.Subscribe<RechargeRespMsg>(Recharge);
            MessageBus.Main.Subscribe<WithholdingRechargeRespMsg>(WithholdingRecharge);
            MessageBus.Main.Subscribe<WithdrawRespMsg>(WithDraw);
        }

        /// <summary>
        /// 网银充值 响应
        /// </summary>
        /// <param name="msg"></param>
        private static void Recharge(RechargeRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的交易流水
                        var trans = context.li_bank_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                        if (trans != null)
                        {
                            if (trans.status == (int)Agp2p.Common.Agp2pEnums.BankTransactionStatusEnum.Acting)
                            {
                                context.ConfirmBankTransaction(trans.id, null);
                                //TODO 检查用户资金信息
                                msg.HasHandle = true;
                            }
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台交易流水记录，交易流水号为：" + msg.RequestId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 一键充值 响应
        /// </summary>
        /// <param name="msg"></param>
        private static void WithholdingRecharge(WithholdingRechargeRespMsg msg)
        {
            try
            {
                //检查请求处理结果
                if (msg.CheckResult())
                {
                    //检查签名
                    if (msg.CheckSignature())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的交易流水
                        var trans = context.li_bank_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                        if (trans != null)
                        {
                            if (trans.status == (int)Agp2p.Common.Agp2pEnums.BankTransactionStatusEnum.Acting)
                            {
                                context.ConfirmBankTransaction(trans.id, null);
                                //一键充值后自动更新银行卡类型（同卡进出只能使用绑定卡提现）
                                var charger = trans.dt_users;
                                var bindCardLastNo = msg.BankAccount.Substring(msg.BankAccount.Length - 4, 4);//快捷充值卡的后四位
                                var bindCardFristNo = msg.BankAccount.Substring(0, 4);//快捷充值卡的前四位 
                                if (charger.li_bank_accounts.Any())
                                {
                                    charger.li_bank_accounts.ForEach(b =>
                                    {
                                        if (b.account.Substring(0, 4).Equals(bindCardFristNo) &&
                                            b.account.Substring(b.account.Length - 4, 4).Equals(bindCardLastNo))
                                        {
                                            b.type = (int)Agp2pEnums.BankAccountType.QuickPay;
                                        }
                                        else
                                        {
                                            b.type = (int) Agp2pEnums.BankAccountType.WebBank;
                                        }
                                    });
                                    context.SubmitChanges();
                                }

                                //TODO 检查用户资金信息 
                                msg.HasHandle = true;
                            }
                        }
                        else
                        {
                            msg.Remarks = "没有找到平台交易流水记录，交易流水号为：" + msg.RequestId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }

        /// <summary>
        /// 提现响应处理
        /// </summary>
        /// <param name="msg"></param>
        private static void WithDraw(WithdrawRespMsg msg)
        {
            try
            {
                Agp2pDataContext context = new Agp2pDataContext();
                //同步返回是正在提现申请受理(忽略)，异步返回才是提现处理
#if DEBUG
                if (msg.Sync)
                {
#endif
#if !DEBUG
                if (!msg.Sync)
                {
#endif
                    if (msg.CheckResult())
                    {
                        if (msg.CheckSignature())
                        {
                            //当noticeType＝0时，00000代表受理成功；当noticeType＝1时，00000代表提现成功；
                            if (msg.NoticeType == "1")
                            {
                                //查找对应的交易流水
                                var trans = context.li_bank_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                                if (trans != null)
                                {
                                    context.ConfirmBankTransaction(trans.id, null);//TODO 检查用户资金信息
                                    msg.HasHandle = true;
                                }
                                else
                                {
                                    msg.Remarks = "提现已处理成功，但没有找到对应的交易记录，交易流水号为：" + msg.RequestId;
                                }
                            }
                            else
                            {
                                //查找对应的请求
                                var requestLog = context.li_pay_request_log.SingleOrDefault(u => u.id == msg.RequestId);
                                if (requestLog != null)
                                {
                                    var trans = context.li_bank_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                                    if (trans == null)
                                    {
                                        //创建提现申请记录
                                        context.Withdraw(Utils.StrToInt(requestLog.remarks, 0),
                                        Utils.StrToDecimal(msg.Sum, 0), "丰付提现处理", msg.RequestId);
                                        //TODO 提现完成特殊处理
                                        //msg.HasHandle = true;
                                    }
                                    else
                                    {
                                        msg.Remarks = "提现已受理，但已生成过交易记录，交易流水号为：" + msg.RequestId;
                                    }
                                }
                                else
                                {
                                    msg.Remarks = "提现已受理，但没有找到对应的请求，请求流水号为：" + msg.RequestId;
                                }
                            }
                        }
                    }
                    //TODO 自动取消提现 暂人工取消
                    //if (!msg.HasHandle)
                    //    context.CancelBankTransaction(trans.id, 1);
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }
    }
}
