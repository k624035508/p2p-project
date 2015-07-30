using System;
using System.Text;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using System.Linq;
using Lip2p.Core;

namespace Lip2p.Web.admin.users
{
    public partial class user_sms : Web.UI.ManagePage
    {
        string mobiles = string.Empty;
        protected string Action = string.Empty;
        protected string groupNames = string.Empty;
        protected Lip2pDataContext context = new Lip2pDataContext();
        protected void Page_Load(object sender, EventArgs e)
        {
            mobiles = DTRequest.GetFormString("mobiles");
            Action =DTRequest.GetFormString("Action");
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("user_sms", DTEnums.ActionEnum.View.ToString()); //检查权限
                if (string.IsNullOrWhiteSpace(Action))
                {
                    ShowInfo(mobiles);
                }
                else
                {
                    groupNames = DTRequest.GetFormString("groupNames");
                    rblSmsType.SelectedValue = "2";
                    div_mobiles.Visible = true;
                    div_group.Visible = true;
                    txtMobileNumbers.Text = mobiles;
                }
                TreeBind(); //绑定类别
            }
        }

        #region 绑定类别=================================
        private void TreeBind()
        {
            this.cblGroupId.Items.Clear();

          //限制当前管理员对会员的查询
            var canAccessGroups = context.li_user_group_access_keys.Where(k => k.owner_manager == GetAdminInfo().id).Select(k => k.user_group).ToArray();

            cblGroupId.Items.AddRange(context.dt_user_groups.Where(g => g.is_lock == 0 && (!canAccessGroups.Any() || canAccessGroups.Contains(g.id)))
                .OrderByDescending(g => g.grade)
                .ThenByDescending(g => g.id)
                .Select(g => new ListItem(g.title, g.id.ToString()))
                .ToArray());

            //BLL.user_groups bll = new BLL.user_groups();
            //DataTable dt = bll.GetList(0, strWhere, "grade asc,id asc").Tables[0];

            //this.cblGroupId.Items.Clear();
            //foreach (DataRow dr in dt.Rows)
            //{
            //    this.cblGroupId.Items.Add(new ListItem(dr["title"].ToString(), dr["id"].ToString()));
            //}
        }
        #endregion

        #region 赋值操作=================================
        private void ShowInfo(string _mobiles)
        {
            if (!string.IsNullOrEmpty(_mobiles))
            {
                div_mobiles.Visible = true;
                div_group.Visible = false;
                rblSmsType.SelectedValue = "1";
                txtMobileNumbers.Text = _mobiles;

            }
            else
            {
                rblSmsType.SelectedValue = "2";
                div_mobiles.Visible = false;
                div_group.Visible = true;
            }
        }
        #endregion

        #region 返回会员组所有手机号码===================
        private string GetGroupMobile(ArrayList al)
        {
            StringBuilder str = new StringBuilder();
            foreach (Object obj in al)
            {
                DataTable dt = new BLL.users().GetList(0, "group_id=" + Convert.ToInt32(obj), "reg_time desc,id desc").Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    if (!string.IsNullOrEmpty(dr["mobile"].ToString()))
                    {
                        str.Append(dr["mobile"].ToString() + ",");
                    }
                }
            }
            return Utils.DelLastComma(str.ToString());
        }
        #endregion

        //选择发送类型
        protected void rblSmsType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblSmsType.SelectedValue == "1")
            {
                div_group.Visible = false;
                div_mobiles.Visible = true;
            }
            else
            {
                div_group.Visible = true;
                if (!string.IsNullOrWhiteSpace(txtMobileNumbers.Text.Trim()))
                {
                    div_mobiles.Visible = true;
                }
                else
                {
                    div_mobiles.Visible = false;
                }
            }
        }

        //提交发送
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("user_sms", DTEnums.ActionEnum.Add.ToString()); //检查权限
            //检查短信内容
            if (txtSmsContent.Text.Trim() == "")
            {
                JscriptMsg("请输入短信内容！", "", "Error");
                return;
            }
            //检查发送类型
            if (rblSmsType.SelectedValue == "1")
            {
                if (txtMobileNumbers.Text.Trim() == "")
                {
                    JscriptMsg("请输入手机号码！", "", "Error");
                    return;
                }
                //开始发送短信
                string msg=string.Empty;
                bool result = SMSHelper.Send(txtMobileNumbers.Text.Trim(), txtSmsContent.Text.Trim(), 2, out msg);
                if (result)
                {
                    AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "发送手机短信"); //记录日志
                    JscriptMsg(msg, "user_sms.aspx", "Success");
                    return;
                }
                JscriptMsg(msg, "", "Error");
                return;
            }
            else
            {
                ArrayList al = new ArrayList();
                string _mobiles = string.Empty;
                if (string.IsNullOrWhiteSpace(txtMobileNumbers.Text.Trim()))
                {
                    for (int i = 0; i < cblGroupId.Items.Count; i++)
                    {
                        if (cblGroupId.Items[i].Selected)
                        {
                            al.Add(cblGroupId.Items[i].Value);
                        }
                    }
                    if (al.Count < 1)
                    {
                        JscriptMsg("请选择会员组别！", "", "Error");
                        return;
                    }
                    _mobiles = GetGroupMobile(al);
                }
                else
                {
                    if (al.Count > 1)
                    {
                        _mobiles = GetGroupMobile(al);
                        string txtmstr = txtMobileNumbers.Text.Trim(); //去掉重复号码 mobiles;
                        string[] mstr = txtmstr.Split(',');
                        string m = string.Empty;
                        string d, b;
                        string bigm = _mobiles;
                        string mobile = string.Empty;
                        for (var k = 0; k < mstr.Length; k++)
                        {
                            if (mstr[k].Length == 11)
                            {
                                mobile = bigm.Replace(mstr[k], "") + "," + mstr[k];
                                bigm = mobile;
                            }
                        }
                        _mobiles = mobile.Replace(",,", "");
                        string i = _mobiles.Substring(0, 1);
                        string l = _mobiles.Substring(_mobiles.Length - 1, 1);
                        if (i.IndexOf(',') != -1)
                        {
                            _mobiles = _mobiles.Substring(1, _mobiles.Length - 1);
                        }
                        if (l.IndexOf(',') != -1)
                        {
                            _mobiles = _mobiles.Substring(0, _mobiles.Length - 1);
                        }
                    }
                    else
                    {
                        _mobiles = txtMobileNumbers.Text.Trim();
                    }                   
                }
               
                //return;
                //开始发送短信
                string msg = string.Empty;
                bool result = SMSHelper.Send(_mobiles, txtSmsContent.Text.Trim(), 2, out msg);
                if (result)
                {
                    AddAdminLog(DTEnums.ActionEnum.Add.ToString(), "发送手机短信"); //记录日志
                    JscriptMsg(msg, "user_sms.aspx", "Success");
                    return;
                }
                JscriptMsg(msg, "", "Error");
                return;
            }
        }



    }
}