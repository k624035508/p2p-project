using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using Lip2p.Core.Message;
using System.Web;

namespace Lip2p.Web.UI.Page
{
    public partial class index : Web.UI.BasePage
    {
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
            var context = new Lip2pDataContext();
            var trialProject =
                context.li_projects.Where(
                    p =>
                        p.tag == (int) Lip2pEnums.ProjectTagEnum.Trial &&
                        (int) Lip2pEnums.ProjectStatusEnum.FaBiao <= p.status)
                    .OrderByDescending(p => p.publish_time)
                    .FirstOrDefault();
            return trialProject;
        }

        protected li_projects QueryFirstDailyProject()
        {
            var context = new Lip2pDataContext();
            var dailyProject =
                context.li_projects.Where(
                    p =>
                        p.tag == (int) Lip2pEnums.ProjectTagEnum.DailyProject &&
                        (int) Lip2pEnums.ProjectStatusEnum.FaBiao <= p.status)
                    .OrderByDescending(p => p.publish_time)
                    .FirstOrDefault();
            return dailyProject;
        }
    }

}
