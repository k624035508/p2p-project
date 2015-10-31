using System;
using Agp2p.Linq2SQL;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class BankTransactionFinishedMsg : ITinyMessage
    {
        public int TransactionId { get; protected set; }

        public BankTransactionFinishedMsg(int bankTransactionId)
        {
            TransactionId = bankTransactionId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
