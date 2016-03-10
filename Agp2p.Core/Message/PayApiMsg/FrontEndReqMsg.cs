using System;
using System.Collections.Generic;
using TinyMessenger;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class FrontEndReqMsg : BaseReqMsg
    {
        public int? UserId { get; set; }
        public int? ProjectCode { get; set; }
        public string SuccessReturnUrl { get; set; }
        public string FailReturnUrl { get; set; }
        public string NoticeUrl { get; set; }
        public Action<string> CallBack { get; set; }

        public FrontEndReqMsg()
        {
            SuccessReturnUrl = "";
            FailReturnUrl = "";
            NoticeUrl = "";
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