using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Lip2p.Common;

namespace Lip2p.Core
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

        #region 九维短信验证码
        /// <summary>
        /// 发送短信验证码，九维接口
        /// </summary>
        /// <param name="mobiles"></param>
        /// <param name="content"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SendSmsCode(string mobile, string content, out string msg)
        {
            //检测号码，忽略不合格的
            Regex r = new Regex(@"^1\d{10}$", RegexOptions.IgnoreCase);
            if (r.Match(mobile) != null)
            {
                //发送短信
                try
                {
                    Guid result = Guid.Empty;
                    if (Guid.TryParse(ESMRootService.ESMService.Service.SendMsg("hcclcom", "hccl", "hccl1023", mobile, content), out result))
                    {
                        msg = "发送短信验证码成功！";
                        return true;
                    }
                    else
                    {
                        msg = "发送短信验证码失败！";
                        return false;
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
        #endregion

        #region 莫名营销短信
        /// <summary>
        /// 发送手机短信 2015/3/26 修改为适应九维天地接口
        /// 发送手机短信 2015/4/25 修改为适应莫名接口
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
                    Match m = r.Match(mobile); //搜索匹配项
                    if (m != null)
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
                        #region 注释九维接口
                        //string result = Utils.HttpPost(siteConfig.smsapiurl,
                        //    "productid=621215&username=" + siteConfig.smsusername + "&password=" + siteConfig.smspassword + "&mobile=" + Utils.DelLastComma(sb.ToString()) + "&content=" + Utils.UrlEncode(content));
                        //string[] strArr = result.Split(new string[] { "," }, StringSplitOptions.None);
                        //if (strArr[0] != "1")
                        //{
                        //    var errorType = "";
                        //    if (strArr[0] == "-1")
                        //        errorType = "用户名或者密码不正确";
                        //    else if (strArr[0] == "2")
                        //        errorType = "短信余额不足";
                        //    else if (strArr[0] == "6")
                        //        errorType = "有效号码为空";
                        //    else if (strArr[0] == "7")
                        //        errorType = "短信内容为空";
                        //    else if (strArr[0] == "8")
                        //        errorType = "一级黑词（" + strArr[1] + "）";
                        //    else if (strArr[0] == "9")
                        //        errorType = "未开通接口提交权限或用户已被禁用";
                        //    else if (strArr[0] == "10")
                        //        errorType = "发送号码过多";
                        //    else if (strArr[0] == "11")
                        //        errorType = "产品ID异常";
                        //    else if (strArr[0] == "12")
                        //        errorType = "参数异常";

                        //    errorMsg = "提交失败，" + errorType;
                        //    continue;
                        //} 
                        #endregion

                        string result = Utils.HttpPost(siteConfig.smsapiurl,
                            "action=send&username=" + siteConfig.smsusername + "&password=" + siteConfig.smspassword + "&phone=" + Utils.DelLastComma(sb.ToString()) + "&content=" + Utils.UrlEncode(content) + "&encode=utf8");

                        if (result != "100")
                        {
                            errorMsg = "提交失败，" + GetMoMingErrorType(result);
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
        /// 查询账户剩余短信数量 2015/3/26 修改为适应九维天地接口
        /// 查询账户剩余短信数量 2015/4/24 修改为适应莫名接口
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
                //string result = Utils.HttpPost("http://esm2.9wtd.com:9001/balance.do", "productid=621215&username=" + siteConfig.smsusername + "&password=" + 230532);
                //string[] strArr = result.Split(new string[] { "||" }, StringSplitOptions.None);
                //var count = Utils.StrToInt(strArr[0], 0);
                //if (count < 0)
                //{
                //    code = count.ToString();
                //    return 0;
                //}
                //code = "100";
                //return count;

                string result = Utils.HttpPost(siteConfig.smsapiurl, "action=getBalance&username=" + siteConfig.smsusername + "&password=" + siteConfig.smspassword);
                string[] strArr = result.Split(new string[] { "||" }, StringSplitOptions.None);
                code = strArr[0];
                if (code == "100")
                {
                    return Utils.StrToInt(strArr[1], 0);
                }
                else
                    code = "115";
                return 0;
            }
            catch
            {
                code = "115";
                return 0;
            }
        }

        private static string GetMoMingErrorType(string result)
        {
            string errorType = string.Empty;
            switch (result)
            {
                case "101":
                    errorType = "验证失败";
                    break;
                case "102":
                    errorType = "短信不足";
                    break;
                case "103":
                    errorType = "操作失败";
                    break;
                case "104":
                    errorType = "非法字符";
                    break;
                case "105":
                    errorType = "内容过多";
                    break;
                case "106":
                    errorType = "号码过多";
                    break;
                case "107":
                    errorType = "频率过快";
                    break;
                case "108":
                    errorType = "号码内容空";
                    break;
                case "109":
                    errorType = "账号冻结";
                    break;
                case "111":
                    errorType = "禁止频繁单条发送";
                    break;
                case "112":
                    errorType = "系统暂停发送";
                    break;
                case "113":
                    errorType = "号码错误";
                    break;
                case "114":
                    errorType = "定时时间格式不对";
                    break;
                case "115":
                    errorType = "连接失败";
                    break;
                case "116":
                    errorType = "禁止接口发送";
                    break;
                case "117":
                    errorType = "绑定IP不正确";
                    break;
                case "120":
                    errorType = "系统升级";
                    break;
            }
            return errorType;
        }
        #endregion

        #region 莫名系统提示短信
        /// <summary>
        /// 发送模板短信（充值、提现、收益）
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="content"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SendTemplateSms(string mobile, string content, out string msg)
        {
            //检测号码，忽略不合格的
            Regex r = new Regex(@"^1\d{10}$", RegexOptions.IgnoreCase);
            if (r.Match(mobile) != null)
            {
                //发送短信
                try
                {
                    string result = Utils.HttpPost("http://api.duanxin.cm/",
                        "action=send&username=70203373&password=e76975f27fe945a4200bbd8bc8033a2c&phone=" + mobile + "&content=" + HttpUtility.UrlEncode(content) + "&encode=utf8");

                    if (result != "100")
                    {
                        msg = "发送短信失败，" + GetMoMingErrorType(result);
                        return false;
                    }
                }
                catch(Exception e)
                {
                    msg = "发送短信失败：" + e.Message;
                    return false;
                }
            }

            msg = "发送短信成功！";
            return true;
        } 
        #endregion

        /// <summary>
        /// 查询已发送数量 2015/3/26 暂无此接口
        /// </summary>
        //public int GetSendQuantity(out string code)
        //{
        //    //检查是否设置好短信账号
        //    if (!Exists())
        //    {
        //        code = "115";
        //        return 0;
        //    }
        //    try
        //    {
        //        string result = Utils.HttpPost(siteConfig.smsapiurl, "cmd=se&uid=" + siteConfig.smsusername + "&pwd=" + siteConfig.smspassword);
        //        string[] strArr = result.Split(new string[] { "||" }, StringSplitOptions.None);
        //        if (strArr[0] != "100")
        //        {
        //            code = strArr[0];
        //            return 0;
        //        }
        //        code = strArr[0];
        //        return Utils.StrToInt(strArr[1], 0);
        //    }
        //    catch
        //    {
        //        code = "115";
        //        return 0;
        //    }
        //}

    }
}
