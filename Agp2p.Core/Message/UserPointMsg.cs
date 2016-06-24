using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;
using TinyMessenger;

namespace Agp2p.Core.Message.UserPointMsg
{
    class UserPointMsg : ITinyMessage
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public Agp2pEnums.PointEnum Type { get; set; }
        public int Point { get; set; }
        public string Remark { get; set; }

        public UserPointMsg(int userId, string userName, Agp2pEnums.PointEnum type, int point = 0)
        {
            UserId = userId;
            UserName = userName;
            Type = type;
            Point = point;
        }

        public object Sender { get; }
    }
}
