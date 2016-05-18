using System;
using System.Collections.Generic;
using Agp2p.Common;
using TinyMessenger;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 企业网银充值
    /// </summary>
    public class CompanyWebRechargeReqMsg : WebRechargeReqMsg
    {
        public CompanyWebRechargeReqMsg(int userId, string sum, string bankCode, string passThrough = "",
            string payType = "3", string mainAccountType = "", string mainAccountCode = "",
            string bankCardTypeFlag = "1")
        {
            UserId = userId;
            Sum = sum;
            BankCode = bankCode;
            BankCardTypeFlag = bankCardTypeFlag;
            PayType = payType;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;
            PassThrough = passThrough;
            BankType = "1";

            Api = (int) Agp2pEnums.SumapayApiEnum.CeRec;
            ApiInterface = SumapayConfig.ApiUrl + "businessUser/webBankRecharge_toRecharge";
            RequestId = Agp2pEnums.SumapayApiEnum.CeRec.ToString().ToUpper() + Utils.GetOrderNumberLonger();
            //企业分账列表处理
            subledgerList = JsonHelper.ObjectToJSON(new List<object>()
                    {
                        new
                        {
                            roleType = "3",
                            roleCode = UserId.ToString(),
                            inOrOut = "0",
                            bizFlag = "0",
                            sum = Sum
                        }
                    });
        }
    }
}
