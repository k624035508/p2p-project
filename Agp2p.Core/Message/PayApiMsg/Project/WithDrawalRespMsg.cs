using Agp2p.Core.PayApiLogic;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人撤标项目响应
    /// </summary>
    public class WithDrawalRespMsg : BaseRespMsg
    {
        public string BidRequestId { get; set; }//原请求流水号
        public string WithdrawalFund { get; set; }//撤标金额
        public string InvestmentSum { get; set; }//项目累计投资金额
        public string ProjectSum { get; set; }//项目总额
        public string RemainInvestmentSum { get; set; }//剩余可投金额
        

        public WithDrawalRespMsg()
        {

        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + ProjectCode + BidRequestId + WithdrawalFund + Result);
        }
    }
}
