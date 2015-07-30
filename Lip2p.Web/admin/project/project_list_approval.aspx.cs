using System;
using Lip2p.Common;
using System.Text;

namespace Lip2p.Web.admin.project
{
    public partial class project_list_approval : project_list
    {
        //页面初始化事件
        protected void Page_Init(object sernder, EventArgs e)
        {
            this.page_name = "approval";            
        }
    }
}