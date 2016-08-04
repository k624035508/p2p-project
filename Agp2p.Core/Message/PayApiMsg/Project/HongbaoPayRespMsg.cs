using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;

namespace Agp2p.Core.Message.PayApiMsg.Project
{
    public class HongbaoPayRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }  //付款金额
        public string OutAccountBalance { get; set; }  //转出账户余额
        public string InAccountBalance { get; set; }  //转入账户总余额
        public string InAccountWithdrawableBalance { get; set; }  //转入账户可提现余额
        public string InAccountFrozenBalance { get; set; }  //转入账户冻结余额
        public string InAccountUnsettledBalance { get; set; }  //未结金额

        public HongbaoPayRespMsg()
        {

        }

        public HongbaoPayRespMsg(string requestStr)
        {
            var map = Utils.UrlParamToData(requestStr);
            RequestId = map["requestId"];
            UserIdIdentity = Utils.StrToInt(map["userIdIdentity"], 0);
            Result = map["result"];
            Sum = map.ContainsKey("sum") ? map["sum"] : "";
            OutAccountBalance = map.ContainsKey("OutAccountBalance") ? map["OutAccountBalance"] : "";
            InAccountBalance = map.ContainsKey("InAccountBalance") ? map["InAccountBalance"] : "";
            InAccountWithdrawableBalance = map.ContainsKey("InAccountWithdrawableBalance") ? map["InAccountWithdrawableBalance"] : "";
            InAccountFrozenBalance = map.ContainsKey("InAccountFrozenBalance") ? map["InAccountFrozenBalancee"] : "";
            InAccountUnsettledBalance = map.ContainsKey("InAccountUnsettledBalance") ? map["InAccountUnsettledBalance"] : "";
            Signature = map["signature"];
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + Sum + OutAccountBalance + InAccountBalance);
        }
    }
}
