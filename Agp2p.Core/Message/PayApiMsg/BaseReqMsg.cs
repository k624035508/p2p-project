using System;
using System.Collections.Generic;
using TinyMessenger;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class BaseReqMsg : ITinyMessage
    {
        public int Api { get; set; }
        public string ApiInterface { get; set; }
        public string RequestId { get; set; }
        public int? UserId { get; set; }
        public string ProjectCode { get; set; }
        public string RequestContent { get; set; }

        public BaseReqMsg()
        {
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        public virtual string GetSignature()
        {
            throw new NotImplementedException();
        }
    }
}