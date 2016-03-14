namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 流标普通项目响应
    /// </summary>
    public class RepealProjectRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//金额
        public string ProjectSum { get; set; }//项目总额
        

        public RepealProjectRespMsg(string requestId, string result, string responseContent) : base(requestId, result, responseContent)
        {
            //根据报文的json数据构造
            
        }

        public override bool CheckSignature()
        {
            return true;
        }

        public override bool CheckResult()
        {
            return Result.Equals("00000");
        }
    }
}
