using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using Agp2p.Core.Message;
using System.Web;

namespace Agp2p.Web.UI.Page
{
    public partial class article : Web.UI.BasePage
    {
        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
        }

      /*  protected List<dt_advert_banner> QueryMallBanner()
        {
            var context = new Agp2pDataContext();
            var mallBanner = context.dt_advert_banner.Where(a => a.is_lock == 0 && a.dt_advert.title.Contains("商城") && a.end_time >= DateTime.Today).OrderBy(a => a.sort_id).ToList();
            return mallBanner;
        } */
    }
}
