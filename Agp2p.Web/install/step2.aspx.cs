using System;
using System.IO;
using System.Text;
using System.Data;
using System.Configuration;

namespace Agp2p.Web.install
{
    public partial class install_step2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
          if (!IsPostBack)
	            {
	                string textStar = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <img src=\"images/";
	                string textEnd = ".gif\" width=\"20\" height=\"20\" />&nbsp;";
	                int s = 1;
	                if (Save())
	                {
	                    Label1.Text = textStar + "ok" + textEnd + "读取文件权限成功！<br />";
	                    Label2.Text = textStar + "ok" + textEnd + "写入文件权限成功！<br />";
	                    Label7.Text = "<img src=\"images/succeed.jpg\" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<font>安装成功!</font>";
	                    Button1.Visible = true;
	                }
	                else 
	                { 
	                    Label1.Text = textStar + "error" + textEnd + "读取文件权限无效！<br />";
	                    Label2.Text = textStar + "error" + textEnd + "写入文件权限无效！<br />";
	                }              
	            }
        }





        private bool Save()
        {
            bool err = false;
            try
            {
                string rootpath=Server.MapPath("/");
                string cmsfile=rootpath+"\\cms.html";
                StreamWriter sw = new StreamWriter(cmsfile, false, Encoding.GetEncoding("UTF-8"));
                sw.WriteLine("测试文件写入权限");
                sw.Flush();
                sw.Close();
                File.Delete(cmsfile);
                err = true;
            }
            catch 
            {
                err = false;
            }
            return err;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/Login.aspx");
        }
    }
}


