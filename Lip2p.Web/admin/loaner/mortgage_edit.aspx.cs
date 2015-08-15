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
    public partial class mortgage_edit : UI.ManagePage
    {
        protected string action = DTEnums.ActionEnum.Add.ToString(), owner_id; // 显式该 loaner，编辑完成返回时返回到该用户的抵押物列表
        private int id = 0;
        Lip2pDataContext context = new Lip2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            string _action = DTRequest.GetQueryString("action");
            owner_id = DTRequest.GetQueryString("owner_id");
            if (!string.IsNullOrEmpty(_action) && _action == DTEnums.ActionEnum.Edit.ToString())
            {
                action = DTEnums.ActionEnum.Edit.ToString();//修改类型
                if (!int.TryParse(Request.QueryString["id"], out id))
                {
                    JscriptMsg("传输参数不正确！", "back", "Error");
                    return;
                }
                var mortgage = context.li_mortgages.FirstOrDefault(q => q.id == id);
                if (mortgage == null)
                {
                    JscriptMsg("记录不存在或已被删除！", "back", "Error");
                    return;
                }
                owner_id = mortgage.owner.ToString();
            }
            if (string.IsNullOrEmpty(owner_id))
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_mortgages", DTEnums.ActionEnum.View.ToString()); //检查权限
                InitMortgageTypes();
                if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
                {
                    ShowInfo(id);
                }
                else
                {
                    InitOwnerLbl();
                }
            }
        }

        private void InitMortgageTypes()
        {
            rblMortgageType.Items.Clear();
            var mortgageTypeses = context.li_mortgage_types.ToList();
            var listItems = mortgageTypeses.Select(c => new ListItem(c.name, c.id.ToString())).ToArray();
            rblMortgageType.Items.AddRange(listItems);
            // default value
            rblMortgageType.SelectedValue = rblMortgageType.Items[0].Value;
            txtScheme.Value = mortgageTypeses.First().scheme;
        }

        private void InitOwnerLbl()
        {
            var loaners = context.li_loaners.OrderByDescending(q => q.last_update_time).ToList();
            lblOwner.Text = loaners.First(l => l.id == Convert.ToInt32(owner_id)).dt_users.real_name;
        }

        #region 赋值操作=================================
        private void ShowInfo(int id)
        {
            var model = context.li_mortgages.First(q => q.id == id);
            txtName.Text = model.name;
            lblOwner.Text = model.li_loaners.dt_users.real_name;
            rblMortgageType.SelectedValue = model.type.ToString();
            txtValuation.Text = model.valuation.ToString("0.##");
            txtRemark.Text = model.remark;
            txtScheme.Value = model.li_mortgage_types.scheme;
            txtProperties.Value = model.properties;

            rptPictures.DataSource = model.li_albums.Where(a => a.mortgage == id && a.type == (int) Lip2pEnums.AlbumTypeEnum.Pictures);
            rptPictures.DataBind();

            rptPropertyCertificates.DataSource = model.li_albums.Where(a => a.mortgage == id && a.type == (int) Lip2pEnums.AlbumTypeEnum.PropertyCertificate);
            rptPropertyCertificates.DataBind();
        }
        #endregion

        #region 增加操作=================================
        private bool DoAdd()
        {
            var check =
                context.li_mortgages.FirstOrDefault(
                    q => q.name == txtName.Text.Trim() && q.owner == Convert.ToInt32(owner_id)); //检测标的物是否重复
            if (check != null)
            {
                JscriptMsg("同一个抵押者的标的物名称有重复！", "", "Error");
                return false;
            }
            var model = new li_mortgages
            {
                name = txtName.Text.Trim(),
                owner = Convert.ToInt32(owner_id),
                type = Convert.ToByte(rblMortgageType.SelectedValue),
                valuation = Convert.ToDecimal(txtValuation.Text.Trim()),
                remark = txtRemark.Text.Trim(),
                properties = txtProperties.Value,
                last_update_time = DateTime.Now
            };

            context.li_mortgages.InsertOnSubmit(model);

            LoadAlbum(model, Lip2pEnums.AlbumTypeEnum.Pictures, 0);
            LoadAlbum(model, Lip2pEnums.AlbumTypeEnum.PropertyCertificate, 1);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "添加标的物:" + model.name); //记录日志
                return true;
            }
            catch (Exception ex)
            {
                JscriptMsg(ex.Message, "", "Error");
                return false;
            }
        }

        private void LoadAlbum(li_mortgages model, Lip2pEnums.AlbumTypeEnum type, int splittedIndex)
        {
            string[] albumArr = GetSplittedFormValue("hid_photo_name", splittedIndex).ToArray();
            string[] remarkArr = GetSplittedFormValue("hid_photo_remark", splittedIndex).ToArray();
            context.li_albums.DeleteAllOnSubmit(context.li_albums.Where(a => a.mortgage == model.id && a.type == (int)type));
            var preAdd = albumArr.Zip(remarkArr, (album, remark) =>
            {
                var albumSplit = album.Split('|');
                return new li_albums
                {
                    original_path = albumSplit[1],
                    thumb_path = albumSplit[2],
                    remark = remark,
                    add_time = DateTime.Now,
                    li_mortgages = model,
                    type = (byte) type
                };
            });
            context.li_albums.InsertAllOnSubmit(preAdd);
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

        #region 修改操作=================================
        private bool DoEdit(int id)
        {
            var model = context.li_mortgages.First(q => q.id == id);
            // 如果标的物已经关联了项目，则不能修改借款人
            if (model.owner.ToString() != owner_id)
            {
                var q = context.li_risk_mortgage.FirstOrDefault(rm => rm.mortgage == model.id);
                if (q != null)
                {
                    JscriptMsg("这个标的物已经关联了项目，不能修改借贷人！", "", "Error");
                    return false;
                }
            }
            model.name = txtName.Text.Trim();
            model.owner = Convert.ToInt32(owner_id);
            model.type = Convert.ToByte(rblMortgageType.SelectedValue);
            model.valuation = Convert.ToDecimal(txtValuation.Text.Trim());
            model.last_update_time = DateTime.Now;
            model.remark = txtRemark.Text.Trim();
            model.properties = txtProperties.Value;

            LoadAlbum(model, Lip2pEnums.AlbumTypeEnum.Pictures, 0);
            LoadAlbum(model, Lip2pEnums.AlbumTypeEnum.PropertyCertificate, 1);

            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改标的物:" + model.name); //记录日志
                return true;
            }
            catch (Exception ex)
            {
                JscriptMsg(ex.Message, "", "Error");
                return false;
            }
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (action == DTEnums.ActionEnum.Edit.ToString()) //修改
            {
                ChkAdminLevel("loan_mortgages", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                if (!DoEdit(id))
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("修改标的物信息成功！", Utils.CombUrlTxt("mortgage_list.aspx", "loaner_id={0}", owner_id), "Success");
            }
            else //添加
            {
                ChkAdminLevel("loan_mortgages", DTEnums.ActionEnum.Add.ToString()); //检查权限
                if (!DoAdd())
                {
                    JscriptMsg("保存过程中发生错误！", "", "Error");
                    return;
                }
                JscriptMsg("添加标的物信息成功！", Utils.CombUrlTxt("mortgage_list.aspx", "loaner_id={0}", owner_id), "Success");
            }
        }

        protected void rblMortgageType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var model = context.li_mortgages.SingleOrDefault(q => q.id == id);
            if (model != null)
            {
                model.type = Convert.ToInt32(rblMortgageType.SelectedValue);
                ShowInfo(id);
            }
            else
            {
                txtScheme.Value = context.li_mortgage_types.Single(t => t.id == Convert.ToInt32(rblMortgageType.SelectedValue)).scheme;
            }
        }
    }
}