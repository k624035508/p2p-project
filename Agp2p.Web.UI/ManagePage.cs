using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Model;

namespace Agp2p.Web.UI
{
    public class ManagePage : System.Web.UI.Page
    {
        protected internal Model.siteconfig siteConfig;
        protected internal Model.costconfig Costconfig;

        public ManagePage()
        {
            this.Load += new EventHandler(ManagePage_Load);
            siteConfig = new BLL.siteconfig().loadConfig();
            Costconfig = new BLL.cost_config().loadConfig();
        }

        private void ManagePage_Load(object sender, EventArgs e)
        {
            //判断管理员是否登录
            if (!IsAdminLogin())
            {
                Response.Write("<script>parent.location.href='" + siteConfig.webpath + siteConfig.webmanagepath + "/login.aspx'</script>");
                Response.End();
            }
        }

        #region 管理员============================================
        /// <summary>
        /// 判断管理员是否已经登录(解决Session超时问题)
        /// </summary>
        public static bool IsAdminLogin()
        {
            //如果Session为Null
            if (HttpContext.Current.Session[DTKeys.SESSION_ADMIN_INFO] != null)
            {
                return true;
            }
            else
            {
                //检查Cookies
                string adminname = Utils.GetCookie("AdminName", "Agp2p");
                string adminpwd = Utils.GetCookie("AdminPwd", "Agp2p");
                if (adminname != "" && adminpwd != "")
                {
                    BLL.manager bll = new BLL.manager();
                    Model.manager model = bll.GetModel(adminname, adminpwd);
                    if (model != null)
                    {
                        HttpContext.Current.Session[DTKeys.SESSION_ADMIN_INFO] = model;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 取得管理员信息
        /// </summary>
        public static Model.manager GetAdminInfo()
        {
            if (IsAdminLogin())
            {
                return HttpContext.Current.Session[DTKeys.SESSION_ADMIN_INFO] as Model.manager;
            }
            return null;
        }

        /// <summary>
        /// 检查管理员权限
        /// </summary>
        /// <param name="nav_name">菜单名称</param>
        /// <param name="action_type">操作类型</param>
        public bool ChkAdminLevel(string nav_name, string action_type)
        {
            bool result = ChkAdminLevelReturn(nav_name, action_type);
            if (!result)
            {
                string msgbox = "parent.jsdialog(\"错误提示\", \"您没有管理该页面的权限，请勿非法进入！\", \"back\", \"Error\")";
                Response.Write("<script type=\"text/javascript\">" + msgbox + "</script>");
                Response.End();
            }
            return result;
        }

        public bool ChkAdminLevelReturn(string nav_name, string action_type)
        {
            Model.manager model = GetAdminInfo();
            BLL.manager_role bll = new BLL.manager_role();
            return bll.Exists(model.role_id, nav_name, action_type);
        }

        /// <summary>
        /// 写入管理日志
        /// </summary>
        /// <param name="action_type"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public bool AddAdminLog(string action_type, string remark)
        {
            if (siteConfig.logstatus > 0)
            {
                Model.manager model = GetAdminInfo();
                int newId = new BLL.manager_log().Add(model.id, model.user_name, action_type, remark);
                if (newId > 0)
                {
                    return true;
                }
            }
            return false;
        }

        protected int GetPageSize(string pageName)
        {
            int pagesize;
            if (int.TryParse(Utils.GetCookie(pageName), out pagesize))
            {
                if (pagesize > 0)
                {
                    return pagesize;
                }
            }
            return 50;
        }

        protected void SetPageSize(string pageName, string pageSizeStr)
        {
            int pagesize;
            if (int.TryParse(pageSizeStr, out pagesize))
            {
                Utils.WriteCookie(GetType().Name + "_page_size", pageSizeStr, 14400);
            }
        }

        #endregion

        #region JS提示============================================
        /// <summary>
        /// 添加编辑删除提示
        /// </summary>
        /// <param name="msgtitle">提示文字</param>
        /// <param name="url">返回地址</param>
        /// <param name="msgcss">CSS样式</param>
        protected void JscriptMsg(string msgtitle, string url, string msgcss)
        {
            string msbox = "parent.jsprint(\"" + msgtitle + "\", \"" + url + "\", \"" + msgcss + "\")";
            ClientScript.RegisterClientScriptBlock(Page.GetType(), "JsPrint", msbox, true);
        }
        /// <summary>
        /// 带回传函数的添加编辑删除提示
        /// </summary>
        /// <param name="msgtitle">提示文字</param>
        /// <param name="url">返回地址</param>
        /// <param name="msgcss">CSS样式</param>
        /// <param name="callback">JS回调函数</param>
        protected void JscriptMsg(string msgtitle, string url, string msgcss, string callback)
        {
            string msbox = "parent.jsprint(\"" + msgtitle + "\", \"" + url + "\", \"" + msgcss + "\", " + callback + ")";
            ClientScript.RegisterClientScriptBlock(Page.GetType(), "JsPrint", msbox, true);
        }

        /// <summary>
        /// 添加编辑删除提示
        /// </summary>
        /// <param name="msgtitle">提示文字</param>
        /// <param name="url">返回地址</param>
        protected void JscriptMsg(string msgtitle, string url)
        {
            string msbox = "parent.jsprint(\"" + msgtitle + "\", \"" + url + "\")";
            ClientScript.RegisterClientScriptBlock(Page.GetType(), "JsPrint", msbox, true);
        }

        protected void ShowJsAlert(string msg)
        {
            Page.Controls.Add(new Literal
            {
                Text = "<script language='javascript'>" + Environment.NewLine + "window.alert('" + msg + "')</script>"
            });
        }
        #endregion

    }
}
