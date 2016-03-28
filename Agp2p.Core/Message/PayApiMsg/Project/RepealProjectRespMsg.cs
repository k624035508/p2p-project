namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 流标普通项目响应
    /// </summary>
    public class RepealProjectRespMsg : BaseRespMsg
    {
        public string Sum { get; set; }//金额
        public string ProjectSum { get; set; }//项目总额
        

        public RepealProjectRespMsg()
        {

        }

        public override bool CheckSignature()
        {
            return base.CheckSignature(RequestId + ProjectCode  + Result);
        }
    }
}
