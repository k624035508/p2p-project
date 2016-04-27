using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 流标普通项目响应
    /// </summary>
    public class RepealProjectRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//金额
        public string ProjectSum { get; set; }//项目总额
        

        public RepealProjectRespMsg(){}
        public RepealProjectRespMsg(string requestStr)
        {
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            ProjectCode = map.ContainsKey("projectCode") ? Utils.StrToInt(map["projectCode"], 0) : 0;
            Sum = map.ContainsKey("sum") ? map["sum"] : "";
            ProjectSum = map.ContainsKey("projectSum") ? map["projectSum"] : "";
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + ProjectCode  + Result);
        }
    }
}
