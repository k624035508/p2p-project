using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 本息到账响应
    /// </summary>
    public class ReturnPrinInteRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//本息到账金额
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编        
        public string RequestTime { get; set; }//请求时间   
        public bool Sync { get; set; }//同步标识

        public string AccountBalance { get; set; }//项目还款账户余额
        public string DealTime { get; set; }//处理时间   

        public int RepayTaskId { get; set; }
        public bool IsEarlyPay { get; set; }
        public bool IsHuoqi { get; set; }

        public ReturnPrinInteRespMsg() { }

        public ReturnPrinInteRespMsg(bool sync = false)
        {
            Sync = sync;
        }

        public override bool CheckSignature()
        {
            if (Sync)
            {
                return base.CheckSignature(RequestId + ProjectCode + Result );
            }
            else
            {
                return base.CheckSignature(RequestId + ProjectCode + Result + AccountBalance);
            }
            
        }
    }
}
