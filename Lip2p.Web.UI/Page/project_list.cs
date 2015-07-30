using System;
using System.Collections.Generic;
using System.Text;
using Lip2p.Common;
using System.Data;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using Lip2p.Linq2SQL;
using System.Linq;
using Lip2p.Core;

namespace Lip2p.Web.UI.Page
{
    /// <summary>
    /// 项目列表继承类
    /// </summary>
    public partial class project_list : Web.UI.BasePage
    {
        protected int page;         //当前页码
        protected int category_id;  //类别ID
        protected int totalcount;   //OUT数据总数
        protected string pagelist;  //分页页码        

        protected Model.article_category model = new Model.article_category();//项目类别

        protected int project_profit_rate_index; //收益利率序号
        protected int project_repayment_index; //还款期限序号
        protected int project_amount_index; //金额范围序号

        protected string category_name;//类别名称
        protected int category_id2;  //切换的类别ID
        protected string category_name2;//切换的类别名称
        protected Model.article_category model2 = new Model.article_category();//切换的项目类别
        protected string category_css;

        protected List<DataTable> dt_projects;//项目信息

        /// <summary>
        /// 重写虚方法,此方法将在Init事件前执行
        /// </summary>
        protected override void ShowPage()
        {
            page = DTRequest.GetQueryInt("page", 1);
            category_id = DTRequest.GetQueryInt("category_id");
            BLL.article_category bll = new BLL.article_category();
            model.title = "所有信息";
            if (category_id > 32 && category_id < 35) //如果ID获取到，将使用ID
            {
                var allCategory = bll.GetList(0, 6);
                foreach (DataRow dr in allCategory.Rows)
                {
                    if (dr["title"].ToString() == "汇商通") continue;
                    if ((int)dr["id"] == category_id)
                    {
                        if (bll.Exists(category_id))
                        {
                            model = bll.GetModel(category_id);
                            category_name = model.title;
                        }
                    }
                    else
                    {
                        category_id2 = (int)dr["id"];
                        model2 = bll.GetModel((int)dr["id"]);
                        category_name2 = model2.title;
                    }
                }
            }
            else
            {
                //HttpContext.Current.Response.Redirect(linkurl("error", "?msg=" + Utils.UrlEncode("出错啦，您要浏览的页面不存在或已删除啦！")));
                return;
            }

            //项目类别样式css
            category_css = category_name.Equals("金屋子") ? "titleleft house pull-left" : "titleleft car pull-left";

            //项目字段
            project_profit_rate_index = DTRequest.GetQueryInt("profit_rate_index");
            project_repayment_index = DTRequest.GetQueryInt("repayment_index");
            project_amount_index = DTRequest.GetQueryInt("amount_index");
        }

        [WebMethod]
        public static string AjaxQueryProjectList(int category_id, short pageIndex, short pageSize)
        {
            int total = 0;
            var projectList = get_project_list(pageSize, pageIndex, out total, category_id, 0, 0, 0);
            return JsonConvert.SerializeObject(projectList);
        }

        protected void link_project_detail(string projectId)
        {
            HttpContext.Current.Response.Redirect(linkurl("invest_detail", projectId));
        }
    }
}
