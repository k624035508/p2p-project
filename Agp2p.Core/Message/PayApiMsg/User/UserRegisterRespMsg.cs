using Agp2p.Common;
using Agp2p.Core.PayApiLogic;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人开户响应
    /// </summary>
    public class UserRegisterRespMsg : BaseRespMsg
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public string MobileNo { get; set; }
        public string PayType { get; set; }

        public UserRegisterRespMsg() { }

        public UserRegisterRespMsg(string requestStr)
        {
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            UserId = map["userId"];
            UserIdIdentity = Utils.StrToInt(map["userIdIdentity"], 0);
            Name = map["name"];
            Telephone = map.ContainsKey("telephone") ? map["telephone"] : "";
            MobileNo = map.ContainsKey("mobileNo") ? map["mobileNo"] : "";
            PayType = map["payType"];
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity + UserId);
        }
    }
}
