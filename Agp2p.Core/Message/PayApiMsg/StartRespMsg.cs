using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class StartRespMsg : BaseRespMsg
    {
        public StartRespMsg(string requestId, string result, string responseContent) : base(requestId, result, responseContent)
        {
        }

        public override bool CheckSignature()
        {
            return false;
        }

        public override bool CheckResult()
        {
            return Result.Equals("00000");
        }
    }
}
