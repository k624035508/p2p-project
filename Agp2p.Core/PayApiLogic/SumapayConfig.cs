using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class SumapayConfig
    {
        public static string MerchantCode = "fbp100012";
        public static string Key = "RPZJNXEUQW89L2AF6HS5YIDGBC37MKT4V";
        public static string TestApiUrl = "https://fbtest.sumapay.com/";
        public static string ApiUrl = "";
        public static string NoticeUrl = "http://test.agrhp2p.com/api/payment/sumapay/notifyUrl.aspx";
    }
}
