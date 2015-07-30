using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.Job.admin
{
    public partial class apply_list : Lip2p.Web.UI.ManagePage
    {
        protected string keywords = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_job", DTEnums.ActionEnum.Show.ToString()); //检查权限
            this.keywords = DTRequest.GetQueryString("keywords");
            if (!Page.IsPostBack)
            {
                RptBind("id>0" + CombSqlTxt(this.keywords), "sort_id asc,add_time desc");
            }
        }

        #region 数据绑定=================================
        private void RptBind(string _strWhere, string _orderby)
        {
            this.txtKeywords.Text = this.keywords;
            BLL.job_apply bll = new BLL.job_apply();
            this.rptList.DataSource = bll.GetList(0, _strWhere, _orderby);
            this.rptList.DataBind();
        }
        #endregion

        #region  获取招聘职位名称=========================
        protected string GetJobName(int jobId)
        {
            BLL.job bll = new BLL.job();
            Model.job model = bll.GetModel(jobId);
            if (model == null) return "";

            return model.title;
        }
        #endregion

        #region 组合SQL查询语句==========================
        protected string CombSqlTxt(string _keywords)
        {
            StringBuilder strTemp = new StringBuilder();
            _keywords = _keywords.Replace("'", "");
            if (!string.IsNullOrEmpty(_keywords))
            {
                strTemp.Append(" and realname like '%" + _keywords + "%'");
            }
            return strTemp.ToString();
        }
        #endregion

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("apply_list.aspx", "keywords={0}", txtKeywords.Text));
        }


        //保存排序
        protected void btnSave_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_job", DTEnums.ActionEnum.Edit.ToString()); //检查权限
            BLL.job_apply bll = new BLL.job_apply();
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
            JscriptMsg("保存排序成功啦！", Utils.CombUrlTxt("apply_list.aspx", "keywords={0}", this.keywords), "Success");
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_job", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            BLL.job_apply bll = new BLL.job_apply();
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    bll.Delete(id);
                }
            }
            JscriptMsg("批量删除成功啦！", Utils.CombUrlTxt("apply_list.aspx", "keywords={0}", this.keywords), "Success");
        }

    }
}