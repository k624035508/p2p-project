using System;
using System.Runtime.Caching;
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
