using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.users
{
    public partial class user_group_access_key_settings : UI.ManagePage
    {
        Lip2pDataContext context = new Lip2pDataContext();
        private string manager_id = string.Empty;
        private string department_id = string.Empty;
        private List<int> groupIdList = new List<int>();
        private List<int> departmentIdList = new List<int>();
        private List<int> departmentIdList2 = new List<int>();
        private List<int> departmentIdList3 = new List<int>();
            
        protected void Page_Load(object sender, EventArgs e)
        {
            manager_id = DTRequest.GetQueryString("manager_id");
            department_id = DTRequest.GetQueryString("department_id");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("user_group_access_key", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind();
                LoadManagerDDL(department_id);
                ShowInfo(department_id, manager_id);
            }
        }

        private void LoadManagerDDL(string department_id)
        {
            department_id = department_id == "" ? ddlDepartments.SelectedValue : department_id;
            List<dt_manager> managers = null;
            ListItem[] managerListItems;

            BLL.department bll = new BLL.department();

            if (string.IsNullOrWhiteSpace(department_id)) // 不限：查自己的组权限 + 下属的组权限，都无选权限的话能看全部
            {
                var manager = context.dt_manager.Single(m => m.id == GetAdminInfo().id);
                if (manager.department_id > 0)
                {
                    managers = context.dt_manager.Where(w => departmentIdList.Contains(Convert.ToInt32(w.department_id))).ToList();
                }
                else
                {
                    managers = context.dt_manager.ToList();
                }
            }
            else // 选了下属，看下属的组权限，无选的话不能看全部
            {
                var department = context.li_departments.FirstOrDefault(w => w.id == Convert.ToInt32(department_id));
                if (department.class_layer == 1)
                {
                    DataTable dt = bll.GetListByOkParentId(department.id, 0);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string Id = dr["id"].ToString();
                        var g = context.li_user_group_departments.Where(w => w.department_id == Convert.ToInt32(Id))
                       .Select(s => s.department_id)
                       .ToList();
                        if (departmentIdList2.Count == 0)
                        {
                            departmentIdList2 = g;
                        }
                        else
                        {
                            departmentIdList2 = departmentIdList2.Concat(g).ToList();
                        }
                    }
                    managers = context.dt_manager.Where(w => departmentIdList2.Contains(Convert.ToInt32(w.department_id))).ToList();
                }
                else   // 选了下属，看下属的组权限，无选的话不能看全部
                {
                    managers = context.dt_manager.Where(w => w.department_id == Convert.ToInt32(department_id)).ToList();
                }
            }

            managerListItems = managers.Select(m =>
                          new ListItem(
                              string.IsNullOrWhiteSpace(m.real_name)
                                  ? m.user_name
                                  : string.Format("{0}({1})", m.user_name, m.real_name), m.id.ToString())).ToArray();

            ddlManager.Items.Clear();
            if (managerListItems.Length == 0)
            {
                ddlManager.Items.Add(new ListItem("暂无管理员", ""));
            }
            ddlManager.Items.AddRange(managerListItems);
            if (!string.IsNullOrWhiteSpace(manager_id))
            {
                ddlManager.SelectedValue = manager_id;
            }
        }

        private void TreeBind()
        {
            var manager = context.dt_manager.FirstOrDefault(w => w.id == GetAdminInfo().id);
            var department = context.li_departments.FirstOrDefault(w => w.id == manager.department_id);
            BLL.department bll = new BLL.department();
            var dt=new DataTable();
            if (department == null)
            {
                dt = bll.GetList(0);
            }
            else if (department.parent_id == 0)
            {
                dt = bll.GetListByOkParentId(department.id,0);
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
                var g =
                    context.li_user_group_departments.Where(w => w.department_id == Convert.ToInt32(Id))
                        .Select(s => s.user_group_id)
                        .ToList();
                if (groupIdList.Count == 0)
                {
                    groupIdList = g;
                }
                else
                {
                    groupIdList = groupIdList.Concat(g).ToList();
                }

                departmentIdList.Add(int.Parse(Id)); 
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(string department_id, string manager_id)
        {
            department_id = department_id == "" ? ddlDepartments.SelectedValue : department_id;
            //if (department_id == "")
            //{
            //    ddlManager.Enabled = false;
            //}
            ddlDepartments.SelectedValue = department_id;

            IQueryable<dt_user_groups> userGroups = context.dt_user_groups;
            BLL.department bll = new BLL.department();

            if (string.IsNullOrWhiteSpace(ddlDepartments.SelectedValue)) // 不限：查自己的组权限 + 下属的组权限，都无选权限的话能看全部
            {
                var manager = context.dt_manager.Single(m => m.id == GetAdminInfo().id);
                if (manager.department_id > 0)
                {
                    userGroups = userGroups.Where(w => groupIdList.Contains(w.id));
                }
            }
            else // 选了下属，看下属的组权限，无选的话不能看全部
            {
                var department = context.li_departments.FirstOrDefault(w => w.id == Convert.ToInt32(department_id));
                if (department.class_layer == 1)
                {
                    DataTable dt = bll.GetListByOkParentId(department.id, 0);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string Id = dr["id"].ToString();
                        var g = context.li_user_group_departments.Where(w => w.department_id == Convert.ToInt32(Id))
                            .Select(s => s.user_group_id)
                            .ToList();
                        if (departmentIdList3.Count == 0)
                        {
                            departmentIdList3 = g;
                        }
                        else
                        {
                            departmentIdList3 = departmentIdList3.Concat(g).ToList();
                        }
                    }
                    userGroups = userGroups.Where(w => departmentIdList3.Contains(w.id));
                    // 不限：查自己的组权限 + 下属的组权限，都无选权限的话能看全部
                }
                else // 选了下属，看下属的组权限，无选的话不能看全部
                {
                    var userGroupLists =
                        context.li_user_group_departments.Where(k => k.department_id == Convert.ToInt32(department_id))
                            .Select(s => s.user_group_id)
                            .ToList();
                    userGroups = userGroups.Where(w => userGroupLists.Contains(w.id));
                }
            }


            //var isClassLayer = context.li_departments.FirstOrDefault(f => f.id == Convert.ToInt32(department_id));
            //if (isClassLayer.class_layer == 1)
            //{
            //    userGroups = context.dt_user_groups.Where(w => groupIdList.Contains(w.id)).ToList();
            //}
            //else
            //{
            //    var userGroupLists = context.li_user_group_departments.Where(k => k.department_id == Convert.ToInt32(department_id)).Select(s => s.user_group_id).ToList();
            //    userGroups = context.dt_user_groups.Where(w => userGroupLists.Contains(w.id)).ToList();
            //}

            var listItems = userGroups.Select(g => new ListItem(g.title, g.id.ToString())).ToArray();
            Dictionary<string, li_user_group_access_keys> selectedGroups = null;
            if (!string.IsNullOrWhiteSpace(ddlManager.SelectedValue))
            {
                selectedGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == Convert.ToInt32(ddlManager.SelectedValue)).ToDictionary(g => g.user_group.ToString());
                if (selectedGroups.Count > 0)
                {
                    listItems.Where(l => selectedGroups.Keys.Contains(l.Value)).ForEach(l => { l.Selected = true; });
                }
                else
                {
                    listItems.Where(l => !selectedGroups.Keys.Contains(l.Value)).ForEach(l => { l.Selected = true; });
                }
            }
            else
            {
                listItems.ForEach(l => l.Selected = true);
            }

           
            cblUserGroup.Items.Clear();
            if (listItems.Length == 0)
            {
                cblUserGroup.Items.Add(new ListItem("暂无用户分组", ""));
            }
            cblUserGroup.Items.AddRange(listItems);
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit()
        {
            // 先删后加
            if (!string.IsNullOrWhiteSpace(ddlManager.SelectedValue) )
            {
                if (!string.IsNullOrWhiteSpace(cblUserGroup.SelectedValue))
                {
                    var ownerManager = Convert.ToInt32(ddlManager.SelectedValue);
                    var selectedGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == ownerManager).ToList();
                    context.li_user_group_access_keys.DeleteAllOnSubmit(selectedGroups);

                    var newKeys = cblUserGroup.Items.Cast<ListItem>()
                        .Where(l => l.Selected)
                        .Select(l => new li_user_group_access_keys
                        {
                            user_group = Convert.ToInt32(l.Value),
                            owner_manager = ownerManager
                        }).ToList();

                    context.li_user_group_access_keys.InsertAllOnSubmit(newKeys);

                    try
                    {
                        context.SubmitChanges();
                        AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改管理员的用户组访问权限: " + ddlManager.SelectedItem.Text); //记录日志
                        return true;
                    }
                    catch (Exception ex)
                    {
                        JscriptMsg("保存过程中发生错误！" + ex.Message, "", "Error");
                        return false;
                    }
                }
                else
                {
                    JscriptMsg("保存用户组访问权限设置可访问会员组不能为空！", "back", "Error");
                    return false;
                }
            }
            else
            {
                JscriptMsg("保存用户组访问权限设置管理员不能为空！", "back", "Error");
                return false;
            }
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("user_group_access_key", DTEnums.ActionEnum.Edit.ToString()); //检查权限
            if (!DoEdit())
            {
                return;
            }
            JscriptMsg("修改管理员的用户组访问权限成功！", Utils.CombUrlTxt("user_group_access_key_settings.aspx", "manager_id={0}&department_id={1}", ddlManager.SelectedValue, ddlDepartments.SelectedValue), "Success");
        }

        protected void ddlManager_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("user_group_access_key_settings.aspx", "manager_id={0}&department_id={1}", ddlManager.SelectedValue, ddlDepartments.SelectedValue));
        }

        protected void ddlDepartments_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("user_group_access_key_settings.aspx", "manager_id={0}&department_id={1}", ddlDepartments.SelectedValue==""?"":ddlManager.SelectedValue, ddlDepartments.SelectedValue));
        }
    }
}