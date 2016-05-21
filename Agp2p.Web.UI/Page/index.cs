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
    public partial class index : Web.UI.BasePage
    {
        protected Dictionary<int, string> CategoryIdTitleMap =
            new Agp2pDataContext().dt_article_category.Where(c => c.channel_id == 6)
                .ToDictionary(c => c.id, c => c.title);

        protected List<dt_advert_banner> QueryBanner(int aid)
        {
            var context = new Agp2pDataContext();
            var invokeBanner =
                context.dt_advert_banner.Where(
                    a =>
                        a.is_lock == 0 && a.aid == aid)
                    .OrderBy(a => a.sort_id).ToList();
                   
            return invokeBanner;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //微信自动登录
            var code = DTRequest.GetQueryString("code");            
            if (!string.IsNullOrEmpty(code))
            {               
                MessageBus.Main.Publish(new UserLoginMsg(0, true, code, () =>
                {
                    HttpContext.Current.Response.Redirect(linkurl("index"));
                }));
            }
        }

        protected li_projects QueryFirstTrialProject()
        {
            var context = new Agp2pDataContext();
            var trialProject =
                context.li_projects.Where(
                    p =>
                        p.tag == (int) Agp2pEnums.ProjectTagEnum.Trial &&
                        (int) Agp2pEnums.ProjectStatusEnum.Financing <= p.status)
                    .OrderByDescending(p => p.publish_time)
                    .FirstOrDefault();
            return trialProject;
        }
        
    }

}
