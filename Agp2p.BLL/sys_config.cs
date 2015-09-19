using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Caching;
using Agp2p.Common;

namespace Agp2p.BLL
{
    public partial class siteconfig
    {
        private readonly DAL.siteconfig dal = new DAL.siteconfig();

        /// <summary>
        ///  读取配置文件
        /// </summary>
        public Model.siteconfig loadConfig()
        {
            try
            {
                Model.siteconfig model = CacheHelper.Get<Model.siteconfig>(DTKeys.CACHE_SITE_CONFIG);
                if (model == null)
                {
                    var configFilePath = Utils.GetXmlMapPath(DTKeys.FILE_SITE_XML_CONFING);
                    CacheHelper.Insert(DTKeys.CACHE_SITE_CONFIG, dal.loadConfig(configFilePath), configFilePath);
                    model = CacheHelper.Get<Model.siteconfig>(DTKeys.CACHE_SITE_CONFIG);
                }
                return model;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///  保存配置文件
        /// </summary>
        public Model.siteconfig saveConifg(Model.siteconfig model)
        {
            return dal.saveConifg(model, Utils.GetXmlMapPath(DTKeys.FILE_SITE_XML_CONFING));
        }

    }
}
