
namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 个人自动还款开通/取消响应
    /// </summary>
    public class AutoRepaySignRespMsg : BaseRespMsg
    {
        public string BankAccount { get; set; }//银行账号
        public string BankName { get; set; }//银行名称
        public string Name { get; set; }//姓名
        public bool Cancel { get; set; }

        public AutoRepaySignRespMsg(bool cancel = false)
        {
            Cancel = cancel;
        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + Result + UserIdIdentity);
        }
    }
}
