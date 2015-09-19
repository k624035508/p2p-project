using System;
using System.Web.UI;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using System.Linq;
using System.Threading;
using Lip2p.Core.Message;

namespace Lip2p.Web.admin.project
{
    public partial class project_edit_publish : project_edit
    {
        private double seconds = 0;
        //页面加载事件
        public override void Page_Init(object sernder, EventArgs e)
        {
            this.page_name = "publish";
            this.channel_id = DTRequest.GetQueryInt("channel_id");       
        }

        public override void Page_Load(object sender, EventArgs e)
        {
            this.Page.ClientScript.GetPostBackEventReference(btnOffProject, string.Empty);
            base.Page_Load(sender, e);
            if (!Page.IsPostBack)
            {
                //复核后不能修改项目基本信息
                throw new NotImplementedException("不确定跳转到什么状态");
                /*if (project_status >= (int)Lip2p.Common.Lip2pEnums.ProjectStatusEnum.FuHe)
                {
                    txt_project_amount.Enabled = false;
                    txt_project_repayment_number.Enabled = false;
                    txt_project_repayment_term.Enabled = false;
                    txt_project_repayment_type.Enabled = false;
                    txt_project_profit_rate.Enabled = false;
                }*/
                //拆标可以修改项目标题
                if (action == DTEnums.ActionEnum.Copy.ToString())
                {
                    txtTitle.Enabled = true;
                    txt_project_no.Enabled = true;
                }
            }
        }

        protected void DoCopy(object sender, EventArgs e)
        {
            var project_parent = context.li_projects.FirstOrDefault(q => q.id == this.id);
            if (project_parent == null)
            {
                JscriptMsg("父标信息不存在或已被删除！", "back", "Error");
                return;
            }
            //父项目金额
            project_parent_amount = project_parent.financing_amount;
            var projectAmount = project_parent_amount - Utils.StrToInt(txt_project_amount.Text, 0);
            if (projectAmount < 0)
            {
                JscriptMsg("项目拆分金额已超过父项目剩余金额！", "back", "Info");
                return;
            }
            else if (projectAmount == 0)
            {
                //删除父项目
                context.li_projects.DeleteOnSubmit(project_parent);
            }
            else
            {
                //更新父项目金额
                project_parent.financing_amount = projectAmount;
            }

            var project_copy = new li_projects();
            SetProjectModel(project_copy);
            //新建一个风控信息
            var risk_parent = context.li_risks.FirstOrDefault(r => r.id == project_parent.risk_id);
            li_risks risk = new li_risks();                
            risk.last_update_time = DateTime.Now;
            risk.creditor = risk_parent.creditor;        
            risk.loaner = risk_parent.loaner;
            risk.loaner_content = risk_parent.loaner_content;
            risk.creditor_content = risk_parent.creditor_content;
            risk.risk_content = txtRiskContent.Value;
            //标的物绑定
            foreach(li_risk_mortgage risk_mortgage in risk_parent.li_risk_mortgage)
            {
                var riskMortgage_new = new li_risk_mortgage
                {
                    mortgage = risk_mortgage.mortgage,
                    last_update_time = DateTime.Now,
                    li_risks = risk
                };
                context.li_risk_mortgage.InsertOnSubmit(riskMortgage_new);
            }
            //绑定相册
            LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.LoanAgreement, 0);
            LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.MortgageContract, 1);
            LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.LienCertificate, 2);
            context.li_risks.InsertOnSubmit(risk);

            project_copy.li_risks = risk;
            throw new NotImplementedException("不确定跳转到什么状态");
            //project_copy.status = (int)Lip2pEnums.ProjectStatusEnum.LiBiao;
            context.li_projects.InsertOnSubmit(project_copy);
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Copy.ToString(), "拆分项目成功！"); //记录日志
                JscriptMsg("拆分项目成功！", String.Format("project_list_{0}.aspx?channel_id={1}", page_name, this.channel_id), "Success");
            }
            catch (Exception)
            {
                JscriptMsg("保存过程中发生错误啦！", "", "Error");
            }
        }

        /// <summary>
        /// 重新立标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnReSet_Click(object sender, EventArgs e)
        {
            var project = context.li_projects.FirstOrDefault(p => p.id == this.id);
            if (project != null)
            {
                throw new NotImplementedException("不确定跳转到什么状态");
                /*var info = project.status == (int)Lip2pEnums.ProjectStatusEnum.FaBiao ? "项目下标成功！" : "重新立标成功";
                project.status = (int)Lip2pEnums.ProjectStatusEnum.QianYue;
                context.SubmitChanges();                
                JscriptMsg(info, String.Format("project_list_{0}.aspx?channel_id={1}", page_name, this.channel_id), "Success");*/
            }
        }

        /// <summary>
        /// 重新立项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnLixiang_Click(object sender, EventArgs e)
        {
            var project = context.li_projects.FirstOrDefault(p => p.id == this.id);
            if (project != null)
            {
                project.status = 0;
                context.SubmitChanges();
                JscriptMsg("重新立项成功！", String.Format("project_list_{0}.aspx?channel_id={1}", page_name, this.channel_id), "Success");
            }
        }

        /// <summary>
        /// 延迟发标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnDelayFaBiao_Click(object sender, EventArgs e)
        {
            if (!DoEdit(this.id))
            {
                JscriptMsg("保存过程中发生错误啦！", "", "Error");
                return;
            }
            JscriptMsg("项目延迟发标成功，" + seconds + "分钟后项目自动发布！", String.Format("project_list_{0}.aspx?channel_id={1}", page_name, this.channel_id), "Success");
        }

        private bool DoEdit(int _id)
        {
            bool result = false;
            var project = context.li_projects.FirstOrDefault(q => q.id == this.id);

            if (project == null)
            {
                JscriptMsg("信息不存在或已被删除！", "back", "Error");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPublishTime.Text))
            {
                JscriptMsg("请输入你要延迟发标的准确时间！", "back", "Error");
                return false;
            }
            if (Convert.ToDateTime(txtPublishTime.Text) <= DateTime.Now.ToLocalTime())
            {
                JscriptMsg("延迟发标时间不能小于等于当前系统时间！", "back", "Error");
                return false;
            }


            //项目信息赋值
            SetProjectModel(project);
            SetRiskModel(project.li_risks);

            //项目状态修改

            throw new NotImplementedException("不确定跳转到什么状态");
            /*if (project.status < (int)Lip2pEnums.ProjectStatusEnum.FengShen)
            {
                //风控信息赋值
                var riskInfo = project.li_risks;
                riskInfo.creditor = Utils.StrToInt(ddlCreditor.SelectedValue, 0);
                riskInfo.creditor_content = txtCreditorContent.Text;
                riskInfo.loaner_content = txtLoanerContent.Text;
                riskInfo.risk_content = txtRiskContent.Value;

                LoadAlbum(riskInfo, Lip2pEnums.AlbumTypeEnum.LoanAgreement, 0);
                LoadAlbum(riskInfo, Lip2pEnums.AlbumTypeEnum.MortgageContract, 1);
                LoadAlbum(riskInfo, Lip2pEnums.AlbumTypeEnum.LienCertificate, 2);
            }*/

            project.status = (int)Lip2pEnums.ProjectStatusEnum.FinancingAtTime;
            project.publish_time = Utils.StrToDateTime(txtPublishTime.Text.Trim());
            //project.publish_time = DateTime.Now.AddMinutes(10);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改项目成功！"); //记录日志

                //启动进程
                TimeSpan time = Convert.ToDateTime(txtPublishTime.Text.Trim()).Subtract(DateTime.Now);
                seconds = Math.Floor(time.TotalMinutes)+1;
                MessageBus.Main.PublishDelay(new ProjectSchedulePublishMsg(project.id), (int)time.TotalMilliseconds);

                result = true;
            }
            catch (Exception)
            {
                return false;
            }
            return result;
        }

    }
}