using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 单笔付款至个人用户账户响应
    /// </summary>
    public class Transfer2UserRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//付款金额
        public string OutAccountBalance { get; set; }//转出账户余额
        public string InAccountBalance { get; set; }//转入账户总余额
        public bool InAccountWithdrawableBalance { get; set; }//转入账户可提现余额
        public bool InAccountFrozenBalance { get; set; }//转入账户冻结余额
        public bool InAccountUnsettledBalance { get; set; }//未结金额

        public Transfer2UserRespMsg(string requestId, string result, string responseContent) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
        }

        public override bool CheckSignature()
        {
            return true;
        }

        public override bool CheckResult()
        {
            return Result.Equals("00000");
        }
    }
}
