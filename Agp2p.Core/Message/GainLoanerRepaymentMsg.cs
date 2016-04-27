using System;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    class GainLoanerRepaymentMsg : ITinyMessage
    {
        public DateTime GainAt { get; protected set; }
        public int RepaymentTaskId { get; protected set; }
        public int LoanerUserId { get; protected set; }
        public decimal Amount { get; protected set; }

        public GainLoanerRepaymentMsg(DateTime gainAt, int repaymentTaskId, int loanerUserId, decimal amount)
        {
            GainAt = gainAt;
            RepaymentTaskId = repaymentTaskId;
            LoanerUserId = loanerUserId;
            Amount = amount;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
