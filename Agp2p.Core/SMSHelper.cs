using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Agp2p.Common;
using Agp2p.Model;

namespace Agp2p.Core
{
    /// <summary>
    /// 手机短信
    /// </summary>
    public class SMSHelper
    {
        /// <summary>
        /// 检查账户信息是否正确
        /// </summary>
        /// <returns></returns>
        public static bool Exists()
        {
            siteconfig siteConfig = ConfigLoader.loadSiteConfig(); //获得站点配置信息
            if (string.IsNullOrEmpty(siteConfig.smsapiurl) || string.IsNullOrEmpty(siteConfig.smsusername) || string.IsNullOrEmpty(siteConfig.smspassword))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 发送短信验证码，九维接口
        /// </summary>
        /// <param name="mobiles"></param>
        /// <param name="content"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SendSmsCode(string mobile, string content, out string msg)
        {
            siteconfig siteConfig = ConfigLoader.loadSiteConfig(); //获得站点配置信息
            //检测号码，忽略不合格的
            Regex r = new Regex(@"^1\d{10}$", RegexOptions.IgnoreCase);
            if (r.Match(mobile).Success)
            {
                //发送短信
                try
                {
                    string result = Utils.HttpPost(siteConfig.smsapiurl,
                        "productid=936245&username=" + siteConfig.smsusername + "&password=" + siteConfig.smspassword + "&mobile=" + mobile + "&content=" + Utils.UrlEncode(content));
                    string[] strArr = result.Split(new string[] { "," }, StringSplitOptions.None);
                    if (strArr[0] != "1")
                    {
                        msg = GetJwErrorType(strArr[0]);
                        return false;
                    }
                    else
                    {
                        msg = "发送短信验证码成功！";
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    msg = "发送短信验证码失败：" + ex.Message;
                    return false;
                }
            }
            else
            {
                msg = "发送短信验证码失败：短信号码错误！";
                return false;
            }
        }

        /// <summary>
        /// 发送营销短信 九维天地接口
        /// </summary>
        /// <param name="mobiles">手机号码，以英文“,”逗号分隔开</param>
        /// <param name="content">短信内容</param>
        /// <param name="pass">短信通道1验证码通道2广告通道 暂无用</param>
        /// <param name="msg">返回提示信息</param>
        /// <returns>bool</returns>
        public static bool Send(string mobiles, string content, int pass, out string msg)
        {
            siteconfig siteConfig = ConfigLoader.loadSiteConfig(); //获得站点配置信息
            //检查是否设置好短信账号
            if (!Exists())
            {
                msg = "短信配置参数有误，请完善后再提交！";
                return false;
            }
            //检查手机号码，如果超过2000则分批发送
            int sucCount = 0; //成功提交数量
            string errorMsg = string.Empty; //错误消息
            string[] oldMobileArr = mobiles.Split(',');
            int batch = oldMobileArr.Length / 2000 + 1; //2000条为一批，求出分多少批

            for (int i = 0; i < batch; i++)
            {
                StringBuilder sb = new StringBuilder();
                int sendCount = 0; //发送数量
                int maxLenght = (i + 1) * 2000; //循环最大的数

                //检测号码，忽略不合格的，重新组合
                for (int j = 0; j < oldMobileArr.Length && j < maxLenght; j++)
                {
                    int arrNum = j + (i * 2000);
                    string pattern = @"^1\d{10}$";
                    string mobile = oldMobileArr[arrNum].Trim();
                    if (mobile.Length != 11)
                    {
                        continue;
                    }
                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase); //正则表达式实例，不区分大小写
                    if (r.Match(mobile).Success)
                    {
                        sendCount++;
                        sb.Append(mobile + ",");
                    }
                }

                //发送短信
                if (sb.ToString().Length > 0)
                {
                    try
                    {
                        string result = Utils.HttpPost(siteConfig.smsapiurl,
                            "productid=936245&username=" + siteConfig.smsusername + "&password=" + siteConfig.smspassword + "&mobile=" + Utils.DelLastComma(sb.ToString()) + "&content=" + Utils.UrlEncode(content));
                        string[] strArr = result.Split(new string[] { "," }, StringSplitOptions.None);
                        if (strArr[0] != "1")
                        {
                            errorMsg = "提交失败，" + GetJwErrorType(strArr[0]);
                            continue;
                        }
                        sucCount += sendCount; //成功数量
                    }
                    catch
                    {
                        //没有动作
                    }
                }
            }

            //返回状态
            if (sucCount > 0)
            {
                msg = "成功提交" + sucCount + "条，失败" + (oldMobileArr.Length - sucCount) + "条";
                return true;
            }
            msg = errorMsg;
            return false;
        }

        /// <summary>
        /// 发送模板短信 九维天地接口（充值、提现、收益）
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="content"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SendTemplateSms(string mobile, string content, out string msg)
        {
            siteconfig siteConfig = ConfigLoader.loadSiteConfig();
            //检测号码，忽略不合格的
            Regex r = new Regex(@"^1\d{10}$", RegexOptions.IgnoreCase);
            if (r.Match(mobile).Success)
            {
                try
                {
                    string result = Utils.HttpPost(siteConfig.smsapiurl,
                        "productid=936245&username=" + siteConfig.smsusername + "&password=" + siteConfig.smspassword + "&mobile=" + mobile + "&content=" + HttpUtility.UrlEncode(content));
                    string[] strArr = result.Split(new string[] { "," }, StringSplitOptions.None);
                    if (strArr[0] != "1")
                    {
                        msg = "发送短信失败，" + GetJwErrorType(strArr[0]);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    msg = "发送短信失败：" + e.Message;
                    return false;
                }
            }

            msg = "发送短信成功！";
            return true;
        }

        private static string GetJwErrorType(string error)
        {
            switch (error)
            {
                case "-1":
                    return "用户名或者密码不正确";
                case "2":
                    return "短信余额不足";
                case "6":
                    return "有效号码为空";
                case "7":
                    return "短信内容为空";
                case "8":
                    return "一级黑词";
                case "9":
                    return "未开通接口提交权限或用户已被禁用";
                case "10":
                    return "发送号码过多";
                case "11":
                    return "产品ID异常";
                case "12":
                    return "参数异常";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 查询账户剩余短信数量 九维天地接口
        /// </summary>
        public static int GetAccountQuantity(out string code)
        {
            siteconfig siteConfig = ConfigLoader.loadSiteConfig(); //获得站点配置信息
            //检查是否设置好短信账号
            if (!Exists())
            {
                code = "115";
                return 0;
            }
            try
            {
                string result = Utils.HttpPost("http://esm2.9wtd.com:9001/balance.do", "productid=936245&username=" + siteConfig.smsusername + "&password=" + 541263);
                string[] strArr = result.Split(new string[] { "||" }, StringSplitOptions.None);
                var count = Utils.StrToInt(strArr[0], 0);
                if (count < 0)
                {
                    code = count.ToString();
                    return 0;
                }
                code = "100";
                return count;
            }
            catch
            {
                code = "115";
                return 0;
            }
        }
    }
}
