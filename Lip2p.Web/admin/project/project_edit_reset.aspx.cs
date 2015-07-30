using System;
using System.Web.UI;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using System.Linq;

namespace Lip2p.Web.admin.project
{
    public partial class project_edit_reset : project_edit
    {
        protected bool show_mortgage_tab = false;

        //页面加载事件
        public override void Page_Init(object sernder, EventArgs e)
        {
            this.page_name = "publish";
            this.channel_id = DTRequest.GetQueryInt("channel_id");
            show_mortgage_tab = !String.IsNullOrEmpty(DTRequest.GetQueryString("show_risk"));          
        }

        public override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);
            if (!Page.IsPostBack)
            {
                //复核后不能修改项目基本信息
                if (project_status >= (int)Lip2p.Common.Lip2pEnums.ProjectStatusEnum.FuHe)
                {
                    txt_project_amount.Enabled = false;
                    txt_project_repayment_number.Enabled = false;
                    txt_project_repayment_term.Enabled = false;
                    txt_project_repayment_type.Enabled = false;
                    txt_project_profit_rate.Enabled = false;
                }
            }
        }

        protected void btnReset(object sender, EventArgs e)
        {
            ChkAdminLevel("project_" + page_name, DTEnums.ActionEnum.Replace.ToString()); //检查权限

            var project = context.li_projects.FirstOrDefault(q => q.id == this.id);
            if (project == null)
            {
                JscriptMsg("信息不存在或已被删除！", "back", "Error");
                return;
            }
            //项目信息赋值
            SetProjectModel(project);
            SetRiskModel(project.li_risks);

            //风控信息赋值
            var riskInfo = project.li_risks;
            riskInfo.creditor = Utils.StrToInt(ddlCreditor.SelectedValue, 0);
            riskInfo.creditor_content = txtCreditorContent.Text;
            riskInfo.loaner_content = txtLoanerContent.Text;
            riskInfo.risk_content = txtRiskContent.Value;
            
            LoadAlbum(riskInfo, Lip2pEnums.AlbumTypeEnum.LoanAgreement, 0);
            LoadAlbum(riskInfo, Lip2pEnums.AlbumTypeEnum.MortgageContract, 1);
            LoadAlbum(riskInfo, Lip2pEnums.AlbumTypeEnum.LienCertificate, 2);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改项目成功！"); //记录日志
                JscriptMsg("修改项目成功！", String.Format("project_list_publish.aspx?channel_id={1}", page_name, this.channel_id), "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("保存过程中发生错误啦：" + ex.Message, "", "Error");
            }
        }
    }
}