
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人自动还款开通/取消响应
    /// </summary>
    public class AutoRepaySignRespMsg : BaseRespMsg
    {
        public string BankAccount { get; set; }//银行账号
        public string BankName { get; set; }//银行名称
        public string Name { get; set; }//姓名
        public bool Cancel { get; set; }

        public AutoRepaySignRespMsg(string requestStr, bool cancel = false)
        {
            Cancel = cancel;
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            UserIdIdentity = map.ContainsKey("userIdIdentity") ? Utils.StrToInt(map["userIdIdentity"], 0) : 0;
            BankAccount = map.ContainsKey("bankAccount") ? map["bankAccount"] : "";
            BankName = map.ContainsKey("bankName") ? map["bankName"] : "";
            Name = map.ContainsKey("name") ? map["name"] : "";
            ProjectCode = map.ContainsKey("projectCode") ? Utils.StrToInt(map["projectCode"], 0) : 0;
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity);
        }
    }
}
