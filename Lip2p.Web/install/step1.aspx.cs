using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;

using System.Xml;
using System.Data.SqlClient;
using System.IO;


namespace Lip2p.Web.install
{
    public partial class install_step1 : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {

            string str = Server.MapPath("~/Install/");
            string path = str + "installok.ok";
            if (File.Exists(path))
            {
                base.Response.Write("<span style='color:#FF0000;'>请先删除Install目录下的installok.ok文件再进行安装！</span>");
                base.Response.End();
            }
            if (!IsPostBack)
            {
                txtSiteName.Text ="DTCMS";
                labState.Text = "初始设置...";
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {


            //重启程序
            System.Web.HttpRuntime.UnloadAppDomain();
            System.Threading.Thread.Sleep(200);
            string error = CreateDatabase();
            if (error == "")
            {
                //转到下一个步骤
                Response.Redirect("step2.aspx");
            }

            litMsg.Text = error;



        }






        public  void ConfigSave(string appkey, string appvalue, Dictionary<string, string> dic)
        {
            if (dic.ContainsKey(appkey))
            {
                dic[appkey] = appvalue;
            }
            else
            {
                dic.Add(appkey, appvalue);
            }
        }

        public  void ConfigSaveAll(Dictionary<string, string> dic)
        {
            Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(System.Web.HttpContext.Current.Request.ApplicationPath);
            ConnectionStringsSection conSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            foreach (KeyValuePair<string, string> item in dic)
            {
                if (conSection.ConnectionStrings[item.Key] == null)
                    conSection.ConnectionStrings.Add(new ConnectionStringSettings(item.Key, item.Value));
                else
                    conSection.ConnectionStrings[item.Key].ConnectionString = item.Value;
            }
            config.Save();
        }

        private string CreateDatabase()
        {
            try
            {
                string error = string.Empty;
                string str3 = Server.MapPath("/Install/db.sql");
                string ServerName = txtServerName.Text.Trim();
                string DbUserName = txtDbUserName.Text.Trim();
                string DbUserPass = txtDbUserPass.Text.Trim();
                string DbName = txtDbName.Text.Trim();
                if ((ServerName == string.Empty) || (DbUserName == string.Empty) || (DbName == string.Empty))
                {
                    return "您输入的信息不完整，请重试";
                }
                else if (!Save())
                {
                    return "数据库连接失败";
                }
                else
                {
                    bool flag = true;
                    string connectionString = string.Format("server={0};database={1};uid={2};pwd={3}", new object[] { ServerName, DbName, DbUserName, DbUserPass });
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {

                        try
                        {
                            error = "数据库连接失败";
                            try
                            {
                                connection.Open();
                            }
                            catch
                            {
                                return DbName + "数据库不存在！请先创建" + DbName;
                            }
                            error = "当前数据库用户没有权限创建表";
                            SqlCommand command = new SqlCommand("IF EXISTS (SELECT name FROM sysobjects WHERE name = 'a' AND type = 'U')DROP table a;create table a(id int not null default 0);drop table a;", connection);
                            command.ExecuteNonQuery();

                            string str12 = File.ReadAllText(str3, Encoding.Default);
                            string[] separator = new string[] { "[go]" };
                            string[] strArray2 = str12.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            error = "数据库安装失败";
                            for (int i = 0; i < strArray2.Length; i++)
                            {
                                command = new SqlCommand(strArray2[i].Replace("{前缀}", txtDbPrefix.Text.Trim()), connection);
                                // str12 = strArray2[i];
                                command.ExecuteNonQuery();
                            }

                            BLL.siteconfig bll = new BLL.siteconfig();
                            Model.siteconfig model = bll.loadConfig();
                            model.webpath = txtSiteUrl.Text.Trim();
                            model.webname = txtSiteName.Text.Trim();

                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            //写配置文件
                            ConfigSave("ConnectionString", connectionString, dic);
                            //保存成功
                            ConfigSaveAll(dic);

                            string Okpath = Server.MapPath("~/Install/installok.ok");
                            File.WriteAllText(Okpath, "ok");
                            return "";
                        }
                        catch (Exception ex)
                        {
                            return "安装出现错误:" + ex.ToString();
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }

                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private bool Save()
        {
            bool err = false;
            try
            {
                string rootpath = Server.MapPath("/");
                string cmsfile = rootpath + "\\cms.html";
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
    }


}

