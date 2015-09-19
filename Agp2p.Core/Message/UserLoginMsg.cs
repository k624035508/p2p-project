using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class UserLoginMsg : ITinyMessage
    {
        public int UserId { get; protected set; }
        public bool Remember { get; protected set; }
        public string Code { get; protected set; }
        public Action DoLogin { get; protected set; }

        public UserLoginMsg(int userId, bool remember, string code, Action doLogin)
        {
            UserId = userId;
            Remember = remember;
            Code = code;
            DoLogin = doLogin;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
