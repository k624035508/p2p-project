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
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agp2p.Web.admin.project
{
    public partial class loan_detail : Web.UI.ManagePage
    {
        protected int ChannelId;
        protected int ProjectId = 0;
        protected int RepayId = 0;
        protected int ProjectStatus;
        protected int LoanType = 0;
        protected BLL.loan Loan;

        public Agp2pDataContext LqContext = new Agp2pDataContext();

        //页面初始化事件
        public virtual void Page_Init(object sernder, EventArgs e)
        {
            ChkAdminLevel("loan_audit", DTEnums.ActionEnum.View.ToString()); //检查权限      
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
                ShowProjectInfo(project);
                if (project.IsHuoqiProject())
                {
                    ShowProfitingClaimInfo(project);
                }
                else
                {
                    ShowClaimsInfo(project);
                }
                LoanType = project.type;
                ShowByStatus();
            }
        }

        protected bool isHuoqiProject;

        private void ShowProfitingClaimInfo(li_projects project)
        {
            isHuoqiProject = true;
            rptClaimList.DataSource = project.li_claims_profiting.AsEnumerable();
            rptClaimList.DataBind();
        }

        private void ShowClaimsInfo(li_projects project)
        {
            isHuoqiProject = false;
            rptClaimList.DataSource = project.li_claims.AsEnumerable();
            rptClaimList.DataBind();
        }

        protected void btnBecomeTransferable_OnClick(object sender, EventArgs e)
        {
            var transferAmount = Request["__EVENTARGUMENT"];
            int claimId = Convert.ToInt32(((LinkButton)sender).CommandArgument);
            var claim = LqContext.li_claims.Single(c => c.id == claimId);

            var remark = string.Format("将项目【{0}】的债权 {1} 设置为可转让", claim.li_projects.title, claimId);
            LqContext.AppendAdminLog(DTEnums.ActionEnum.Edit.ToString(), remark, false);
            TransactionFacade.StaticProjectWithdraw(LqContext, claimId, 1 - Convert.ToDecimal(transferAmount)/100);

            ShowClaimsInfo(claim.li_projects);

            JscriptMsg(remark, "");
        }

        protected static string GetFriendlyUserName(dt_users user)
        {
            return string.IsNullOrWhiteSpace(user.real_name)
                ? user.user_name
                : $"{user.user_name}({user.real_name})";
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
                    btnActivate.Visible = true;
                    btnCut.Visible = true;
                    btnCancel.Visible = true;
                    div_financing_add_day.Visible = true;
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
        public virtual void ShowProjectInfo(li_projects _project)
        {
            spa_category.InnerText = new article_category().GetTitle(_project.category_id);//项目类别
            spa_type.InnerText = Utils.GetAgp2pEnumDes((Agp2pEnums.LoanTypeEnum)_project.type);//借款主体
            spa_title.InnerText = _project.title;
            spa_no.InnerText = _project.no;
            spa_amount.InnerText = _project.financing_amount.ToString();//借款金额
            spa_repayment.InnerText = _project.repayment_term_span_count +
                                      Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTermSpanEnum)_project.repayment_term_span); //借款期限
            spa_repayment_type.InnerText = Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTypeEnum)_project.repayment_type);//还款方式
            spa_profit_rate.InnerText = _project.profit_rate_year.ToString("N1");//年化利率
            
            if (_project.tag != null)
                spa_tag.InnerText = Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectTagEnum)_project.tag);
            spa_financing_day.InnerText = _project.financing_day.ToString();
            spa_add_time.InnerText = _project.add_time.ToString("yyyy-MM-dd HH:mm:ss");//申请时间
            spa_publish_time.InnerText = _project.publish_time?.ToString("yyyy-MM-dd HH:mm:ss");//发布时间
            spa_make_loan_time.InnerText = _project.make_loan_time?.ToString("yyyy-MM-dd HH:mm:ss");//放款时间
            spa_bond_fee.InnerText = _project.bond_fee_rate?.ToString("N4");
            spa_loan_fee.InnerText = _project.loan_fee_rate?.ToString("N4");
            spa_contact_no.InnerText = _project.contract_no;

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
                if (_project.type == (int)Agp2pEnums.LoanTypeEnum.Creditor)
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
                rptMortgageContracts.DataSource = risk.li_albums.Where(a => a.type == (int)Agp2pEnums.AlbumTypeEnum.MortgageContract);
                rptMortgageContracts.DataBind();
                rptLienCertificates.DataSource = risk.li_albums.Where(a => a.type == (int)Agp2pEnums.AlbumTypeEnum.LienCertificate);
                rptLienCertificates.DataBind();
                rptLoanAgreement.DataSource = risk.li_albums.Where(a => a.type == (int)Agp2pEnums.AlbumTypeEnum.LoanAgreement);
                rptLoanAgreement.DataBind();
            }
        }

        /// <summary>
        /// 显示借款人、抵押物信息
        /// </summary>
        /// <param name="loaner"></param>
        /// <param name="riskId"></param>
        private void ShowLoanerInfo(li_loaners loaner, int riskId)
        {
            if (loaner == null) return;
            //借款人信息
            sp_loaner_name.InnerText = loaner?.dt_users.real_name;
            sp_loaner_gender.InnerText = loaner?.dt_users.sex;
            sp_loaner_job.InnerText = loaner?.job;
            sp_loaner_working_at.InnerText = loaner?.working_at;
            sp_loaner_tel.InnerText = loaner?.dt_users.telphone;
            sp_loaner_id_card_number.InnerText = loaner?.dt_users.id_card_number;
            //企业信息
            if (loaner?.li_loaner_companies != null)
            {
                sp_company_name.InnerText = loaner.li_loaner_companies.name;
                sp_company_business_scope.InnerText = loaner.li_loaner_companies.business_scope;
                sp_company_business_status.InnerText = loaner.li_loaner_companies.business_status;
                sp_company_registered_capital.InnerText = loaner.li_loaner_companies.registered_capital;
                sp_company_setup_time.InnerText = loaner.li_loaner_companies.setup_time.ToString("yyyy年MM月dd日");
            }

            if (loaner != null)
            {
                rptList.DataSource = Loan.LoadMortgageList(loaner.id, riskId, false);
                rptList.DataBind();
            }
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
                    : (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationCancel;
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
                    PublishProject(project, false);
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

        private void PublishProject(li_projects project, bool publishdelay)
        {
            ChkAdminLevel("loan_financing", DTEnums.ActionEnum.Add.ToString());
            if (!string.IsNullOrEmpty(rblTag.SelectedValue))
                project.tag = Utils.StrToInt(rblTag.SelectedValue, 0);
            project.status = publishdelay ? (int)Agp2pEnums.ProjectStatusEnum.FinancingAtTime : (int)Agp2pEnums.ProjectStatusEnum.Financing;
            project.publish_time = string.IsNullOrEmpty(txtPublishTime.Text.Trim())
                ? DateTime.Now
                : DateTime.Parse(txtPublishTime.Text.Trim());
            project.financing_day = Convert.ToInt16(txt_financing_day.Text.Trim());
            LqContext.SubmitChanges();
        }

        /// <summary>
        /// 延迟发布借款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnApplyOnTime_OnClick(object sender, EventArgs e)
        {
            var project = LqContext.li_projects.SingleOrDefault(p => p.id == ProjectId);
            if (string.IsNullOrWhiteSpace(txtPublishTime.Text))
            {
                JscriptMsg("请输入你要延迟发标的准确时间！", "back", "Error");
                return;
            }
            if (Convert.ToDateTime(txtPublishTime.Text.Trim()) <= DateTime.Now)
            {
                JscriptMsg("延迟发布时间不能小于当前时间！", "back", "Error");
                return;
            }

            if (project != null)
            {
                try
                {
                    PublishProject(project, true);
                    //启动延迟发布进程
                    TimeSpan time = Convert.ToDateTime(txtPublishTime.Text.Trim()).Subtract(DateTime.Now);
                    var seconds = Math.Floor(time.TotalMinutes) + 1;
                    MessageBus.Main.PublishDelay(new ProjectSchedulePublishMsg(project.id), (int)time.TotalMilliseconds);
                    JscriptMsg("延迟发布借款成功，" + seconds + "分钟后自动发布！",
                        Utils.CombUrlTxt("loan_financing.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                            ((int)Agp2pEnums.ProjectStatusEnum.Financing).ToString()));
                }
                catch (Exception ex)
                {
                    JscriptMsg("延迟发布失败：" + ex.Message, "back", "Error");
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
                    //TODO　对接托管接口
                    ChkAdminLevel("loan_financing", DTEnums.ActionEnum.Edit.ToString());
                    project.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationCancel;
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
            ChkAdminLevel("loan_financing", DTEnums.ActionEnum.Delete.ToString());
            var project = LqContext.li_projects.SingleOrDefault(p => p.id == ProjectId);
            if (project != null)
            {
                var msg = new RepealProjectReqMsg(ProjectId, project.investment_amount.ToString("F"));
                MessageBus.Main.Publish(msg);
                var msgResp = BaseRespMsg.NewInstance<RepealProjectRespMsg>(msg.SynResult);
                MessageBus.Main.Publish(msgResp);
                if (msgResp.HasHandle)
                {
                    project.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingFail;
                    JscriptMsg("流标操作成功！",
                        Utils.CombUrlTxt("loan_financing.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                            ((int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying).ToString()));
                }
                else
                {
                    JscriptMsg("流标操作失败：" + msgResp.Remarks, "back", "Error");
                }
            }
            else
            {
                JscriptMsg("项目不存在或已被删除！", "back", "Error");
            }
        }

        /// <summary>
        /// 重新激活项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnActivate_OnClick(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txt_financing_add_day.Text.Trim()))
            {
                JscriptMsg("请输入募集顺延天数！", "back", "Error");
                return;
            }
           
            var project = LqContext.li_projects.SingleOrDefault(p => p.id == ProjectId);
            if (project != null)
            {
                try
                {
                    //ChkAdminLevel("loan_financing", DTEnums.ActionEnum.Delete.ToString());
                    project.status = (int)Agp2pEnums.ProjectStatusEnum.Financing;
                    project.financing_day = (short) (int.Parse(txt_financing_add_day.Text) + DateTime.Now.Subtract((DateTime)project.publish_time).Days);
                    LqContext.SubmitChanges();
                    JscriptMsg("借款激活操作成功！",
                        Utils.CombUrlTxt("loan_financing.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                            ((int)Agp2pEnums.ProjectStatusEnum.Financing).ToString()));
                    //发送顺延通知短信
                    var dtSmsTemplate = LqContext.dt_sms_template.FirstOrDefault(t => t.call_index == "project_financing_add_day");
                    if (dtSmsTemplate != null)
                    {
                        project.li_project_transactions.Where(
                            t => t.type == (int) Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                                 t.status == (int) Agp2pEnums.ProjectTransactionStatusEnum.Success)
                            .ToList()
                            .ForEach(i =>
                            {
                                var msgContent = dtSmsTemplate.content.Replace("{date}",
                                    DateTime.Now.ToString("yyyy年MM月dd日"))
                                    .Replace("{day}", txt_financing_add_day.Text)
                                    .Replace("{project}", i.li_projects.title);
                                try
                                {
                                    string errorMsg;
                                    if (!SMSHelper.SendTemplateSms(i.dt_users.mobile, msgContent, out errorMsg))
                                    {
                                        LqContext.AppendAdminLogAndSave("WithdrawSms",
                                            "发送借款募集顺延通知失败：" + errorMsg + "（客户ID：" + i.dt_users.user_name + "）");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LqContext.AppendAdminLogAndSave("WithdrawSms",
                                        "发送借款募集顺延通知失败：" + ex.GetSimpleCrashInfo() + "（客户ID：" + i.dt_users.user_name + "）");
                                }
                            });
                    }
                }
                catch (Exception ex)
                {
                    JscriptMsg("借款激活操作失败：" + ex.Message, "back", "Error");
                }
            }
            else
            {
                JscriptMsg("项目不存在或已被删除！", "back", "Error");
            }
        }

        protected void btnCut_OnClick(object sender, EventArgs e)
        {
            try
            {
                //ChkAdminLevel("loan_financing", DTEnums.ActionEnum.Delete.ToString());
                LqContext.FinishInvestmentEvenTimeout(ProjectId);
                JscriptMsg("借款截标操作成功！",
                    Utils.CombUrlTxt("loan_financing.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                        ((int)Agp2pEnums.ProjectStatusEnum.Financing).ToString()));
            }
            catch (Exception ex)
            {
                JscriptMsg("借款截标操作失败：" + ex.Message, "back", "Error");
            }
        }

        /// <summary>
        /// 作废项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_OnClick(object sender, EventArgs e)
        {
            var project = LqContext.li_projects.SingleOrDefault(p => p.id == ProjectId);
            if (project != null)
            {
                if (project.investment_amount == 0)
                {
                    try
                    {
                        //TODO　对接托管接口
                        ChkAdminLevel("loan_financing", DTEnums.ActionEnum.Edit.ToString());
                        project.status = (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationCancel;
                        LqContext.SubmitChanges();
                        JscriptMsg("项目作废成功！",
                            Utils.CombUrlTxt("loan_financing.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                                ((int)Agp2pEnums.ProjectStatusEnum.Financing).ToString()));
                    }
                    catch (Exception ex)
                    {
                        JscriptMsg("项目作废失败：" + ex.Message, "back", "Error");
                    }
                }
                else
                {
                    JscriptMsg("已有投资者投标，不能作废！", "back", "Error");
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
                ChkAdminLevel("make_loan_audit", DTEnums.ActionEnum.Audit.ToString());
                var project = LqContext.li_projects.SingleOrDefault(p => p.id == ProjectId);
                //调用托管平台实名验证接口
                if (project != null)
                {
                    var user = project.li_risks.li_loaners.dt_users;
                    //TODO 是否集合项目
                    var msg = new MakeLoanReqMsg(user.id, ProjectId, project.investment_amount.ToString("F"), false, user.dt_user_groups.title.Equals("融资合作组"));
                    MessageBus.Main.Publish(msg);
                    var msgResp = BaseRespMsg.NewInstance<MakeLoanRespMsg>(msg.SynResult);
                    MessageBus.Main.Publish(msgResp);
                    if (msgResp.HasHandle)
                    {
                        JscriptMsg("放款操作成功！",
                            Utils.CombUrlTxt("../audit/make_loan_audit.aspx", "channel_id={0}&status={1}", this.ChannelId.ToString(),
                                ((int)Agp2pEnums.ProjectStatusEnum.ProjectRepaying).ToString()));
                    }
                    else
                    {
                        JscriptMsg("放款操作失败：" + msgResp.Remarks, "back", "Error");
                    }
                }
                else
                {
                    JscriptMsg("放款操作失败，没有找到项目！", "back", "Error");
                }
                
            }
            catch (Exception ex)
            {
                JscriptMsg("放款操作失败：" + ex.Message, "back", "Error");
            }
        }

        protected void btnExport_OnClick(object sender, EventArgs e)
        {
            var p = LqContext.li_projects.Single(pr => pr.id == ProjectId);

            var projectInfoField = new[]
            {
                "借款类别",
                "借款主体",
                "借款标题",
                "借款编号",
                "借款合同编号",
                "借款金额",
                "年化利率（%）",
                "还款期限",
                "还款方式",
                "平台服务费率（%）",
                "风险保证金费率（%）"
            };

            var projectInfoValue = new object[]
            {
                p.dt_article_category.title,
                Utils.GetAgp2pEnumDes((Agp2pEnums.LoanTypeEnum)p.type),
                p.title,
                p.no,
                p.contract_no,
                p.financing_amount.ToString("c"),
                p.profit_rate_year.ToString("n1"),
                p.repayment_term_span_count + p.GetProjectTermSpanEnumDesc(),
                p.GetProjectRepaymentTypeDesc(),
                p.loan_fee_rate?.ToString("n4"),
                p.bond_fee_rate?.ToString("n4")
            };

            var loanerInfoField = new[]
            {
                "姓名",
                "性别",
                "电话",
                "身份证号码",
                "电子邮件",
                "年龄",
                "籍贯",
                "工作职务",
                "工作地点",
                "工作单位",
                "学历",
                "婚姻状态",
                "收入",
                "涉诉情况"
            };

            var l = p.li_risks.li_loaners;
            var loanerInfoValue = new object[]
            {
                l.dt_users.real_name,
                l.dt_users.sex,
                l.dt_users.mobile,
                l.dt_users.id_card_number,
                l.dt_users.email,
                l.age,
                l.native_place,
                l.job,
                l.working_at,
                l.working_company,
                l.educational_background,
                Utils.GetAgp2pEnumDes((Agp2pEnums.MaritalStatusEnum) l.marital_status),
                l.income,
                l.lawsuit,
            };

            var c = l.li_loaner_companies;
            IEnumerable<string> companyInfoField = Enumerable.Empty<string>();
            IEnumerable<object> companyInfoValue = Enumerable.Empty<string>();

            if (c != null)
            {
                companyInfoField = new[]
                {
                    "企业全称",
                    "企业法人",
                    "成立时间",
                    "注册资本",
                    "营业执照号",
                    "机构代码号",
                    "所属行业",
                    "经营范围",
                    "经营状态",
                    "涉诉情况",
                    "地址",
                    "备注",
                };

                companyInfoValue = new object[]
                {
                    c.name,
                    c.manager,
                    c.setup_time.ToString("yyyy/MM/dd"),
                    c.registered_capital,
                    c.business_license_no,
                    c.organization_no,
                    c.business_belong,
                    c.business_scope,
                    c.business_status,
                    c.business_lawsuit,
                    c.address,
                    c.remark
                };
            }
            var m = p.li_risks.li_risk_mortgage.Select(rm => rm.li_mortgages).SingleOrDefault();

            IEnumerable<string> mortgageInfoField = Enumerable.Empty<string>();
            IEnumerable<object> mortgageInfoValue = Enumerable.Empty<object>();
            if (m != null)
            {
                var schemeObj = (JObject)JsonConvert.DeserializeObject(m.li_mortgage_types.scheme);
                var kv = (JObject)JsonConvert.DeserializeObject(m.properties);

                var properties = schemeObj.Cast<KeyValuePair<string, JToken>>()
                    .Select(pair => new Tuple<string, string>(pair.Value.ToString(), kv[pair.Key]?.ToString() ?? "")).ToList();

                mortgageInfoField = new[]
                {
                    "类别",
                    "抵押物估价",
                    "备注"
                }.Concat(properties.Select(pr => pr.Item1));

                mortgageInfoValue = new object[]
                {
                    m.li_mortgage_types.name,
                    m.valuation,
                    m.remark
                }.Concat(properties.Select(pr => pr.Item2));
            }

            var riskInfoField = new[]
            {
                "债权人",
                "债权人描述",
                "借款描述",
                "借款用途",
                "还款来源",
                "担保机构",
                "风险描述",
            };

            var cre = p.li_risks.li_creditors;
            var riskInfoValue = new[]
            {
                cre?.dt_users?.real_name ?? "",
                "",
                p.li_risks.loaner_content,
                p.li_risks.loan_usage,
                p.li_risks.source_of_repayment,
                p.li_risks.li_guarantors?.name,
                Utils.DropHTML(p.li_risks.risk_content),
            };

            var repaymentPlanField = new[]
            {
                "期次",
                "还款日",
                "应还本息",
                "应还本金",
                "应还利息",
            };

            var repaymentTasks = p.li_repayment_tasks.Where(t => t.status != (int) Agp2pEnums.RepaymentStatusEnum.Invalid).ToList();
            var repaymentPlanFieldAll = Enumerable.Repeat(repaymentPlanField, repaymentTasks.Count).SelectMany(s => s);
            var repaymentPlanValueAll = repaymentTasks.Select(t => new[]
            {
                $"{t.term}/{repaymentTasks.Count}",
                t.should_repay_time.ToString("yyyy/MM/dd"),
                (t.repay_principal + t.repay_interest).ToString("c"),
                t.repay_principal.ToString("c"),
                t.repay_interest.ToString("c"),
            }).SelectMany(s => s);

            var partTitle = new[] {"项目基本信息", "借款人信息", "借款企业信息", "抵押物信息", "风控信息", "还款计划"};
            var dataPair = new[]
            {
                new Tuple<IEnumerable<string>, IEnumerable<object>>(projectInfoField, projectInfoValue), 
                new Tuple<IEnumerable<string>, IEnumerable<object>>(loanerInfoField, loanerInfoValue),
                new Tuple<IEnumerable<string>, IEnumerable<object>>(companyInfoField, companyInfoValue),
                new Tuple<IEnumerable<string>, IEnumerable<object>>(mortgageInfoField, mortgageInfoValue),
                new Tuple<IEnumerable<string>, IEnumerable<object>>(riskInfoField, riskInfoValue),
                new Tuple<IEnumerable<string>, IEnumerable<object>>(repaymentPlanFieldAll, repaymentPlanValueAll),
            }; 

            var titles = new[] { "序号", "目录", "数据"};

            Utils.ExportXls("项目信息", ws =>
            {
                Utils.ForEach(titles.Zip(Utils.Infinite(), (s, i) => new { s, i }), t => { ws.Cell(1, t.i + 1).Value = t.s; });

                int index = 1;

                ws.Column("C").Style.Alignment.WrapText = true;
                ws.Column("A").Width = 5;
                ws.Column("B").Width = 25;
                ws.Column("C").Width = 30;

                Enumerable.Range(0, partTitle.Count())
                    .Where(i => dataPair[i].Item2.Any())
                    .Select(i => new { Title = partTitle[i], Zipped = dataPair[i].Item1.Zip(dataPair[i].Item2.Select(v => v?.ToString() ?? ""), (Name, Value) => new {Name, Value}) })
                    .Each(tri =>
                    {
                        var startPoint = ws.LastRowUsed().RowBelow().Cell("A");
                        startPoint.Value = tri.Title;
                        startPoint.CellBelow().Value = Utils.Infinite(index).Zip(tri.Zipped, (i, z) => new {index = i, name = z.Name, value = z.Value});

                        ws.Range(startPoint, startPoint.CellRight().CellRight()).Merge().AddToNamed("Titles");
                        index += tri.Zipped.Count();
                    });

                var titlesStyle = ws.Workbook.Style;
                titlesStyle.Font.Bold = true;
                titlesStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titlesStyle.Fill.BackgroundColor = XLColor.Cyan;
                ws.Workbook.NamedRanges.NamedRange("Titles").Ranges.Style = titlesStyle;

                titlesStyle.Fill.BackgroundColor = XLColor.Transparent;
                ws.Range("A1", "C1").Style = titlesStyle;

            }, Response);
        }
    }

}