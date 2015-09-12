using System;
using System.Web.UI;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using System.Linq;

namespace Lip2p.Web.admin.project
{
    public partial class project_edit_audit : project_edit
    {       
        //页面加载事件
        public override void Page_Init(object sernder, EventArgs e)
        {
            this.page_name = "audit";
            this.channel_id = DTRequest.GetQueryInt("channel_id");       
        }

        public override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);
            if (!Page.IsPostBack)
            {
                throw new NotImplementedException("不确定跳转到什么状态");
                /*if (project_status < (int)Lip2p.Common.Lip2pEnums.ProjectStatusEnum.QianYue)
                {
                    div_project_profit_rate.Visible = false;
                }*/
            }
        }

        protected void btnAudit_Click(object sender, EventArgs e)
        {
            var project = context.li_projects.FirstOrDefault(p => p.id == this.id);
            if (project != null)
            {
                //项目风审通过后直接签约
                throw new NotImplementedException("不确定跳转到什么状态");
                /*if (project.status < (int)Lip2pEnums.ProjectStatusEnum.QianYue)
                    project.status = (int)Lip2pEnums.ProjectStatusEnum.QianYue;
                else
                    project.status += 10;
                context.SubmitChanges();
                JscriptMsg("项目审批通过！", String.Format("project_list_audit.aspx?channel_id={1}", page_name, this.channel_id), "Success");*/
            }
        }

        protected void btnNotAudit_Click(object sender, EventArgs e)
        {
            var project = context.li_projects.FirstOrDefault(p => p.id == this.id);
            if (project != null)
            {
                project.status -= 10;
                context.SubmitChanges();
                JscriptMsg("项目审批不通过！", String.Format("project_list_audit.aspx?channel_id={1}", page_name, this.channel_id), "Success");
            }
        }
    }
}