using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人开户响应
    /// </summary>
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
        }

        public UserRegisterRespMsg(string requestId, string result, string responseContent) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
            UserId = 1030;//测试
        }

        public override bool CheckSignature()
        {
            return true;
        }

        public override bool CheckResult()
        {
            return Result.Equals("00000");
        }
    }
}
