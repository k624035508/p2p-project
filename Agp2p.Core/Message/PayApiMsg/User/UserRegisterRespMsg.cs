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
        public string PayType { get; set; }

        public UserRegisterRespMsg()
        {
            
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity + UserId);
        }
    }
}
