using System;
using System.Collections.Generic;
using System.Text;
using Lip2p.Common;

namespace Lip2p.DAL
{
    public partial class orderconfig
    {
        private static object lockHelper = new object();

        /// <summary>
        ///  读取站点配置文件
        /// </summary>
        public Model.orderconfig loadConfig(string configFilePath)
        {
            return SerializationHelper.Load<Model.orderconfig>(configFilePath);
        }

        /// <summary>
        /// 写入站点配置文件
        /// </summary>
        public Model.orderconfig saveConifg(Model.orderconfig model, string configFilePath)
        {
            lock (lockHelper)
            {
                SerializationHelper.Save(model, configFilePath);
            }
            return model;
        }

    }
}
