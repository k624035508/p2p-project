namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人存管账户/协议还款项目响应
    /// </summary>
    public class RepayRespMsg : BaseRespMsg
    {
        public string UserBalance { get; set; }//账户总额
        public string WithdrawableBalance { get; set; }//可用余额
        public string FrozenBalance { get; set; }//冻结余额
        public string UnsettledBalance { get; set; }//未结金额

        //协议还款参数
        public string BankAccount { get; set; }//银行账号
        public string BankName { get; set; }//银行名称
        public string Name { get; set; }//姓名
        public string Sum { get; set; }//还款金额
        public string PayType { get; set; }//手续费收取方式

        public bool BankRepay { get; set; }//是否协议还款
        public bool AutoRepay { get; set; }//是否自动还款

        public RepayRespMsg(string requestId, string result, string responseContent, bool bankRepay = false, bool autoRepay = false) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
            BankRepay = bankRepay;
            AutoRepay = autoRepay;
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
