using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Core.Message;
using Agp2p.Linq2SQL;
using Agp2p.Web.admin.settings;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;
using siteconfig = Agp2p.BLL.siteconfig;

namespace Agp2p.Web
{
    public class Global : HttpApplication
    {
        private static DailyTimer dailyTimer;

        public static void SchaduleDailyTimer(int hours, int minutes = 0, int seconds = 0, bool todayExecuted = false)
        {
            if (dailyTimer != null && dailyTimer.Running)
            {
                dailyTimer.Release();
            }
            dailyTimer = new DailyTimer(hours, minutes, seconds, callBackEnum =>
            {
                MessageBus.Main.Publish(new TimerMsg(onTime: callBackEnum == CallBackEnum.OnTime));
                if (callBackEnum == CallBackEnum.OnTime)
                {
                    new Agp2pDataContext().AppendAdminLogAndSave("Timer", "全局定时器执行了一次");
                }
            }, ex =>
            {
                new Agp2pDataContext().AppendAdminLogAndSave("Timer", "全局定时器报错：" + ex.Message);
            });
        }

        /// <summary>
        /// 重新启动延迟发标的计划任务
        /// </summary>
        public void DelayedRelease()
        {
            var linqContext = new Agp2pDataContext();
            var project = linqContext.li_projects.Where(p => p.status==(int)Agp2pEnums.ProjectStatusEnum.FinancingAtTime).ToList();
            for (int i = 0; i < project.Count; i++)
            {
                //启动进程
                TimeSpan time = Convert.ToDateTime(project[i].publish_time).Subtract(DateTime.Now);
                if ((int)time.TotalMilliseconds > 600)
                {
                    MessageBus.Main.PublishDelay(new ProjectSchedulePublishMsg(project[i].id), (int)time.TotalMilliseconds);
                }
                else
                {
                    project[i].status = (int)Agp2pEnums.ProjectStatusEnum.Financing;
                }
            }
            linqContext.SubmitChanges();
        }

        public static void InitDailyTimer(string autoRepayTime = null)
        {
            autoRepayTime = autoRepayTime ?? ConfigLoader.loadSiteConfig().systemTimerTriggerTime;
            var match = new Regex(@"^(\d{1,2}):(\d{2}):(\d{2})$").Match(autoRepayTime);
            if (match.Success)
            {
                SchaduleDailyTimer(Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value), Convert.ToInt32(match.Groups[3].Value));
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            MessageBus.Main.exceptionCallback = ex =>
            {
                new Agp2pDataContext().AppendAdminLogAndSave("MessageBusError", ex.Message);
            };
            InitDailyTimer();
            DelayedRelease();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            //AutoGenerateTemplate();
        }

        private static void AutoGenerateTemplate()
        {
            var templateGenerated = Convert.ToBoolean(MemoryCache.Default["auto_template_generated"]); // Global 不能保存成员变量，每次都会初始化
            if (!templateGenerated && !Utils.IsDebugging())
            {
                MemoryCache.Default["auto_template_generated"] = true;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var config = new siteconfig().loadConfig();
                templet_list.MarkTemplates("main", "Agp2p", config);
                if (config.mobilestatus == 1)
                {
                    templet_list.MarkTemplates("mobile", "weChat", config);
                }
                stopwatch.Stop();

                new Agp2pDataContext().AppendAdminLogAndSave(DTEnums.ActionEnum.Build.ToString(), "自动生成模版成功，耗时：" + stopwatch.Elapsed);
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}