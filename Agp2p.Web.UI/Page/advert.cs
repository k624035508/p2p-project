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
    public class advert : usercenter
    {
        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();

        }

        public new static string AjaxQueryBanner()
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var context = new Agp2pDataContext();
            var data = context.dt_advert_banner.Where(a => a.is_lock == 0 && a.dt_advert.title.Contains("个人") && a.end_time >= DateTime.Today).Select(a => new
            {
                banId = a.id,
                banTitle = a.title,
                banImg = a.file_path,
                banUrl = a.link_url
            });

            return JsonConvert.SerializeObject(data);
        }
    }
}
