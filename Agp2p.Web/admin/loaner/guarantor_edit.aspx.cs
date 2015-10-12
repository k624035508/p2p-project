using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.loaner
{
    public partial class guarantor_edit : UI.ManagePage
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
                var guarantor = context.li_guarantors.FirstOrDefault(q => q.id == id);
                if (guarantor == null)
                {
                    JscriptMsg("记录不存在或已被删除！", "back", "Error");
                    return;
                }
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("organization_manage", DTEnums.ActionEnum.View.ToString()); //检查权限
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(id);
                }
                else if (action == DTEnums.ActionEnum.Add.ToString())
                {
                    txtShareholdersInfo.Value = File.ReadAllText(Request.MapPath("./_shareholdersInfoTemplate.html"));
                    txtCreditSituationInfo.Value = File.ReadAllText(Request.MapPath("./_creditSituationInfoTemplate.html"));
                }
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(int id)
        {
            var model = context.li_guarantors.First(q => q.id == id);

            txtGuarantorName.Text = model.name;
            txtRegistNumber.Text = model.regist_number;
            rblType.SelectedValue = model.type.ToString();
            txtLegalPerson.Text = model.legal_person;
            txtRegisteredCapital.Text = model.registered_capital;
            txtSetupDate.Text = model.setup_time.ToString("yyyy-MM-dd");
            txtAddr.Text = model.address;
            txtDescription.Text = model.description;
            txtBusinessScope.Text = model.business_scope;
            txtShareholdersInfo.Value = model.shareholders_info;
            txtCreditSituationInfo.Value = model.credit_situation_info;
            txtCMBusinessTypes.Text = model.cm_business_types;
            txtCMCooperationTime.Text = model.cm_cooperation_time;
            txtCMCooperationTotalDegree.Text = model.cm_total_degree;
            txtCMCooperationUsedDegree.Text = model.cm_used_degree;
            txtCMCooperationRemainDegree.Text = model.cm_remain_degree;

            rptPics.DataSource = model.li_albums;
            rptPics.DataBind();
        }
        #endregion

        private void LoadAlbum(li_guarantors model, Agp2pEnums.AlbumTypeEnum type)
        {
            string[] albumArr = Request.Form.GetValues("hid_photo_name");
            string[] remarkArr = Request.Form.GetValues("hid_photo_remark");
            context.li_albums.DeleteAllOnSubmit(context.li_albums.Where(a => a.guarantor == model.id && a.type == (int) type));
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
                        li_guarantors = model,
                        type = (byte)type
                    };
                });
                context.li_albums.InsertAllOnSubmit(preAdd);
            }
        }

        #region 增加操作=================================
        private bool DoAdd()
        {
            if (context.li_guarantors.Any(q => q.name == txtGuarantorName.Text.Trim()))
            {
                JscriptMsg("担保机构名称重复！", "", "Error");
                return false;
            }
            var model = new li_guarantors
            {
                name = txtGuarantorName.Text,
                type = Convert.ToByte(rblType.SelectedValue),
                regist_number = txtRegistNumber.Text,
                legal_person = txtLegalPerson.Text,
                registered_capital = txtRegisteredCapital.Text,
                setup_time = Convert.ToDateTime(txtSetupDate.Text),
                address = txtAddr.Text,
                description = txtDescription.Text,
                business_scope = txtBusinessScope.Text,
                shareholders_info = txtShareholdersInfo.Value,
                credit_situation_info = txtCreditSituationInfo.Value,
                cm_business_types = txtCMBusinessTypes.Text,
                cm_cooperation_time = txtCMCooperationTime.Text,
                cm_total_degree = txtCMCooperationTotalDegree.Text,
                cm_used_degree = txtCMCooperationUsedDegree.Text,
                cm_remain_degree = txtCMCooperationRemainDegree.Text,
            };

            context.li_guarantors.InsertOnSubmit(model);
            LoadAlbum(model, Agp2pEnums.AlbumTypeEnum.Pictures);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加担保机构信息:" + txtGuarantorName.Text); //记录日志
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
            var model = context.li_guarantors.Single(q => q.id == id);

            model.name = txtGuarantorName.Text.Trim();
            model.type = Convert.ToByte(rblType.SelectedValue);
            model.regist_number = txtRegistNumber.Text;
            model.legal_person = txtLegalPerson.Text;
            model.registered_capital = txtRegisteredCapital.Text;
            model.setup_time = Convert.ToDateTime(txtSetupDate.Text);
            model.address = txtAddr.Text;
            model.description = txtDescription.Text;
            model.business_scope = txtBusinessScope.Text;
            model.shareholders_info = txtShareholdersInfo.Value;
            model.credit_situation_info = txtCreditSituationInfo.Value;
            model.cm_business_types = txtCMBusinessTypes.Text;
            model.cm_cooperation_time = txtCMCooperationTime.Text;
            model.cm_total_degree = txtCMCooperationTotalDegree.Text;
            model.cm_used_degree = txtCMCooperationUsedDegree.Text;
            model.cm_remain_degree = txtCMCooperationRemainDegree.Text;

            LoadAlbum(model, Agp2pEnums.AlbumTypeEnum.Pictures);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改担保机构信息:" + model.name); //记录日志
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
                ChkAdminLevel("organization_manage", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改担保机构信息信息成功！", "guarantor_list.aspx", "Success");
            }
            else //添加
            {
                ChkAdminLevel("organization_manage", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加担保机构信息信息成功！", "guarantor_list.aspx", "Success");
            }
        }
    }
}