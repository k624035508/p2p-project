using Lip2p.Common;
using Lip2p.Linq2SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Lip2p.Web.admin.project
{
    public partial class loan_apply_edit : Web.UI.ManagePage
    {
        protected string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        protected int channel_id;
        protected int project_id = 0;
        protected int risk_id = 0;
        protected int select_tab_index = 0; //刷新前选择的tab

        protected Lip2pDataContext context = new Lip2pDataContext();

        //页面初始化事件
        public virtual void Page_Init(object sernder, EventArgs e)
        {
            ChkAdminLevel("loan_apply_edit", DTEnums.ActionEnum.View.ToString()); //检查权限      
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
            if (!Page.IsPostBack)
            {
                CategoryDDLBind(this.channel_id);
                CreditorDDLBind();
                LoanerDDLBind();                

                if (!string.IsNullOrEmpty(action) && action == DTEnums.ActionEnum.Edit.ToString())
                {
                    this.project_id = DTRequest.GetQueryInt("id");
                    if (this.project_id == 0)
                    {
                        JscriptMsg("传输参数不正确！", "back", "Error");
                        return;
                    }
                    li_projects project = context.li_projects.FirstOrDefault(q => q.id == this.project_id);
                    if (project == null)
                    {
                        JscriptMsg("项目不存在或已被删除！", "back", "Error");
                        return;
                    }
                    ShowInfo(project);
                }
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

        /// <summary>
        /// 初始化借款人
        /// </summary>
        protected void LoanerDDLBind()
        {
            ddlLoaner.Items.Clear();
            ddlLoaner.Items.Add(new ListItem("请选择借款人...", ""));
            var models =
                context.li_loaners.OrderByDescending(l => l.last_update_time)
                    .Select(l => new ListItem(l.dt_users.real_name, l.id.ToString())).ToArray();
            ddlLoaner.Items.AddRange(models);
        }

        /// <summary>
        /// 初始化债权人
        /// </summary>
        protected void CreditorDDLBind()
        {
            ddlCreditor.Items.Clear();
            ddlCreditor.Items.Add(new ListItem("请选择债权人...", ""));
            var dtCreditor = context.li_creditors.OrderByDescending(q => q.last_update_time)
                .Select(c => new ListItem(c.real_name, c.user_id.ToString())).ToArray();
            ddlCreditor.Items.AddRange(dtCreditor);
        }

        #endregion

        #region 显示项目信息

        public virtual void ShowInfo(li_projects _project)
        {
            ddlCategoryId.SelectedValue = _project.category_id.ToString();//项目类别
            rbl_project_type.SelectedValue = _project.type.ToString();//借款主体

            txtSeoTitle.Text = _project.seo_title;
            txtSeoKeywords.Text = _project.seo_keywords;
            txtSeoDescription.Text = _project.seo_description;
            txtSortId.Text = _project.sort_id.ToString();
            txtClick.Text = _project.click.ToString();
            txtImgUrl.Text = _project.img_url;
            txtTitle.Text = _project.title;
            txt_project_no.Text = _project.no.ToString();
            txt_project_amount.Text = _project.financing_amount.ToString();//借款金额            
            txt_project_repayment_number.Text = _project.repayment_term_span_count.ToString();//借款期限
            txt_project_repayment_term.SelectedValue = _project.repayment_term_span.ToString();//借款期限单位
            txt_project_repayment_type.Text = _project.repayment_type.ToString();//还款方式
            txt_project_profit_rate.Text = _project.profit_rate_year.ToString();//年化利率
            txtAddTime.Text = _project.add_time.ToString("yyyy-MM-dd HH:mm:ss");//申请时间

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

                //债权人信息
                ddlCreditor.SelectedValue = risk.creditor.ToString();
                txtCreditorContent.Text = risk.creditor_content;
                txtLoanerContent.Text = risk.loaner_content;
                txtRiskContent.Value = risk.risk_content;
                                
                // 加载相册
                rptMortgageContracts.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Lip2pEnums.AlbumTypeEnum.MortgageContract);
                rptMortgageContracts.DataBind();
                rptLienCertificates.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Lip2pEnums.AlbumTypeEnum.LienCertificate);
                rptLienCertificates.DataBind();
                rptLoanAgreement.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Lip2pEnums.AlbumTypeEnum.LoanAgreement);
                rptLoanAgreement.DataBind();
            }

        }
        #endregion

        #region 增加、修改操作=================================

        protected void SetProjectModel(li_projects project)
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
            project.profit_rate_year = Utils.StrToInt(txt_project_profit_rate.Text.Trim(), 0);            
        }

        #region 相册
        protected void LoadAlbum(li_risks model, Lip2pEnums.AlbumTypeEnum type, int splittedIndex)
        {
            string[] albumArr = GetSplittedFormValue("hid_photo_name", splittedIndex).ToArray();
            string[] remarkArr = GetSplittedFormValue("hid_photo_remark", splittedIndex).ToArray();
            context.li_albums.DeleteAllOnSubmit(context.li_albums.Where(a => a.risk == model.id && a.type == (int)type));
            if (albumArr != null)
            {
                var preAdd = albumArr.Zip(remarkArr, (album, remark) =>
                {
                    var albumSplit = album.Split('|');
                    return new li_albums
                    {
                        original_path = albumSplit[1],
                        thumb_path = albumSplit[2],
                        remark = remark,
                        add_time = DateTime.Now,
                        li_risks = model,
                        type = (byte)type
                    };
                });
                context.li_albums.InsertAllOnSubmit(preAdd);
            }
        }

        /// <summary>
        /// 拆分相册，由于从 Request.Form 读到的内容是全部相册合在一起的，
        /// 所以需要加个隐藏字段将他们隔开，再在后台拆分开
        /// </summary>
        /// <param name="inputName"></param>
        /// <param name="splittedIndex"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSplittedFormValue(string inputName, int splittedIndex)
        {
            var strings = Request.Form.GetValues(inputName);
            var currentRangeIndex = 0;
            foreach (var t in strings) // 有 splitter 的存在，不为 null
            {
                if (t == "splitter")
                {
                    if (currentRangeIndex < splittedIndex)
                    {
                        currentRangeIndex += 1;
                    }
                    else
                    {
                        yield break;
                    }
                }
                else if (currentRangeIndex == splittedIndex)
                {
                    yield return t;
                }
            }
        }

        #endregion

        private bool DoAdd()
        {
            var project = new li_projects();
            //新增风控信息，并加入风控信息id
            var risk = new li_risks();
            project.li_risks = risk;
            //风控信息赋值
            risk.loaner = Utils.StrToInt(ddlLoaner.SelectedValue, 0);
            risk.creditor = Utils.StrToInt(ddlCreditor.SelectedValue, 0);
            risk.creditor_content = txtCreditorContent.Text;
            risk.loaner_content = txtLoanerContent.Text;
            risk.risk_content = txtRiskContent.Value;
            risk.last_update_time = DateTime.Now;
            //相关图片资料
            LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.LoanAgreement, 0);
            LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.MortgageContract, 1);
            LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.LienCertificate, 2);
            //项目信息
            SetProjectModel(project);
            //修改项目状态为待初审
            project.status = (int)Lip2pEnums.ProjectStatusEnum.FinancingApplicationChecking;
            // 保存抵押物信息
            BindMortgages(risk);
            context.li_risks.InsertOnSubmit(risk);
            context.li_projects.InsertOnSubmit(project);           
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加项目成功！"); //记录日志                
            }
            catch (Exception ex)
            {
                JscriptMsg(ex.Message, "back", "Error");
                return false;
            }

            return true;
        }

        private bool DoEdit(int _id)
        {
            var project = context.li_projects.FirstOrDefault(q => q.id == this.project_id);

            if (project == null)
            {
                JscriptMsg("信息不存在或已被删除！", "back", "Error");
                return false;
            }
            //项目信息赋值
            SetProjectModel(project);
            // 保存抵押物信息
            BindMortgages(project.li_risks);
            try
            {
                context.SubmitChanges();
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
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("loan_apply_edit", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(this.project_id))
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("修改信息成功！", String.Format("loan_apply.aspx?channel_id={0}", this.channel_id), "Success");
            }
            else //添加
            {
                ChkAdminLevel("loan_apply_edit", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("添加信息成功！", String.Format("loan_apply.aspx?channel_id={0}", this.channel_id), "Success");
            }
        }
        #endregion

        #region 选择借款人
        public class MortgageItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public string typeName { get; set; }
            public decimal valuation { get; set; }
            public byte status { get; set; }
            public bool check { get; set; }
            public bool enable { get; set; }
        }

        protected string QueryUsingProject(int mortgageId)
        {
            var mortgage = context.li_mortgages.Single(m => m.id == mortgageId);
            var projs = mortgage.li_risk_mortgage.Select(rm => rm.li_risks)
                .SelectMany(r => r.li_projects.Where(p => p.status < (int)Lip2pEnums.ProjectStatusEnum.RepayCompleteIntime)).ToList();
            var projectNames = projs.Select(p => p.title).ToList();
            var riskCount = projs.GroupBy(p => p.risk_id).Count();
            return string.Join(",", projectNames) + (riskCount <= 1 ? "" : " 警告：此抵押物被多个风控信息关联");
        }

        protected void LoadMortgageList(int loaner_id, int risk_id)
        {
            // status: 抵押物是否被其他风控信息使用, check: 抵押物是否被当前风控信息使用
            // 未关联风控信息的抵押物
            var allMortgages =
                from m in context.li_mortgages
                where m.owner == loaner_id
                orderby m.last_update_time descending
                select new MortgageItem
                {
                    id = m.id,
                    name = m.name,
                    typeName = m.li_mortgage_types.name,
                    valuation = m.valuation,
                    status = (byte)Lip2pEnums.MortgageStatusEnum.Mortgageable,
                    check = false,
                    enable = true
                };
            // 已关联风控信息的抵押物，如果是别的风控信息，禁用；否则可设置关联
            var mortgageInUse =
                (from m in context.li_mortgages
                 from rm in context.li_risk_mortgage
                 from r in context.li_risks
                 where
                     loaner_id == m.owner && m.id == rm.mortgage && rm.risk == r.id
                     && r.id == risk_id // 新加的条件，仅显示当前的风控信息相关的绑定，暂时不考虑项目的问题
                                        /*&& r.li_projects.Any(
                                            p =>
                                                p.status != (int) Lip2pEnums.ProjectStatusEnum.WanCheng)*/ // 有项目未完成，其他项目就不可以用此项目正在使用的抵押物
                 select new MortgageItem
                 {
                     id = m.id,
                     name = m.name,
                     typeName = m.li_mortgage_types.name,
                     valuation = m.valuation,
                     status = (byte)Lip2pEnums.MortgageStatusEnum.Mortgaged,
                     check = r.id == risk_id,
                     enable = r.id == risk_id
                 }).GroupBy(m => m.id).ToDictionary(m => m.Key, m => m.First()); // 旧数据中可能会有一个抵押物多次绑定多个未完成的风控信息的情况，这里只取第一个

            rptList.DataSource = allMortgages.Select(m => mortgageInUse.ContainsKey(m.id) ? mortgageInUse[m.id] : m);
            rptList.DataBind();
        }

        private void BindMortgages(li_risks risk)
        {
            var selectedLoaner = Convert.ToInt32(ddlLoaner.SelectedValue);
            if (risk.loaner != selectedLoaner) // 更换借款人后，之前的抵押物绑定需要全部删除
            {
                var dtRiskMortgages = context.li_risk_mortgage.Where(rm => rm.risk == risk_id).ToList();
                context.li_risk_mortgage.DeleteAllOnSubmit(dtRiskMortgages);

                risk.loaner = selectedLoaner;
                risk.last_update_time = DateTime.Now;
            }

            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int mortgageId = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (!cb.Enabled) continue;
                var riskMortgage = context.li_risk_mortgage.FirstOrDefault(rm => rm.risk == risk_id && rm.mortgage == mortgageId);
                if (cb.Checked) // 绑定抵押物
                {
                    if (riskMortgage != null) continue;
                    riskMortgage = new li_risk_mortgage
                    {
                        mortgage = mortgageId,
                        last_update_time = DateTime.Now,
                        li_risks = risk
                    };
                    context.li_risk_mortgage.InsertOnSubmit(riskMortgage);
                }
                else // 解除绑定
                {
                    if (riskMortgage == null) continue;
                    context.li_risk_mortgage.DeleteOnSubmit(riskMortgage);
                }
            }
        }

        protected void ddlLoaner_SelectedIndexChanged(object sender, EventArgs e)
        {
            select_tab_index = 1;
            var loaner = context.li_loaners.SingleOrDefault(l => l.id == Utils.StrToInt(ddlLoaner.SelectedValue, 0));

            sp_loaner_name.InnerText = loaner.dt_users.real_name;
            sp_loaner_gender.InnerText = loaner.dt_users.sex;
            sp_loaner_job.InnerText = loaner.job;
            sp_loaner_working_at.InnerText = loaner.working_at;
            sp_loaner_tel.InnerText = loaner.dt_users.telphone;
            sp_loaner_id_card_number.InnerText = loaner.dt_users.id_card_number;

            LoadMortgageList(Utils.StrToInt(ddlLoaner.SelectedValue, 0), this.risk_id);
        } 
        #endregion

        protected void rbl_project_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            select_tab_index = 0;
        }


    }

}