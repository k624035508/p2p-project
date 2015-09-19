using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Data.Linq;

namespace Agp2p.Web.admin.users
{
    /// <summary>
    /// 会员组别列表
    /// </summary>
    public partial class group_list : UI.ManagePage
    {
        //定义搜索关键词
        public string keywords = string.Empty;
        private string department_id = string.Empty;
        private List<int>  idstrList=new List<int>();
        private List<int> departmentIdList = new List<int>();

        //页面加载
        protected void Page_Load(object sender, EventArgs e)
        {
            department_id = DTRequest.GetQueryString("department_id");
            //取到搜索关键词
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("user_group", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");  //关键字查询
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;

                DdlDepartmentsBind();
                RptBind();
            }
        }

        private void DdlSubordinatesBind()
        {
            Agp2pDataContext context = new Agp2pDataContext();
            var subordinates = context.dt_manager.Single(m => m.id == GetAdminInfo().id)
                .dt_manager2.Select(s =>
                    new ListItem(
                        string.IsNullOrWhiteSpace(s.real_name)
                            ? s.user_name
                            : string.Format("{0}({1})", s.user_name, s.real_name), s.id.ToString())).ToArray();
            ddlSubordinates.Items.Clear();
            ddlSubordinates.Items.Add(new ListItem("不限", ""));
            ddlSubordinates.Items.AddRange(subordinates);
            ddlSubordinates.SelectedValue = DTRequest.GetQueryString("subordinate");
        }

        private void DdlDepartmentsBind()
        {
            Agp2pDataContext context = new Agp2pDataContext();
            var manager = context.dt_manager.FirstOrDefault(w => w.id == GetAdminInfo().id);
            var department = context.li_departments.FirstOrDefault(w => w.id == manager.department_id);
            BLL.department bll = new BLL.department();
            var dt = new DataTable();
            if (department == null)
            {
                dt = bll.GetList(0);
            }
            else if (department.parent_id == 0)
            {
                dt = bll.GetListByOkParentId(department.id, 0);
            }
            else
            {
                dt = bll.GetListByOkId(department.id, 0);
            }

            this.ddlDepartments.Items.Clear();
            this.ddlDepartments.Items.Add(new ListItem("不限", ""));
            foreach (DataRow dr in dt.Rows)
            {
                string Id = dr["id"].ToString();
                int ClassLayer = int.Parse(dr["class_layer"].ToString());
                string Title = dr["department_name"].ToString().Trim();

                if (ClassLayer == 1)
                {
                    this.ddlDepartments.Items.Add(new ListItem(Title, Id));
                }
                else
                {
                    Title = "├ " + Title;
                    Title = Utils.StringOfChar(ClassLayer - 1, "　") + Title;
                    this.ddlDepartments.Items.Add(new ListItem(Title, Id));
                }
                var g =context.li_user_group_departments.Where(w => w.department_id == Convert.ToInt32(Id))
                       .Select(s => s.user_group_id)
                       .ToList();
                if (idstrList.Count == 0)
                {
                    idstrList = g;
                }
                else
                {
                    idstrList = idstrList.Concat(g).ToList();
                }
            }
            ddlDepartments.SelectedValue = department_id;
        }

        #region 数据绑定=================================

        protected class GroupDetails
        {
            public int? id { get; set; }
            public int? is_default { get; set; }
            public string GroupName { get; set; }
            public string Servers { get; set; }
            public int UserCount { get; set; }
            public string DepartmentName { get; set; }
            public decimal IdleMoney { get; set; }
            public decimal LockedMoney { get; set; }
            public decimal InvestingMoney { get; set; }
            public decimal UnpaidPrincipal { get; set; }
            public decimal TotalInvestment { get; set; }
            public decimal ProfitingMoney { get; set; }
            public decimal PaidMoney { get; set; }
            public decimal TotalProfit { get; set; }
            public decimal TotalCharge { get; set; }
            public decimal TotalWithdraw { get; set; }
        }

        private void RptBind()
        {
            Agp2pDataContext context = new Agp2pDataContext();
            BLL.department bll = new BLL.department();
            var options = new DataLoadOptions();
            options.LoadWith<dt_user_groups>(g => g.dt_users);
            options.LoadWith<dt_users>(u => u.li_wallets);
            context.LoadOptions = options;

            IQueryable<dt_user_groups> query = context.dt_user_groups;

            //if (string.IsNullOrWhiteSpace(ddlSubordinates.SelectedValue)) // 不限：查自己的组权限 + 下属的组权限，都无选权限的话能看全部
            //{
            //    var subordinatesId = context.dt_manager.Single(m => m.id == GetAdminInfo().id).dt_manager2.Select(s => s.id).ToArray();

            //    var canAccessGroups =
            //        context.li_user_group_access_keys.Where(
            //            k => k.owner_manager == GetAdminInfo().id || subordinatesId.Contains(k.owner_manager))
            //            .Select(k => k.user_group).Distinct()
            //            .ToArray();
            //    query = query.Where(w => !canAccessGroups.Any() || canAccessGroups.Contains(w.id));
            //}
            //else // 选了下属，看下属的组权限，无选的话不能看全部
            //{
            //    var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == Convert.ToInt32(ddlSubordinates.SelectedValue)).Select(k => k.user_group).ToArray();
            //    query = query.Where(w => canAccessGroups.Contains(w.id));
            //}

            if (string.IsNullOrWhiteSpace(ddlDepartments.SelectedValue)) // 不限：查自己的组权限 + 下属的组权限，都无选权限的话能看全部
            {
                var manager = context.dt_manager.Single(m => m.id == GetAdminInfo().id);
                if (manager.department_id > 0)
                {
                    query = query.Where(w => idstrList.Contains(w.id));
                }
            }
            else // 选了下属，看下属的组权限，无选的话不能看全部
            {
                var department = context.li_departments.FirstOrDefault(w => w.id == Convert.ToInt32(ddlDepartments.SelectedValue));
                if (department.class_layer == 1)
                {
                    DataTable dt = bll.GetListByOkParentId(department.id, 0);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string Id = dr["id"].ToString();
                        var g =context.li_user_group_departments.Where(w => w.department_id == Convert.ToInt32(Id))
                       .Select(s => s.user_group_id)
                       .ToList();
                        if (departmentIdList.Count == 0)
                        {
                            departmentIdList = g;
                        }
                        else
                        {
                            departmentIdList = departmentIdList.Concat(g).ToList();
                        }
                    }
                    query = query.Where(w => departmentIdList.Contains(w.id));  // 不限：查自己的组权限 + 下属的组权限，都无选权限的话能看全部
                }
                else   // 选了下属，看下属的组权限，无选的话不能看全部
                {
                    var canAccessGroups = context.li_user_group_departments.Where(w => w.department_id == Convert.ToInt32(ddlDepartments.SelectedValue)).Select(s => s.user_group_id).ToArray();
                    query = query.Where(w => canAccessGroups.Contains(w.id));
                }
            }
            query = query.Where(w => w.title.Contains(txtKeywords.Text));

            var groups = query.OrderBy(g => g.grade).AsEnumerable().Select(g => new GroupDetails
            {
                id = g.id,
                is_default = g.is_default,
                GroupName = g.title,
                Servers = QueryServers(g),
                UserCount = QueryGroupUserCount(g),
                DepartmentName = QueryDepartmentName(g),
                IdleMoney = QueryGroupUserWalletSum(g, w => w.idle_money),
                LockedMoney = QueryGroupUserWalletSum(g, w => w.locked_money),
                InvestingMoney = QueryGroupUserWalletSum(g, w => w.investing_money),
                UnpaidPrincipal = QueryGroupUserWalletSum(g, w => w.total_investment - w.investing_money),
                TotalInvestment = QueryGroupUserWalletSum(g, w => w.total_investment),
                ProfitingMoney = QueryGroupUserWalletSum(g, w => w.profiting_money),
                PaidMoney = QueryGroupUserWalletSum(g, w => w.total_profit),
                TotalProfit = QueryGroupUserWalletSum(g, w => w.total_profit + w.profiting_money),
                TotalCharge = QueryGroupUserWalletSum(g, w => w.total_charge),
                TotalWithdraw = QueryGroupUserWalletSum(g, w => w.total_withdraw)
            }).ToLazyList();

            rptList.DataSource = groups.Concat(Enumerable.Range(0, 1).Select(i => new GroupDetails
            {
                GroupName = "合计",
                is_default = 1, // hide operation
                UserCount = groups.Sum(g => g.UserCount),
                //DepartmentName = groups.Select(g => g.DepartmentName).ToString(),
                IdleMoney = groups.Sum(w => w.IdleMoney),
                LockedMoney = groups.Sum(w => w.LockedMoney),
                InvestingMoney = groups.Sum(w => w.InvestingMoney),
                UnpaidPrincipal = groups.Sum(w => w.TotalInvestment - w.InvestingMoney),
                TotalInvestment = groups.Sum(w => w.TotalInvestment),
                ProfitingMoney = groups.Sum(w => w.ProfitingMoney),
                PaidMoney = groups.Sum(w => w.TotalProfit),
                TotalProfit = groups.Sum(w => w.TotalProfit + w.ProfitingMoney),
                TotalCharge = groups.Sum(w => w.TotalCharge),
                TotalWithdraw = groups.Sum(w => w.TotalWithdraw)
            }));

            rptList.DataBind();
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("group_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text.Trim(), ddlDepartments.SelectedValue));
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("user_group", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            BLL.user_groups bll = new BLL.user_groups();
            //批量删除
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    if (bll.Delete(id))
                    {
                        sucCount += 1;
                    }
                    else
                    {
                        errorCount += 1;
                    }
                }
            }
            AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除用户组成功" + sucCount + "条，失败" + errorCount + "条"); //记录日志
            JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！",
                Utils.CombUrlTxt("group_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text.Trim(), ddlDepartments.SelectedValue), "Success");
        }

        protected void txtKeywords_TextChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("group_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text.Trim(), ddlDepartments.SelectedValue));
        }

        protected string QueryServers(dt_user_groups group)
        {
            var servers = group.li_user_group_servers.Select(g => g.dt_users).Select(
                u =>
                    string.IsNullOrWhiteSpace(u.real_name)
                        ? u.user_name
                        : string.Format("{0}({1})", u.user_name, u.real_name)).ToList();
            return string.Join("<br>", servers);
        }

        protected int QueryGroupUserCount(dt_user_groups group)
        {
            return group.dt_users.Count();
        }

        protected string QueryDepartmentName(dt_user_groups group)
        {
            Agp2pDataContext context = new Agp2pDataContext();
            var c = context.li_user_group_departments.FirstOrDefault(w => w.user_group_id == group.id);
            if (c != null)
            {
                return context.li_departments.FirstOrDefault(w => w.id == c.department_id).department_name;
            }
            else
            {
                return null;
            }         
        }

        protected decimal QueryGroupUserWalletSum(dt_user_groups group, Func<li_wallets, decimal> columnSelector)
        {
            return group.dt_users.Select(u => u.li_wallets).Sum(columnSelector);
        }

        protected void ddlSubordinates_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("group_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text.Trim(), ddlDepartments.SelectedValue));
        }

        protected void ddlDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("group_list.aspx", "keywords={0}&department_id={1}", txtKeywords.Text.Trim(), ddlDepartments.SelectedValue));
        }
    }
}