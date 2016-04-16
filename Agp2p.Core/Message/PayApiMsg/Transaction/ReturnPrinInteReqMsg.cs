using System;
using System.Collections.Generic;
using System.Linq;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using TinyMessenger;
using xBrainLab.Security.Cryptography;

namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 本息到账普通/集合项目
    /// </summary>
    public class ReturnPrinInteReqMsg : BackEndReqMsg
    {
        public string Sum { get; set; }//本息到账金额
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编码
        public bool Collective { get; set; }//集合项目标识
        /// <summary>
        /// 分账列表，用以描述参与分账的角色和应得/扣金额，为json格式，由数组构成，每条明细对应数组中的一条记录。
        /// 该数组中可能有一条或多条记录。数组中每条记录包含以下字段：角色类型(roleType)（0：用户、1：商户）、角色编码(roleCode)（用户为第三方用户标识，商户为商户编码）、
        /// 进出方向(inOrOut)（0：进账、1：出账）、金额(sum)。在本息到账接口中，分账列表对应记录一笔或多笔用户的进账明细以及一笔或多笔商户的进账明细，各方进账金额之和等于sum
        /// </summary>
        public string SubledgerList { get; set; }

        public ReturnPrinInteReqMsg(int projectCode, string sum, bool collective = false, string payType = "3", string mainAccountType = "", string mainAccountCode = "")
        {
            ProjectCode = projectCode;
            Sum = sum;
            PayType = payType;
            MainAccountType = mainAccountType;
            MainAccountCode = mainAccountCode;

            Api = collective ? (int)Agp2pEnums.SumapayApiEnum.RetCo : (int) Agp2pEnums.SumapayApiEnum.RetPt;
            ApiInterface = SumapayConfig.TestApiUrl + (collective ? "main/CollectiveFinance_returnPrinInte" : "main/TransactionForFT_returnPrinInte");
            RequestId = ((Agp2pEnums.SumapayApiEnum)Api).ToString().ToUpper() + Utils.GetOrderNumberLonger();
            Collective = collective;
        }

        public override string GetSignature()
        {
            return
                SumaPayUtils.GenSign(RequestId + SumapayConfig.MerchantCode + ProjectCode + Sum  + PayType + SubledgerList + NoticeUrl + MainAccountType + MainAccountCode, SumapayConfig.Key);
        }

        public override string GetPostPara()
        {
            var postStr =
                $"requestId={RequestId}&merchantCode={SumapayConfig.MerchantCode}&projectCode={ProjectCode}&sum={Sum}&payType={PayType}&subledgerList={SubledgerList}&noticeUrl={NoticeUrl}&signature={GetSignature()}";
            if (!string.IsNullOrEmpty(MainAccountType)) postStr += $"&mainAccountType={MainAccountType}";
            if (!string.IsNullOrEmpty(MainAccountCode)) postStr += $"&mainAccountCode={MainAccountCode}";
            return postStr;
        }

        public void SetSubledgerList(List<li_project_transactions> trans)
        {
            var subledgerList = new List<object>();
            trans.ForEach(t =>
            {
                subledgerList.Add( 
                    //投资者收益
                    new
                    {
                        roleType = "0",
                        roleCode = t.investor.ToString(),
                        inOrOut = "0",
                        sum = (t.interest + t.principal).ToString()
                    });
            });
            SubledgerList = JsonHelper.ObjectToJSON(subledgerList);
        }
    }
}
