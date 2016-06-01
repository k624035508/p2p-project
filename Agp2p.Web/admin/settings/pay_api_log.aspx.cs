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

            TotalCount = query.Count();

            var responseList = query.AsEnumerable().Skip(PageSize * (PageIndex - 1)).Take(PageSize).
                Zip(Utils.Infinite(1), (req, index) => new { req, index })
                .SelectMany(r =>
            {
                var respList = new List<ResponseLog>();
                //请求日志
                var requestLog = new RequestLog
                {
                    Index = r.index,
                    RequestId = r.req.id,
                    ProjectId = r.req.project_id ?? 0,
                    UserId = r.req.user_id,
                    Type = Utils.GetAgp2pEnumDes((Agp2pEnums.SumapayApiEnum)r.req.api),
                    Status = Utils.GetAgp2pEnumDes((Agp2pEnums.SumapayRequestEnum)r.req.status),
                    RequestTime = r.req.request_time.ToString("yyyy-M-d HH:mm:ss")
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
                        ResponseTime = rs.response_time.ToString("yyyy-M-d HH:mm:ss"),
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
            return responseList.ToList();
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

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("manager_log", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            BLL.manager_log bll = new BLL.manager_log();
            int sucCount = bll.Delete(7);
            AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除资金托管日志" + sucCount + "条"); //记录日志
            JscriptMsg("删除日志" + sucCount + "条", Utils.CombUrlTxt("pay_api_log.aspx", "keywords={0}", txtKeywords.Text), "Success");
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

        protected void excBtn_OnClick(object sender, EventArgs e)
        {
            string requestId = ((LinkButton)sender).CommandArgument;
            var request = context.li_pay_request_log.SingleOrDefault(r => r.id == requestId);
            if (request != null)
            {
                var project = context.li_projects.SingleOrDefault(p => p.id == request.project_id);
                switch (request.api)
                {
#if DEBUG
                    //个人自动账户/银行还款开通
                    case (int)Agp2pEnums.SumapayApiEnum.AcReO:
                    case (int)Agp2pEnums.SumapayApiEnum.AbReO:
                        if (project != null)
                        {
                            project.autoRepay = true;
                            context.SubmitChanges();
                        }
                        break;
                    //个人自动还款取消
                    case (int)Agp2pEnums.SumapayApiEnum.ClRep:
                        if (project != null)
                        {
                            project.autoRepay = false;
                            context.SubmitChanges();
                        }
                        break;
                    //个人网银/一键充值
                    case (int) Agp2pEnums.SumapayApiEnum.WeRec:
                    case (int) Agp2pEnums.SumapayApiEnum.WhRec:
                    case (int) Agp2pEnums.SumapayApiEnum.WhReM:
                        var trans = context.li_bank_transactions.SingleOrDefault(u => u.no_order == requestId);
                        if (trans?.status == (int)Agp2pEnums.BankTransactionStatusEnum.Acting)
                        {
                            context.ConfirmBankTransaction(trans.id, null);
                        }
                        break;
                    //个人提现
                    case (int)Agp2pEnums.SumapayApiEnum.Wdraw:
                    case (int)Agp2pEnums.SumapayApiEnum.WdraM:
                        var transT = context.li_bank_transactions.SingleOrDefault(u => u.no_order == requestId);
                        if (transT?.status == (int)Agp2pEnums.BankTransactionStatusEnum.Acting)
                        {
                            context.ConfirmBankTransaction(transT.id, null);
                        }
                        break;
                    //普通/集合项目放款
                    case (int)Agp2pEnums.SumapayApiEnum.ALoan:
                    case (int)Agp2pEnums.SumapayApiEnum.CLoan:
                        if (project != null)
                        {
                            if (project.IsHuoqiProject())
                            {
                                TransactionFacade.MakeLoan(context, DateTime.Now, project, project.li_risks.li_loaners.user_id);
                            }
                            else
                            {
                                context.StartRepayment(project.id);
                            }
                        }
                        break;
                    //个人自动还款普通/集合项目
                    case (int)Agp2pEnums.SumapayApiEnum.AcRep:
                    case (int)Agp2pEnums.SumapayApiEnum.AbRep:
                        if (!string.IsNullOrEmpty(request.remarks))
                        {
                            if (project != null && !project.IsHuoqiProject() && !project.IsNewbieProject())
                            {
                                var dic = Utils.UrlParamToData(request.remarks);
                                int repayId = Utils.StrToInt(dic["repayTaskId"], 0);
                                var repayTask = context.li_repayment_tasks.SingleOrDefault(r => r.id == repayId);
                                if (repayTask != null)
                                {
                                    context.GainLoanerRepayment(DateTime.Now, repayId, (int)request.user_id, repayTask.repay_principal + repayTask.repay_interest);
                                }
                            }
                        }
                        break;
                    //普通/集合项目本息到账
                    case (int)Agp2pEnums.SumapayApiEnum.RetPt:
                    case (int)Agp2pEnums.SumapayApiEnum.RetCo:
                        if (project != null && !project.IsHuoqiProject() && !project.IsNewbieProject())
                        {
                            if (!string.IsNullOrEmpty(request.remarks))
                            {
                                var dic = Utils.UrlParamToData(request.remarks);
                                int repayId = Utils.StrToInt(dic["repayTaskId"], 0);
                                context.ExecuteRepaymentTask(repayId);
                            }
                        }
                        break;
#endif
                    default:
                        throw new NotImplementedException("该接口操作暂不能在平台单方面执行。");
                }
            }
        }
    }
}