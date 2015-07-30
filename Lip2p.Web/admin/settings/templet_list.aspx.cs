using System;
using System.IO;
using System.Data;
using System.Xml;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Model;

namespace Lip2p.Web.admin.settings
{
    public partial class templet_list : Web.UI.ManagePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("app_templet_list", DTEnums.ActionEnum.View.ToString()); //检查权限
                TreeBind(); //绑定频道分类
                RptBind(); //绑定模板
            }
        }

        #region 绑定类别=================================
        private void TreeBind()
        {
            BLL.channel_category bll = new BLL.channel_category();
            DataTable dt = bll.GetList(0, "", "sort_id asc,id desc").Tables[0];

            this.ddlCategoryPath.Items.Clear();
            this.ddlCategoryPath.Items.Add(new ListItem("生成模板到", ""));
            if (siteConfig.mobilestatus == 1)
            {
                this.ddlCategoryPath.Items.Add(new ListItem("├ 手机网站", "mobile"));
            }
            foreach (DataRow dr in dt.Rows)
            {
                if (string.IsNullOrEmpty(dr["templet_path"].ToString()))
                {
                    this.ddlCategoryPath.Items.Add(new ListItem("├ " + dr["title"].ToString(), dr["build_path"].ToString()));
                }
                else
                {
                    this.ddlCategoryPath.Items.Add(new ListItem("├ " + dr["title"].ToString() + "(当前模板：" + dr["templet_path"].ToString() + ")", dr["build_path"].ToString()));
                }
            }
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("skinname", Type.GetType("System.String"));
            dt.Columns.Add("name", Type.GetType("System.String"));
            dt.Columns.Add("img", Type.GetType("System.String"));
            dt.Columns.Add("author", Type.GetType("System.String"));
            dt.Columns.Add("createdate", Type.GetType("System.String"));
            dt.Columns.Add("version", Type.GetType("System.String"));
            dt.Columns.Add("fordntver", Type.GetType("System.String"));

            DirectoryInfo dirInfo = new DirectoryInfo(Utils.GetMapPath("../../templates/"));
            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                DataRow dr = dt.NewRow();
                Model.template model = GetInfo(dir.FullName);
                if (model != null)
                {
                    dr["skinname"] = dir.Name;  //文件夹名称
                    dr["name"] = model.name;    // 模板名称
                    dr["img"] = "../../templates/" + dir.Name + "/about.png";   // 模板图片
                    dr["author"] = model.author;    //作者
                    dr["createdate"] = model.createdate;    //创建日期
                    dr["version"] = model.version;  //模板版本
                    dr["fordntver"] = model.fordntver;  //适用的版本
                    dt.Rows.Add(dr);
                }
            }
            this.rptList.DataSource = dt;
            this.rptList.DataBind();
        }
        #endregion

        #region 读取模板配置信息=========================
        /// <summary>
        /// 从模板说明文件中获得模板说明信息
        /// </summary>
        /// <param name="xmlPath">模板路径(不包含文件名)</param>
        /// <returns>模板说明信息</returns>
        private Model.template GetInfo(string xmlPath)
        {
            Model.template model = new Model.template();
            ///存放关于信息的文件 about.xml是否存在,不存在返回空串
            if (!File.Exists(xmlPath + @"\about.xml"))
            {
                return null;
            }
            try
            {
                XmlNodeList xnList = XmlHelper.ReadNodes(xmlPath + @"\about.xml", "about");
                foreach (XmlNode n in xnList)
                {
                    if (n.NodeType != XmlNodeType.Comment && n.Name.ToLower() == "template")
                    {
                        model.name = n.Attributes["name"] != null ? n.Attributes["name"].Value.ToString() : "";
                        model.author = n.Attributes["author"] != null ? n.Attributes["author"].Value.ToString() : "";
                        model.createdate = n.Attributes["createdate"] != null ? n.Attributes["createdate"].Value.ToString() : "";
                        model.version = n.Attributes["version"] != null ? n.Attributes["version"].Value.ToString() : "";
                        model.fordntver = n.Attributes["fordntver"] != null ? n.Attributes["fordntver"].Value.ToString() : "";
                    }
                }
            }
            catch
            {
                return null;
            }
            return model;
        }
        #endregion

        #region 全部生成模板=============================
        /// <summary>
        /// 生成全部模板
        /// </summary>
        public static void MarkTemplates(string buildPath, string skinName, siteconfig siteconfig)
        {
            //取得ASP目录下的所有文件
            string fullDirPath = Utils.GetMapPath(string.Format("{0}aspx/{1}/", siteconfig.webpath, buildPath));
            //获得URL配置列表
            BLL.url_rewrite bll = new BLL.url_rewrite();
            List<Model.url_rewrite> ls = bll.GetList("");

            DirectoryInfo dirFile = new DirectoryInfo(fullDirPath);
            //删除不属于URL映射表里的文件，防止冗余
            if (Directory.Exists(fullDirPath))
            {
                foreach (FileInfo file in dirFile.GetFiles())
                {
                    //检查文件
                    //Model.url_rewrite modelt = ls.Find(p => p.page.ToLower() == file.Name.ToLower());
                    //if (modelt == null)
                    //{
                        file.Delete();
                    //}
                }
            }

            //遍历URL配置列表
            foreach (Model.url_rewrite modelt in ls)
            {
                //如果URL配置对应的模板文件存在则生成
                string fullPath = Utils.GetMapPath(string.Format("{0}templates/{1}/{2}", siteconfig.webpath, skinName, modelt.templet));
                if (File.Exists(fullPath))
                {
                    PageTemplate.GetTemplate(siteconfig.webpath, "templates", skinName, modelt.templet, modelt.page, modelt.inherit, buildPath, 1);
                }
            }
        }
        #endregion

        //管理模板
        protected void btnManage_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                string skinName = ((HiddenField)rptList.Items[i].FindControl("hideSkinName")).Value;
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    Response.Redirect("templet_file_list.aspx?skin=" + Utils.UrlEncode(skinName));
                }
            }
        }

        //生成模板
        protected void ddlCategoryPath_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChkAdminLevel("app_templet_list", DTEnums.ActionEnum.Build.ToString()); //检查权限
            if (ddlCategoryPath.SelectedValue == "")
            {
                ddlCategoryPath.SelectedIndex = 0;
                JscriptMsg("请要选择分类站点", "", "Error");
                return;
            }
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                string skinName = ((HiddenField)rptList.Items[i].FindControl("hideSkinName")).Value;
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    MarkTemplates(ddlCategoryPath.SelectedValue, skinName, siteConfig);
                    //修改当前频道分类当前模板名
                    if (ddlCategoryPath.SelectedValue != "mobile")
                    {
                        new BLL.channel_category().UpdateField(ddlCategoryPath.SelectedValue, "templet_path='" + skinName + "'");
                    }
                    AddAdminLog(DTEnums.ActionEnum.Build.ToString(), "生成模板:" + skinName);//记录日志
                    JscriptMsg("生成模板成功！", "templet_list.aspx", "Success");
                    return;
                }
            }
            ddlCategoryPath.SelectedIndex = 0;
            JscriptMsg("请先选择要生成的模板", "", "Error");
            return;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public static string AjaxApplyTemplate(string buildPath, string templateName)
        {
            if (!IsAdminLogin())
            {
                HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "请先登录为管理员";
            }
            MarkTemplates(buildPath, templateName, new BLL.siteconfig().loadConfig());
            return "生成模版成功";
        }
    }
}