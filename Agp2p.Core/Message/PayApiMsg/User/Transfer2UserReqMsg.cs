using System;
using System.Collections.Generic;
using Agp2p.Common;


namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 单笔付款至个人用户账户
    /// </summary>
    public class Transfer2UserReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//投标金额
        public string UserAccountType { get; set; }//用户账户类型

        public Transfer2UserReqMsg(int userId, string sum, string userAccountType)
        {
            UserId = userId;
            UserAccountType = userAccountType;
            Sum = sum;
            Api = (int) Agp2pEnums.SumapayApiEnum.TranU;
            ApiInterface = SumapayConfig.NoticeUrl + "main/TransferFundForFT_transferToUser";
            RequestId = Agp2pEnums.SumapayApiEnum.TranU.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + UserId + UserAccountType + Sum + NoticeUrl, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&userIdIdentity={UserId}&sum={Sum}&userAccountType={UserAccountType}&signature={GetSignature()}";
            if (!string.IsNullOrEmpty(NoticeUrl)) postStr += $"&noticeUrl={NoticeUrl}";
            return postStr;
        }
    }
}
