using System;
using System.Collections.Generic;
using System.Web;

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
            SuccessReturnUrl = "http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/api/payment/sumapay/successReturnUrl.aspx";
            FailReturnUrl = "http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/api/payment/sumapay/failReturnUrl.aspx";
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