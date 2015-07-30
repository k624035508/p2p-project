using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Lip2p.Linq2SQL
{
    partial class Lip2pDataContext
    {
        public Lip2pDataContext()
            : base(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString())
        {
            OnCreated();
        }
    }
}
