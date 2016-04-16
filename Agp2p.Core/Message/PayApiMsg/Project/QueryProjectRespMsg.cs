using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 查询项目响应
    /// </summary>
    public class QueryProjectRespMsg : BaseRespMsg
    {
        public string InvestSum { get; set; }//累计投资金额
        public string RepaySum { get; set; }//累计还款金额
        public string ProjectSum { get; set; }//项目总额
        public string RemainInvestmentSum { get; set; }//剩余可投金额        
        public string LoanAccountBalance { get; set; }//放款账户余额
        public string RepayAccountBalance { get; set; }//还款账户余额
        //public string TradeList { get; set; }//交易明细列表
        public bool Sync { get; set; }

        public QueryProjectRespMsg()
        { }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + ProjectCode + InvestSum + RepaySum);
        }
    }
}
