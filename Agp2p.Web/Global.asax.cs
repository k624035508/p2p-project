using System;
using System.Collections.Generic;
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

        public static Dictionary<TimerMsg.Type, DailyTimer> TimerDict = new Dictionary<TimerMsg.Type, DailyTimer>();

        public static void SchaduleDailyTimer(TimerMsg.Type timerType, int hours, int minutes = 0, int seconds = 0, bool todayExecuted = false)
        {
            var dailyTimer = TimerDict.GetValueOrDefault(timerType, (DailyTimer) null);
            if (dailyTimer != null && dailyTimer.Running)
            {
                dailyTimer.Release();
            }
            dailyTimer = new DailyTimer(hours, minutes, seconds, callBackEnum =>
            {
                MessageBus.Main.Publish(new TimerMsg(timerType, onTime: callBackEnum == CallBackEnum.OnTime));
                if (callBackEnum == CallBackEnum.OnTime)
                {
                    new Agp2pDataContext().AppendAdminLogAndSave("Timer", "全局定时器执行了一次");
                }
            }, ex =>
            {
                new Agp2pDataContext().AppendAdminLogAndSave("Timer", "全局定时器报错：" + ex.GetSimpleCrashInfo());
                //if (Utils.IsDebugging()) throw ex;
            });
            TimerDict[timerType] = dailyTimer;
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

        public static void InitDailyTimer(TimerMsg.Type timerType, string autoRepayTime)
        {
            var match = new Regex(@"^(\d{1,2}):(\d{2}):(\d{2})$").Match(autoRepayTime);
            if (match.Success)
            {
                SchaduleDailyTimer(timerType, Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value), Convert.ToInt32(match.Groups[3].Value));
            }
            else
            {
                throw new InvalidOperationException("不正确的日期格式：" + autoRepayTime);
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            MessageBus.Main.exceptionCallback = ex =>
            {
                new Agp2pDataContext().AppendAdminLogAndSave("MessageBusError", ex.GetSimpleCrashInfo());
            };
            InitDailyTimer(TimerMsg.Type.AutoRepayTimer, ConfigLoader.loadSiteConfig().systemTimerTriggerTime);
            InitDailyTimer(TimerMsg.Type.LoanerRepayTimer, ConfigLoader.loadSiteConfig().loanerRepayTime);
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