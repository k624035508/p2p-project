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
#if DEBUG
        public static string MerchantCode = "fbp100012";
#endif
#if !DEBUG
        public static string MerchantCode = "4410000231";
#endif
#if DEBUG
        public static string Key = "RPZJNXEUQW89L2AF6HS5YIDGBC37MKT4V";
#endif
#if !DEBUG
        public static string Key = "yease5wrZsJuB9OObWqZ2O94ZmoPrGrH";
#endif
#if DEBUG
        public static string ApiUrl = "https://fbtest.sumapay.com/";
#endif
#if !DEBUG
        public static string ApiUrl = "https://www.sumapay.com/";
#endif
#if DEBUG
        public static string NoticeUrl = "http://test.agrhp2p.com/api/payment/sumapay/noticeUrl.aspx";
#endif
#if !DEBUG
        public static string NoticeUrl = "https://www.agrhp2p.com/api/payment/sumapay/noticeUrl.aspx";
#endif
    }
}
