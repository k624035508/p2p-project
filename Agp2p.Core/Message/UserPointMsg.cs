using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;
using TinyMessenger;

namespace Agp2p.Core.Message.UserPointMsg
{
    public class UserPointMsg : ITinyMessage
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Type { get; set; }
        public int Point { get; set; }
        public string Remark { get; set; }

        public UserPointMsg(int userId, string userName, int type, int point = 0)
        {
            UserId = userId;
            UserName = userName;
            Type = type;
            Point = point;
        }

        public object Sender { get; }
    }
}
