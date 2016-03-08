using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class UserRegisterRespMsg : BaseRespMsg
    {
        public string IdentityId { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public string PayType { get; set; }

        public UserRegisterRespMsg(string requestId, string result, string responseContent, int userId,
            string identityId, string telephone, string name, string payType) : base(requestId, result, responseContent)
        {
            UserId = userId;
            IdentityId = identityId;
            Name = name;
            Telephone = telephone;
            PayType = payType;
            Result = result;
        }

        public override bool CheckSignature()
        {
            return false;
        }

        public override bool CheckResult()
        {
            return Result.Equals("00000");
        }
    }
}
