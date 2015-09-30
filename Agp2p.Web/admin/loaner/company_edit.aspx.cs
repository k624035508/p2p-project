using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.loaner
{
    public partial class company_edit : UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int id = 0;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                if (!int.TryParse(Request.QueryString["id"], out id))
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                var context = this.context;
                var company = context.li_loaner_companies.FirstOrDefault(q => q.id == id);
                if (company == null)
                {
                    JscriptMsg("记录不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_company", DTEnums.ActionEnum.View.ToString()); //检查权限
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(id);
                }
                else if (action == DTEnums.ActionEnum.Add.ToString())
                {
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int id)
        {
            var model = context.li_loaner_companies.First(q => q.id == id);
            txtLoaners.Text = string.Join(",", model.li_loaners.Select(l => string.IsNullOrWhiteSpace(l.dt_users.real_name)
                ? l.dt_users.user_name : string.Format("{0}({1})", l.dt_users.user_name, l.dt_users.real_name)));

            txtCompanyName.Text = model.name;
            txtSetupTime.Text = model.setup_time.ToString("yyyy-MM-dd");
            txtRegisteredCapital.Text = model.registered_capital;
            txtBusinessScope.Text = model.business_scope;
            txtBusinessStatus.Text = model.business_status;
            txtBusinessLawsuit.Text = model.business_lawsuit;
            txtIncomeYearly.Text = model.income_yearly;
            txtNetAssets.Text = model.net_assets;
            txtCompanyRemark.Text = model.remark;
            txtAddress.Text = model.address;
            txtBusinessBelong.Text = model.business_belong;
            txtNetProfit.Text = model.net_profit_yearly;
            txtManager.Text = model.manager;

            rptPics.DataSource = model.li_albums;
            rptPics.DataBind();
        }
        #endregion

        private void LoadAlbum(li_loaner_companies model, Agp2pEnums.AlbumTypeEnum type)
        {
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] remarkArr = Request.Form.GetValues("hid_photo_remark");
            context.li_albums.DeleteAllOnSubmit(context.li_albums.Where(a => a.company == model.id && a.type == (int) type));
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
                        li_loaner_companies = model,
                        type = (byte)type
                    };
                });
                context.li_albums.InsertAllOnSubmit(preAdd);
            }
        }

        #region 增加操作=================================
        private bool DoAdd()
        {
            if (context.li_loaner_companies.Any(q => q.name == txtCompanyName.Text.Trim()))
            {
                JscriptMsg("企业名称重复！", "", "Error");
                return false;
            }
            var model = new li_loaner_companies
            {
                name = txtCompanyName.Text.Trim(),
                setup_time = Convert.ToDateTime(txtSetupTime.Text),
                business_scope = txtBusinessScope.Text,
                business_status = txtBusinessStatus.Text,
                business_lawsuit = txtBusinessLawsuit.Text,
                registered_capital = txtRegisteredCapital.Text,
                income_yearly = txtIncomeYearly.Text,
                net_assets = txtNetAssets.Text,
                remark = txtCompanyRemark.Text,
                manager = txtManager.Text,
                net_profit_yearly = txtNetProfit.Text,
                address = txtAddress.Text,
                business_belong = txtBusinessBelong.Text
            };
            context.li_loaner_companies.InsertOnSubmit(model);
            LoadAlbum(model, Agp2pEnums.AlbumTypeEnum.Pictures);

            var currentLoanerNames = txtLoaners.Text.Trim().Split(',');
            currentLoanerNames.ForEach(ln =>
            {
                var loaner = context.li_loaners.SingleOrDefault(l => l.dt_users.user_name == ln) ??
                             context.li_loaners.Single(l => l.dt_users.real_name == ln);
                if (loaner.li_loaner_companies != null)
                {
                    JscriptMsg(loaner.dt_users.user_name + " 已经绑定企业，不能重复绑定！", "", "Error");
                }
                else
                {
                    loaner.li_loaner_companies = model;
                }
            });

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加企业信息:" + txtCompanyName.Text); //记录日志
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int id)
        {
            var model = context.li_loaner_companies.Single(q => q.id == id);

            var pattern = new Regex(@"^([^\s\(\)]+)(?:\(.+\))?$");

            var currentLoanerNames = txtLoaners.Text.Trim().Split(',').Select(l =>
            {
                var match = pattern.Match(l);
                if (match.Success) return match.Groups[1].ToString();
                throw new Exception("借款人输入格式异常");
            }).ToArray();

            // 原来有，现在无的，删除关联
            model.li_loaners.Where(l => !currentLoanerNames.Contains(l.dt_users.user_name))
                .ForEach(l =>
                {
                    l.company_id = null;
                });
            currentLoanerNames.ForEach(ln =>
            {
                var loaner = context.li_loaners.SingleOrDefault(l => l.dt_users.user_name == ln) ??
                             context.li_loaners.Single(l => l.dt_users.real_name == ln);
                if (loaner.li_loaner_companies == model)
                {
                    // do nothing
                }
                else if (loaner.li_loaner_companies != null)
                {
                    JscriptMsg(loaner.dt_users.user_name + " 已经绑定企业，不能重复绑定！", "", "Error");
                }
                else
                {
                    loaner.li_loaner_companies = model;
                }
            });

            model.name = txtCompanyName.Text.Trim();
            model.setup_time = Convert.ToDateTime(txtSetupTime.Text.Trim());
            model.registered_capital = txtRegisteredCapital.Text;
            model.business_scope = txtBusinessScope.Text;
            model.business_status = txtBusinessStatus.Text;
            model.business_lawsuit = txtBusinessLawsuit.Text;
            model.income_yearly = txtIncomeYearly.Text;
            model.net_assets = txtNetAssets.Text;
            model.remark = txtCompanyRemark.Text;
            model.manager = txtManager.Text;
            model.net_profit_yearly = txtNetProfit.Text;
            model.address = txtAddress.Text;
            model.business_belong = txtBusinessBelong.Text;

            LoadAlbum(model, Agp2pEnums.AlbumTypeEnum.Pictures);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改企业信息:" + model.name); //记录日志
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("loan_company", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改企业信息信息成功！", "company_list.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("loan_company", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加企业信息信息成功！", "company_list.aspx", "Success");
            }
        }
    }
}