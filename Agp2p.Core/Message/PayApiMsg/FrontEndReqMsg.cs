using System;
using System.Collections.Generic;
using TinyMessenger;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 请求前台接口
    /// </summary>
    public class FrontEndReqMsg : BaseReqMsg
    {
        public string SuccessReturnUrl { get; set; }
        public string FailReturnUrl { get; set; }

        public FrontEndReqMsg()
        {
        }

        public override string GetSignature()
        {
            throw new NotImplementedException();
        }

        public virtual SortedDictionary<string, string> GetSubmitPara()
        {
            throw new NotImplementedException();
        }
    }
}