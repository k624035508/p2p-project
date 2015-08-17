using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.project
{
    public partial class project_edit : Web.UI.ManagePage
    {
        protected string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        protected string channel_name = string.Empty;
        protected int channel_id;
        protected int id = 0;
        protected string page_name = string.Empty;

        protected int project_status = 0;//项目状态
        protected int risk_id = 0;//风控信息
        protected decimal project_parent_amount = 0;//父项目总金额（拆标使用）
        protected li_loaners loaner = new li_loaners();//借款人
        protected List<li_mortgages> mortgages = new List<li_mortgages>();//标的物

        protected Lip2pDataContext context = new Lip2pDataContext();

        //页面初始化事件
        public virtual void Page_Init(object sernder, EventArgs e)
        {
            this.channel_id = DTRequest.GetQueryInt("channel_id");
        }

        //页面加载事件
        public virtual void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            risk_id = DTRequest.GetQueryInt("risk_id");

            if (this.channel_id == 0)
            {
                JscriptMsg("频道参数不正确！", "back", "Error");
                return;
            }
            this.channel_name = new BLL.channel().GetChannelName(this.channel_id); //取得频道名称

            li_projects project = null;
            if (!string.IsNullOrEmpty(_action) && (_action == DTEnums.ActionEnum.Edit.ToString() || _action == DTEnums.ActionEnum.Copy.ToString()))
            {
                this.action = _action;//修改类型
                this.id = DTRequest.GetQueryInt("id");
                if (this.id == 0)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                project = context.li_projects.FirstOrDefault(q => q.id == this.id);
                if (project == null)
                {
                    JscriptMsg("项目不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("project_" + page_name, DTEnums.ActionEnum.View.ToString()); //检查权限                
                TreeBind(this.channel_id); //绑定类别
                InitCreditorDDL();

                if (action == DTEnums.ActionEnum.Edit.ToString() || action == DTEnums.ActionEnum.Copy.ToString()) //修改
                {
                    ShowInfo(project);
                }
            }
        }

        #region 项目类别、债权人初始化
        /// <summary>
        /// 绑定类别
        /// </summary>
        /// <param name="ddlCategoryId"></param>
        /// <param name="_channel_id"></param>
        private void TreeBind(int _channel_id)
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
        /// 初始化债权人
        /// </summary>
        protected void InitCreditorDDL()
        {
            var context = new Lip2pDataContext();
            this.ddlCreditor.Items.Clear();
            var dtCreditor = context.li_creditors.OrderByDescending(q => q.last_update_time).ToList();
            foreach (var creditor in dtCreditor)
            {
                ddlCreditor.Items.Add(new ListItem(creditor.real_name, creditor.user_id.ToString()));
            }
        }        
        #endregion

        #region 显示项目信息

        public virtual void ShowInfo(li_projects _project)
        {
            ddlCategoryId.SelectedValue = _project.category_id.ToString();
            
            txtSeoTitle.Text = _project.seo_title;
            txtSeoKeywords.Text = _project.seo_keywords;
            txtSeoDescription.Text = _project.seo_description;
            txtSortId.Text = _project.sort_id.ToString();
            txtClick.Text = _project.click.ToString();
            txtImgUrl.Text = _project.img_url;

            txtTitle.Text = _project.title;
            txt_project_amount.Text = _project.financing_amount.ToString();
            txt_project_no.Text = _project.no.ToString();
            txt_project_repayment_number.Text = _project.repayment_term_span_count.ToString();
            txt_project_repayment_term.SelectedValue = _project.repayment_term_span.ToString();
            txt_project_repayment_type.Text = _project.repayment_type.ToString();
            txtLoanAgreementNo.Text = _project.loan_agreement_no;

            if (txt_project_profit_rate != null && _project.profit_rate_year > 0)
                txt_project_profit_rate.Text = _project.profit_rate_year.ToString();
            if (txtPublishTime != null && _project.publish_time != null)
                txtPublishTime.Text = ((DateTime)_project.publish_time).ToString("yyyy-MM-dd HH:mm:ss");

            if (action != DTEnums.ActionEnum.Copy.ToString())
                txtAddTime.Text = _project.add_time.ToString("yyyy-MM-dd HH:mm:ss");
            else
                txt_project_amount.Text = "";

            project_status = _project.status;
            project_parent_amount = _project.financing_amount;            

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
                SetRiskModel(risk);
                //查询债权人
                //if (project_status >= (int)Lip2pEnums.ProjectStatusEnum.FengShen)
                //{
                    if (this.tab_item_risks != null)
                        this.tab_item_risks.Visible = true;
                    if (this.div_risks_info != null)
                        this.div_risks_info.Visible = true;

                    var creditor = context.li_creditors.FirstOrDefault(c => c.user_id == risk.creditor);
                    //风控信息
                    if (creditor != null)
                    {                       
                        ddlCreditor.SelectedValue = creditor.user_id.ToString();
                        txtCreditorContent.Text = risk.creditor_content;
                        txtLoanerContent.Text = risk.loaner_content;
                        txtRiskContent.Value = risk.risk_content;
                    }
                    // 加载相册
                    rptMortgageContracts.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Lip2pEnums.AlbumTypeEnum.MortgageContract);
                    rptMortgageContracts.DataBind();
                    rptLienCertificates.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Lip2pEnums.AlbumTypeEnum.LienCertificate);
                    rptLienCertificates.DataBind();
                    rptLoanAgreement.DataSource = risk.li_albums.Where(a => a.risk == risk.id && a.type == (int)Lip2pEnums.AlbumTypeEnum.LoanAgreement);
                    rptLoanAgreement.DataBind();                    
                //}

            }         
            
        }
        #endregion

        #region 增加、修改操作=================================

        protected void SetProjectModel(li_projects project)
        {            
            //基础信息            
            project.seo_title = txtSeoTitle.Text.Trim();
            project.seo_keywords = txtSeoKeywords.Text.Trim();
            project.seo_description = txtSeoDescription.Text.Trim();
            project.sort_id = Utils.StrToInt(txtSortId.Text.Trim(), 99);
            project.click = int.Parse(txtClick.Text.Trim());
            project.img_url = txtImgUrl.Text.Trim();
            project.add_time = Utils.StrToDateTime(txtAddTime.Text.Trim());
            project.user_name = GetAdminInfo().user_name; //获得当前登录用户名
            project.update_time = DateTime.Now;
            //项目信息
            project.category_id = Utils.StrToInt(ddlCategoryId.SelectedValue, 0);
            project.title = txtTitle.Text.Trim();
            project.no = txt_project_no.Text.Trim();
            project.financing_amount = Utils.StrToInt(txt_project_amount.Text.Trim(), 0);
            project.repayment_term_span_count = Utils.StrToInt(txt_project_repayment_number.Text.Trim(), 0);
            project.repayment_term_span = Utils.StrToInt(txt_project_repayment_term.SelectedValue, 20);
            project.repayment_type = Utils.StrToInt(txt_project_repayment_type.SelectedValue, 10);
            project.loan_agreement_no = txtLoanAgreementNo.Text.Trim();
            if (txt_project_profit_rate != null)
                project.profit_rate_year = Utils.StrToInt(txt_project_profit_rate.Text.Trim(), 0);
            if (txtPublishTime != null)
                project.publish_time = Utils.StrToDateTime(txtPublishTime.Text.Trim());
        }

        protected void SetRiskModel(li_risks risk)
        {
            if (risk != null)
            {
                this.risk_id = risk.id;
                if (risk.li_loaners != null)
                {
                    loaner = risk.li_loaners;
                    if(loaner.dt_users == null) loaner.dt_users = new dt_users();
                }

                //查询风控信息下面的标的物
                mortgages = (from rm in context.li_risk_mortgage
                             from m in context.li_mortgages
                             where rm.risk == risk.id && rm.mortgage == m.id
                             select m).ToList();
            }
        }

        private bool DoAdd()
        {
            bool result = false;
            var project = new li_projects();
            //新增风控信息，并加入风控信息id
            var risk = context.li_risks.FirstOrDefault(r => r.id == risk_id);
            if (risk != null)
            {
                project.risk_id = risk.id;
                //风控信息赋值
                risk.creditor = Utils.StrToInt(ddlCreditor.SelectedValue, 0);
                risk.creditor_content = txtCreditorContent.Text;
                risk.loaner_content = txtLoanerContent.Text;
                risk.risk_content = txtRiskContent.Value;

                LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.LoanAgreement, 0);
                LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.MortgageContract, 1);
                LoadAlbum(risk, Lip2pEnums.AlbumTypeEnum.LienCertificate, 2);
            }
            else
            {
                JscriptMsg("标的信息不完善！", "back", "Info");
                return false;
            }
            SetProjectModel(project);
                        
            //修改项目状态为立项
            project.status = (int)Lip2pEnums.ProjectStatusEnum.LiXiang;
            context.li_projects.InsertOnSubmit(project);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加项目成功！"); //记录日志
                result = true;
            }
            catch (Exception)
            {
                return false;
            }

            return result;
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
        private bool DoEdit(int _id)
        {
            bool result = false;
            var project = context.li_projects.FirstOrDefault(q => q.id == this.id);

            if (project == null)
            {
                JscriptMsg("信息不存在或已被删除！", "back", "Error");
                return false;
            }
            //项目信息赋值
            SetProjectModel(project);
            SetRiskModel(project.li_risks);

            //项目状态修改
            if (project.status < (int)Lip2pEnums.ProjectStatusEnum.FengShen)
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
            }
            project.status += 10;

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改项目成功！"); //记录日志
                result = true;
            }
            catch (Exception)
            {
                return false;
            }
            return result;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("project_" + page_name, DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(this.id))
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                //JscriptMsg("修改信息成功！", String.Format("project_edit_{0}.aspx?action={1}&channel_id={2}&id={3}", page_name,
                //    DTEnums.ActionEnum.Edit, this.channel_id, this.id), "Success");
                JscriptMsg("修改信息成功！", String.Format("project_list_{0}.aspx?channel_id={1}", page_name, this.channel_id), "Success");
            }
            else //添加
            {
                ChkAdminLevel("project_" + page_name, DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误啦！", "", "Error");
                    return;
                }
                JscriptMsg("添加信息成功！", String.Format("project_list_{0}.aspx?channel_id={1}", page_name, this.channel_id), "Success");
            }
        }
        #endregion

    }
}