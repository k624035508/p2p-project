namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 本息到账响应
    /// </summary>
    public class ReturnPrinInteRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//本息到账金额
        public string AccountBalance { get; set; }//项目还款账户余额
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编        
        public string RequestTime { get; set; }//请求时间   
        public string DealTime { get; set; }//处理时间   
        public bool Sync { get; set; }//同步标识

        public ReturnPrinInteRespMsg(string requestId, string result, string responseContent, bool sync = false) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
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
