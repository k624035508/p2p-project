
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人自动投标续约响应
    /// </summary>
    public class AutoBidSignRespMsg : BaseRespMsg
    {
        public string ProtocolCode { get; set; }//授权协议号
        public string ContractFund { get; set; }//签约金额
        public bool Cancel { get; set; }

        public AutoBidSignRespMsg(string requestStr, bool cancel = false)
        {
            Cancel = cancel;
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            UserIdIdentity = map.ContainsKey("userIdIdentity") ? Utils.StrToInt(map["userIdIdentity"], 0) : 0;
            ProtocolCode = map.ContainsKey("protocolCode") ? map["protocolCode"] : "";
            ContractFund = map.ContainsKey("contractFund") ? map["contractFund"] : "";

        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity + ProtocolCode);
        }
    }
}
