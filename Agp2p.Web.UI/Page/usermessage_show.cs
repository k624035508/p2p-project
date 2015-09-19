using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web;
using Agp2p.Common;

namespace Agp2p.Web.UI.Page
{
    public partial class usermessage_show : usercenter
    {
        protected int id;
        protected Model.user_message model = new Model.user_message();
        protected int next_id = 0;
        protected int message_status;

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
            id = DTRequest.GetQueryInt("id");
            message_status = DTRequest.GetQueryInt("status", 0);

            BLL.user_message bll = new BLL.user_message();
            if (!bll.Exists(id))
            {
                HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您要浏览的页面不存在或已删除啦！")));
                return;
            }
            model = bll.GetModel(id);
            if (model.accept_user_name != userModel.user_name || (model.type == 2 && model.post_user_name != userModel.user_name))
            {
                HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您所查看的并非自己的短消息！")));
                return;
            }
            //设为已阅读状态
            bll.UpdateField(id, "is_read=1,read_time='" + DateTime.Now + "'");
            //查找下一条
            var messageDt = new DataTable();
            if (message_status == 0)
                messageDt = bll.GetList(10, string.Format("accept_user_name='{0}' and type=1 and Id<{1}", userModel.user_name, id), "id desc").Tables[0];
            else if (message_status == 1)
                messageDt = bll.GetList(10, string.Format("accept_user_name='{0}' and type=1 and is_read=1 and Id<{1}", userModel.user_name, id), "id desc").Tables[0];
            else
                messageDt = bll.GetList(10, string.Format("accept_user_name='{0}' and type=1 and is_read=0 and Id<{1}", userModel.user_name, id), "id desc").Tables[0];
            next_id = messageDt != null && messageDt.Rows.Count > 0 ? (int)messageDt.Rows[0]["id"] : 0;
        }
    }
}
