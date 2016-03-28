
namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人自动投标续约响应
    /// </summary>
    public class AutoBidSignRespMsg : BaseRespMsg
    {
        public string ProtocolCode { get; set; }//授权协议号
        public string ContractFund { get; set; }//签约金额
        public bool Cancel { get; set; }

        public AutoBidSignRespMsg(bool cancel = false)
        {
            Cancel = cancel;
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity + ProtocolCode);
        }
    }
}
