using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Data;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.LineService.admin
{
    public partial class view : Lip2p.Web.UI.ManagePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("plugin_lineservice_list", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //绑定模板
                ShowInfo();
            }
        }

        #region 绑定模板================================
        private void TreeBind()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Utils.GetMapPath("../skin/"));
            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                this.rblSkin.Items.Add(new ListItem(dir.Name, dir.Name));
            }
        }
        #endregion

        #region 赋值操作=================================
        private void ShowInfo()
        {
            string fullPath = Utils.GetMapPath("../set.config");
            XmlDocument doc = new XmlDocument();
            doc.Load(fullPath);
            XmlNode phone = doc.SelectSingleNode(@"config/telephone");
            XmlNode status = doc.SelectSingleNode(@"config/status");
            XmlNode skin = doc.SelectSingleNode(@"config/skin");
            txtPhone.Text = phone.InnerText;
            rblStatus.SelectedValue = status.InnerText;
            rblSkin.SelectedValue = skin.InnerText;
        }
        #endregion

        //保存
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_lineservice_list", DTEnums.ActionEnum.Edit.ToString()); //检查权限
            string fullPath = Utils.GetMapPath("../set.config");
            XmlHelper.UpdateNodeInnerText(fullPath, @"config/telephone", txtPhone.Text.Trim());
            XmlHelper.UpdateNodeInnerText(fullPath, @"config/status", rblStatus.SelectedValue);
            XmlHelper.UpdateNodeInnerText(fullPath, @"config/skin", rblSkin.SelectedValue);
            JscriptMsg("保存成功！", "view.aspx", "Success");
        }

    }
}