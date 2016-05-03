using System;
using System.Linq;
using Agp2p.Common;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
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
            MessageBus.Main.Subscribe<WithdrawRespMsg>(WithDraw);
        }

        /// <summary>
        /// 网银/一键充值 响应
        /// </summary>
        /// <param name="msg"></param>
        private static void Recharge(RechargeRespMsg msg)
        {
            try
            {
                //检查请求处理结果 TODO 失败就取消充值申请记录 一键充值只处理异步
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

                                //TODO 检查用户资金信息 一键充值后自动取消注销银行卡（同卡进出只能使用绑定卡提现）
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
                //查找对应的交易流水
                var trans = context.li_bank_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                if (trans != null)
                {
                    if (msg.CheckResult())
                    {
                        if (msg.CheckSignature())
                        {
                            //异步返回才放款,内网测试使用同步
#if DEBUG
                            if (msg.Sync)
                            {
#endif
#if !DEBUG
                            if (!msg.Sync)
                            {
#endif
                                context.ConfirmBankTransaction(trans.id, null);
                                //TODO 检查用户资金信息

                                msg.HasHandle = true;
                            }
                        }
                    }
                    //取消提现
                    if (!msg.HasHandle)
                        context.CancelBankTransaction(trans.id, 1);
                }
                else
                {
                    msg.Remarks = "没有找到平台交易流水记录，交易流水号为：" + msg.RequestId;
                }
            }
            catch (Exception ex)
            {
                msg.Remarks = "内部错误：" + ex.Message;
            }
        }
    }
}
