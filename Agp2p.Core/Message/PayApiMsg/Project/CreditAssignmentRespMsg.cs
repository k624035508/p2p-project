using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 债权转让响应
    /// </summary>
    public class CreditAssignmentRespMsg : BaseRespMsg
    {
        public string AssignmentSum { get; set; }//转让金额
        public string OriginalRequestId { get; set; }//原请求流水号
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编        


        public CreditAssignmentRespMsg(string requestStr)
        {
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            UserIdIdentity = Utils.StrToInt(map["userIdIdentity"], 0);
            ProjectCode = Utils.StrToInt(map["projectCode"], 0);
            AssignmentSum = map["assignmentSum"];
            OriginalRequestId = map["originalRequestId"];
            MainAccountType = map.ContainsKey("mainAccountType") ? map["mainAccountType"] : "";
            MainAccountCode = map.ContainsKey("mainAccountCode") ? map["mainAccountCode"] : "";
            PayType = map.ContainsKey("payType") ? map["payType"] : "";

        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId  + Result + AssignmentSum + UserIdIdentity);
        }
    }
}
