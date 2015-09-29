using System;
using System.Collections.Generic;
using System.Text;
using Agp2p.Common;

namespace Agp2p.DAL
{
    /// <summary>
    /// 数据访问类:平台费用配置
    /// </summary>
    public partial class cost_config
    {
        private static object lockHelper = new object();

        /// <summary>
        ///  读取站点配置文件
        /// </summary>
        public Model.costconfig loadConfig(string configFilePath)
        {
            return SerializationHelper.Load<Model.costconfig>(configFilePath);
        }

        /// <summary>
        /// 写入站点配置文件
        /// </summary>
        public Model.costconfig saveConifg(Model.costconfig model, string configFilePath)
        {
            lock (lockHelper)
            {
                SerializationHelper.Save(model, configFilePath);
            }
            return model;
        }

    }
}
