using System;
using System.Collections.Generic;
using System.Text;
using Agp2p.Common;
using System.Data;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using Agp2p.Linq2SQL;
using System.Linq;
using Agp2p.BLL;
using Agp2p.Core;


namespace Agp2p.Web.UI.Page
{
    public partial class point_list : Web.UI.BasePage
    {
        protected const int PAGE_SIZE = 12;
        protected int page;   //当前页码
        protected int totalcount;   //OUT数据总数
        protected string pagelist;  //分页页码

        protected int category_id;  //商品分类
        protected int rootCategoryId; //商品分类父类
        protected int point_range;  //积分详情
        protected Agp2pDataContext context = new Agp2pDataContext();

        protected Dictionary<int, string> SubCategoryIdTitleMap;

        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            page = DTRequest.GetQueryInt("page", 1);
            //项目字段
            category_id = DTRequest.GetQueryInt("category_id");
            point_range = DTRequest.GetQueryInt("point_range");

           
        }

       
        public static IEnumerable<dt_article> QueryMallProducts(int pageSize, int pageIndex, out int total, 
            int categoryId = 0, int pointRange = 0)
        {
            Agp2pDataContext context = new Agp2pDataContext();
            var query = context.dt_article.Where(a => a.channel_id == 16);
            if (categoryId > 0)   //商品类型
            {
                switch ((Agp2pEnums.MallProductsType)categoryId)
                {
                    case Agp2pEnums.MallProductsType.Shiwu:
                        query = query.Where(a => a.category_id == 73);
                        break;
                    case Agp2pEnums.MallProductsType.Xuni:
                        query = query.Where(a => a.dt_article_category.parent_id == 72);
                        break;    
                }
            }

            if (pointRange > 0)  //积分范围
            {
                switch ((Agp2pEnums.MallPointRange)pointRange)
                {
                    case Agp2pEnums.MallPointRange.LessThanFive:
                        query = query.Where(a => a.dt_article_attribute_value.point < 50000);
                        break;
                    case Agp2pEnums.MallPointRange.LessThanTen:
                        query = query.Where(a => a.dt_article_attribute_value.point >= 50000 && a.dt_article_attribute_value.point < 100000);
                        break;
                }
            }
            query.AsEnumerableAutoPartialQuery(out total, pageSize);

            return query.OrderBy(q => q.add_time).Skip(pageSize*pageIndex).Take(pageSize);
        }

    }
}
