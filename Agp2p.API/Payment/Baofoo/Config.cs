using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Xml;
using System.Collections.Generic;
using Agp2p.Common;

namespace Agp2p.API.Payment.Baofoo
{
    public class Config
    {
        //网银在线商户编号
        public static string partner = "100000178";
        //网银在线MD5校验码
        public static string key = "abcdefg";
        //加密方式默认1 MD5
        public static string key_type = "1";
        //终端id
        public static string terminal_id = "10000001";
        //接口版本
        public static string interface_version = "4.0";
        //显示支付通知页面
        public static string return_url = "http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/api/payment/baofoo/return_url.aspx";
        //支付完成后的回调处理页面
        public static string notify_url = "http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/api/payment/baofoo/notify_url.aspx";
        //支付网关地址
        public static string gateway = "http://vgw.baofoo.com/payindex";
    }
}
