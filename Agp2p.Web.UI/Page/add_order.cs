using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.UI.Page
{
    public partial class add_order : usercenter
    {
        [WebMethod]
        public new static string AjaxAppendAddress(string address, string postalCode, string orderName, string orderPhone)
        {
            var userInfo = GetUserInfoByLinq();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (userInfo == null)
            {
                HttpContext.Current.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            if (!new Regex(@"^\d{11,}$").IsMatch(orderPhone))
            {
                HttpContext.Current.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return "手机号码格式不正确";
            }
            var context= new Agp2pDataContext();

        }
    }
}
