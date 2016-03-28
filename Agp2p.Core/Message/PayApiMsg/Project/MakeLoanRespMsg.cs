namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 项目放款响应
    /// </summary>
    public class MakeLoanRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//放款金额
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编        
        public bool Collective { get; set; }//集合项目标识
        public bool Sync { get; set; }//同步标识

        public MakeLoanRespMsg(bool collective = false, bool sync = false)
        {
            Collective = collective;
            Sync = sync;
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + ProjectCode + Result);
        }
    }
}
