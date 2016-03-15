namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人网银充值响应
    /// </summary>
    public class RechargeRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//充值金额
        public string UserBalance { get; set; }//账户总额
        public string WithdrawableBalance { get; set; }//可用余额
        public string FrozenBalance { get; set; }//冻结余额
        public string UnsettledBalance { get; set; }//未结金额
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public string BankAccount { get; set; }//银行账号
        public string BankName { get; set; }//银行名称
        public string Name { get; set; }//用户姓名
        public bool Quick { get; set; }

        public RechargeRespMsg(string requestId, string result, string responseContent, bool quick = false) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
            Quick = quick;
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
