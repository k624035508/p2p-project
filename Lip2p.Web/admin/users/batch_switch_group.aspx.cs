using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Security.Policy;
using System.Web.UI.WebControls;
using Lip2p.BLL;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using System.Web;
using System.Web.UI;
using System.Collections;
using System.Data.Common;

namespace Lip2p.Web.admin.users
{
    public partial class batch_switch_group : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        private Lip2pDataContext context = new Lip2pDataContext();
        private int groupId;
        private string act;

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            var group_id = DTRequest.GetQueryString("group_id");
            act = DTRequest.GetQueryString("act");
            if (string.IsNullOrWhiteSpace(group_id) || string.IsNullOrWhiteSpace(act))
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            groupId = Convert.ToInt32(group_id);
            if (!Page.IsPostBack)
            {
                if (!ChkAdminLevel("user_group_" + act, DTEnums.ActionEnum.View.ToString())) //检查权限
                    return;
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind();
            }
        }

        #region 数据绑定=================================
        /// <summary>
        /// 会员数据绑定
        /// </summary>
        private void RptBind()
        {
            var query = QueryUser(groupId);

            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.reg_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("batch_switch_group.aspx", "page={0}&group_id={1}&keywords={2}&act={3}", "__id__", groupId.ToString(), txtKeywords.Text.Trim(), act);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IQueryable<dt_users> QueryUser(int groupId)
        {
            IQueryable<dt_users> query = context.dt_users;
            if (act == "include")
            {
                query = query.Where(u => u.group_id != groupId && u.dt_user_groups.is_default == 1);
            }
            else
            {
                query = query.Where(u => u.group_id == groupId && u.dt_user_groups.is_default != 1);
            }
            return query.Where(u => u.real_name.Contains(txtKeywords.Text) || u.user_name.Contains(txtKeywords.Text) || u.mobile.Contains(txtKeywords.Text));
        }

        #endregion

        #region 返回每页数量=============================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie(GetType().Name + "_page_size"), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    return _pagesize;
                }
            }
            return _default_size;
        }
        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie(GetType().Name + "_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("batch_switch_group.aspx", "group_id={0}&page={1}&keywords={2}&act={3}", groupId.ToString(), page.ToString(), txtKeywords.Text.Trim(), act));
        }

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("batch_switch_group.aspx", "group_id={0}&page={1}&keywords={2}&act={3}", groupId.ToString(), page.ToString(), txtKeywords.Text.Trim(), act));
        }

        //批量切换
        protected void btnDoSwitch_Click(object sender, EventArgs e)
        {
            if (!ChkAdminLevel("user_group_" + act, DTEnums.ActionEnum.Edit.ToString())) //检查权限
                return;

            int toGroupId;
            int sucCount = 0;
            var currGroupId = Convert.ToInt32(DTRequest.GetQueryString("group_id"));
            if (act == "include")
            {
                toGroupId = currGroupId;
            }
            else
            {
                toGroupId = context.dt_user_groups.Single(g => g.is_default == 1).id;
            }
            try
            {
                var userMap =
                    QueryUser(currGroupId)
                        .OrderByDescending(q => q.reg_time)
                        .Skip(pageSize * (page - 1))
                        .Take(pageSize)
                        .ToDictionary(u => u.id);
                for (var i = 0; i < rptList.Items.Count; i++)
                {
                    var cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                    if (!cb.Checked) continue;
                    var id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);

                    var curr = userMap[id];
                    curr.group_id = toGroupId;
                    sucCount += 1;
                }
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "设置成功 " + sucCount + " 条"); //记录日志
                JscriptMsg("设置成功 " + sucCount + " 条",
                    Utils.CombUrlTxt("batch_switch_group.aspx", "group_id={0}&page={1}&keywords={2}&act={3}",
                        currGroupId.ToString(), page.ToString(), txtKeywords.Text, act), "Success");
            }
            catch (Exception)
            {
                JscriptMsg("设置失败！",
                    Utils.CombUrlTxt("batch_switch_group.aspx", "group_id={0}&page={1}&keywords={2}&act={3}",
                        currGroupId.ToString(), page.ToString(), txtKeywords.Text, act), "Failure");
            }
        }

        protected string GetGroupName()
        {
            if (act == "include")
            {
                return context.dt_user_groups.Single(g => g.id == groupId).title;
            }
            else
            {
                return context.dt_user_groups.Single(g => g.is_default == 1).title;
            }
        }
    }
}