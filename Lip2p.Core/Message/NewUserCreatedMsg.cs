using System;
using TinyMessenger;

namespace Lip2p.Core.Message
{
    public class NewUserCreatedMsg : ITinyMessage
    {
        public int UserId { get; protected set; }
        public DateTime RegTime  { get; protected set; }

        public NewUserCreatedMsg(int userId, DateTime regTime)
        {
            UserId = userId;
            RegTime = regTime;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
