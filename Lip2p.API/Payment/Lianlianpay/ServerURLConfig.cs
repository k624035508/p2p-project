using System;

/**
* 请求服务地址 配置
*
*/

namespace Lip2p.API.Payment.Lianlianpay
{
	public class ServerURLConfig
	{
        public static string BANK_PAY_URL = "https://yintong.com.cn/payment/bankgateway.htm";//网银支付接口
		public static string PAY_URL = "https://yintong.com.cn/payment/authpay.htm"; // 连连支付WEB收银台支付服务地址
        public static string WAP_PAY_URL = "https://yintong.com.cn/llpayh5/authpay.htm";//wap认证支付接口
		public static string QUERY_USER_BANKCARD_URL = "https://yintong.com.cn/traderapi/userbankcard.htm"; // 用户已绑定银行卡列表查询
		public static string QUERY_BANKCARD_URL = "https://yintong.com.cn/traderapi/bankcardquery.htm"; //银行卡卡bin信息查询
        public static string QUERY_ORDER = "https://yintong.com.cn/traderapi/orderquery.htm";//支付结果查询服务
	}
}

