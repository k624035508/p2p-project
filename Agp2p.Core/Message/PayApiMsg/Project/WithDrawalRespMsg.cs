using Agp2p.Common;
using Agp2p.Core.PayApiLogic;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人撤标项目响应
    /// </summary>
    public class WithDrawalRespMsg : BaseRespMsg
    {
        public string BidRequestId { get; set; }//原请求流水号
        public string WithdrawalFund { get; set; }//撤标金额
        public string InvestmentSum { get; set; }//项目累计投资金额
        public string ProjectSum { get; set; }//项目总额
        public string RemainInvestmentSum { get; set; }//剩余可投金额

        public WithDrawalRespMsg() { }
        public WithDrawalRespMsg(string requestStr)
        {
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            ProjectCode = map.ContainsKey("projectCode") ? Utils.StrToInt(map["projectCode"], 0) : 0;
            BidRequestId = map.ContainsKey("bidRequestId") ? map["bidRequestId"] : "";
            WithdrawalFund = map.ContainsKey("withdrawalFund") ? map["withdrawalFund"] : "";
            InvestmentSum = map.ContainsKey("investmentSum") ? map["investmentSum"] : "";
            ProjectSum = map.ContainsKey("projectSum") ? map["projectSum"] : "";
            RemainInvestmentSum = map.ContainsKey("remainInvestmentSum") ? map["remainInvestmentSum"] : "";
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + ProjectCode + BidRequestId + WithdrawalFund + Result);
        }
    }
}
