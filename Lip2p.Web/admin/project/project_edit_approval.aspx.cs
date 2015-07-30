using System;
using System.Linq;
using System.Web.UI;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.project
{
    public partial class project_edit_approval : project_edit
    {
        protected bool show_mortgage_tab = false;
        private li_projects project_session = new li_projects();

        public override void Page_Init(object sernder, EventArgs e)
        {
            this.page_name = "approval";
            this.channel_id = DTRequest.GetQueryInt("channel_id");
            show_mortgage_tab = !String.IsNullOrEmpty(DTRequest.GetQueryString("show_risk"));          
        }

        public override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);
            if (!Page.IsPostBack)
            {
                if (show_mortgage_tab)
                {
                    //新增项目才需要从缓存中显示项目信息
                    if (action == DTEnums.ActionEnum.Add.ToString())
                    {
                        project_session = (li_projects)SessionHelper.Get("project_edit_approval");
                        if (risk_id > 0)
                        {
                            var risk = context.li_risks.FirstOrDefault(r => r.id == risk_id);
                            if (risk == null)
                            {
                                JscriptMsg("风控信息不存在或已被删除！", "back", "Error");
                            }
                            else
                                project_session.li_risks = risk;
                            ShowInfo(project_session);
                        }
                        else
                            JscriptMsg("风控信息ID传输错误！", "back", "Error");
                    }
                }
                //风审后不能修改项目基本信息
                //if (project_status >= (int)Lip2p.Common.Lip2pEnums.ProjectStatusEnum.FengShen)
                //{
                //    ddlCategoryId.Enabled = false;
                //    txtTitle.Enabled = false;
                //    txt_project_no.Enabled = false;
                //}
            }
        }

        /// <summary>
        /// 重新立项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnReSet_Click(object sender, EventArgs e)
        {
            var project = context.li_projects.FirstOrDefault(p => p.id == this.id);
            if (project != null)
            {
                project.status = 0;
                context.SubmitChanges();
                JscriptMsg("重新立项成功！", String.Format("project_list_{0}.aspx?channel_id={1}", page_name, this.channel_id), "Success");
            }
        }

        protected void OpenMortgageDialog(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Add.ToString())
            {
                //缓存当前填写的项目信息
                SetProjectModel(project_session);
                SessionHelper.Set("project_edit_approval", project_session);
            }
            //打开选择标的物窗口
            string msbox = "SelectMortgageSetting(\"" + this.risk_id + "\", \"" + this.channel_id + "\", \"" + this.id + "\")";
            ClientScript.RegisterClientScriptBlock(Page.GetType(), "SelectMortgageSetting", msbox, true);
            show_mortgage_tab = true;
        }
    }
}