using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人提现
    /// </summary>
    public class CompanyWithdrawReqMsg: WithdrawReqMsg
    {
        public CompanyWithdrawReqMsg(int userId, string sum, string bankId, string bankCode = "", string bankAccount = "",
            string payType = "3", string mainAccountType = "", string mainAccountCode = "")
        {
            UserId = userId;
            Sum = sum;
            PayType = payType;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            BankId = bankId;
            SetBankCodeAccount(bankCode, bankAccount);

            Api = (int)Agp2pEnums.SumapayApiEnum.Cdraw;
            ApiInterface = SumapayConfig.ApiUrl + "businessUser/withdraw_toWithdraw";
            RequestId = Agp2pEnums.SumapayApiEnum.Cdraw.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }
    }
}
