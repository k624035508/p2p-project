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
    /// <summary>
    /// 项目列表继承类
    /// </summary>
    public partial class projects : Web.UI.BasePage
    {
        protected const int PAGE_SIZE = 15;         //当前页码
        protected int page;         //当前页码
        protected int rootCategoryId;  // 大类别ID
        protected int category_id;  //类别ID
        protected int totalcount;   //OUT数据总数
        protected string pagelist;  //分页页码        

        protected int project_profit_rate_index; //收益利率序号
        protected int project_repayment_index; //还款期限序号
        protected int project_status_index; // 项目状态

        protected Agp2pDataContext context = new Agp2pDataContext();

        protected Dictionary<int, string> SubCategoryIdTitleMap;
        protected Dictionary<int, int> FinancingProjectMap;

        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            page = DTRequest.GetQueryInt("page", 1);
            //项目字段
            project_profit_rate_index = DTRequest.GetQueryInt("profit_rate_index");
            project_repayment_index = DTRequest.GetQueryInt("repayment_index");
            project_status_index = DTRequest.GetQueryInt("status_index");

            category_id = DTRequest.GetQueryInt("category_id");

            if (category_id == 0)
                category_id = context.dt_article_category.Single(ca => ca.call_index == "piaojuzhuanqu").id;

            var category = context.dt_article_category.Single(ca => ca.id == category_id);
            if (category.parent_id.GetValueOrDefault() == 0)
            {
                // 当前 category_id 就是大类别ID
                rootCategoryId = category_id;
            }
            else
            {
                // 当前 category_id 不是大类别ID
                rootCategoryId = category.parent_id.Value;
            }

            //HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您要浏览的页面不存在或已删除啦！")));

            SubCategoryIdTitleMap = context.dt_article_category.Where(
                c => c.channel_id == 6 && c.parent_id == rootCategoryId)
                .OrderBy(c => c.sort_id)
                .ToDictionary(c => c.id, c => c.title);

            FinancingProjectMap =
                context.li_projects.Where(p => p.status == (int) Agp2pEnums.ProjectStatusEnum.Financing)
                    .GroupBy(p => p.dt_article_category.parent_id == 0 ? p.category_id : p.dt_article_category.parent_id.Value)
                    .Where(g => g.Any())
                    .ToDictionary(g => g.Key, g => g.Count());
        }

        [WebMethod]
        public static string AjaxQueryProjectList(int categoryId, short pageIndex, short pageSize)
        {
            int total = 0;
            var projectList = get_project_list(pageSize, pageIndex + 1, out total, categoryId, 0, 0, 0);
            return JsonConvert.SerializeObject(projectList);
        }

        protected void link_project_detail(string projectId)
        {
            HttpContext.Current.Response.Redirect(linkurl("project", projectId));
        }
    }
}
