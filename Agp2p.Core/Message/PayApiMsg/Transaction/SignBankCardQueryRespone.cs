using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg.Transaction
{
    /// <summary>
    /// 个人用户签约银行卡查询响应
    /// </summary>
    public class SignBankCardQueryRespone : BaseRespMsg
    {
        public string WithdrawBankList { get; set; } //用户提现卡列表
        public string RechargeProtocolList { get; set; } //一键充值卡列表
        public List<RechargeProtocol> RechargeProtocols { get; set; }
        public string RepayProtocolList { get; set; } //协议还款卡列表

        public SignBankCardQueryRespone(string requestStr)
        {
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            Result = map["result"];
            Signature = map.ContainsKey("signature") ? map["signature"] : "";

            UserIdIdentity = map.ContainsKey("userIdIdentity") ? Utils.StrToInt(map["userIdIdentity"], 0) : 0;
            WithdrawBankList = map.ContainsKey("withdrawBankList") ? map["withdrawBankList"] : "";
            //获取一键充值卡列表
            RechargeProtocolList = map.ContainsKey("rechargeProtocolList") ? map["rechargeProtocolList"] : "";
            if (!string.IsNullOrEmpty(RechargeProtocolList))
            {
                RechargeProtocols = JsonHelper.JSONToObject<List<RechargeProtocol>>(RechargeProtocolList);
            }
            RepayProtocolList = map.ContainsKey("repayProtocolList") ? map["repayProtocolList"] : "";
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity);
        }

        public class RechargeProtocol
        {
            public string BankName { get; set; }
            public string BankAccount { get; set; }
            public string ProtocolNo { get; set; }
        }
    }
}
