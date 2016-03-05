using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agp2p.API.Payment.Sumapay
{
    /// <summary>
    /// 丰付各请求接口提交类
    /// </summary>
    public class SumapaySubmit
    {
        /// <summary>
        /// 获取请求接口的基本参数
        /// </summary>
        /// <param name="requestId">请求流水号</param>
        /// <param name="merchantCode">商户编号</param>
        /// <param name="userIdIdentity">第三方用户标识</param>
        /// <param name="signature">数字签名</param>
        /// <param name="failReturnUrl">失败跳转地址</param>
        /// <param name="noticeUrl">异步通知地址</param>
        /// <param name="successReturnUrl">成功跳转地址</param>
        /// <returns></returns>
        private static Dictionary<string, string> BuildRequestBasePara(string requestId, string userIdIdentity, string signature,
            string merchantCode = SumapayConfig.MerchantCode, string successReturnUrl = SumapayConfig.SuccessReturnUrl, 
            string failReturnUrl = SumapayConfig.FailReturnUrl, string noticeUrl = SumapayConfig.NoticeUrl)
        {
            Dictionary<string, string> sPara = new Dictionary<string, string>
            {
                {"requestId", requestId},
                {"userIdIdentity", userIdIdentity},
                {"signature", signature},
                {"merchantCode", merchantCode},
                {"successReturnUrl", successReturnUrl},
                {"failReturnUrl", failReturnUrl},
                {"noticeUrl", noticeUrl}
            };

            return sPara;
        }

        private static void UserRegister()
        {
            
        }

    }
}
