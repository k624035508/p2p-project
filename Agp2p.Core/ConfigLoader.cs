using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using Agp2p.Common;
using Agp2p.Model;

namespace Agp2p.Core
{
    /// <summary>
    /// 数据访问类:站点配置
    /// </summary>
    public static class ConfigLoader 
    {
        //private static object lockHelper = new object();

        public static void CleanCache()
        {
            MemoryCache.Default.Remove("site_config");
            MemoryCache.Default.Remove("user_config");
            MemoryCache.Default.Remove("cost_config");
        }

        public static Dictionary<int, string> loadSumapayErrorNumberDescDict()
        {
            try
            {
                var configCache = (Dictionary<int, string>)MemoryCache.Default.Get("sumapay_err_num_config");
                if (configCache != null) return configCache;

                var numReg = new Regex(@"\d+");
                configCache = Utils.ReadAllLinesFromResource(Assembly.GetExecutingAssembly(), "Agp2p.Core.sumapay_error_no.txt").SelectMany(line =>
                {
                    var splitAt = line.IndexOf('：');
                    if (splitAt == -1)
                    {
                        splitAt = line.IndexOf(':');
                    }

                    var numPart = line.Substring(0, splitAt);
                    var descPart = line.Substring(splitAt + 1);
                    return numPart.MatchSteam(numReg).Select(m => new {Number = Convert.ToInt32(m.Value), Description = descPart});
                }).GroupBy(pair => pair.Number, pair => pair.Description).ToDictionary(g => g.Key, g => string.Join("，", g.ToList()));

                MemoryCache.Default.Set("sumapay_err_num_config", configCache, DateTime.Now.AddMinutes(20)); // 20 分钟超时
                return configCache;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        ///  读取站点配置文件，不要将 Config 存到静态变量，因为这个方法已经做了缓存控制
        /// </summary>
        /// <param name="loadOutsideProject"></param>
        public static siteconfig loadSiteConfig(bool loadOutsideProject = true)
        {
            try
            {
                var configCache = (siteconfig)MemoryCache.Default.Get("site_config");
                if (configCache != null) return configCache;

                configCache = SerializationHelper.Load<siteconfig>(Utils.GetXmlMapPath(DTKeys.FILE_SITE_XML_CONFING, loadOutsideProject));
                MemoryCache.Default.Set("site_config", configCache, DateTime.Now.AddMinutes(20)); // 20 分钟超时
                return configCache;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static costconfig loadCostConfig(bool loadOutsideProject = false)
        {
            try
            {
                var configCache = (costconfig)MemoryCache.Default.Get("cost_config");
                if (configCache != null) return configCache;

                configCache = SerializationHelper.Load<costconfig>(Utils.GetXmlMapPath(DTKeys.FILE_COST_XML_CONFING, loadOutsideProject));
                MemoryCache.Default.Set("cost_config", configCache, DateTime.Now.AddMinutes(20)); // 20 分钟超时
                return configCache;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static userconfig loadUserConfig(bool loadOutsideProject = false)
        {
            try
            {
                var configCache = (userconfig)MemoryCache.Default.Get("user_config");
                if (configCache != null) return configCache;

                configCache = SerializationHelper.Load<userconfig>(Utils.GetXmlMapPath(DTKeys.FILE_USER_XML_CONFING, loadOutsideProject));
                MemoryCache.Default.Set("user_config", configCache, DateTime.Now.AddMinutes(20)); // 20 分钟超时
                return configCache;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        /*/// <summary>
        /// 写入站点配置文件
        /// </summary>
        public static Config save(Config model)
        {
            lock (lockHelper)
            {
                SerializationHelper.Save(model, Utils.GetXmlMapPath(DTKeys.FILE_SITE_XML_CONFING));
            }
            return model;
        }*/

    }
}
