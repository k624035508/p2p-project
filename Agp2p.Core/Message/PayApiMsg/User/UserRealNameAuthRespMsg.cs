
namespace Agp2p.Core.Message.PayApiMsg
{
    /// <summary>
    /// 实名认证响应
    /// </summary>
    public class UserRealNameAuthRespMsg : BaseRespMsg
    {
        public string Status { get; set; }//认证结果
        public string Token { get; set; }//认证token
        public string PictureData { get; set; }//认证图片数据
        public string UserName { get; set; }//用户姓名
        public string IdNumber { get; set; }//证件号
        public string PayType { get; set; }//手续费收取方式

        public UserRealNameAuthRespMsg()
        {
            
        }

        public override bool CheckSignature()
        {
            return this.CheckSignature(RequestId + Result + UserName + Status + Token + IdNumber);
        }
    }
}
