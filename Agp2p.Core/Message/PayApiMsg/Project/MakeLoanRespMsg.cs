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
        public bool Collective { get; set; }//集合项目标识
        public bool Sync { get; set; }//同步标识

        public MakeLoanRespMsg(string requestStr, bool sync = false, bool collective = false)
        {
            Collective = collective;
            Sync = sync;

            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            UserIdIdentity = Utils.StrToInt(map["userIdIdentity"], 0);
            ProjectCode = map["projectCode"];
            Sum = map["sum"];
            PayType = map["payType"];
            MainAccountType = map.ContainsKey("mainAccountType") ? map["mainAccountType"] : "";
            MainAccountCode = map.ContainsKey("mainAccountCode") ? map["mainAccountCode"] : "";
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + ProjectCode + Result);
        }
    }
}
