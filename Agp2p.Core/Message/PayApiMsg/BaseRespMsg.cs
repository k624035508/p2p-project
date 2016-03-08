using System;
using System.Collections.Generic;
using TinyMessenger;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class BaseRespMsg : ITinyMessage
    {
        public int? UserId { get; set; }
        public int? ProjectCode { get; set; }
        public string RequestId { get; set; }
        public string Result { get; set; }
        public string ResponseContent { get; set; }

        public BaseRespMsg(string requestId, string result, string responseContent)
        {
            RequestId = requestId;
            Result = result;
            ResponseContent = responseContent;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool CheckSignature()
        {
            throw new NotImplementedException();
        }

        public virtual bool CheckResult()
        {
            throw new NotImplementedException();
        }
    }
}