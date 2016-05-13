using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Security.Cryptography.X509Certificates;

namespace Agp2p.Common
{
    public class SumaPayUtils
    {
        /// <summary>
        /// HMACMD5签名函数
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GenSign(string msg, string key)
        {
            if (msg == null || key == null)
            {
                throw new Exception("Invalid arguments!");
            }
            try
            {
                byte[] msg_bytes = Encoding.UTF8.GetBytes(msg);
                byte[] key_bytes = Encoding.UTF8.GetBytes(key);

                HMACMD5 hmac = new HMACMD5(key_bytes);
                byte[] result_bytes = hmac.ComputeHash(msg_bytes);

                string sign = "";
                for (int i = 0; i < result_bytes.Length; i++)
                {
                    // byte转换为16进制格式字符串。如果字符串只有1位，则前面补零。
                    string hex = result_bytes[i].ToString("x");
                    if (hex.Length == 1)
                    {
                        hex = "0" + hex;
                    }
                    sign += hex;
                }
                return sign;
            }
            catch (Exception ex)
            {
                throw new Exception("GenSign failed: " + ex.Message);
            }
        }
        /// <summary>
        /// Sets the cert policy.
        /// </summary>
        public static void SetCertificatePolicy()
        {
           System.Net.ServicePointManager.ServerCertificateValidationCallback
                       += RemoteCertificateValidate;
        }

        /// <summary>
        /// Remotes the certificate validate.
        /// </summary>
        private static bool RemoteCertificateValidate(
           object sender,System.Security.Cryptography.X509Certificates.X509Certificate cert,
            System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {

            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }
            //SslPolicyErrors.RemoteCertificateNameMismatch 一般是访问的url名字和证书中的cnname名称不一致

            return true;//真实商用环境需要return false
        }
    }
}