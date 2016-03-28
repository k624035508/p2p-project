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
        protected int Tpye;
        protected string Keywords = string.Empty;

        private readonly Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            Tpye = DTRequest.GetQueryInt("type");
            Keywords = DTRequest.GetQueryString("keywords");
            PageIndex = DTRequest.GetQueryInt("page", 1);

            if (!Page.IsPostBack)
            {
                ChkAdminLevel("pay_api_log", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind();
                RptBind();
            }
        }

        protected void TreeBind()
        {
            ddlCategoryId.Items.Clear();
            ddlCategoryId.Items.Add(new ListItem("所有类型", ""));
            ddlCategoryId.Items.AddRange(Utils.GetEnumValues<Agp2pEnums.SumapayApiEnum>()
                            .Select(te => new ListItem(Utils.GetAgp2pEnumDes(te), ((int)te).ToString()))
                            .ToArray());
        }

        #region 数据绑定=================================
        protected void RptBind()
        {
            //绑定列表
            rptList1.DataSource = GetList();
            rptList1.DataBind();
            //绑定页码
            txtPageNum.Text = PageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("pay_api_log.aspx", "keywords={0}&type={1}&page={2}",
                txtKeywords.Text, Tpye.ToString(), "__id__");
            PageContent.InnerHtml = Utils.OutPageList(PageSize, PageIndex, TotalCount, pageUrl, 8);
        }

        private List<ResponseLog> GetList()
        {
            PageSize = GetPageSize(GetType().Name + "_page_size");
            var query =
                context.li_pay_request_log.Where(r => (Tpye == 0 || r.api == Tpye) && r.id.Contains(Keywords))
                    .OrderByDescending(r => r.request_time)
                    .AsQueryable();

            var responseList = query.AsEnumerable().
                Zip(Utils.Infinite(1), (req, index) => new { req, index })
                .SelectMany(r =>
            {
                var respList = new List<ResponseLog>();
                //请求日志
                var requestLog = new RequestLog
                {
                    Index = r.index,
                    RequestId = r.req.id,
                    ProjectId = string.IsNullOrEmpty(r.req.project_id)? 0 : Convert.ToInt32(r.req.project_id),
                    UserId = r.req.user_id,
                    Type = Utils.GetAgp2pEnumDes((Agp2pEnums.SumapayApiEnum)r.req.api),
                    Status = Utils.GetAgp2pEnumDes((Agp2pEnums.SumapayRequestEnum)r.req.status),
                    RequestTime = r.req.request_time.ToString("yyyy-M-d hh:mm:ss")
                };
                if (requestLog.ProjectId > 0)
                {
                    requestLog.ProjectTitle =
                        context.li_projects.SingleOrDefault(p => p.id == requestLog.ProjectId)?.title;
                }
                if (requestLog.UserId > 0)
                {
                    var user = context.dt_users.SingleOrDefault(u => u.id == requestLog.UserId);
                    requestLog.UserName = $"{user?.user_name}({user?.real_name})";
                }
                //请求日志的所有响应
                if (r.req.li_pay_response_log.Any())
                {
                    respList = r.req.li_pay_response_log.OrderByDescending(resp => resp.response_time)
                    .Select(rs => new ResponseLog()
                    {
                        Id = rs.id,
                        ResponseTime = rs.response_time.ToString("yyyy-M-d hh:mm:ss"),
                        ResponseResult = rs.result,
                        ResponseRemark = rs.remarks
                    }).ToList();
                    respList.First().RequestLog = requestLog;
                }
                else
                {
                    respList.Add(new ResponseLog()
                    {
                        RequestLog = requestLog
                    });
                }
                return respList;
            }).AsQueryable();
            
            TotalCount = responseList.Count();
            return responseList.Skip(PageSize * (PageIndex - 1)).Take(PageSize).ToList();
        }       
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("pay_api_log.aspx", "keywords={0}&type={1}",
                txtKeywords.Text, Tpye.ToString()));
        }

        //筛选类别
        protected void ddlCategoryId_SelectedIndexChanged(object sender, EventArgs e)
        {            
            Response.Redirect(Utils.CombUrlTxt("pay_api_log.aspx", "keywords={0}&type={1}",
                txtKeywords.Text, Tpye.ToString()));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("pay_api_log.aspx", "keywords={0}&type={1}",
                txtKeywords.Text, Tpye.ToString()));
        }

        public class RequestLog
        {
            public int Index { get; set; }
            public string RequestId { get; set; }
            public int? ProjectId { get; set; }
            public string ProjectTitle { get; set; }
            public int? UserId { get; set; }
            public string UserName { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
            public string RequestTime { get; set; }
        }

        public class ResponseLog
        {
            public int Id { get; set; }
            public string RequestId { get; set; }
            public string ResponseTime { get; set; }
            public string ResponseResult { get; set; }
            public string ResponseRemark { get; set; }
            public RequestLog RequestLog { get; set; }
        }

        protected void rblStatus_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
        }
    }
}