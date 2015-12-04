using System.Web;

namespace Agp2p.API.Payment.Ecpss
{
    public class Config
    {
        //汇潮商户编号
        public static string partner = "29013";
        //汇潮MD5校验码
        public static string key = "^HFL[g)k";
        //显示支付通知页面
        public static string return_url = "https://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/api/payment/ecpss/return_url.aspx";

        //支付完成后的回调处理页面
        public static string notify_url = "http://" + HttpContext.Current.Request.Url.Authority.ToLower() + "/api/payment/ecpss/notify_url.aspx";
        //支付网关地址
        public static string gateway = "https://pay.ecpss.com/sslpayment";
        //查询订单接口地址
        public static string queryUrl = "https://merchant.ecpss.com/merchantBatchQueryAPI";

        //汇潮商户编号(快捷)
        public static string partner_quick = "29217";
        //汇潮MD5校验码(快捷)
        public static string key_quick = "cXLfLBgs";
    }
}
