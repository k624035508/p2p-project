using System;
using System.Collections.Generic;
using TinyMessenger;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class BaseReqMsg : ITinyMessage
    {
        public int? UserIdIdentity { get; set; }
        public int? ProjectCode { get; set; }
        public string MerchantCode { get; set; }
        public string SuccessReturnUrl { get; set; }
        public string FailReturnUrl { get; set; }
        public string NoticeUrl { get; set; }
        public int Api { get; set; }
        public string ApiUrl { get; set; }
        public string RequestId { get; set; }
        public Action<string> CallBack { get; set; }

        public BaseReqMsg()
        {
            MerchantCode = "";
            SuccessReturnUrl = "";
            FailReturnUrl = "";
            NoticeUrl = "";
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string GetSignature()
        {
            throw new NotImplementedException();
        }

        public virtual SortedDictionary<string, string> GetSubmitPara()
        {
            throw new NotImplementedException();
        }
    }
}