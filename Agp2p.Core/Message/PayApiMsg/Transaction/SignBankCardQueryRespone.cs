using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg.Transaction
{
    /// <summary>
    /// 个人用户签约银行卡查询响应
    /// </summary>
    public class SignBankCardQueryRespone : BaseRespMsg
    {
        public string WithdrawBankList { get; set; } //用户提现卡列表
        public List<RechargeProtocol> RechargeProtocolList { get; set; } //一键充值卡列表
        public string RepayProtocolList { get; set; } //协议还款卡列表

        public SignBankCardQueryRespone()
        {
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity);
        }

        public class RechargeProtocol
        {
            public string BankName { get; set; }
            public string BankAccount { get; set; }
            public string ProtocolNo { get; set; }
        }

        /// <summary>
        /// 检查在丰付平台绑定的银行卡是否一致
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <returns></returns>
        public bool CheckRechargeProtocol(string bankName, string bankAccount)
        {
            if (CheckResult())
            {
                if (CheckSignature())
                {
                    if (RechargeProtocolList != null && RechargeProtocolList.Any())
                    {
                        //丰付同卡进出，只能绑定一张卡，所以正式环境只需要拿第一张卡
                        var rechargeProtocol = RechargeProtocolList.FirstOrDefault();
                        if (rechargeProtocol != null)
                        {
                            return rechargeProtocol.BankName.Equals(bankName) && rechargeProtocol.BankAccount.Substring(0, 4).Equals(bankAccount.Substring(0, 4)) &&
                                rechargeProtocol.BankAccount.Substring(rechargeProtocol.BankAccount.Length - 4, 4)
                                    .Equals(bankAccount.Substring(bankAccount.Length - 4, 4));
                        }
                    }
                }
            }
            return false;
        }

    }
}
