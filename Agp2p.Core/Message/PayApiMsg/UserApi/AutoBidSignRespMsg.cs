using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人自动投标续约响应
    /// </summary>
    public class AutoBidSignRespMsg : BaseRespMsg
    {
        public string ProtocolCode { get; set; }//授权协议号
        public string ContractFund { get; set; }//签约金额
        public string Remarks { get; set; }
        public bool Cancel { get; set; }

        public AutoBidSignRespMsg(string requestId, string result, string responseContent, bool cancel = false) : base(requestId, result, responseContent)
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
