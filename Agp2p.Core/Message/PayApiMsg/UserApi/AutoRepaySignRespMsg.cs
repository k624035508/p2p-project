using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人自动还款开通/取消响应
    /// </summary>
    public class AutoRepaySignRespMsg : BaseRespMsg
    {
        public string BankAccount { get; set; }//银行账号
        public string BankName { get; set; }//银行名称
        public string Name { get; set; }//姓名
        public bool Cancel { get; set; }

        public AutoRepaySignRespMsg(string requestId, string result, string responseContent, bool cancel = false) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
            Cancel = cancel;
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
