namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 项目放款响应(单普通项目）
    /// </summary>
    public class MakeLoanRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//放款金额
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编        
        public bool Collective { get; set; }//集合项目标识
        public bool Sync { get; set; }//同步标识

        public MakeLoanRespMsg(string requestId, string result, string responseContent, bool collective = false, bool sync = false) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
            Collective = collective;
            Sync = sync;
        }

        public override bool CheckSignature()
        {
            return true;
        }

        public override bool CheckResult()
        {
            return Result.Equals("00000");
        }
    }
}
