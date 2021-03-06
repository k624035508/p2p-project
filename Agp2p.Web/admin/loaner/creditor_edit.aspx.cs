﻿using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.loaner
{
    public partial class creditor_edit : UI.ManagePage
    {
        private string action = DTEnums.ActionEnum.Add.ToString(); //操作类型
        private int user_id = 0;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                if (!int.TryParse(Request.QueryString["user_id"], out user_id))
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                var context = this.context;
                var creditor = context.li_creditors.FirstOrDefault(q => q.user_id == user_id);
                if (creditor == null)
                {
                    JscriptMsg("记录不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_creditor", DTEnums.ActionEnum.View.ToString()); //检查权限
                InitSelectUserDDL();
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(user_id);
                }
                else if (action == DTEnums.ActionEnum.Add.ToString())
                {
                    var user = context.dt_users.First(u => u.id == Convert.ToInt32(ddlSelectUser.SelectedValue));
                    lblRealName.Text = user.real_name;
                    lblEmail.Text = user.email;
                    lblIdCardNumber.Text = user.id_card_number;
                    lblsex.Text = user.sex;
                    lblTel.Text = user.mobile;
                }
            }
        }

        private void InitSelectUserDDL()
        {
            var users = context.dt_users.OrderByDescending(u => u.id);
            ddlSelectUser.Items.Clear();
            ddlSelectUser.Items.AddRange(users.Select(u => new ListItem(u.user_name, u.id.ToString())).ToArray());
        }

        #region 赋值操作=================================
        private void ShowInfo(int user_id)
        {
            var model = context.li_creditors.First(q => q.user_id == user_id);
            var user = model.dt_users;
            ddlSelectUser.SelectedValue = user.id.ToString();
            lblRealName.Text = user.real_name;
            lblEmail.Text = user.email;
            lblIdCardNumber.Text = user.id_card_number;
            lblsex.Text = user.sex;
            lblTel.Text = user.mobile;
            txtWorkingUnit.Text = model.working_company;
            txtRemark.Text = model.remark;
            txtAge.Text = model.age.ToString();
            txtJob.Text = model.job;

            rptIdCardPics.DataSource = model.dt_users.li_albums.Where(a => a.the_user == user_id && a.type == (int) Agp2pEnums.AlbumTypeEnum.IdCard);
            rptIdCardPics.DataBind();
        }
        #endregion

        private void LoadAlbum(li_creditors model, Agp2pEnums.AlbumTypeEnum type)
        {
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] remarkArr = Request.Form.GetValues("hid_photo_remark");
            context.li_albums.DeleteAllOnSubmit(context.li_albums.Where(a => a.the_user == model.user_id && a.type == (int) type));
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
                        the_user = model.user_id,
                        type = (byte)type
                    };
                });
                context.li_albums.InsertAllOnSubmit(preAdd);
            }
        }

        #region 增加操作=================================
        private bool DoAdd()
        {
            var model = new li_creditors
            {
                user_id = Convert.ToInt32(ddlSelectUser.SelectedValue),
                age = Convert.ToInt16(txtAge.Text.Trim()),
                job = txtJob.Text,
                working_company = txtWorkingUnit.Text,
                remark = txtRemark.Text,
                last_update_time = DateTime.Now
            };
            context.li_creditors.InsertOnSubmit(model);
            LoadAlbum(model, Agp2pEnums.AlbumTypeEnum.IdCard);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加债权人:" + ddlSelectUser.SelectedItem.Text); //记录日志
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 修改操作=================================
        private bool DoEdit(int user_id)
        {
            var model = context.li_creditors.First(q => q.user_id == user_id);

            model.user_id = Convert.ToInt32(ddlSelectUser.SelectedValue);
            model.age = Convert.ToInt16(txtAge.Text.Trim());
            model.job = txtJob.Text;
            model.working_company = txtWorkingUnit.Text;
            model.remark = txtRemark.Text;
            model.last_update_time = DateTime.Now;

            LoadAlbum(model, Agp2pEnums.AlbumTypeEnum.IdCard);
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改债权人:" + ddlSelectUser.SelectedItem.Text); //记录日志
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
            if (string.IsNullOrEmpty(lblIdCardNumber.Text) || string.IsNullOrEmpty(lblRealName.Text))
            {
                JscriptMsg("关联的会员必须要实名认证！", "", "Error");
                return;
            }

            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("loan_creditor", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(user_id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改债权人信息成功！", "creditor_list.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("loan_creditor", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加债权人信息成功！", "creditor_list.aspx", "Success");
            }
        }

        protected void ddlSelectUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(ddlSelectUser.SelectedValue);
            var user = context.dt_users.First(u => u.id == userId);
            lblRealName.Text = user.real_name;
            lblEmail.Text = user.email;
            lblIdCardNumber.Text = user.id_card_number;
            lblsex.Text = user.sex;
            lblTel.Text = user.mobile;
        }
    }
}