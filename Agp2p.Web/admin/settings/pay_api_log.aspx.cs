using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Core;

namespace Agp2p.Web.admin.repayment
{
    public partial class pay_api_log : Web.UI.ManagePage
    {
        protected int TotalCount;
        protected int PageIndex;
        protected int PageSize;
        protected int ProjectStatus;
        protected string Keywords = string.Empty;

        private readonly Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            this.ProjectStatus = DTRequest.GetQueryInt("status");


            if (!Page.IsPostBack)
            {
                ChkAdminLevel("pay_api_log", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //绑定类别
                RptBind();
            }
        }

        protected void TreeBind()
        {
            this.ddlCategoryId.Items.Clear();
            this.ddlCategoryId.Items.Add(new ListItem("所有类型", ""));
            this.ddlCategoryId.Items.AddRange(Utils.GetEnumValues<Agp2pEnums.SumapayApiEnum>()
                            .Select(te => new ListItem(Utils.GetAgp2pEnumDes(te), ((int)te).ToString()))
                            .ToArray());
        }

        #region 数据绑定=================================
        protected void RptBind()
        {
            this.PageIndex = DTRequest.GetQueryInt("page", 1);

            //绑定列表
            this.rptList1.DataSource = GetList();
            this.rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = this.PageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("pay_api_log.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}&page={4}",
                txtKeywords.Text, this.ProjectStatus.ToString(), "__id__");
            PageContent.InnerHtml = Utils.OutPageList(this.PageSize, this.PageIndex, this.TotalCount, pageUrl, 8);
        }

        private List<RequestLog> GetList()
        {
            PageSize = GetPageSize(GetType().Name + "_page_size");
            var query =
                context.li_pay_request_log.Where(r => r.id.Contains(Keywords))
                    .OrderByDescending(r => r.request_time)
                    .AsQueryable();


            var requestList = query.AsEnumerable().Select(r =>
            {
                var requestLog = new RequestLog
                {
                    RequestId = r.id,
                    ProjectId = string.IsNullOrEmpty(r.project_id)? 0 : Convert.ToInt32(r.project_id),
                    UserId = r.user_id,
                    Type = r.api,
                    Status = r.status,
                    RequestTime = r.request_time.ToString("yyyy-M-d hh:mm:ss")
                };
                requestLog.ResponseLogs.AddRange(r.li_pay_response_log.Select(rs => new ResponseLog()
                {
                    Id = rs.id,
                    RequestId = rs.request_id,
                    ResponseTime = rs.response_time.ToString("yyyy-M-d hh:mm:ss"),
                    ResponseResult = rs.result,
                    ResponseRemark = rs.remarks
                }));
                if (requestLog.ProjectId > 0)
                {
                    requestLog.ProjectTitle =
                        context.li_projects.SingleOrDefault(p => p.id == requestLog.ProjectId).title;
                }
                if (requestLog.UserId > 0)
                {
                    requestLog.UserName =
                        context.dt_users.SingleOrDefault(u => u.id == requestLog.UserId).user_name;
                }

                return requestLog;
            }).AsQueryable();
            
            this.TotalCount = requestList.Count();
            return requestList.Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("pay_api_log.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {            
            Response.Redirect(Utils.CombUrlTxt("pay_api_log.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                ddlCategoryId.SelectedValue, txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("pay_api_log.aspx", "channel_id={0}&category_id={1}&keywords={2}&status={3}",
                txtKeywords.Text, this.ProjectStatus.ToString()));
        }

        class RequestLog
        {
            public string RequestId { get; set; }
            public int? ProjectId { get; set; }
            public string ProjectTitle { get; set; }
            public int? UserId { get; set; }
            public string UserName { get; set; }
            public int Type { get; set; }
            public int Status { get; set; }
            public string RequestTime { get; set; }
            public List<ResponseLog> ResponseLogs { get; set; }
        }

        public class ResponseLog
        {
            public int Id { get; set; }
            public string RequestId { get; set; }
            public string ResponseTime { get; set; }
            public string ResponseResult { get; set; }
            public string ResponseRemark { get; set; }
        }

        protected void rblStatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
        }
    }
}