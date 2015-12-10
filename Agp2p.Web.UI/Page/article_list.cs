using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.UI.Page
{
    public partial class article_list : Web.UI.BasePage
    {
        protected int page;         //当前页码
        protected int category_id;  //类别ID
        protected string category_ids;  //类别ID
        protected int totalcount;   //OUT数据总数
        protected string pagelist;  //分页页码

        protected dt_article article;

        protected Model.article_category category = new Model.article_category { title = "所有信息" };
        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            page = DTRequest.GetQueryInt("page", 1);
            category_id = DTRequest.GetQueryInt("category_id");
            category_ids = DTRequest.GetQueryString("category_ids");
            if (category_id > 0) //如果ID获取到，将使用ID
            {
                var bll = new BLL.article_category();
                if (bll.Exists(category_id))
                    category = bll.GetModel(category_id);
            }
            else if (!string.IsNullOrWhiteSpace(category_ids))
            {
                var firstCategoryId = Convert.ToInt32(category_ids.Split(',')[0]);
                var bll = new BLL.article_category();
                if (bll.Exists(firstCategoryId))
                    category = bll.GetModel(firstCategoryId);
            }

            var articleId = DTRequest.GetQueryInt("articleId");
            if (articleId != 0)
            {
                var context = new Agp2pDataContext();
                article = context.dt_article.Single(a => a.id == articleId);
                article.click += 1;
                context.SubmitChanges();
            }
        }
    }
}
