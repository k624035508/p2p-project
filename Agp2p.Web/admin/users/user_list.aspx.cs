using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Core.ActivityLogic;
using Agp2p.Linq2SQL;
using System.IO;

namespace Agp2p.Web.admin.users
{
    public partial class user_list : UI.ManagePage
    {
        Agp2pDataContext context = new Agp2pDataContext();
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected int group_id;
        protected string keywords = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            ChkAdminLevel("user_list", DTEnums.ActionEnum.View.ToString()); //检查权限
            group_id = DTRequest.GetQueryInt("group_id");
            //keywords = DTRequest.GetQueryString("keywords");

            pageSize = GetPageSize(8); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            if (!Page.IsPostBack)
            {
#if !DEBUG
                //正式环境不能删除用户
                btnDelete.Visible = false;
#endif

                var startTime = DTRequest.GetQueryString("startTime");
                if (!string.IsNullOrEmpty(startTime))
                    txtStartTime.Text = startTime;
                var endTime = DTRequest.GetQueryString("endTime");
                if (!string.IsNullOrEmpty(endTime))
                    txtEndTime.Text = endTime;
                var keywords = DTRequest.GetQueryString("keywords");  //关键字查询
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                TreeBind(); //绑定类别
                RptBind();
            }
        }

        #region 绑定组别=================================
        private void TreeBind()
        {
            ddlGroupId.Items.Clear();
            ddlGroupId.Items.Add(new ListItem("所有会员组", ""));
            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();

            ddlGroupId.Items.AddRange(context.dt_user_groups.Where(g => g.is_lock == 0 && (!canAccessGroups.Any() || canAccessGroups.Contains(g.id)))
                .OrderByDescending(g => g.id)
                .Select(g => new ListItem(g.title, g.id.ToString()))
                .ToArray());
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind()
        {
            var query = QueryUsers();

            totalCount = query.Count();

            rptList.DataSource = query
                    .OrderByDescending(u => u.reg_time)
                    .ThenByDescending(u => u.id)
                    .Skip(pageSize*(page - 1))
                    .Take(pageSize)
                    .ToList();

            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("user_list.aspx", "group_id={0}&keywords={1}&page={2}&startTime={3}&endTime={4}", group_id.ToString(), txtKeywords.Text, "__id__", txtStartTime.Text, txtEndTime.Text);
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IQueryable<dt_users> QueryUsers()
        {
            IQueryable<dt_users> query = context.dt_users;
            if (group_id > 0) // 选择了某一组
            {
                ddlGroupId.SelectedValue = group_id.ToString();
                query = query.Where(u => u.group_id == group_id);
            }
            else
            {
                // 限制当前管理员对会员的查询
                var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();
                query = query.Where(u => !canAccessGroups.Any() || canAccessGroups.Contains(u.group_id));
            }
            //txtKeywords.Text = keywords;
            if (!string.IsNullOrWhiteSpace(txtStartTime.Text))
                query = query.Where(h => Convert.ToDateTime(txtStartTime.Text) <= h.reg_time.Value.Date);
            if (!string.IsNullOrWhiteSpace(txtEndTime.Text))
                query = query.Where(h => h.reg_time.Value.Date <= Convert.ToDateTime(txtEndTime.Text));

            if (!string.IsNullOrWhiteSpace(txtKeywords.Text))
            {
                query = query.Where(u =>
                            u.user_name.Contains(txtKeywords.Text) || u.real_name.Contains(txtKeywords.Text) || u.mobile.Contains(txtKeywords.Text));
            }
            return query;
        }

        #endregion

        #region 返回用户每页数量=========================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie("user_list_page_size"), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    return _pagesize;
                }
            }
            return _default_size;
        }
        #endregion

        #region 返回用户状态=============================
        protected string GetUserStatus(int status)
        {
            var result = string.Empty;
            switch (status)
            {
                case 0:
                    result = "正常";
                    break;
                case 1:
                    result = "待验证";
                    break;
                case 2:
                    result = "待审核";
                    break;
                case 3:
                    result = "已禁用";
                    break;
            }
            return result;
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("user_list.aspx", "group_id={0}&keywords={1}&startTime={2}&endTime={3}", group_id.ToString(), txtKeywords.Text, txtStartTime.Text, txtEndTime.Text));
        }

        //筛选类别
        protected void ddlGroupId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("user_list.aspx", "group_id={0}&keywords={1}&startTime={2}&endTime={3}", ddlGroupId.SelectedValue, txtKeywords.Text, txtStartTime.Text, txtEndTime.Text));
        }

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie("user_list_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("user_list.aspx", "group_id={0}&keywords={1}&startTime={2}&endTime={3}", group_id.ToString(), txtKeywords.Text, txtStartTime.Text, txtEndTime.Text));
        }

        //注册用户送红包
        protected void giveHongBao_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("user_list", DTEnums.ActionEnum.GiveHongBao.ToString()); //检查权限
            var registerId = Utils.StrToInt(((LinkButton)sender).CommandArgument, 0);
            try {
                HongBaoActivity.GiveUser(registerId, 7);
                JscriptMsg("赠送注册新用户红包成功！", "../users/user_list.aspx");
            }
            catch(Exception ex)
            {
                JscriptMsg("赠送红包失败：" + ex.Message, "back", "Error");
            }
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("user_list", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            var sucCount = 0;
            var errorCount = 0;
            var bll = new BLL.users();
            for (var i = 0; i < rptList.Items.Count; i++)
            {
                var id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                var cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    //查询该用户钱包
                    var walletDel = context.li_wallets.FirstOrDefault(w => w.user_id == id);
                    if (walletDel != null)
                    {
                        var wallet_historiesDel = context.li_wallet_histories.Where(wh => wh.user_id == id).ToList();
                        //无资金流水的会员可以删除
                        if (!wallet_historiesDel.Any())
                        {
                            context.li_wallet_histories.DeleteAllOnSubmit(wallet_historiesDel);
                            context.li_wallets.DeleteOnSubmit(walletDel);
                            try
                            {
                                context.SubmitChanges();
                            }
                            catch (Exception ex)
                            {
                                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除用户" + id + "失败：" + ex.Message);
                                errorCount += 1;
                                continue;
                            }
                        }
                    }

                    //var preDel = context.dt_users.FirstOrDefault(u => u.id == id);
                    //if (preDel != null)
                    //{
                    //context.dt_users.DeleteOnSubmit(preDel);
                    if(bll.Delete(id))
                    {
                        sucCount += 1;
                    }
                    else
                    {
                        errorCount += 1;
                    }
                }
            }
            //try
            //{
                //context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除用户" + sucCount + "条，失败" + errorCount + "条"); //记录日志
                JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！",
                    Utils.CombUrlTxt("user_list.aspx", "group_id={0}&keywords={1}", group_id.ToString(), txtKeywords.Text), "Success");
            //}
            //catch (Exception ex)
            //{
            //    JscriptMsg("删除用户失败！" + ex.Message,
            //        Utils.CombUrlTxt("user_list.aspx", "group_id={0}&keywords={1}", group_id.ToString(), keywords), "Failure");
            //}
        }

        protected decimal QueryInvestmentToday(dt_users user)
        {
            return
                user.li_project_transactions.Where(
                    tr =>
                        tr.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                        tr.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        tr.create_time.Date == DateTime.Now.Date).Sum(t => t.principal);
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("user_list", DTEnums.ActionEnum.DownLoad.ToString()); //检查权限
            var users = QueryUsers();
            var xlsData = users
                .OrderByDescending(u => u.reg_time)
                .ThenByDescending(u => u.id).Skip(pageSize*(page - 1)).Take(pageSize)
                .Select(
                    u => new
                    {
                        u.id,
                        u.real_name,
                        u.reg_time,
                        u.dt_user_groups.title,
                        u.mobile,
                        u.id_card_number,
                        u.email,
                        investmentToday = QueryInvestmentToday(u),
                        u.li_wallets.investing_money,
                        u.li_wallets.idle_money,
                        userStatus = GetUserStatus(u.status.GetValueOrDefault(0))
                    });

            var titles = new[] { "用户", "姓名", "注册时间", "会员组", "手机", "身份证号", "邮箱", "当天投资金额", "再投金额", "余额", "状态"};
            Utils.ExportXls("会员列表", titles, xlsData, Response);
        }
    }
}