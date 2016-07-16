using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.UI.Page
{
    public partial class point_detail : Web.UI.BasePage
    {
        Agp2pDataContext context = new Agp2pDataContext();
        protected int goods_id;
        protected dt_article articleModel;
        protected dt_article_attribute_value articleDetail;
        /// <summary>
        /// 重写父类的虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            Init += Page_Init;
            goods_id = DTRequest.GetQueryInt("id");
            articleModel = context.dt_article.SingleOrDefault(a => a.id == goods_id);
            articleDetail = articleModel.dt_article_attribute_value;
        }

        /// <summary>
        /// OnInit事件
        /// </summary>
        void Page_Init(object sender, EventArgs e)
        {
           
        }


    }
}
