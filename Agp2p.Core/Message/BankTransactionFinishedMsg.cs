using System;
using Agp2p.Linq2SQL;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class BankTransactionFinishedMsg : ITinyMessage
    {
        public li_bank_transactions Transaction { get; protected set; }

        public BankTransactionFinishedMsg(li_bank_transactions bankTransaction)
        {
            Transaction = bankTransaction;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
