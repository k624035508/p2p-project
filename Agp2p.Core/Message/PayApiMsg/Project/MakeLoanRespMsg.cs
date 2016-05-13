using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 项目放款响应
    /// </summary>
    public class MakeLoanRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//放款金额
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编        
        public bool Sync { get; set; }//同步标识

        public MakeLoanRespMsg()
        {
            Sync = true;
        }
        public MakeLoanRespMsg(string requestStr)
        {
            Sync = false;
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            ProjectCode = map.ContainsKey("projectCode") ? Utils.StrToInt(map["projectCode"], 0) : 0;
            Sum = map.ContainsKey("sum") ? map["sum"] : "";
            PayType = map.ContainsKey("feeType") ? map["feeType"] : "";
            MainAccountType = map.ContainsKey("mainRoleType") ? map["mainRoleType"] : "";
            MainAccountCode = map.ContainsKey("mainRoleCode") ? map["mainRoleCode"] : "";
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + ProjectCode + Result);
        }
    }
}
