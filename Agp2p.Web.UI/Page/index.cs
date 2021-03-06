﻿using System;
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
        protected List<dt_advert_banner> QueryBanner()
        {
            var context = new Agp2pDataContext();
            var invokeBanner =
                context.dt_advert_banner.Where(
                    a =>
                        a.is_lock == 0 && a.dt_advert.title.Contains("首页") && a.end_time >= DateTime.Today)
                    .OrderBy(a => a.sort_id).ToList();
                   
            return invokeBanner;
        }

        protected List<dt_advert_banner> QueryWechatBanner()
        {
            var context = new Agp2pDataContext();
            var invokeBanner =
                context.dt_advert_banner.Where(
                    a =>
                        a.is_lock == 0 && a.dt_advert.title.Contains("移动端") && a.end_time >= DateTime.Today)
                    .OrderBy(a => a.sort_id).ToList();

            return invokeBanner;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //微信自动登录
            //var code = DTRequest.GetQueryString("code");            
            //if (!string.IsNullOrEmpty(code))
            //{               
            //    MessageBus.Main.Publish(new UserLoginMsg(0, true, code, () =>
            //    {
            //        HttpContext.Current.Response.Redirect(linkurl("index"));
            //    }));
            //}
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
