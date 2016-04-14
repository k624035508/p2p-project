
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人投标/自动投标项目响应
    /// </summary>
    public class BidRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//投标金额
        public string InvestmentSum { get; set; }//项目累计投资金额
        public string ProjectSum { get; set; }//项目总额
        public string RemainInvestmentSum { get; set; }//剩余可投金额
        public string ProtocolCode { get; set; }//自动投标授权协议号

        public BidRespMsg(){}

        public BidRespMsg(string requestStr)
        {
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            UserIdIdentity = Utils.StrToInt(map["userIdIdentity"], 0);
            ProjectCode = map["projectCode"];
            Sum = map["sum"];
            InvestmentSum = map.ContainsKey("protocolCode") ? map["investmentSum"] : "";
            ProjectSum = map.ContainsKey("protocolCode") ? map["projectSum"] : "";
            RemainInvestmentSum = map["remainInvestmentSum"];
            ProtocolCode = map.ContainsKey("protocolCode") ? map["protocolCode"] : "";

        }

        public override bool CheckSignature()
        {
            if (string.IsNullOrEmpty(ProtocolCode))
            {
                return base.CheckSignature(RequestId + Result + Sum + UserIdIdentity);
            }
            else
            {
                return base.CheckSignature(RequestId + Result + Sum + ProtocolCode);
            }
        }
    }
}
