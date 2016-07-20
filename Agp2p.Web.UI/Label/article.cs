using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.UI
{
    public partial class BasePage : System.Web.UI.Page
    {
        #region 列表标签======================================
        /// <summary>
        /// 文章列表
        /// </summary>
        /// <param name="channel_name">频道名称</param>
        /// <param name="top">显示条数</param>
        /// <param name="strwhere">查询条件</param>
        /// <returns>DataTable</returns>
        protected DataTable get_article_list(string channel_name, int top, string strwhere)
        {
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(channel_name))
            {
                dt = new BLL.article().GetList(channel_name, top, strwhere, "add_time desc").Tables[0];
            }
            return dt;
        }

        /// <summary>
        /// 文章列表
        /// </summary>
        /// <param name="channel_name">频道名称</param>
        /// <param name="category_id">分类ID</param>
        /// <param name="top">显示条数</param>
        /// <param name="strwhere">查询条件</param>
        /// <returns>DataTable</returns>
        protected DataTable get_article_list(string channel_name, int category_id, int top, string strwhere)
        {
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(channel_name))
            {
                dt = new BLL.article().GetList(channel_name, category_id, top, strwhere, "add_time desc").Tables[0];
            }
            return dt;
        }

        /// <summary>
        /// 文章列表
        /// </summary>
        /// <param name="channel_name">频道名称</param>
        /// <param name="category_id">分类ID</param>
        /// <param name="top">显示条数</param>
        /// <param name="strwhere">查询条件</param>
        /// <param name="orderby">排序</param>
        /// <returns>DataTable</returns>
        protected DataTable get_article_list(string channel_name, int category_id, int top, string strwhere, string orderby)
        {
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(channel_name))
            {
                dt = new BLL.article().GetList(channel_name, category_id, top, strwhere, orderby).Tables[0];
            }
            return dt;
        }

        /// <summary>
        /// 文章分页列表(自定义页面大小)
        /// </summary>
        /// <param name="channel_name">频道名称</param>
        /// <param name="category_id">分类ID</param>
        /// <param name="page_size">页面大小</param>
        /// <param name="page_index">当前页码</param>
        /// <param name="strwhere">查询条件</param>
        /// <param name="orderby">排序</param>
        /// <param name="totalcount">总记录数</param>
        /// <returns>DateTable</returns>
        protected DataTable get_article_list(string channel_name, int category_id, int page_size, int page_index, string strwhere, string orderby, out int totalcount)
        {
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(channel_name))
            {
                dt = new BLL.article().GetList(channel_name, category_id, page_index, strwhere, orderby, out totalcount, out page_size).Tables[0];
            }
            else
            {
                totalcount = 0;
            }
            return dt;
        }

        /// <summary>
        /// 文章分页列表(自动页面大小)
        /// </summary>
        /// <param name="channel_name">频道名称</param>
        /// <param name="category_id">分类ID</param>
        /// <param name="page_index">当前页码</param>
        /// <param name="strwhere">查询条件</param>
        /// <param name="totalcount">总记录数</param>
        /// <param name="_key">URL配置名称</param>
        /// <param name="_params">传输参数</param>
        /// <returns>DataTable</returns>
        protected DataTable get_article_list(string channel_name, int category_id, int page_index, string strwhere, out int totalcount, out string pagelist, string _key, params object[] _params)
        {
            DataTable dt = new DataTable();
            int pagesize;
            if (!string.IsNullOrEmpty(channel_name))
            {
                dt = new BLL.article().GetList(channel_name, category_id, page_index, strwhere, "sort_id asc,add_time desc", out totalcount, out pagesize).Tables[0];
                pagelist = Utils.OutPageList(pagesize, page_index, totalcount, linkurl(_key, _params), 8);
            }
            else
            {
                totalcount = 0;
                pagelist = "";
            }
            return dt;
        }

        /// <summary>
        /// 文章分页列表(可排序)
        /// </summary>
        /// <param name="channel_name">频道名称</param>
        /// <param name="category_id">分类ID</param>
        /// <param name="page_index">当前页码</param>
        /// <param name="strwhere">查询条件</param>
        /// <param name="orderby">排序</param>
        /// <param name="totalcount">总记录数</param>
        /// <param name="_key">URL配置名称</param>
        /// <param name="_params">传输参数</param>
        /// <returns>DataTable</returns>
        protected DataTable get_article_list(string channel_name, int category_id, int page_index, string strwhere, string orderby, out int totalcount, out string pagelist, string _key, params object[] _params)
        {
            DataTable dt = new DataTable();
            int pagesize;
            if (!string.IsNullOrEmpty(channel_name))
            {
                dt = new BLL.article().GetList(channel_name, category_id, page_index, strwhere, orderby, out totalcount, out pagesize).Tables[0];
                pagelist = Utils.OutPageList(pagesize, page_index, totalcount, linkurl(_key, _params), 8);
            }
            else
            {
                totalcount = 0;
                pagelist = "";
            }
            return dt;
        }        
        #endregion

        #region 内容标签======================================
        /// <summary>
        /// 根据调用标识取得内容
        /// </summary>
        /// <param name="call_index">调用别名</param>
        /// <returns>String</returns>
        protected string get_article_content(string call_index)
        {
            if (string.IsNullOrEmpty(call_index))
                return string.Empty;
            BLL.article bll = new BLL.article();
            if (bll.Exists(call_index))
            {
                return bll.GetModel(call_index).content;
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取扩展字段的值
        /// </summary>
        /// <param name="article_id">内容ID</param>
        /// <param name="field_name">扩展字段名</param>
        /// <returns>String</returns>
        protected string get_article_field(int article_id, string field_name)
        {
            Model.article model = new BLL.article().GetModel(article_id);
            if (model != null && model.fields.ContainsKey(field_name))
            {
                return model.fields[field_name];
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取扩展字段的值
        /// </summary>
        /// <param name="call_index">调用别名</param>
        /// <param name="field_name">扩展字段名</param>
        /// <returns>String</returns>
        protected string get_article_field(string call_index, string field_name)
        {
            if (string.IsNullOrEmpty(call_index))
                return string.Empty;
            BLL.article bll = new BLL.article();
            if (!bll.Exists(call_index))
            {
                return string.Empty;
            }
            Model.article model = bll.GetModel(call_index);
            if (model != null && model.fields.ContainsKey(field_name))
            {
                return model.fields[field_name];
            }
            return string.Empty;
        }
        #endregion

        protected dt_article GetArticle(int id)
        {
            var context = new Agp2pDataContext();
            return context.dt_article.Single(a => a.id == id);
        }

        protected IEnumerable<dt_article> GetArticles(int categoryId, int pageSize, int pageIndex = 0)
        {
            int count;
            return GetArticles("" + categoryId, pageSize, pageIndex, out count);
        }

        protected IEnumerable<dt_article> GetArticles(string categoryIds, int pageSize, int pageIndex = 0)
        {
            int count;
            return GetArticles(categoryIds, pageSize, pageIndex, out count);
        }

        protected IEnumerable<dt_article> GetArticles(string categoryIds, int pageSize, int pageIndex, out int totalCount)
        {
            var context = new Agp2pDataContext();
            var categoryIdArr = categoryIds.Split(',').Select(str => Convert.ToInt32(str)).ToArray();
            var queryable = context.dt_article.Where(a => categoryIdArr.Contains(a.category_id) && a.status == 0);
            totalCount = queryable.Count();
            return queryable.OrderBy(a => a.sort_id).ThenByDescending(a => a.add_time)
                .Skip(pageSize*pageIndex)
                .Take(pageSize)
                .AsEnumerable();
        }
        //查询虚拟券
        protected dt_article GetXuniquan()
        {
            var context = new Agp2pDataContext();
            var xuniquan = context.dt_article.Where(d => d.title.Contains("1%加息券")).FirstOrDefault();
            return xuniquan;
        }


        /// <summary>
        /// 获取最新动态（包括置顶的公告）
        /// </summary>
        /// <returns></returns>
        protected dt_article GetNewestAnnounce()
        {
            var context = new Agp2pDataContext();
            var queryable = context.dt_article.Where(a => a.status == 0 && (a.category_id == 42 || (a.category_id == 43 && a.is_top == 1)));
            return queryable.OrderBy(a => a.sort_id).ThenByDescending(a => a.add_time).FirstOrDefault();
        }

        #region 购物标签======================================
        /// <summary>
        /// 返回相应的图片
        /// </summary>
        /// <param name="article_id">信息ID</param>
        /// <returns>String</returns>
        protected string get_article_img_url(int article_id)
        {
            Model.article model = new BLL.article().GetModel(article_id);
            if (model != null)
            {
                return model.img_url;
            }
            return "";
        }

        /// <summary>
        /// 返回对应商品的会员价格
        /// </summary>
        /// <param name="article_id">信息ID</param>
        /// <returns>Decimal</returns>
        protected decimal get_user_article_price(int article_id)
        {
            Model.users userModel = GetUserInfo();
            if (userModel == null)
            {
                return -1;
            }
            Model.article model = new BLL.article().GetModel(article_id);
            if (model != null)
            {
                if (model.group_price != null)
                {
                    Model.user_group_price priceModel = model.group_price.Find(p => p.group_id == userModel.group_id);
                    if (priceModel != null)
                    {
                        return priceModel.price;
                    }
                }
                if (model.fields.ContainsKey("sell_price"))
                {
                    return Utils.StrToDecimal(model.fields["sell_price"], -1);
                }
            }
            return -1;
        }
        #endregion

    }
}
