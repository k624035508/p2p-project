using Agp2p.Common;
using Agp2p.Linq2SQL;
using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Core;
using Agp2p.Core.AutoLogic;

namespace Agp2p.Web.admin.project
{
    public partial class loan_apply_detail : Web.UI.ManagePage
    {
        protected string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        protected int channel_id;
        protected int project_id = 0;
        protected int risk_id = 0;
        protected int select_tab_index = 0; //刷新前选择的tab
        protected bool save_only = false;

        protected BLL.loan Loan;

        protected Agp2pDataContext LqContext = new Agp2pDataContext();

        //页面初始化事件
        public virtual void Page_Init(object sernder, EventArgs e)
        {
            ChkAdminLevel("loan_apply", DTEnums.ActionEnum.View.ToString()); //检查权限      
            this.channel_id = DTRequest.GetQueryInt("channel_id");
            if (this.channel_id == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
        }

        //页面加载事件
        public virtual void Page_Load(object sender, EventArgs e)
        {
            this.action = DTRequest.GetQueryString("action");
            this.project_id = DTRequest.GetQueryInt("id");
            this.Loan = new BLL.loan(LqContext);

            if (!Page.IsPostBack)
            {
                CategoryDDLBind(this.channel_id);
                BindControls();

                if (!string.IsNullOrEmpty(action) && (action == DTEnums.ActionEnum.Edit.ToString() || action == DTEnums.ActionEnum.Copy.ToString()))
                {
                    if (this.project_id == 0)
                    {
                        JscriptMsg("传输参数不正确！", "back", "Error");
                        return;
                    }
                    LqContext = new Agp2pDataContext();
                    li_projects project = LqContext.li_projects.FirstOrDefault(q => q.id == this.project_id);
                    if (project == null)
                    {
                        JscriptMsg("项目不存在或已被删除！", "back", "Error");
                        return;
                    }
                    ShowInfo(project);
                }
                SetLoanType();
            }
        }

        #region 项目类别、借款人、债权人初始化
        /// <summary>
        /// 绑定类别
        /// </summary>
        /// <param name="ddlCategoryId"></param>
        /// <param name="_channel_id"></param>
        private void CategoryDDLBind(int _channel_id)
        {
            BLL.article_category bll = new BLL.article_category();
            DataTable dt = bll.GetList(0, _channel_id);

            ddlCategoryId.Items.Clear();
            ddlCategoryId.Items.Add(new ListItem("请选择类别...", ""));
            foreach (DataRow dr in dt.Rows)
            {
                string Id = dr["id"].ToString();
                int ClassLayer = int.Parse(dr["class_layer"].ToString());
                string Title = dr["title"].ToString().Trim();

                if (ClassLayer == 1)
                {
                    ddlCategoryId.Items.Add(new ListItem(Title, Id));
                }
                else
                {
                    Title = "├ " + Title;
                    Title = Utils.StringOfChar(ClassLayer - 1, "　") + Title;
                    ddlCategoryId.Items.Add(new ListItem(Title, Id));
                }
            }
        }

        protected void BindControls()
        {
            //初始化借款人
            ddlLoaner.Items.Clear();
            ddlLoaner.Items.Add(new ListItem("请选择借款人...", ""));
            ddlLoaner.Items.AddRange(LqContext.li_loaners.Where(l => l.status == (int)Agp2pEnums.LoanerStatusEnum.Normal).OrderByDescending(l => l.last_update_time)
                    .Select(l => new ListItem(l.dt_users.real_name, l.id.ToString())).ToArray());

            //初始化债权人
            ddlCreditor.Items.Clear();
            ddlCreditor.Items.Add(new ListItem("请选择债权人...", ""));
            ddlCreditor.Items.AddRange(LqContext.li_creditors.OrderByDescending(q => q.last_update_time)
                .Select(c => new ListItem(c.dt_users.real_name, c.user_id.ToString())).ToArray());

            //初始化借款人
            ddl_guarantor.Items.Clear();
            ddl_guarantor.Items.Add(new ListItem("请选择担保机构...", ""));
            ddl_guarantor.Items.AddRange(LqContext.li_guarantors.OrderBy(l => l.id)
                    .Select(l => new ListItem(l.name, l.id.ToString())).ToArray());

        }

        #endregion

        /// <summary>
        /// 显示项目信息
        /// </summary>
        /// <param name="_project"></param>
        public virtual void ShowInfo(li_projects _project)
        {
            ddlCategoryId.SelectedValue = _project.category_id.ToString();//项目类别
            BindDDlCategory();
            rbl_project_type.SelectedValue = _project.type.ToString();//借款主体

            txtSeoTitle.Text = _project.seo_title;
            txtSeoKeywords.Text = _project.seo_keywords;
            txtSeoDescription.Text = _project.seo_description;
            txtSortId.Text = _project.sort_id.ToString();
            txtClick.Text = _project.click.ToString();
            txtImgUrl.Text = _project.img_url;
            txtTitle.Text = action == DTEnums.ActionEnum.Copy.ToString() ? "" : _project.title;
            txt_project_no.Text = _project.no;
            txt_project_amount.Text = _project.financing_amount.ToString();//借款金额            
            txt_project_repayment_number.Text = _project.repayment_term_span_count.ToString();//借款期限
            txt_project_repayment_term.SelectedValue = _project.repayment_term_span.ToString();//借款期限单位
            txt_project_repayment_type.Text = _project.repayment_type.ToString();//还款方式
            txt_project_profit_rate.Text = _project.profit_rate_year.ToString("N1");//年化利率
            txtAddTime.Text = action == DTEnums.ActionEnum.Copy.ToString() ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : _project.add_time.ToString("yyyy-MM-dd HH:mm:ss");//申请时间
            txt_bond_fee_rate.Text = (_project.bond_fee_rate * 100).ToString();
            txt_loan_fee_rate.Text = (_project.loan_fee_rate * 100).ToString();
            txt_contact_no.Text = _project.contract_no;

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
                this.risk_id = risk.id;
                //借款人信息
                ddlLoaner.SelectedValue = risk.loaner.ToString();
                ShowLoanerInfo(risk.li_loaners);
                //债权人信息
                if (rbl_project_type.SelectedValue == ((int)Agp2pEnums.LoanTypeEnum.Creditor).ToString())
                {
                    ddlCreditor.SelectedValue = risk.creditor.ToString();
                    txtCreditorContent.Text = risk.creditor_content;
                }
                if (risk.guarantor_id != null)
                    ddl_guarantor.SelectedValue = risk.guarantor_id.ToString();
                //txtCreditRating.Text = risk.credit_rating;
                txtRepaymentSource.Text = risk.source_of_repayment;//还款来源
                txtLoanUse.Text = risk.loan_usage;//借款用途
                txtLoanerContent.Text = risk.loaner_content;//借款描述
                txtRiskContent.Value = risk.risk_content;//风控描述
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
        /// <param name="loaner_id"></param>
        private void ShowLoanerInfo(li_loaners loaner)
        {
            if(loaner == null) return;
            //借款人信息
            sp_loaner_name.InnerText = loaner.dt_users.real_name;
            sp_loaner_gender.InnerText = loaner.dt_users.sex;
            sp_loaner_job.InnerText = loaner.job;
            sp_loaner_working_at.InnerText = loaner.working_at;
            sp_loaner_tel.InnerText = loaner.dt_users.telphone;
            sp_loaner_id_card_number.InnerText = loaner.dt_users.id_card_number;
            //企业信息
            if (rbl_project_type.SelectedValue == ((int)Agp2pEnums.LoanTypeEnum.Company).ToString() && loaner.li_loaner_companies != null)
            {
                sp_company_name.InnerText = loaner.li_loaner_companies.name;
                sp_company_business_scope.InnerText = loaner.li_loaner_companies.business_scope;
                sp_company_business_status.InnerText = loaner.li_loaner_companies.business_status;
                sp_company_registered_capital.InnerText = loaner.li_loaner_companies.registered_capital;
                sp_company_setup_time.InnerText = loaner.li_loaner_companies.setup_time.ToString("yyyy年MM月dd日");
            }

            var allMortgages = Loan.LoadMortgageList(Utils.StrToInt(ddlLoaner.SelectedValue, 0), this.risk_id);
            rptList.DataSource = allMortgages.ToList().Where(m => m != null && m.enable);
            rptList.DataBind();
        }

        #region 增加、修改操作=================================

        /// <summary>
        /// 项目信息赋值
        /// </summary>
        /// <param name="project"></param>
        private void SetProjectModel(li_projects project)
        {
            project.seo_title = txtSeoTitle.Text.Trim();
            project.seo_keywords = txtSeoKeywords.Text.Trim();
            project.seo_description = txtSeoDescription.Text.Trim();
            project.sort_id = Utils.StrToInt(txtSortId.Text.Trim(), 99);
            project.click = Utils.StrToInt(txtClick.Text.Trim(), 0);
            project.img_url = txtImgUrl.Text.Trim();
            project.add_time = string.IsNullOrEmpty(txtAddTime.Text.Trim()) ? DateTime.Now : Utils.StrToDateTime(txtAddTime.Text.Trim());
            project.user_name = GetAdminInfo().user_name;
            project.category_id = Utils.StrToInt(ddlCategoryId.SelectedValue, 0);
            project.title = txtTitle.Text.Trim();
            project.type = Utils.StrToInt(rbl_project_type.SelectedValue, 10);
            project.no = txt_project_no.Text.Trim();
            project.financing_amount = Utils.StrToInt(txt_project_amount.Text.Trim(), 0);
            project.repayment_term_span_count = Utils.StrToInt(txt_project_repayment_number.Text.Trim(), 0);
            project.repayment_term_span = Utils.StrToInt(txt_project_repayment_term.SelectedValue, 20);
            project.repayment_type = Utils.StrToInt(txt_project_repayment_type.SelectedValue, 10);
            project.profit_rate_year = Utils.StrToDecimal(txt_project_profit_rate.Text.Trim(), 0);
            project.bond_fee_rate = Utils.StrToDecimal(txt_bond_fee_rate.Text.Trim(), 0)/100;
            project.loan_fee_rate = Utils.StrToDecimal(txt_loan_fee_rate.Text.Trim(), 0)/100;
            project.contract_no = txt_contact_no.Text.Trim();

            //提交操作则状态为待初审            
            project.status = save_only ? (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationUncommitted : (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationChecking;
        }

        /// <summary>
        /// 风控信息赋值
        /// </summary>
        /// <param name="risk"></param>
        private void SetRiskModel(li_risks risk)
        {
            risk.last_update_time = DateTime.Now;
            if (string.IsNullOrEmpty(ddlLoaner.SelectedValue)) return;

            risk.loaner = Utils.StrToInt(ddlLoaner.SelectedValue, 0);
            //债权人信息
            if (rbl_project_type.SelectedValue == ((int)Agp2pEnums.LoanTypeEnum.Creditor).ToString())
            {
                risk.creditor = Utils.StrToInt(ddlCreditor.SelectedValue, 0);
                risk.creditor_content = txtCreditorContent.Text;
            }
            risk.loaner_content = txtLoanerContent.Text;
            risk.loan_usage = txtLoanUse.Text;
            risk.source_of_repayment = txtRepaymentSource.Text;
            risk.risk_content = txtRiskContent.Value;
            if (ddl_guarantor.SelectedIndex > 0)
                risk.guarantor_id = Utils.StrToInt(ddl_guarantor.SelectedValue, 0);
            //相关图片资料
            Loan.LoadAlbum(risk, Agp2pEnums.AlbumTypeEnum.LoanAgreement, 0, Request);
            Loan.LoadAlbum(risk, Agp2pEnums.AlbumTypeEnum.MortgageContract, 1, Request);
            Loan.LoadAlbum(risk, Agp2pEnums.AlbumTypeEnum.LienCertificate, 2, Request);

        }

        private bool DoAdd()
        {
            var project = new li_projects();
            var risk = new li_risks();
            project.li_risks = risk;
            SetRiskModel(risk);
            SetProjectModel(project);
            BindMortgages(risk);
            //项目编号
            var latestProject = LqContext.li_projects.Where(p => p.category_id == project.category_id && !p.title.Equals("")).OrderByDescending(p => p.id).FirstOrDefault();
            int prjectCount = latestProject == null ? 0 : Utils.StrToInt(latestProject.title.Substring(latestProject.title.Length - 5), 0);
            project.title += new BLL.article_category().GetModel(project.category_id).call_index.ToUpper() + (prjectCount + 1).ToString("00000");

            LqContext.li_risks.InsertOnSubmit(risk);
            LqContext.li_projects.InsertOnSubmit(project);
            try
            {
                LqContext.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加项目成功！"); //记录日志                
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        private bool DoEdit(int _id)
        {
            var project = LqContext.li_projects.FirstOrDefault(q => q.id == this.project_id);
            if (project == null)
            {
                JscriptMsg("信息不存在或已被删除！", "back", "Error");
                return false;
            }
            SetRiskModel(project.li_risks);
            SetProjectModel(project);
            BindMortgages(project.li_risks);
            try
            {
                LqContext.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改项目成功！"); //记录日志
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
                sumbit();
        }

        private void sumbit() 
        {
            if (Convert.ToDecimal(txt_project_amount.Text.Trim()) <= 0)
            {
                JscriptMsg("借款金额必须大于0！", "", "Error");
                return;
            }
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("loan_apply", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(this.project_id))
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("修改信息成功！", String.Format("loan_apply.aspx?channel_id={0}", this.channel_id), "Success");
            }
            else if (action == DTEnums.ActionEnum.Copy.ToString()) //复制
            {
                txtTitle.Text = "";
                ChkAdminLevel("loan_apply", DTEnums.ActionEnum.Copy.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("复制信息成功！", String.Format("loan_apply.aspx?channel_id={0}", this.channel_id), "Success");
            }
            else //添加
            {
                ChkAdminLevel("loan_apply", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("添加信息成功！", String.Format("loan_apply.aspx?channel_id={0}", this.channel_id), "Success");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            save_only = true;
            sumbit();
        }

        /// <summary>
        /// 绑定抵押物
        /// </summary>
        /// <param name="risk"></param>
        private void BindMortgages(li_risks risk)
        {
            if (string.IsNullOrEmpty(ddlLoaner.SelectedValue)) return;
            var selectedLoaner = Convert.ToInt32(ddlLoaner.SelectedValue);
            if (risk.loaner != selectedLoaner) // 更换借款人后，之前的抵押物绑定需要全部删除
            {
                var dtRiskMortgages = LqContext.li_risk_mortgage.Where(rm => rm.risk == risk.id).ToList();
                LqContext.li_risk_mortgage.DeleteAllOnSubmit(dtRiskMortgages);

                risk.loaner = selectedLoaner;
                risk.last_update_time = DateTime.Now;
            }

            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int mortgageId = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (!cb.Enabled) continue;
                var riskMortgage = LqContext.li_risk_mortgage.FirstOrDefault(rm => rm.risk == risk.id && rm.mortgage == mortgageId);
                if (cb.Checked) // 绑定抵押物
                {
                    if (riskMortgage != null) continue;
                    riskMortgage = new li_risk_mortgage
                    {
                        mortgage = mortgageId,
                        last_update_time = DateTime.Now,
                        li_risks = risk
                    };
                    LqContext.li_risk_mortgage.InsertOnSubmit(riskMortgage);
                }
                else // 解除绑定
                {
                    if (riskMortgage == null) continue;
                    LqContext.li_risk_mortgage.DeleteOnSubmit(riskMortgage);
                }
            }
        }
        #endregion

        /// <summary>
        /// 选择借款人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlLoaner_SelectedIndexChanged(object sender, EventArgs e)
        {
            select_tab_index = 1;
            var loanId = Utils.StrToInt(ddlLoaner.SelectedValue, 0);
            if (loanId > 0)
            {
                var loaner = LqContext.li_loaners.SingleOrDefault(l => l.id == loanId);
                ShowLoanerInfo(loaner);
            }
        }

        /// <summary>
        /// 切换借款主体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rbl_project_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            select_tab_index = 0;
            SetLoanType();
        }

        /// <summary>
        /// 根据借款主体显示页面样式
        /// </summary>
        private void SetLoanType()
        {
            if (rbl_project_type.SelectedValue == ((int)Agp2pEnums.LoanTypeEnum.Company).ToString())
            {
                dl_company.Visible = true;
                dl_creditor.Visible = false;
                dl_creditor_dic.Visible = false;
            }
            else if (rbl_project_type.SelectedValue == ((int)Agp2pEnums.LoanTypeEnum.Personal).ToString())
            {
                dl_company.Visible = false;
                dl_creditor.Visible = false;
                dl_creditor_dic.Visible = false;
            }
            else
            {
                dl_company.Visible = false;
                dl_creditor.Visible = true;
                dl_creditor_dic.Visible = true;
            }
        }

        private void BindDDlCategory()
        {
            var category =
                LqContext.dt_article_category.Single(c => c.id == Utils.StrToInt(ddlCategoryId.SelectedValue, 0));
            //银票项目
            if (category != null && category.call_index.Contains("yp"))
            {
                txt_project_repayment_term.Items.Clear();
                txt_project_repayment_term.Items.Add(new ListItem("日", "30"));
                txt_project_repayment_term.SelectedIndex = 0;

                txt_project_repayment_type.Items.Clear();
                txt_project_repayment_type.Items.Add(new ListItem("到期还本付息", "30"));
                txt_project_repayment_type.SelectedIndex = 0;

                txt_bond_fee_rate.Text = (Costconfig.bond_fee_rate_bank * 100).ToString("N2");
                txt_loan_fee_rate.Text = (Costconfig.loan_fee_rate_bank * 100).ToString("N2");
            }
            //新手项目
            else if (category != null && category.call_index.Equals("newbie"))
            {
                div_risks_info.Visible = false;
                div_mortgages_info.Visible = false;
                div_loan_fee_rate.Visible = false;
                div_bond_fee_rate.Visible = false;
                div_project_profit_rate.Visible = false;
                li_mortgages.Visible = false;
                li_risk.Visible = false;

                txt_project_repayment_type.Items.Clear();
                txt_project_repayment_type.Items.Add(new ListItem("到期还本付息", "30"));
                txt_project_repayment_type.SelectedIndex = 0;

                txt_project_repayment_term.Items.Clear();
                txt_project_repayment_term.Items.Add(new ListItem("日", "30"));
                txt_project_repayment_term.SelectedIndex = 0;
            }
            //活期项目
            else if (category != null && category.call_index.Equals("huoqi"))
            {
                div_loan_fee_rate.Visible = false;
                div_bond_fee_rate.Visible = false;

                txt_project_repayment_type.Items.Clear();
                txt_project_repayment_type.Items.Add(new ListItem("每日收益", "40"));
                txt_project_repayment_type.SelectedIndex = 0;

                txt_project_repayment_term.Items.Clear();
                txt_project_repayment_term.Items.Add(new ListItem("日", "30"));
                txt_project_repayment_term.SelectedIndex = 0;
            }
            else
            {
                txt_project_repayment_term.Items.Clear();
                txt_project_repayment_term.Items.Add(new ListItem("年", "10"));
                txt_project_repayment_term.Items.Add(new ListItem("月", "20"));
                txt_project_repayment_term.Items.Add(new ListItem("日", "30"));
                txt_project_repayment_term.SelectedIndex = 1;

                txt_project_repayment_type.Items.Clear();
                txt_project_repayment_type.Items.Add(new ListItem("先息后本", "10"));
                txt_project_repayment_type.Items.Add(new ListItem("等额本息", "20"));
                txt_project_repayment_type.Items.Add(new ListItem("到期还本付息", "30"));

                txt_project_repayment_type.SelectedIndex = 0;
                txt_bond_fee_rate.Text = (Costconfig.bond_fee_rate * 100).ToString("N1");
                txt_loan_fee_rate.Text = (Costconfig.loan_fee_rate * 100).ToString("N0");
            }
        }

        protected void ddlCategoryId_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlCategoryId.SelectedValue))
            {
                div_risks_info.Visible = true;
                div_mortgages_info.Visible = true;
                div_loan_fee_rate.Visible = true;
                div_bond_fee_rate.Visible = true;
                div_project_profit_rate.Visible = true;
                li_mortgages.Visible = true;
                li_risk.Visible = true;
                BindDDlCategory();
            }
        }
    }

}