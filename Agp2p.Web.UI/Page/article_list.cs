using System;
using System.Collections.Generic;
using System.Text;
using Agp2p.Common;

namespace Agp2p.Web.UI.Page
{
    public partial class article_list : Web.UI.BasePage
    {
        protected int page;         //当前页码
        protected int category_id;  //类别ID
        protected int totalcount;   //OUT数据总数
        protected string pagelist;  //分页页码

        protected Model.article_category category = new Model.article_category { title = "所有信息" };
        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            page = DTRequest.GetQueryInt("page", 1);
            category_id = DTRequest.GetQueryInt("category_id");
            if (category_id > 0) //如果ID获取到，将使用ID
            {
                var bll = new BLL.article_category();
                if (bll.Exists(category_id))
                    category = bll.GetModel(category_id);
            }
          
        }
    }
}
