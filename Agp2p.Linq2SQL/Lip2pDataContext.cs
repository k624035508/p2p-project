using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Agp2p.Linq2SQL
{
    partial class Agp2pDataContext
    {
        public Agp2pDataContext()
            : base(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString())
        {
            OnCreated();
        }
    }
}
