using System;
using System.ComponentModel;
using TinyMessenger;

namespace Agp2p.Core.Message
{
    public class StaticClaimTransferSuccessMsg : ITinyMessage
    {

        public int NeedTransferClaimId { get; protected set; }

        public StaticClaimTransferSuccessMsg(int needTransferClaimId)
        {
            NeedTransferClaimId = needTransferClaimId;
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }
    }
}
