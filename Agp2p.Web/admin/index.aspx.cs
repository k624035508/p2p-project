using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Newtonsoft.Json;

namespace Agp2p.Web.admin
{
    public partial class index : Web.UI.ManagePage
    {
        protected Model.manager admin_info;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                admin_info = GetAdminInfo();
            }
        }

        //安全退出
        protected void lbtnExit_Click(object sender, EventArgs e)
        {
            Session[DTKeys.SESSION_ADMIN_INFO] = null;
            Utils.WriteCookie("AdminName", "Agp2p", -14400);
            Utils.WriteCookie("AdminPwd", "Agp2p", -14400);
            Response.Redirect("login.aspx");
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static int AjaxQueryUnreadMessagesCount()
        {
            var manager = GetAdminInfo();
            if (manager == null)
            {
                return 0;
            }

            var context = new Agp2pDataContext();
            return context.li_manager_messages.Count(m => m.receiver == manager.id && !m.isRead);
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string AjaxQueryManagerMessages(int type, int pageSize, int pageIndex)
        {
            var manager = GetAdminInfo();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (manager == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            var context = new Agp2pDataContext();
            var query = context.li_manager_messages.Where(m => m.receiver == manager.id && (type == 0 || m.source == type));
            var msgs =
                query
                    .OrderByDescending(msg => msg.id)
                    .Skip(pageSize * pageIndex).Take(pageSize).AsEnumerable()
                    .Select(msg => new
                    {
                        msg.id,
                        msg.title,
                        msg.body,
                        msg.isRead,
                        creationTime = msg.creationTime.ToString("yyyy-MM-dd HH:mm")
                    }).ToList();
            return JsonConvert.SerializeObject(new {totalCount = query.Count(), msgs});
        }

        [WebMethod]
        public static string AjaxSetManagerMessagesRead(string msgIds)
        {
            var manager = GetAdminInfo();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (manager == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            if (string.IsNullOrWhiteSpace(msgIds))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "没有指定消息";
            }

            var preRead = msgIds.Split(';').Select(str => Convert.ToInt32(str)).ToArray();

            var context = new Agp2pDataContext();
            context.li_manager_messages.Where(m => m.receiver == manager.id)
                .Where(m => preRead.Contains(m.id))
                .ForEach(m =>
                {
                    m.isRead = true;
                });
            context.SubmitChanges();
            ManagerMessageHubFacade.Instance.OnManagerReadMsg(manager.user_name);
            return "设置成功";
        }

        [WebMethod]
        public static string AjaxDeleteManagerMessages(string msgIds)
        {
            var manager = GetAdminInfo();
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            if (manager == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录";
            }

            if (string.IsNullOrWhiteSpace(msgIds))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "没有指定消息";
            }

            var preRead = msgIds.Split(';').Select(str => Convert.ToInt32(str)).ToArray();

            var context = new Agp2pDataContext();
            var msgs = context.li_manager_messages.Where(m => m.receiver == manager.id).Where(m => preRead.Contains(m.id)).ToList();
            context.li_manager_messages.DeleteAllOnSubmit(msgs);
            context.SubmitChanges();
            return "删除成功";
        }
    }
}