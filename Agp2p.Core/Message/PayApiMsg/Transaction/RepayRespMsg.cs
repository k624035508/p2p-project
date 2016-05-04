using Agp2p.Common;

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

        //自动还款参数
        public string AccountBalance { get; set; }//账户总额
        public string FeeType { get; set; }//手续费收取方式

        public bool BankRepay { get; set; }//是否协议还款
        public bool AutoRepay { get; set; }//是否自动还款
        public bool HuoqiRepay { get; set; }//是否活期还款
        public bool Sync { get; set; }//同步标识


        public RepayRespMsg()
        {
            Sync = true;
        }
    
        public RepayRespMsg(string requestStr, bool bankRepay = false, bool autoRepay = false)
        {
            Sync = false;

            BankRepay = bankRepay;
            AutoRepay = autoRepay;

            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map["signature"];

            UserIdIdentity = map.ContainsKey("userIdIdentity") ? Utils.StrToInt(map["userIdIdentity"], 0) : 0;
            ProjectCode = map.ContainsKey("projectCode") ? Utils.StrToInt(map["projectCode"], 0) : 0;

            //账户还款参数
            UserBalance = map.ContainsKey("userBalance") ? map["userBalance"] : "";
            WithdrawableBalance = map.ContainsKey("withdrawableBalance") ? map["withdrawableBalance"] : "";
            FrozenBalance = map.ContainsKey("frozenBalance") ? map["frozenBalance"] : "";
            UnsettledBalance = map.ContainsKey("unsettledBalance") ? map["unsettledBalance"] : "";
            //协议还款参数
            PayType = map.ContainsKey("payType") ? map["payType"] : "";
            Sum = map.ContainsKey("sum") ? map["sum"] : "";
            BankAccount = map.ContainsKey("bankAccount") ? map["bankAccount"] : "";
            BankName = map.ContainsKey("bankName") ? map["bankName"] : "";
            Name = map.ContainsKey("name") ? map["name"] : "";
            //自动还款参数
            AccountBalance = map.ContainsKey("accountBalance") ? map["accountBalance"] : "";
            FeeType = map.ContainsKey("feeType") ? map["feeType"] : "";
        }

        public override bool CheckSignature()
        {
            if (BankRepay)
            {
                return base.CheckSignature(RequestId + Result + UserIdIdentity);
            }
            else if (AutoRepay)
            {
                return base.CheckSignature(RequestId + UserIdIdentity + Result + AccountBalance);
            }
            return base.CheckSignature(RequestId + Result + UserIdIdentity + UserBalance);
        }
    }
}
