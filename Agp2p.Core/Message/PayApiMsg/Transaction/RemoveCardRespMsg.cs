﻿
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人银行卡解绑响应
    /// </summary>
    public class RemoveCardRespMsg : BaseRespMsg
    {
        public string FailReason { get; set; }

        public RemoveCardRespMsg(string requestStr)
        {
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map.ContainsKey("signature") ? map["signature"] : "";

            UserIdIdentity = map.ContainsKey("userIdIdentity") ? Utils.StrToInt(map["userIdIdentity"], 0) : 0;
            FailReason = map.ContainsKey("failReason") ? map["failReason"] : "";
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity + FailReason);
        }
    }
}
