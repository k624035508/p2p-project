using System;
using System.Xml;
using System.Text;
using System.Data;
using System.Web;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.LineService
{
    public class online_js : IHttpHandler
    {
        Lip2p.Model.siteconfig siteConfig = new Lip2p.BLL.siteconfig().loadConfig();

        public void ProcessRequest(HttpContext context)
        {
            string fullPath = Utils.GetMapPath(siteConfig.webpath + "plugins/lineservice/set.config");
            XmlDocument doc = new XmlDocument();
            doc.Load(fullPath);
            XmlNode phone = doc.SelectSingleNode(@"config/telephone");
            XmlNode status = doc.SelectSingleNode(@"config/status");
            XmlNode skin = doc.SelectSingleNode(@"config/skin");

            context.Response.Write("var default_view =" + status.InnerText + ";");
            context.Response.Write("var htmlStr = '<link rel=\"stylesheet\" type=\"text/css\" href=\"" + siteConfig.webpath + "plugins/lineservice/skin/" + skin.InnerText + "/style.css\" />'\n");
            context.Response.Write("+ '<scr'+'ipt type=\"text/javascript\" src=\"" + siteConfig.webpath + "plugins/lineservice/js/jquery.lazyload.min.js\"></sc'+'ript>'\n");
            context.Response.Write("+ '<scr'+'ipt type=\"text/javascript\" src=\"" + siteConfig.webpath + "plugins/lineservice/js/script.js\"></sc'+'ript>'\n");
            context.Response.Write("+ '<div id=\"online_service_bar\">'\n");
            context.Response.Write("+ '<div id=\"online_service_minibar\"></div>'\n");
            context.Response.Write("+ '<div id=\"online_service_fullbar\">'\n");
            context.Response.Write("+ '<div class=\"service_bar_head\"><span id=\"service_bar_close\" title=\"点击关闭\">点击关闭</span></div>'\n");
            context.Response.Write("+ '<div class=\"service_bar_main\">'\n");
            context.Response.Write("+ '<ul class=\"service_menu\">'\n");
            DataSet ds1 = new BLL.online_service_group().GetList(0, "is_lock=0", "sort_id asc,id desc");
            foreach (DataRow dr1 in ds1.Tables[0].Rows)
            {
                context.Response.Write("+ '<li'");
                if (Convert.ToInt32(dr1["default_view"]) == 1)
                {
                    context.Response.Write("+ ' class=\"hover\"'");
                }
                context.Response.Write("+ '>'\n");
                context.Response.Write("+ '<dl><dt>" + dr1["title"].ToString() + "</dt><dd>'\n");
                DataSet ds2 = new BLL.online_service().GetList(0, "is_lock=0 and group_id=" + dr1["id"].ToString(), "sort_id asc,add_time desc");
                foreach (DataRow dr2 in ds2.Tables[0].Rows)
                {
                    context.Response.Write("+ '<a target=\"_blank\" href=\"" + dr2["link_url"].ToString() + "\"><img border=\"0\" src=\"" 
                        + dr2["img_url"].ToString() + "\" alt=\"" + dr2["title"].ToString() + "\" title=\"" + dr2["title"].ToString() + "\" /></a>'\n");
                }
                context.Response.Write("+ '</dd></dl>'\n");
                context.Response.Write("+ '</li>'\n");
            }
            context.Response.Write("+ '</ul>'\n");
            context.Response.Write("+ '<div class=\"service_tel\">" + phone.InnerText + "</div>'\n");
            context.Response.Write("+ '</div>'\n");
            context.Response.Write("+ '</div>'\n");
            context.Response.Write("+ '</div>'\n");
            context.Response.Write("document.write(htmlStr);\n");
        }
        
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
