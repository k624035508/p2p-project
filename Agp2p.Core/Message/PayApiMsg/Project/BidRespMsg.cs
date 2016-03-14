namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人投标/自动投标项目响应
    /// </summary>
    public class BidRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//投标金额
        public string InvestmentSum { get; set; }//项目累计投资金额
        public string ProjectSum { get; set; }//项目总额
        public string RemainInvestmentSum { get; set; }//剩余可投金额
        public string ProtocolCode { get; set; }//自动投标授权协议号

        public BidRespMsg(string requestId, string result, string responseContent) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
            
        }

        public override bool CheckSignature()
        {
            //TODO 根据协议号ProtocolCode分开自动投标与手动投标验证


            return true;
        }

        public override bool CheckResult()
        {
            return Result.Equals("00000");
        }
    }
}
