using System;
using System.Data.Linq;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.manager
{
    public partial class department_select_member : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        private Agp2pDataContext context = new Agp2pDataContext();
        private int departmentId;
        private string act;

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(GetType().Name + "_page_size"); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            var department_id = DTRequest.GetQueryString("departmentid");
            act = DTRequest.GetQueryString("act");
            if (string.IsNullOrWhiteSpace(department_id) || string.IsNullOrWhiteSpace(act))
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            departmentId = Convert.ToInt32(department_id);
            if (!Page.IsPostBack)
            {
                //if (!ChkAdminLevel("user_group_" + act, DTEnums.ActionEnum.View.ToString())) //检查权限
                //    return;
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            var query = Query_Department_User(departmentId);

            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.add_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("department_select_member.aspx", "page={0}&departmentid={1}&keywords={2}&act={3}", "__id__", departmentId.ToString(), txtKeywords.Text.Trim(), act);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IQueryable<dt_manager> Query_Department_User(int departmentId)
        {
            IQueryable<dt_manager> query = context.dt_manager;
            if (act == "include")
            {
                query = query.Where(u => Convert.ToInt32(u.department_id) != departmentId && u.is_default==0);
            }
            else
            {
                query = query.Where(u => Convert.ToInt32(u.department_id) == departmentId && u.is_default==1);
            }
            return query.Where(u => u.real_name.Contains(txtKeywords.Text) || u.user_name.Contains(txtKeywords.Text) || u.telephone.Contains(txtKeywords.Text));
        }

        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            SetPageSize(GetType().Name + "_page_size", txtPageNum.Text.Trim());
            Response.Redirect(Utils.CombUrlTxt("department_select_member.aspx", "departmentid={0}&page={1}&keywords={2}&act={3}", departmentId.ToString(), page.ToString(), txtKeywords.Text.Trim(), act));
        }

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("department_select_member.aspx", "departmentid={0}&page={1}&keywords={2}&act={3}", departmentId.ToString(), page.ToString(), txtKeywords.Text.Trim(), act));
        }

        //批量切换
        protected void btnDoSwitch_Click(object sender, EventArgs e)
        {
            //if (!ChkAdminLevel("user_group_" + act, DTEnums.ActionEnum.Edit.ToString())) //检查权限
               // return;

            int toDepartmentId=0;
            int is_default = 0;
            var currDepartmentId = Convert.ToInt32(DTRequest.GetQueryString("departmentid"));
            if (act == "include")
            {
                toDepartmentId = currDepartmentId;
                is_default = 1;
            }
            try
            {
                var sucCount = 0;
                var userMap =
                    Query_Department_User(currDepartmentId)
                        .OrderByDescending(q => q.add_time)
                        .Skip(pageSize * (page - 1))
                        .Take(pageSize)
                        .ToDictionary(u => u.id);
                for (var i = 0; i < rptList.Items.Count; i++)
                {
                    var cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                    if (!cb.Checked) continue;
                    var id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);

                    var curr = userMap[id];
                    curr.department_id = toDepartmentId;
                    curr.is_default = (byte) is_default;
                    sucCount += 1;
                }
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "设置成功 " + sucCount + " 条"); //记录日志
                JscriptMsg("设置成功 " + sucCount + " 条",
                    Utils.CombUrlTxt("department_select_member.aspx", "departmentid={0}&page={1}&keywords={2}&act={3}",
                        currDepartmentId.ToString(), page.ToString(), txtKeywords.Text, act), "Success");
            }
            catch (Exception)
            {
                JscriptMsg("设置失败！",
                    Utils.CombUrlTxt("department_select_member.aspx", "departmentid={0}&page={1}&keywords={2}&act={3}",
                        currDepartmentId.ToString(), page.ToString(), txtKeywords.Text, act), "Failure");
            }
        }

        protected string GetGroupName()
        {
            var dname = context.li_departments.Single(g => g.id == departmentId).department_name;
            if (act == "include")
            {
                return "批量添加用户到" + dname;
            }
            else
            {
                return dname+"批量移除用户"; //context.li_departments.Single(g => g.is_default == 1).department_name;
            }
        }
    }
}