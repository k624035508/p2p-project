using System.Web;

namespace Agp2p.API.Payment.Ecpss
{
    public class Config
    {
        //网银在线商户编号
        public static string partner = "24419";
        //网银在线MD5校验码
        public static string key = "!i^GySRo";
        //显示支付通知页面
        public static string return_url = "https://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/api/payment/ecpss/return_url.aspx";

        //支付完成后的回调处理页面
        public static string notify_url = "http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/api/payment/ecpss/notify_url.aspx";
        //支付网关地址
        public static string gateway = "https://pay.ecpss.com/sslpayment";
        //查询订单接口地址
        public static string queryUrl = "https://merchant.ecpss.com/merchantBatchQueryAPI";
    }
}
