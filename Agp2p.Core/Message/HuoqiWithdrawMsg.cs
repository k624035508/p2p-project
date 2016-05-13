using System;
using Agp2p.Linq2SQL;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class HuoqiWithdrawMsg : ITinyMessage
    {
        public int UserId { get; protected set; }
        public int ProjectId { get; protected set; }
        public decimal Amount { get; protected set; }

        public HuoqiWithdrawMsg(int userId, int projectId, decimal amount)
        {
            UserId = userId;
            ProjectId = projectId;
            Amount = amount;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
