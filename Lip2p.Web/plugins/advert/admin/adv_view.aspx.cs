using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.Advert.admin
{
    public partial class adv_view : Lip2p.Web.UI.ManagePage
    {
        protected int id = 0;
        protected Model.advert model = new Model.advert();

        protected void Page_Load(object sender, EventArgs e)
        {
            this.id = DTRequest.GetQueryInt("id", 0);
            if (this.id < 1)
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            if (!new BLL.advert().Exists(this.id))
            {
                JscriptMsg("信息不存在或已被删除！", "back", "Error");
                return;
            }
            if (!Page.IsPostBack)
            {
                model = new BLL.advert().GetModel(id);
            }
        }

        #region 显示广告类型=================================
        protected string GetTypeName(int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return "文字";
                case 2:
                    return "图片";
                case 3:
                    return "幻灯片";
                case 4:
                    return "动画";
                case 5:
                    return "FLV视频";
                case 6:
                    return "代码";
                default:
                    return "其它";
            }
        }
        #endregion

    }
}