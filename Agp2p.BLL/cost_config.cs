using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Caching;
using Agp2p.Common;

namespace Agp2p.BLL
{
    public partial class cost_config
    {
        private readonly DAL.cost_config dal = new DAL.cost_config();

        /// <summary>
        ///  读取用户配置文件
        /// </summary>
        public Model.costconfig loadConfig()
        {
            Model.costconfig model = CacheHelper.Get<Model.costconfig>(DTKeys.CACHE_COST_CONFIG);
            if (model == null)
            {
                CacheHelper.Insert(DTKeys.CACHE_COST_CONFIG, dal.loadConfig(Utils.GetXmlMapPath(DTKeys.FILE_COST_XML_CONFING)),
                    Utils.GetXmlMapPath(DTKeys.FILE_COST_XML_CONFING));
                model = CacheHelper.Get<Model.costconfig>(DTKeys.CACHE_COST_CONFIG);
            }
            return model;
        }

        /// <summary>
        ///  保存用户配置文件
        /// </summary>
        public Model.costconfig saveConifg(Model.costconfig model)
        {
            return dal.saveConifg(model, Utils.GetXmlMapPath(DTKeys.FILE_COST_XML_CONFING));
        }

    }
}
