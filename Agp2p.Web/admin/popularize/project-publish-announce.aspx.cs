using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Core;

namespace Agp2p.Web.admin.popularize
{
    public partial class ProjectPublishAnnounce : Web.UI.ManagePage
    {
        protected int projectChannelId;
        protected int TotalCount;
        protected int PageIndex;
        protected int PageSize;

        protected int CategoryId;
        protected int ProjectStatus;
        protected string Keywords = string.Empty;
        protected Dictionary<int, string> CategoryIdTitleMap;

        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            this.CategoryId = DTRequest.GetQueryInt("category_id");
            this.ProjectStatus = DTRequest.GetQueryInt("project_status");
            this.Keywords = DTRequest.GetQueryString("keywords");

            projectChannelId = context.dt_channel.Single(ch => ch.name == "project").id;

            CategoryIdTitleMap = context.dt_article_category.Where(c => c.channel_id == projectChannelId).OrderBy(c => c.sort_id).ToDictionary(c => c.id, c => c.title);
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("project-publish-announce", DTEnums.ActionEnum.View.ToString());
                if (!string.IsNullOrEmpty(Keywords))
                    txtKeywords.Text = Keywords;
                TreeBind();
                RptBind();
            }
        }

        protected void TreeBind()
        {
            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有产品", ""));
            this.ddlCategoryId.Items.AddRange(CategoryIdTitleMap.Select(c => new ListItem(c.Value, c.Key.ToString())).ToArray());
        }


        #region 数据绑定=================================
        protected void RptBind()
        {
            this.PageIndex = DTRequest.GetQueryInt("page", 1);
            if (this.CategoryId > 0)
            {
                this.ddlCategoryId.SelectedValue = this.CategoryId.ToString();
            }

            //绑定列表
            this.rptList1.DataSource = GetList();
            this.rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = this.PageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("project-publish-announce.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}&page={4}",
                projectChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString(), "__id__");
            PageContent.InnerHtml = Utils.OutPageList(this.PageSize, this.PageIndex, this.TotalCount, pageUrl, 8);
        }

        /// <summary>
        /// 获取项目列表
        /// </summary>
        /// <param name="_channel_name"></param>
        /// <param name="_category_id"></param>
        /// <param name="_pageIndex"></param>
        /// <param name="_keyword"></param>
        /// <param name="_project_status"></param>
        /// <returns></returns>
        private List<li_projects> GetList()
        {
            PageSize = GetPageSize(GetType().Name + "_page_size");
            var query =
                context.li_projects.Where(
                    p =>
                        p.status < (int) Agp2pEnums.ProjectStatusEnum.Financing &&
                        p.status != (int) Agp2pEnums.ProjectStatusEnum.FinancingFail &&
                        (p.title.Contains(Keywords) || p.no.Contains(Keywords)));
            if (CategoryId > 0)
                query = query.Where(q => q.category_id == CategoryId);
            
            this.TotalCount = query.Count();
            return query.OrderByDescending(q => q.sort_id).ThenByDescending(q => q.add_time).ThenByDescending(q => q.id)
                .Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project-publish-announce.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                projectChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("project-publish-announce.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                projectChannelId.ToString(), this.CategoryId.ToString(), txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        protected string QueryLoaner(int projectId)
        {
            return context.GetLonerName(projectId);
        }

        protected void ddlCategoryId_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("project-publish-announce.aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                projectChannelId.ToString(), ddlCategoryId.SelectedValue, txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        protected void btnGenerate_OnClick(object sender, EventArgs e)
        {
            var publishTime = HttpContext.Current.Request["__EVENTARGUMENT"];

            if (string.IsNullOrWhiteSpace(publishTime))
            {
                JscriptMsg("发标时间不能不填", "");
                return;
            }

            var projIds =
                rptList1.Items.Cast<Control>()
                    .Where(c => ((CheckBox) c.FindControl("chkId")).Checked)
                    .Select(c => Convert.ToInt32(((HiddenField) c.FindControl("hidId")).Value))
                    .ToArray();

            if (!projIds.Any())
            {
                JscriptMsg("请先选择项目", "");
                return;
            }

            var projects = context.li_projects.Where(p => projIds.Contains(p.id)).ToList();

            //构造兑付项目表格
            var tableTemplate = "<table class='table table-bordered'><tbody><tr><th>序号</th><th>项目类别</th><th>项目名称</th><th>项目金额</th><th>项目期限</th><th>年化利率</th><th>还款方式</th><th>发标时间</th></tr>{trs}</tbody></table>";
            var trTemplate = "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td></tr>";

            var trAll = Enumerable.Range(0, projects.Count).Select(i =>
            {
                var p = projects[i];
                return string.Format(trTemplate, i + 1, p.dt_article_category.title, p.title, p.financing_amount, p.repayment_term_span_count + p.GetProjectTermSpanEnumDesc(),
                    (p.profit_rate_year / 100).ToString("p2"), p.GetProjectRepaymentTypeDesc(), publishTime);
            });

            var tableContent = tableTemplate.Replace("{trs}", string.Join("", trAll));

            var channelId = context.dt_channel.Single(ch => ch.name == "content_project").id;
            var categoryId = context.dt_article_category.Single(c => c.call_index == "content_project_fabiao").id;

            var announcement = new dt_article
            {
                add_time = DateTime.Now,
                content = tableContent,
                title = publishTime + "发标公告",
                channel_id = channelId,
                category_id = categoryId,
                sort_id = 99,
                is_sys = 1,
                is_top = 0,
                is_red = 0,
                is_hot = 0,
                is_msg = 0,
                is_slide = 0,
                user_name = GetAdminInfo().user_name,
                status = 0
            };

            var articleAttr = new dt_article_attribute_value
            {
                dt_article = announcement,
                author = "安广融合",
                source = "安广融合理财平台"
            };

            context.dt_article_attribute_value.InsertOnSubmit(articleAttr);
            context.dt_article.InsertOnSubmit(announcement);
            context.SubmitChanges();

            JscriptMsg("发布成功", "");
        }
    }
}