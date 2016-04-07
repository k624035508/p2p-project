using System;
using System.Collections.Generic;
using Agp2p.Common;
using Agp2p.Core.PayApiLogic;
using Agp2p.Linq2SQL;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class BaseRespMsg : ITinyMessage
    {
        public int? UserIdIdentity { get; set; }
        public string ProjectCode { get; set; }
        public string RequestId { get; set; }
        public string Result { get; set; }
        public string ResponseContent { get; set; }
        public bool HasHandle { get; set; }
        public string Remarks { get; set; }
        public string Signature { get; set; }

        public BaseRespMsg()
        {
            HasHandle = false;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }

        public static TMsg NewInstance<TMsg>(string responseContent) where TMsg : BaseRespMsg
        {
            var instance = JsonHelper.JSONToObject<TMsg>(responseContent);
            instance.ResponseContent = responseContent;
            return instance;
        }

        public virtual bool CheckSignature()
        {
            throw new NotImplementedException();
        }

        public bool CheckSignature(string paras)
        {
            if (!Signature.Equals(SumaPayUtils.GenSign(paras, SumapayConfig.Key)))
            {
                Remarks += "数字签名验证不通过;";
                return false;
            }
            return true;
        }

        public virtual bool CheckResult()
        {
            if (!Result.Equals("00000"))
            {
                //TODO 根据结果编码记录失败原因
                Remarks += Result;
                return false;
            }
            return true;
        }
    }
}