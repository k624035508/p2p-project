using System;
using System.Web.UI;
using Lip2p.Common;
using System.Text;

namespace Lip2p.Web.admin.project
{
    public partial class project_list_publish : project_list
    {
        //页面初始化事件
        protected void Page_Init(object sernder, EventArgs e)
        {
            page_name = "publish";
        }
    }
}