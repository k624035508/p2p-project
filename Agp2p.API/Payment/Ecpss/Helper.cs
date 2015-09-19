using System;

using System.Security.Cryptography;
/// <summary>
/// Helper 的摘要说明
/// </summary>
namespace Agp2p.API.Payment.Ecpss
{
    public class Helper
    {
        public Helper()
        {
        }
        //将字符串经过md5加密，返回加密后的字符串的小写表示
        public static string Md5Encrypt(string strToBeEncrypt)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] FromData = System.Text.Encoding.GetEncoding("utf-8").GetBytes(strToBeEncrypt);
            Byte[] TargetData = md5.ComputeHash(FromData);
            string Byte2String = "";
            for (int i = 0; i < TargetData.Length; i++)
            {
                Byte2String += TargetData[i].ToString("x2");
            }
            return Byte2String.ToUpper();
        }

        public static bool CheckReturnMD5(string billNo, string amount, string status, string returnMD5)
        {
            return Md5Encrypt(billNo + "&" + amount + "&" + status + "&" + Config.key).Equals(returnMD5);
        }

        public static string GetResultInfo(string resultCode)
        {
            string resultInfo = string.Empty; 
            switch (resultCode)
            {
                case "88":
                    resultInfo = "交易成功";
                    break;
                case "1":
                    resultInfo = "银行代码错误";
                    break;
                case "10":
                    resultInfo = "商户信息不存在";
                    break;
                case "11":
                    resultInfo = "加密参数MD5KEY值有误";
                    break;
                case "13":
                    resultInfo = "签名验证错误";
                    break;
                case "15":
                    resultInfo = "商户号未开通";
                    break;
                case "16":
                    resultInfo = "商户通道未开通";
                    break;
                case "22":
                    resultInfo = "交易网址未绑定或审核通过";
                    break;
                case "24":
                    resultInfo = "交易流水号重复";
                    break;
                case "25":
                    resultInfo = "商户交易流水号重复（一天内）";
                    break;
                case "26":
                    resultInfo = "参数有空值";
                    break;               
            }
            return resultInfo;
        }
        
        
    }
}
