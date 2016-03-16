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
                //检查签名
                if (msg.CheckSignature())
                {
                    //检查请求处理结果
                    if (msg.CheckResult())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的交易流水
                        var trans = context.li_bank_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                        if (trans != null)
                        {
                            //TODO 更新流水信息 平台账户充值 检查用户资金信息

                            msg.HasHandle = true;
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
                //检查签名
                if (msg.CheckSignature())
                {
                    //检查请求处理结果
                    if (msg.CheckResult())
                    {
                        Agp2pDataContext context = new Agp2pDataContext();
                        //查找对应的交易流水
                        var trans = context.li_bank_transactions.SingleOrDefault(u => u.no_order == msg.RequestId);
                        if (trans != null)
                        {
                            //TODO 更新流水信息 提现 检查用户资金信息
                            if (!msg.Sync)
                            {
                                
                            }

                            context.SubmitChanges();
                            msg.HasHandle = true;
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
    }
}
