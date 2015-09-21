using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.users
{
    public partial class user_edit : UI.ManagePage
    {
        Agp2pDataContext context = new Agp2pDataContext();
        string defaultpassword = "0|0|0|0"; //默认显示密码
        protected string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        protected string pagein = string.Empty; 
        private int id = 0;
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            pagein = DTRequest.GetQueryString("action");
            pageSize = GetPageSize(8); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                id = DTRequest.GetQueryInt("id");
                ViewState["id"] = id;
                if (id == 0)
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                if (!new BLL.users().Exists(id))
                {
                    JscriptMsg("信息不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("user_list", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(ddlGroupId,id); //绑定类别
                TreeBind1(ddlServingGroup);
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(id);
                }
                if (!string.IsNullOrEmpty(_action) && _action == "Pageinviter")
                {
                    id = DTRequest.GetQueryInt("id");
                    ShowInfo(id);
                }
            }
        }

        #region 绑定类别=================================
        private void TreeBind(DropDownList dropDownList, int _id)
        {
            BLL.users bll = new BLL.users();
            Model.users model = bll.GetModel(_id);

            dropDownList.Items.Clear();
            dropDownList.Items.Add(new ListItem("请选择组别...", ""));
            // 限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();

            dropDownList.Items.AddRange(context.dt_user_groups.Where(g => g.is_lock == 0 && (!canAccessGroups.Any() || canAccessGroups.Contains(g.id)))
                .OrderByDescending(g => g.id)
                .Select(g => new ListItem(g.title, g.id.ToString()))
                .ToArray());
        }
        #endregion

        #region 绑定类别=================================
        private void TreeBind1(DropDownList dropDownList)
        {
            var groups = context.dt_user_groups.Where(g => g.is_lock != 1).OrderBy(g => g.grade).ToList();

            dropDownList.Items.Clear();
            dropDownList.Items.Add(new ListItem("请选择组别...", ""));
            dropDownList.Items.AddRange(groups.Select(g => new ListItem(g.title, g.id.ToString())).ToArray());
        }
        #endregion

        #region 赋值操作=================================
        private void ShowInfo(int _id)
        {
            BLL.users bll = new BLL.users();
            Model.users model = bll.GetModel(_id);

            ddlGroupId.SelectedValue = model.group_id.ToString();
            rblStatus.SelectedValue = model.status.ToString();
            txtUserName.Text = model.user_name;
            txtUserName.ReadOnly = true;
            txtUserName.Attributes.Remove("ajaxurl");
            if (!string.IsNullOrEmpty(model.password))
            {
                txtPassword.Attributes["value"] = txtPassword1.Attributes["value"] = defaultpassword;
            }
            txtEmail.Text = model.email;
            txtNickName.Text = model.nick_name;
            txtAvatar.Text = model.avatar;
            rblSex.SelectedValue = model.sex;
            if (model.birthday != null)
            {
                txtBirthday.Text = model.birthday.GetValueOrDefault().ToString("yyyy-M-d");
            }
            txtTelphone.Text = model.telphone;
            txtMobile.Text = model.mobile;
            txtQQ.Text = model.qq;
            txtAddress.Text = model.area.Replace(",", "") + model.address;
            //txtAmount.Text = model.amount.ToString();
            txtPoint.Text = model.point.ToString();
            //txtExp.Text = model.exp.ToString();
            lblRegTime.Text = model.reg_time.ToString();
            lblRegIP.Text = model.reg_ip;
            txtIdCard.Text = model.id_card_number;
            txtRealName.Text = model.real_name;

            //查找最近登录信息
            Model.user_login_log logModel = new BLL.user_login_log().GetLastModel(model.user_name);
            if (logModel != null)
            {
                lblLastTime.Text = logModel.login_time.ToString();
                lblLastIP.Text = logModel.login_ip;
            }
            var wallet = context.li_wallets.FirstOrDefault(w => w.user_id == _id);
            if (wallet != null)
            {
                lblIdleMoney.Text = wallet.idle_money.ToString("c");
                lblInvestingMoney.Text = wallet.investing_money.ToString("c");
                lblLockedMoney.Text = wallet.locked_money.ToString("c");
                lblProfitingMoney.Text = wallet.profiting_money.ToString("c");
                lblTotalProfit.Text = wallet.total_profit.ToString("c");
                lblTotalInvestment.Text = wallet.total_investment.ToString("c");
            }

            // 加载会员部功能，被推荐人自动归组
            var groupServer = context.li_user_group_servers.FirstOrDefault(s => s.serving_user == _id);
            ddlServingGroup.SelectedValue = groupServer == null ? "" : groupServer.group_id.ToString();

            // 查询推荐人数
            //lblTotalInvitee.Text = context.dt_users.Single(u => u.id == _id).li_invitations1.Count.ToString();

            // 加载相册
            rptIdCardPic.DataSource = context.li_albums.Where(a => a.the_user == model.id && a.type == (int)Agp2pEnums.AlbumTypeEnum.IdCard);
            rptIdCardPic.DataBind();


            //会员组信息
            IQueryable<dt_users> query = context.dt_users;
            query = query.Where(u => u.li_invitations.inviter == model.id);
            rptList.DataSource = query
                    .OrderByDescending(u => u.reg_time)
                    .ThenByDescending(u => u.id)
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToList();
            rptList.DataBind();

            // 读出借款人信息
            var loanerInfo = context.li_loaners.SingleOrDefault(l => l.user_id == model.id);
            chkIsLoaner.Checked = loanerInfo != null;
            if (loanerInfo != null)
            {
                txtAge.Text = loanerInfo.age.ToString();
                txtJob.Text = loanerInfo.job;
                txtWorkingAt.Text = loanerInfo.working_at;
                txtEducationalBackground.Text = loanerInfo.educational_background;
                rblMaritalStatus.SelectedValue = loanerInfo.marital_status.ToString();
                txtIncome.Text = loanerInfo.income;

                var loanerCompany = loanerInfo.li_loaner_companies;
                chkBindCompany.Checked = loanerCompany != null;
                if (loanerCompany != null)
                {
                    txtCompanyName.Text = loanerCompany.name;
                    txtSetupTime.Text = loanerCompany.setup_time.ToString("yyyy-MM-dd");
                    txtRegisteredCapital.Text = loanerCompany.registered_capital;
                    txtBusinessScope.Text = loanerCompany.business_scope;
                    txtBusinessStatus.Text = loanerCompany.business_status;
                    txtBusinessLawsuit.Text = loanerCompany.business_lawsuit;
                    txtIncomeYearly.Text = loanerCompany.income_yearly;
                    txtNetAssets.Text = loanerCompany.net_assets;
                    txtCompanyRemark.Text = loanerCompany.remark;
                }
            }

            //绑定页码
            this.totalCount = query.Count();
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("user_edit.aspx", "page={0}&action={1}&id={2}", "__id__", "Pageinviter", _id.ToString());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }
        #endregion

        #region 返回用户每页数量=========================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie("user_list_page_size"), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    return _pagesize;
                }
            }
            return _default_size;
        }
        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie("user_list_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("user_edit.aspx", "action={0}&id={1}", "Pageinviter", id.ToString()));
        }

        #region 增加操作=================================
        private bool DoAdd()
        {
            bool result = false;
            Model.users model = new Model.users();
            BLL.users bll = new BLL.users();

            model.group_id = int.Parse(ddlGroupId.SelectedValue);
            model.status = int.Parse(rblStatus.SelectedValue);
            //检测用户名是否重复
            if (bll.Exists(txtUserName.Text.Trim()))
            {
                return false;
            }
            model.user_name = Utils.DropHTML(txtUserName.Text.Trim());
            //获得6位的salt加密字符串
            model.salt = Utils.GetCheckCode(6);
            //以随机生成的6位字符串做为密钥加密
            model.password = DESEncrypt.Encrypt(txtPassword.Text.Trim(), model.salt);
            model.email = Utils.DropHTML(txtEmail.Text);
            model.nick_name = Utils.DropHTML(txtNickName.Text);
            model.avatar = Utils.DropHTML(txtAvatar.Text);
            model.sex = rblSex.SelectedValue;
            DateTime _birthday;
            if (DateTime.TryParse(txtBirthday.Text.Trim(), out _birthday))
            {
                model.birthday = _birthday;
            }
            model.telphone = Utils.DropHTML(txtTelphone.Text.Trim());
            model.mobile = Utils.DropHTML(txtMobile.Text.Trim());
            model.qq = Utils.DropHTML(txtQQ.Text);
            model.address = Utils.DropHTML(txtAddress.Text.Trim());
            //model.amount = decimal.Parse(txtAmount.Text.Trim());
            model.point = int.Parse(txtPoint.Text.Trim());
            //model.exp = int.Parse(txtExp.Text.Trim());
            model.reg_time = DateTime.Now;
            model.reg_ip = DTRequest.GetIP();
            model.id_card_number = txtIdCard.Text.Trim();
            model.real_name = txtRealName.Text.Trim();

            int userId = bll.Add(model);
            if (0 < userId)
            {
                // 会员部功能，被推荐人自动归组
                if (ddlServingGroup.SelectedValue != "")
                {
                    context.li_user_group_servers.InsertOnSubmit(new li_user_group_servers
                    {
                        group_id = Convert.ToInt32(ddlServingGroup.Text),
                        serving_user = userId
                    });
                }
                // 创建借款人信息
                UpdateLoanerInfo(userId, model.reg_time);

                // 保存身份证照片
                try
                {
                    var user = context.dt_users.First(u => u.id == userId);
                    LoadAlbum(user, Agp2pEnums.AlbumTypeEnum.IdCard);
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加用户出错:" + ex.Message);
                }
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加用户:" + model.user_name); //记录日志
                result = true;
            }
            return result;
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int _id)
        {
            bool result = false;
            BLL.users bll = new BLL.users();
            Model.users model = bll.GetModel(_id);

            model.group_id = int.Parse(ddlGroupId.SelectedValue);
            model.status = int.Parse(rblStatus.SelectedValue);
            //判断密码是否更改
            if (txtPassword.Text.Trim() != defaultpassword)
            {
                //获取用户已生成的salt作为密钥加密
                model.password = DESEncrypt.Encrypt(txtPassword.Text.Trim(), model.salt);
            }
            model.email = Utils.DropHTML(txtEmail.Text);
            model.nick_name = Utils.DropHTML(txtNickName.Text);
            model.avatar = Utils.DropHTML(txtAvatar.Text);
            model.sex = rblSex.SelectedValue;
            DateTime _birthday;
            if (DateTime.TryParse(txtBirthday.Text.Trim(), out _birthday))
            {
                model.birthday = _birthday;
            }
            model.telphone = Utils.DropHTML(txtTelphone.Text.Trim());
            model.mobile = Utils.DropHTML(txtMobile.Text.Trim());
            model.qq = Utils.DropHTML(txtQQ.Text);
            if (!string.IsNullOrEmpty(model.area))
                model.address = Utils.DropHTML(txtAddress.Text.Trim().Replace(model.area.Replace(",", ""), ""));
            //model.amount = Utils.StrToDecimal(txtAmount.Text.Trim(), 0);
            model.point = Utils.StrToInt(txtPoint.Text.Trim(), 0);
            //model.exp = Utils.StrToInt(txtExp.Text.Trim(), 0);
            model.id_card_number = txtIdCard.Text.Trim();
            model.real_name = txtRealName.Text.Trim();

            if (bll.Update(model))
            {
                // 会员部功能，被推荐人自动归组
                var groupServer = context.li_user_group_servers.FirstOrDefault(s => s.serving_user == model.id);
                if (groupServer == null && ddlServingGroup.SelectedValue != "")
                {
                    context.li_user_group_servers.InsertOnSubmit(new li_user_group_servers
                    {
                        group_id = Convert.ToInt32(ddlServingGroup.Text),
                        serving_user = model.id
                    });
                }
                else if (groupServer != null && ddlServingGroup.SelectedValue == "")
                {
                    context.li_user_group_servers.DeleteOnSubmit(groupServer);
                }
                else if (groupServer != null)
                {
                    groupServer.group_id = Convert.ToInt32(ddlServingGroup.SelectedValue);
                }

                // 修改借款人信息
                UpdateLoanerInfo(model.id, model.reg_time);

                //保存身份证照片
                try
                {
                    var user = context.dt_users.First(u => u.id == model.id);
                    LoadAlbum(user, Agp2pEnums.AlbumTypeEnum.IdCard);
                    context.SubmitChanges();
                    AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改用户信息:" + model.user_name); //记录日志
                    result = true;
                }
                catch (Exception ex)
                {
                    AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加用户出错:" + ex.Message);
                }
            }
            return result;
        }

        private void UpdateLoanerInfo(int userId, DateTime lastUpdateTime)
        {
            var loanerInfo = context.li_loaners.SingleOrDefault(l => l.user_id == userId);
            if (chkIsLoaner.Checked)
            {
                if (loanerInfo == null)
                {
                    loanerInfo = new li_loaners
                    {
                        age = Convert.ToInt16(txtAge.Text),
                        user_id = userId,
                        educational_background = txtEducationalBackground.Text,
                        income = txtIncome.Text,
                        job = txtJob.Text,
                        last_update_time = lastUpdateTime,
                        working_at = txtWorkingAt.Text,
                        marital_status = Convert.ToByte(rblMaritalStatus.SelectedValue),
                    };
                    context.li_loaners.InsertOnSubmit(loanerInfo);

                    if (chkBindCompany.Checked)
                    {
                        var loanerCompany = new li_loaner_companies
                        {
                            name = txtCompanyName.Text,
                            business_lawsuit = txtBusinessLawsuit.Text,
                            business_scope = txtBusinessScope.Text,
                            business_status = txtBusinessStatus.Text,
                            income_yearly = txtIncomeYearly.Text,
                            net_assets = txtNetAssets.Text,
                            registered_capital = txtRegisteredCapital.Text,
                            setup_time = Convert.ToDateTime(txtSetupTime.Text),
                            remark = txtCompanyRemark.Text,
                        };
                        loanerInfo.li_loaner_companies = loanerCompany;
                        context.li_loaner_companies.InsertOnSubmit(loanerCompany);
                    }
                }
                else
                {
                    loanerInfo.age = Convert.ToInt16(txtAge.Text);
                    loanerInfo.user_id = userId;
                    loanerInfo.educational_background = txtEducationalBackground.Text;
                    loanerInfo.income = txtIncome.Text;
                    loanerInfo.job = txtJob.Text;
                    loanerInfo.last_update_time = lastUpdateTime;
                    loanerInfo.working_at = txtWorkingAt.Text;
                    loanerInfo.marital_status = Convert.ToByte(rblMaritalStatus.SelectedValue);

                    UpdateLoanerCompanyInfo(loanerInfo);
                }
            }
            else if (loanerInfo != null)
            {
                var c = loanerInfo.li_loaner_companies;
                if (c != null)
                {
                    context.li_loaner_companies.DeleteOnSubmit(c);
                }
                context.li_loaners.DeleteOnSubmit(loanerInfo);
            }
        }

        private void UpdateLoanerCompanyInfo(li_loaners loaner)
        {
            var company = loaner.li_loaner_companies;
            if (chkBindCompany.Checked)
            {
                if (company == null)
                {
                    company = new li_loaner_companies
                    {
                        name = txtCompanyName.Text,
                        business_lawsuit = txtBusinessLawsuit.Text,
                        business_scope = txtBusinessScope.Text,
                        business_status = txtBusinessStatus.Text,
                        income_yearly = txtIncomeYearly.Text,
                        net_assets = txtNetAssets.Text,
                        registered_capital = txtRegisteredCapital.Text,
                        setup_time = Convert.ToDateTime(txtSetupTime.Text),
                        remark = txtCompanyRemark.Text,
                    };
                    loaner.li_loaner_companies = company;
                    context.li_loaner_companies.InsertOnSubmit(company);
                }
                else
                {
                    company.name = txtCompanyName.Text;
                    company.business_lawsuit = txtBusinessLawsuit.Text;
                    company.business_scope = txtBusinessScope.Text;
                    company.business_status = txtBusinessStatus.Text;
                    company.income_yearly = txtIncomeYearly.Text;
                    company.net_assets = txtNetAssets.Text;
                    company.registered_capital = txtRegisteredCapital.Text;
                    company.setup_time = Convert.ToDateTime(txtSetupTime.Text);
                    company.remark = txtCompanyRemark.Text;
                }
            }
            else if (company != null)
            {
                /*if (company.li_loaners.Count == 1)
                    context.li_loaner_companies.DeleteOnSubmit(company);*/
                loaner.li_loaner_companies = null;
            }
        }
        #endregion

        private void LoadAlbum(dt_users model, Agp2pEnums.AlbumTypeEnum type)
        {
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] remarkArr = Request.Form.GetValues("hid_photo_remark");
            context.li_albums.DeleteAllOnSubmit(context.li_albums.Where(a => a.the_user == model.id && a.type == (int)type));
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
                        dt_users = model,
                        type = (byte)type
                    };
                });
                context.li_albums.InsertAllOnSubmit(preAdd);
            }
        }


        protected string QueryInvestmentToday(dt_users user)
        {
            return
                user.li_project_transactions.Where(
                    tr =>
                        tr.status == (int)Agp2pEnums.ProjectTransactionStatusEnum.Success &&
                        tr.type == (int)Agp2pEnums.ProjectTransactionTypeEnum.Invest &&
                        tr.create_time.Date == DateTime.Now.Date).Sum(t => t.principal).ToString("c");
        }

        #region 返回用户状态=============================
        protected string GetUserStatus(int status)
        {
            var result = string.Empty;
            switch (status)
            {
                case 0:
                    result = "正常";
                    break;
                case 1:
                    result = "待验证";
                    break;
                case 2:
                    result = "待审核";
                    break;
                case 3:
                    result = "已禁用";
                    break;
            }
            return result;
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("user_list", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改用户成功！", "user_list.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("user_list", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加用户成功！", "user_list.aspx", "Success");
            }
        }

        protected void chkIsLoaner_OnCheckedChanged(object sender, EventArgs e)
        {
            if (pagein == "Edit")
            {
                txtUserName.Attributes.Remove("ajaxurl");
                ClientScript.RegisterClientScriptBlock(Page.GetType(), "SwitchTab", "$(function (){ $('.content-tab-ul-wrap a:nth(3)').click();});", true);
            }
        }

        protected void chkBindCompany_OnCheckedChanged(object sender, EventArgs e)
        {
            if (pagein == "Edit")
            {
                txtUserName.Attributes.Remove("ajaxurl");
                ClientScript.RegisterClientScriptBlock(Page.GetType(), "SwitchTab", "$(function (){ $('.content-tab-ul-wrap a:nth(3)').click();});", true);
            }
        }
    }
}