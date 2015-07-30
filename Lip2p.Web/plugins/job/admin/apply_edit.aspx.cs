using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.Job.admin
{
    public partial class apply_edit : Lip2p.Web.UI.ManagePage
    {
        private int id = 0;
        protected Model.job_apply model = new Model.job_apply();

        protected void Page_Load(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_job", DTEnums.ActionEnum.View.ToString()); //检查权限
            if (!int.TryParse(Request.QueryString["id"] as string, out this.id))
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            if (!new BLL.job_apply().Exists(this.id))
            {
                JscriptMsg("信息不存在或已被删除！", "back", "Error");
                return;
            }
            if (!Page.IsPostBack)
            {
                ShowInfo(this.id);
                new BLL.job_apply().UpdateField(id,"is_see=1");//更新查看状态
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int _id)
        {
            BLL.job_apply bll = new BLL.job_apply();
            model = bll.GetModel(_id);
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

    }
}