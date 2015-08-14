using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using Lip2p.Common;
using Lip2p.Core;
using Lip2p.Core.Message;
using Lip2p.Linq2SQL;
using Lip2p.Web.admin.settings;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;
using siteconfig = Lip2p.BLL.siteconfig;

namespace Lip2p.Web
{
    public class Global : HttpApplication
    {
        private const string CacheName = "timer_task";

        private static void AddTask(string name, object value, DateTime runAt, CacheItemRemovedCallback cb)
        {
            HttpRuntime.Cache.Insert(name, value, null,
                runAt, Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, cb); // 系统自身每 20 秒检查一次是否有缓存超时
        }

        public static void SchaduleDailyTimer(int hours, int minutes = 0, int seconds = 0, bool todayExecuted = false)
        {
            var todayTriggerTime = DateTime.Now.Date.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);
            if (DateTime.Now < todayTriggerTime)
            {
                AddTask(CacheName, 0, todayTriggerTime, (key, value, reason) =>
                {
                    if (reason == CacheItemRemovedReason.Removed) return; // 被替换/系统退出则不触发
                    MessageBus.Main.PublishAsync(new TimerMsg(onTime: true));
                    SchaduleDailyTimer(hours, minutes, seconds, todayExecuted: true);
                });
            }
            else
            {
                if (!todayExecuted)
                {
                    MessageBus.Main.PublishAsync(new TimerMsg(onTime: false));
                }

                var nextDayTriggerTime = todayTriggerTime.AddDays(1);
                AddTask(CacheName, 0, nextDayTriggerTime, (key, value, reason) =>
                {
                    if (reason == CacheItemRemovedReason.Removed) return; // 被替换/系统退出则不触发
                    MessageBus.Main.PublishAsync(new TimerMsg(onTime: true));
                    SchaduleDailyTimer(hours, minutes, seconds, todayExecuted: true);
                });
            }
        }

        public void DelayedRelease()
        {
            var linqContext = new Lip2pDataContext();
            var project = linqContext.li_projects.Where(p => p.status==(int)Lip2pEnums.ProjectStatusEnum.DelayFaBiao).ToList();
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
                    project[i].status = (int)Lip2pEnums.ProjectStatusEnum.FaBiao;
                }
            }
            linqContext.SubmitChanges();
        }

        public static void InitDailyTimer(string autoRepayTime = null)
        {
            autoRepayTime = autoRepayTime ?? ConfigLoader.loadSiteConfig().autoRepayTime;
            var match = new Regex(@"^(\d{1,2}):(\d{2}):(\d{2})$").Match(autoRepayTime);
            if (match.Success)
            {
                SchaduleDailyTimer(Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value), Convert.ToInt32(match.Groups[3].Value));
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
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
                templet_list.MarkTemplates("main", "lip2p", config);
                if (config.mobilestatus == 1)
                {
                    templet_list.MarkTemplates("mobile", "weChat", config);
                }
                stopwatch.Stop();

                new Lip2pDataContext().AppendAdminLogAndSave(DTEnums.ActionEnum.Build.ToString(), "自动生成模版成功，耗时：" + stopwatch.Elapsed);
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