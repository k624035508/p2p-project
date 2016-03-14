using System;
using System.Collections.Generic;
using Agp2p.Common;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 流标普通项目
    /// </summary>
    public class RepealProjectReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//原投标订单金额

        public RepealProjectReqMsg(string projectCode, string sum)
        {
            ProjectCode = projectCode;
            Sum = sum;
            Api = (int) Agp2pEnums.SumapayApiEnum.RePro;
            ApiInterface = TestApiUrl + "main/TransactionForFT_repealProject";
            RequestId = Agp2pEnums.SumapayApiEnum.RePro.ToString().ToUpper() + Utils.GetOrderNumberLonger();
        }

        public override string GetSignature()
        {
            HMACMD5 hmac = new HMACMD5(Key);
            return
                hmac.ComputeHashToBase64String(RequestId + MerchantCode + ProjectCode  + Sum + NoticeUrl);
        }

        public override string GetPostPara()
        {
            return
                $"requestId={RequestId}&merchantCode={MerchantCode}&projectCode={ProjectCode}&sum={Sum}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
        }
    }
}
