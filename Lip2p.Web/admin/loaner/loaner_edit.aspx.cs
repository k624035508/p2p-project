using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.loaner
{
    public partial class loaner_edit : UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int id = 0;
        Lip2pDataContext context = new Lip2pDataContext();

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
                var loaner = context.li_loaners.FirstOrDefault(q => q.id == id);
                if (loaner == null)
                {
                    JscriptMsg("记录不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_loaners", DTEnums.ActionEnum.View.ToString()); //检查权限
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(id);
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int id)
        {
            var model = context.li_loaners.First(q => q.id == id);
            txtName.Text = model.name;
            txtTel.Text = model.tel;
            txtAge.Text = model.age.ToString();
            rblGender.SelectedValue = model.gender.ToString();
            txtCencus.Text = model.cencus;
            txtJob.Text = model.job;
            txtWorkingAt.Text = model.working_at;
            txtIdCardNumber.Text = model.id_card_number;
            txtEmail.Text = model.email;
            txtEducationalBackground.Text = model.educational_background;
            rblMaritalStatus.SelectedValue = model.marital_status.ToString();
            txtIncome.Text = model.income;

            rptIdCardPics.DataSource = model.li_albums.Where(a => a.loaner == id && a.type == (int) Lip2pEnums.AlbumTypeEnum.IdCard);
            rptIdCardPics.DataBind();
        }
        #endregion

        private void LoadAlbum(li_loaners model, Lip2pEnums.AlbumTypeEnum type)
        {
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] remarkArr = Request.Form.GetValues("hid_photo_remark");
            context.li_albums.DeleteAllOnSubmit(context.li_albums.Where(a => a.loaner == model.id && a.type == (int) type));
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
                        li_loaners = model,
                        type = (byte)type
                    };
                });
                context.li_albums.InsertAllOnSubmit(preAdd);
            }
        }

        #region 增加操作=================================
        private bool DoAdd()
        {
            var check = context.li_loaners.FirstOrDefault(q => q.id_card_number == txtIdCardNumber.Text.Trim()); //检测用户名是否重复
            if (check != null)
            {
                JscriptMsg("身份证号重复！", "", "Error");
                return false;
            }
            var model = new li_loaners
            {
                name = txtName.Text.Trim(),
                tel = txtTel.Text.Trim(),
                age = Convert.ToInt16(txtAge.Text.Trim()),
                gender = Convert.ToByte(rblGender.SelectedValue),
                cencus = txtCencus.Text.Trim(),
                job = txtJob.Text.Trim(),
                working_at = txtWorkingAt.Text.Trim(),
                id_card_number = txtIdCardNumber.Text.Trim(),
                email = txtEmail.Text.Trim(),
                educational_background = txtEducationalBackground.Text.Trim(),
                marital_status = Convert.ToByte(rblMaritalStatus.SelectedValue),
                income = txtIncome.Text.Trim(),
                last_update_time = DateTime.Now
            };
            context.li_loaners.InsertOnSubmit(model);
            LoadAlbum(model, Lip2pEnums.AlbumTypeEnum.IdCard);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加借贷人:" + model.name); //记录日志
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int id)
        {
            var model = context.li_loaners.First(q => q.id == id);

            model.name = txtName.Text.Trim();
            model.tel = txtTel.Text.Trim();
            model.age = Convert.ToInt16(txtAge.Text.Trim());
            model.gender = Convert.ToByte(rblGender.SelectedValue);
            model.cencus = txtCencus.Text.Trim();
            model.job = txtJob.Text.Trim();
            model.working_at = txtWorkingAt.Text.Trim();
            model.id_card_number = txtIdCardNumber.Text.Trim();
            model.email = txtEmail.Text.Trim();
            model.educational_background = txtEducationalBackground.Text.Trim();
            model.marital_status = Convert.ToByte(rblMaritalStatus.SelectedValue);
            model.income = txtIncome.Text.Trim();
            model.last_update_time = DateTime.Now;

            LoadAlbum(model, Lip2pEnums.AlbumTypeEnum.IdCard);
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改借贷人:" + model.name); //记录日志
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
                ChkAdminLevel("loan_loaners", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改借贷人信息成功！", "loaner_list.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("loan_loaners", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加借贷人信息成功！", "loaner_list.aspx", "Success");
            }
        }
    }
}