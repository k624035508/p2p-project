namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 债权转让响应
    /// </summary>
    public class CreditAssignmentRespMsg : BaseRespMsg
    {
        public string AssignmentSum { get; set; }//转让金额
        public bool OriginalRequestId { get; set; }//原请求流水号
        public string PayType { get; set; }//手续费收取方式
        public string MainAccountType { get; set; }//主账户类型
        public string MainAccountCode { get; set; }//主账户编        


        public CreditAssignmentRespMsg()
        {

        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId  + Result + AssignmentSum + UserIdIdentity);
        }
    }
}
