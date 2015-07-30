using System;
using System.Web.UI;
using Lip2p.Common;
using System.Text;
using System.Web.UI.WebControls;
using System.Linq;

namespace Lip2p.Web.admin.project
{
    public partial class project_list_audit : project_list
    {
        //页面初始化事件
        protected void Page_Init(object sernder, EventArgs e)
        {
            page_name = "audit";
        }

        //批量审核
        protected void btnAudit_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("project_" + page_name, DTEnums.ActionEnum.Audit.ToString()); //检查权限
            Repeater rptList = new Repeater();
            var context = new Lip2p.Linq2SQL.Lip2pDataContext();
            switch (this.prolistview)
            {
                case "Txt":
                    rptList = this.rptList1;
                    break;
                default:
                    rptList = this.rptList2;
                    break;
            }
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    var project = context.li_projects.FirstOrDefault(p => p.id == id);
                    if (project != null)
                    {
                        //项目风审通过后直接签约
                        if (project.status < (int)Lip2pEnums.ProjectStatusEnum.QianYue)
                            project.status = (int)Lip2pEnums.ProjectStatusEnum.QianYue;
                        else
                            project.status += 10;
                    }
                }
            }
            context.SubmitChanges();
            AddAdminLog(DTEnums.ActionEnum.Audit.ToString(), "审核" + this.channel_name + "频道内容信息"); //记录日志
            JscriptMsg("批量审核成功！", Utils.CombUrlTxt("project_list_" + page_name + ".aspx", "channel_id={0}&category_id={1}&keywords={2}&project_status={3}",
                this.channel_id.ToString(), this.category_id.ToString(), this.keywords, this.project_status), "Success");
        }
    }
}