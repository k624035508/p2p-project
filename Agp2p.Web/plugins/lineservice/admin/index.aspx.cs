using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.LineService.admin
{
    public partial class index : Lip2p.Web.UI.ManagePage
    {
        protected int group_id = 0;
        protected string keywords = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.group_id = DTRequest.GetQueryInt("group_id");
            this.keywords = DTRequest.GetQueryString("keywords");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("plugin_lineservice_list", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //客服分组
                RptBind("id>0" + CombSqlTxt(this.group_id, this.keywords), "sort_id asc,add_time desc");
            }
        }

        #region 绑定分组=================================
        private void TreeBind()
        {
            BLL.online_service_group bll = new BLL.online_service_group();
            DataTable dt = bll.GetList(0, "is_lock=0", "sort_id asc,id desc").Tables[0];

            this.ddlGroupId.Items.Clear();
            this.ddlGroupId.Items.Add(new ListItem("所有分组", ""));
            foreach (DataRow dr in dt.Rows)
            {
                this.ddlGroupId.Items.Add(new ListItem(dr["title"].ToString(), dr["id"].ToString()));
            }
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind(string _strWhere, string _orderby)
        {
            if (this.group_id > 0)
            {
                ddlGroupId.SelectedValue = this.group_id.ToString();
            }
            txtKeywords.Text = this.keywords;
            BLL.online_service bll = new BLL.online_service();
            this.rptList.DataSource = bll.GetList(0, _strWhere, _orderby);
            this.rptList.DataBind();
        }
        #endregion

        #region 组合SQL查询语句==========================
        protected string CombSqlTxt(int _group_id, string _keywords)
        {
            StringBuilder strTemp = new StringBuilder();
            if (_group_id > 0)
            {
                strTemp.Append(" and group_id=" + _group_id);
            }
            _keywords = _keywords.Replace("'", "");
            if (!string.IsNullOrEmpty(_keywords))
            {
                strTemp.Append(" and title like '%" + _keywords + "%'");
            }
            return strTemp.ToString();
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("index.aspx", "group_id={0}&keywords={1}", this.group_id.ToString(), txtKeywords.Text));
        }

        //筛选分组
        protected void ddlGroupId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("index.aspx", "group_id={0}&keywords={1}", ddlGroupId.SelectedValue, this.keywords));
        }

        //设置操作
        protected void rptList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            /*int id = Convert.ToInt32(((HiddenField)e.Item.FindControl("hidId")).Value);
            BLL.online_service bll = new BLL.online_service();
            Model.online_service model = bll.GetModel(id);
            switch (e.CommandName.ToLower())
            {
                case "ibtnlock":
                    if (model.is_lock == 1)
                        bll.UpdateField(id, "is_lock=0");
                    else
                        bll.UpdateField(id, "is_lock=1");
                    break;
            }
            this.RptBind("id>0" + CombSqlTxt(this.keywords), "sort_id asc,id desc");*/
        }

        //保存排序
        protected void btnSave_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_lineservice_list", DTEnums.ActionEnum.Edit.ToString()); //检查权限
            BLL.online_service bll = new BLL.online_service();
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                int sortId;
                if (!int.TryParse(((TextBox)rptList.Items[i].FindControl("txtSortId")).Text.Trim(), out sortId))
                {
                    sortId = 99;
                }
                bll.UpdateField(id, "sort_id=" + sortId.ToString());
            }
            AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改在线客服排序"); //记录日志
            JscriptMsg("保存排序成功！", Utils.CombUrlTxt("index.aspx", "group_id={0}&keywords={1}", this.group_id.ToString(), this.keywords), "Success");
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_lineservice_list", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            BLL.online_service bll = new BLL.online_service();
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    if (bll.Delete(id))
                    {
                        sucCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
            }
            AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除在线客服成功" + sucCount + "条，失败" + errorCount + "条"); //记录日志
            JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！",
                Utils.CombUrlTxt("index.aspx", "group_id={0}&keywords={1}", this.group_id.ToString(), this.keywords), "Success");
        }

    }
}