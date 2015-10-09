using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.BLL;
using Agp2p.Core;

namespace Agp2p.Web.admin.project
{
    public partial class loan_detail : Web.UI.ManagePage
    {
        protected int ChannelId;
        protected int ProjectId = 0;
        protected int RepayId = 0;
        protected int ProjectStatus;
        protected int LoanType = 0;
        protected  BLL.loan Loan;

        public Agp2pDataContext LqContext = new Agp2pDataContext();

        //页面初始化事件
        public virtual void Page_Init(object sernder, EventArgs e)
        {
            ChkAdminLevel("loan_detail", DTEnums.ActionEnum.View.ToString()); //检查权限      
            this.ChannelId = DTRequest.GetQueryInt("channel_id");
            if (this.ChannelId == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
        }

        //页面加载事件
        public virtual void Page_Load(object sender, EventArgs e)
        {
            this.ProjectId = DTRequest.GetQueryInt("id");
            this.ProjectStatus = DTRequest.GetQueryInt("status");
            if (!Page.IsPostBack)
            {         
                if (this.ProjectId == 0)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                li_projects project = LqContext.li_projects.FirstOrDefault(q => q.id == this.ProjectId);
                if (project == null)
                {
                    JscriptMsg("项目不存在或已被删除！", "back", "Error");
                    return;
                }
                Loan = new BLL.loan(LqContext);
                ShowByStatus();
                ShowInfo(project);
                LoanType = project.type;
            }
        }

        private void ShowByStatus()
        {
            switch (ProjectStatus)
            {
                case (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationChecking:
                    btnAudit.Visible = true;
                    btnNotAudit.Visible = true;
                    break;
                case (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationSuccess:
                    div_fabiao.Visible = true;
                    btnApply.Visible = true;
                    btnApplyOnTime.Visible = true;
                    //借款标识
                    rblTag.Items.AddRange(
                        Utils.GetEnumValues<Agp2pEnums.ProjectTagEnum>()
                            .Select(te => new ListItem(Utils.GetAgp2pEnumDes(te), ((int)te).ToString()))
                            .ToArray());
                    break;
                case (int)Agp2pEnums.ProjectStatusEnum.Financing:
                    btnDrop.Visible = true;
                    break;
                case (int)Agp2pEnums.ProjectStatusEnum.FinancingTimeout:
                    btnFail.Visible = true;
                    break;
                case (int)Agp2pEnums.ProjectStatusEnum.FinancingSuccess:
                    btnFail.Visible = true;
                    btnMakeLoan.Visible = true;
                    break;
            }
        }

        /// <summary>
        /// 显示项目信息
        /// </summary>
        /// <param name="_project"></param>
        public virtual void ShowInfo(li_projects _project)
        {
            spa_category.InnerText = new article_category().GetTitle(_project.category_id);//项目类别
            spa_type.InnerText = Utils.GetAgp2pEnumDes((Agp2pEnums.LoanTypeEnum)_project.type);//借款主体
            spa_title.InnerText = _project.title;
            spa_no.InnerText = _project.no;
            spa_amount.InnerText = _project.financing_amount.ToString();//借款金额            
            spa_repayment.InnerText = _project.repayment_term_span_count +
                                      Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTermSpanEnum)_project.repayment_term_span); //借款期限
            spa_repayment_type.InnerText = Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTypeEnum)_project.repayment_type);//还款方式
            spa_profit_rate.InnerText = _project.profit_rate_year.ToString();//年化利率
            if (_project.tag != null)
                spa_tag.InnerText = Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectTagEnum) _project.tag);
            spa_financing_day.InnerText = _project.financing_day.ToString();
            spa_add_time.InnerText = _project.add_time.ToString("yyyy-MM-dd HH:mm:ss");//申请时间
            spa_publish_time.InnerText = _project.publish_time?.ToString("yyyy-MM-dd HH:mm:ss");//发布时间
            spa_make_loan_time.InnerText = _project.make_loan_time?.ToString("yyyy-MM-dd HH:mm:ss");//放款时间

            ShowRiskInfo(_project);
        }

        /// <summary>
        /// 显示风险信息
        /// </summary>
        protected void ShowRiskInfo(li_projects _project)
        {
            if (_project.li_risks != null)
            {
                var risk = _project.li_risks;
                //借款人信息
                ShowLoanerInfo(risk.li_loaners, risk.id);
                //债权人信息
                if (_project.type == (int) Agp2pEnums.LoanTypeEnum.Creditor)
                {
                    spa_creditor.InnerText = risk.li_creditors.dt_users.real_name;
                    spa_creditorContent.InnerText = risk.creditor_content;
                }
                //风控信息
                spa_loanerContent.InnerText = risk.loaner_content;//借款描述
                spa_loanUse.InnerText = risk.loan_usage;//借款用途
                spa_repaymentSource.InnerText = risk.source_of_repayment;//还款来源
                spa_txtRiskContent.InnerHtml = risk.risk_content;//风控描述
                // 加载相册
                rptMortgageContracts.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Agp2pEnums.AlbumTypeEnum.MortgageContract);
                rptMortgageContracts.DataBind();
                rptLienCertificates.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Agp2pEnums.AlbumTypeEnum.LienCertificate);
                rptLienCertificates.DataBind();
                rptLoanAgreement.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Agp2pEnums.AlbumTypeEnum.LoanAgreement);
                rptLoanAgreement.DataBind();
            }
        }

        /// <summary>
        /// 显示借款人、抵押物信息
        /// </summary>
        /// <param name="loaner"></param>
        /// <param name="riskId"></param>
        private void ShowLoanerInfo(li_loaners loaner,int riskId)
        {
            //借款人信息
            sp_loaner_name.InnerText = loaner.dt_users.real_name;
            sp_loaner_gender.InnerText = loaner.dt_users.sex;
            sp_loaner_job.InnerText = loaner.job;
            sp_loaner_working_at.InnerText = loaner.working_at;
            sp_loaner_tel.InnerText = loaner.dt_users.telphone;
            sp_loaner_id_card_number.InnerText = loaner.dt_users.id_card_number;
            //企业信息
            if (loaner.li_loaner_companies != null)
            {
                sp_company_name.InnerText = loaner.li_loaner_companies.name;
                sp_company_business_scope.InnerText = loaner.li_loaner_companies.business_scope;
                sp_company_business_status.InnerText = loaner.li_loaner_companies.business_status;
                sp_company_income_yearly.InnerText = loaner.li_loaner_companies.income_yearly;
                sp_company_registered_capital.InnerText = loaner.li_loaner_companies.registered_capital;
                sp_company_setup_time.InnerText = loaner.li_loaner_companies.setup_time.ToString("yyyy年MM月dd日");
            }

            var allMortgages = Loan.LoadMortgageList(loaner.id, riskId);
            rptList.DataSource = allMortgages;
            rptList.DataBind();
        }

        protected void btnAudit_OnClick(object sender, EventArgs e)
        {
            do_loan_audit(true);
        }

        protected void btnNotAudit_OnClick(object sender, EventArgs e)
        {
            do_loan_audit(false);
        }

        private void do_loan_audit(bool auditSuccess)
        {
            ChkAdminLevel("loan_audit", DTEnums.ActionEnum.Audit.ToString()); //检查权限
            var project = LqContext.li_projects.FirstOrDefault(p => p.id == ProjectId);
            if (project != null)
            {
                project.status = auditSuccess
                    ? (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationSuccess
                    : (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationFail;
                LqContext.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Audit.ToString(), "审核操作成功！"); //记录日志
                JscriptMsg("审核操作成功！", Utils.CombUrlTxt("loan_audit.aspx", "channel_id={0}", this.ChannelId.ToString()));
            }
            else
            {
                JscriptMsg("项目不存在或已被删除！", "back", "Error");
            }
        }

        /// <summary>
        /// 发布借款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnApply_OnClick(object sender, EventArgs e)
        {
            var project = LqContext.li_projects.SingleOrDefault(p => p.id == ProjectId);
            if (project != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(rblTag.SelectedValue))
                        project.tag = Utils.StrToInt(rblTag.SelectedValue, 0);
                    project.status = (int)Agp2pEnums.ProjectStatusEnum.Financing;
                    project.publish_time = string.IsNullOrEmpty(txtPublishTime.Text.Trim())
                        ? DateTime.Now
                        : DateTime.Parse(txtPublishTime.Text.Trim());
                    project.financing_day = Convert.ToInt16(txt_financing_day.Text.Trim());
                    LqContext.SubmitChanges();
                    JscriptMsg("发布借款成功！",
                        Utils.CombUrlTxt("loan_financing.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                            ((int)Agp2pEnums.ProjectStatusEnum.Financing).ToString()));
                }
                catch (Exception ex)
                {
                    JscriptMsg("发布借款失败：" + ex.Message, "back", "Error");
                }
            }
            else
            {
                JscriptMsg("项目不存在或已被删除！", "back", "Error");
            }
        }

        /// <summary>
        /// 撤销借款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnDrop_OnClick(object sender, EventArgs e)
        {
            var project = LqContext.li_projects.SingleOrDefault(p => p.id == ProjectId);
            if (project != null)
            {
                try
                {
                    project.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationFail;
                    LqContext.SubmitChanges();
                    JscriptMsg("撤销借款成功！",
                        Utils.CombUrlTxt("loan_financing.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                            ((int)Agp2pEnums.ProjectStatusEnum.Financing).ToString()));
                }
                catch (Exception ex)
                {
                    JscriptMsg("撤销借款失败：" + ex.Message, "back", "Error");
                }
            }
            else
            {
                JscriptMsg("项目不存在或已被删除！", "back", "Error");
            }
        }

        /// <summary>
        /// 借款流标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnFail_OnClick(object sender, EventArgs e)
        {
            var project = LqContext.li_projects.SingleOrDefault(p => p.id == ProjectId);
            if (project != null)
            {
                try
                {
                    project.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingFail;
                    //TODO 资金原路退回投资者账户
                    LqContext.ProjectFinancingFail(project.id);
                    LqContext.SubmitChanges();
                    JscriptMsg("借款流标操作成功！",
                        Utils.CombUrlTxt("loan_financing.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                            ((int)Agp2pEnums.ProjectStatusEnum.Financing).ToString()));
                }
                catch (Exception ex)
                {
                    JscriptMsg("借款流标操作失败：" + ex.Message, "back", "Error");
                }
            }
            else
            {
                JscriptMsg("项目不存在或已被删除！", "back", "Error");
            }
        }

        /// <summary>
        /// 放款给借款人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnMakeLoan_OnClick(object sender, EventArgs e)
        {
            try
            {
                //TODO 资金打入借款人账户
                LqContext.StartRepayment(ProjectId);
                JscriptMsg("放款操作成功！",
                    Utils.CombUrlTxt("loan_financing_success.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                        ((int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying).ToString()));
            }
            catch (Exception ex)
            {
                JscriptMsg("放款操作失败：" + ex.Message, "back", "Error");
            }
        }

        protected void btnApplyOnTime_OnClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }

}