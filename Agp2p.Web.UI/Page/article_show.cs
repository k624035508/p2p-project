using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using Agp2p.Common;
using System.Data;
using System.Linq;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.UI.Page
{
    public partial class article_show : Web.UI.BasePage
    {
        protected int id;
        protected int category_id;
        protected string categoryTitle;
        protected string page = string.Empty;
        protected Model.article model = new Model.article();
        protected dt_article_attribute_value article_attr = new dt_article_attribute_value();
        
        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            var dbContext = new Agp2pDataContext();
            id = DTRequest.GetQueryInt("id");
            category_id = DTRequest.GetQueryInt("category_id");
            page = DTRequest.GetQueryString("page");
            BLL.article bll = new BLL.article();

            if (0 < id) //如果ID获取到，将使用ID
            {
                if (!bll.Exists(id))
                {
                    HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您要浏览的页面不存在或已删除啦！")));
                    return;
                }
                model = bll.GetModel(id);
            }
            else if (!string.IsNullOrEmpty(page)) //否则检查设置的别名
            {
                if (!bll.Exists(page))
                {
                    HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您要浏览的页面不存在或已删除啦！")));
                    return;
                }
                model = bll.GetModel(page);
            }
            else
            {
                return;
            }
            categoryTitle = dbContext.dt_article_category.Single(c => c.id == model.category_id).title;

            article_attr = dbContext.dt_article_attribute_value.FirstOrDefault(u => u.article_id == model.id);
            if (article_attr == null)
            {
                HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您要浏览的页面不存在或已删除啦！")));
                return;
            }
            //跳转URL
            if (model.link_url != null)
                model.link_url = model.link_url.Trim();
            if (!string.IsNullOrEmpty(model.link_url))
            {
                HttpContext.Current.Response.Redirect(model.link_url);
            }
        }

        protected DataTable get_is_red_article()
        {
            return new BLL.article().GetList(5, " category_id=" + category_id + " and is_red=1", "id asc").Tables[0];
        }
        
        /// <summary>
        /// 获取上一条下一条的链接
        /// </summary>
        /// <param name="urlkey">urlkey</param>
        /// <param name="type">-1代表上一条，1代表下一条</param>
        /// <param name="defaultvalue">默认文本</param>
        /// <param name="callIndex">是否使用别名，0使用ID，1使用别名</param>
        /// <returns>A链接</returns>
        protected string get_prevandnext_article(string urlkey, int type, string defaultvalue, int callIndex)
        {
            string symbol = (type == -1 ? "<" : ">");
            BLL.article bll = new BLL.article();
            string str = string.Empty;
            str = " and category_id=" + model.category_id;

            string orderby = type == -1 ? "id desc" : "id asc";
            DataSet ds = bll.GetList(1, "channel_id=" + model.channel_id + " " + str + " and status=0 and Id" + symbol + id, orderby);
            if (ds == null || ds.Tables[0].Rows.Count <= 0)
            {
                return defaultvalue;
            }
            if (callIndex == 1 && !string.IsNullOrEmpty(ds.Tables[0].Rows[0]["call_index"].ToString()))
            {
                return "<a href=\"" + linkurl(urlkey, ds.Tables[0].Rows[0]["call_index"].ToString()) + "\">" + ds.Tables[0].Rows[0]["title"] + "</a>";
            }
            return "<a href=\"" + linkurl(urlkey, ds.Tables[0].Rows[0]["id"].ToString()) + "\">" + ds.Tables[0].Rows[0]["title"] + "</a>";
        }
    }
}
