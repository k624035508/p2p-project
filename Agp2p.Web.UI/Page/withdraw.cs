using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.UI.Page
{
    public class withdraw : usercenter
    {      

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
            
        }

        public new static string AjaxQueryBankCards()
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var context = new Agp2pDataContext();
            var data = context.li_bank_accounts.Where(a => a.owner == userInfo.id).Select(a => new
            {
                cardId = a.id,
                bankName = a.bank,
                last4Char = a.account.Substring(a.account.Length - 4),
                cardNumber = a.account,
                openingBank = a.opening_bank,
                bankLocation = a.location,
                type = a.type
            });

            return JsonConvert.SerializeObject(data);
        }

    }
}
