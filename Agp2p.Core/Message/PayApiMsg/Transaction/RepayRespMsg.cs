namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人存管账户还款项目响应
    /// </summary>
    public class RepayRespMsg : BaseRespMsg
    {
        public string UserBalance { get; set; }//账户总额
        public string WithdrawableBalance { get; set; }//可用余额
        public string FrozenBalance { get; set; }//冻结余额
        public string UnsettledBalance { get; set; }//未结金额

        public RepayRespMsg(string requestId, string result, string responseContent) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
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
