using System;
using System.Collections.Generic;
using TinyMessenger;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 请求后台接口
    /// </summary>
    public class BackEndReqMsg : BaseReqMsg
    {
        public BackEndReqMsg()
        {
            
        }

        public override string GetSignature()
        {
            throw new NotImplementedException();
        }

        public virtual string GetPostPara()
        {
            throw new NotImplementedException();
        }
    }
}