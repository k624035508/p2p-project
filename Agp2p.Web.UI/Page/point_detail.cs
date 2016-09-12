using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Web;

namespace Agp2p.Web.UI.Page
{
    public partial class point_detail : Web.UI.BasePage
    {
        Agp2pDataContext context = new Agp2pDataContext();
        protected int goods_id;
        protected dt_article articleModel;
        protected dt_article_attribute_value articleDetail;
        protected List<dt_article> jiaxiquanValue;
        protected dt_article_albums detailAlbums;

        /// <summary>
        /// 重写父类的虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            Init += Page_Init;
            goods_id = DTRequest.GetQueryInt("id");
            articleModel = context.dt_article.SingleOrDefault(a => a.id == goods_id);
            if (articleModel == null)
            {
                HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您要浏览的页面不存在或已删除啦！")));
                return;
            }
            articleDetail = articleModel.dt_article_attribute_value;
            jiaxiquanValue = context.dt_article.Where(a => a.dt_article_category.call_index == "jiaxijuan").ToList();
            detailAlbums = context.dt_article_albums.Where(a => a.article_id == goods_id && a.remark.Contains("详情")).FirstOrDefault();
        }

        /// <summary>
        /// OnInit事件
        /// </summary>
        void Page_Init(object sender, EventArgs e)
        {
           
        }


    }
}
