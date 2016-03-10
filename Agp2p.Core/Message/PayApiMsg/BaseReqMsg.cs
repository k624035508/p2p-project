using System;
using System.Collections.Generic;
using TinyMessenger;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class BaseReqMsg : ITinyMessage
    {
        public const string MerchantCode = "";
        public const string Key = "";
        public const string TestApiUrl = "";
        public const string ApiUrl = "";

        public int Api { get; set; }
        public string ApiInterface { get; set; }
        public string RequestId { get; set; }

        public BaseReqMsg()
        {
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string GetSignature()
        {
            throw new NotImplementedException();
        }
    }
}