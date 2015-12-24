using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.predict
{
    public partial class projects_costing_predict : UI.ManagePage
    {

        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("projects_costing_predict", DTEnums.ActionEnum.View.ToString()); //检查权限
            }
        }


    }
}