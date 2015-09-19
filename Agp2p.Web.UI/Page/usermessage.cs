using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.UI.Page
{
    public partial class usermessage : usercenter
    {
        protected int page;         //当前页码
        protected int totalcount;   //OUT数据总数
        protected int message_status;       //消息状态
        protected DataTable messageDt;        

        /// <summary>
        /// 重写虚方法,此方法在Init事件执行
        /// </summary>
        protected override void InitPage()
        {
            base.InitPage();
            page = DTRequest.GetQueryInt("page", 1);
            message_status = DTRequest.GetQueryInt("message_status", 0);

            if (message_status == 0)
                messageDt = get_user_message_list(10, page, "accept_user_name='" + userModel.user_name + "' and type=1", out totalcount);
            else if(message_status == 1)
                messageDt = get_user_message_list(10, page, "accept_user_name='" + userModel.user_name + "' and type=1 and is_read=1", out totalcount);
            else
                messageDt = get_user_message_list(10, page, "accept_user_name='" + userModel.user_name + "' and type=1 and (is_read=0 or is_read is null)", out totalcount);             
        }

        [WebMethod]
        public static string AjaxDeleteMessage(string messageIds)
        {
            var userInfo = GetUserInfo();
            if (userInfo == null)
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }
            var ids = messageIds.Split(';').Select(str => Convert.ToInt32(str)).ToArray();
            if (!ids.Any())
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "请先选择要删除的消息";
            }
            var context = new Agp2pDataContext();
            var msgs = context.dt_user_message.Where(m => ids.Contains(m.id)).ToList();
            context.dt_user_message.DeleteAllOnSubmit(msgs);
            context.SubmitChanges();
            return "删除成功";
        }
    }
}
