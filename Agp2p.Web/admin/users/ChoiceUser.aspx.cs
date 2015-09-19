using System;
using System.Data.Linq;
using System.Linq;
using System.Web.UI.WebControls;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Collections;
using System.Text;
using System.Data;
using System.Web;

namespace Agp2p.Web.admin.users
{
    public partial class ChoiceUser : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        private Agp2pDataContext context = new Agp2pDataContext();
        private int groupId;
        private string act;
        private string title;
        private string groupNames;
        private string mobiles = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(10); //每页数量
            page = DTRequest.GetQueryInt("page", 1);
            var group_id = DTRequest.GetQueryString("group_id");
            title = DTRequest.GetFormString("title");
            groupNames = DTRequest.GetFormString("groupNames");
            mobiles = DTRequest.GetFormString("mobiles").Trim();
            if (!string.IsNullOrWhiteSpace(title))
            {
                hidtitle.Value = title;
            }
            if (!string.IsNullOrWhiteSpace(groupNames))
            {
                hidgroupNames.Value = groupNames;
            }
            if (!string.IsNullOrWhiteSpace(mobiles))
            {
                hidmobiles.Value = mobiles;
            }
            if (!string.IsNullOrWhiteSpace(hidtitle.Value.ToString()))
            {
                dt_user_groups ug = context.dt_user_groups.FirstOrDefault(c => c.title == hidtitle.Value.ToString());
                group_id = ug.id.ToString();
            }

            if (string.IsNullOrWhiteSpace(group_id))
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            groupId = Convert.ToInt32(group_id);
            if (!Page.IsPostBack)
            {
                //ChkAdminLevel("ChoiceUser", DTEnums.ActionEnum.Edit.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;

                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            var query = QueryUser(groupId);

            totalCount = query.Count();
            rptList.DataSource =
                query.OrderByDescending(q => q.reg_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            var pageUrl = Utils.CombUrlTxt("ChoiceUser.aspx", "page={0}&group_id={1}&keywords={2}", "__id__", groupId.ToString(), txtKeywords.Text.Trim());
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
        }

        private IQueryable<dt_users> QueryUser(int groupId)
        {
            IQueryable<dt_users> query = context.dt_users;
            query = query.Where(u => u.group_id == groupId && u.dt_user_groups.is_default != 1);
            return query.Where(u => u.real_name.Contains(txtKeywords.Text) || u.user_name.Contains(txtKeywords.Text) || u.mobile.Contains(txtKeywords.Text));
        }

        #endregion

        #region 返回每页数量=============================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie(GetType().Name + "_page_size"), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    return _pagesize;
                }
            }
            return _default_size;
        }
        #endregion

        //设置分页数量
        protected void txtPageNum_TextChanged(object sender, EventArgs e)
        {
            int _pagesize;
            if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    Utils.WriteCookie(GetType().Name + "_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("ChoiceUser.aspx", "group_id={0}&page={1}&keywords={2}", groupId.ToString(), page.ToString(), txtKeywords.Text.Trim()));
        }

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("ChoiceUser.aspx", "group_id={0}&page={1}&keywords={2}}", groupId.ToString(), page.ToString(), txtKeywords.Text.Trim()));
        }

        //批量切换
        protected void btnDoSwitch_Click(object sender, EventArgs e)
        {
            var currGroupId = groupId;//Convert.ToInt32(DTRequest.GetQueryString("group_id"));
            int toGroupId = currGroupId;
            string mobile = string.Empty;
            string mobileStr = string.Empty;
            try
            {
                var sucCount = 0;
                int c =int.Parse(hidcheck.Value);  //全选
                if (c == 0)
                {
                    ArrayList al = new ArrayList();
                    al.Add(toGroupId);
                    mobile = GetGroupMobile(al);
                    for (var i = 0; i < rptList.Items.Count; i++)
                    {
                        var cb = (CheckBox) rptList.Items[i].FindControl("chkId");
                        if (cb.Checked) 
                            continue;
                        if (mobileStr == "")
                        {
                            if (!string.IsNullOrWhiteSpace(((HiddenField) rptList.Items[i].FindControl("hidId")).Value))
                            {
                                mobileStr = ((HiddenField)rptList.Items[i].FindControl("hidId")).Value;//不需要发送对象                      
                            }
                            else
                            {
                                //JscriptMsg("此用户没有注册手机信息！", "back", "Error");
                                continue;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(((HiddenField) rptList.Items[i].FindControl("hidId")).Value))
                            {
                                mobileStr = mobileStr+","+((HiddenField)rptList.Items[i].FindControl("hidId")).Value;
                            }
                            else
                            {
                                //JscriptMsg("此用户没有注册手机信息！", "back", "Error");
                                continue;
                            }
                        }
                    }
                    if (mobileStr == "")
                    {
                        if (!string.IsNullOrEmpty(hidmobiles.Value.ToString()))
                        {
                            if (!string.IsNullOrEmpty(mobile))
                            {
                                string[] mstr = mobile.Split(',');
                                string m = string.Empty;
                                string d, b;
                                string bigm = hidmobiles.Value.ToString();
                                for (var k = 0; k < mstr.Length; k++)
                                {
                                    if (mstr[k].Length == 11)
                                    {
                                        d = mstr[k];
                                        mobile = bigm.Replace(mstr[k], "") + "," + mstr[k];
                                        bigm = mobile;
                                    }
                                }
                                //mobile = mobiles + m;
                            }
                            else
                            {
                                mobile = hidmobiles.Value.ToString();
                            }

                        }
                    }
                    else
                    {
                        if (hidmobiles.Value.ToString() != "")
                        {
                            if (!string.IsNullOrEmpty(mobile))
                            {
                                string[] mstr = mobileStr.Split(',');
                                string m = string.Empty;
                                string d, b;
                                string bigm = mobile;
                                for (var k = 0; k < mstr.Length; k++)
                                {
                                    if (mstr[k].Length == 11)
                                    {
                                        mobile = mobile.Replace(mstr[k], "");
                                        bigm = mobile;
                                    }
                                }

                                string[] mstr1 = mobile.Split(',');
                                string m1 = string.Empty;
                                string d1, b1;
                                string bigm1 = hidmobiles.Value.ToString();
                                for (var k1 = 0; k1 < mstr1.Length; k1++)
                                {
                                    if (mstr1[k1].Length == 11)
                                    {
                                        d = mstr1[k1];
                                        mobile = bigm1.Replace(mstr1[k1], "") + "," + mstr1[k1];
                                        bigm1 = mobile;
                                    }
                                }

                                string bigm2 = bigm1;
                                for (var k2 = 0; k2 < mstr.Length; k2++)
                                {
                                    if (mstr[k2].Length == 11)
                                    {
                                        mobile = bigm2.Replace(mstr[k2], "");
                                        bigm2 = mobile;
                                    }
                                }
                            }
                            else
                            {
                                mobile = hidmobiles.Value.ToString();
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < rptList.Items.Count; i++)
                    {
                        var cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                        if (!cb.Checked) continue;
                        if (mobileStr == "")
                        {
                            if (!string.IsNullOrWhiteSpace(((HiddenField)rptList.Items[i].FindControl("hidId")).Value))
                            {
                                mobileStr = ((HiddenField)rptList.Items[i].FindControl("hidId")).Value;
                            }
                            else
                            {
                                JscriptMsg("此用户没有注册手机信息！", "back", "Error");
                                continue;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(((HiddenField)rptList.Items[i].FindControl("hidId")).Value))
                            {
                                mobileStr = mobileStr+","+((HiddenField)rptList.Items[i].FindControl("hidId")).Value;
                            }
                            else
                            {
                                JscriptMsg("此用户没有注册手机信息！", "back", "Error");
                                continue;
                            }
                        }
                    }

                    if (mobileStr == "")
                    {
                        if (hidmobiles.Value.ToString() != "")
                        {
                            mobile = hidmobiles.Value.ToString();
                        }
                    }
                    else
                    {
                        if (hidmobiles.Value.ToString() != "")
                        {
                            string[] mstr = mobileStr.Split(',');
                            string m = string.Empty;
                            string d, b;
                            string bigm = hidmobiles.Value.ToString();
                            for (var k = 0; k < mstr.Length; k++)
                            {
                                if (mstr[k].Length == 11)
                                {
                                    mobile = bigm.Replace(mstr[k], "") + "," + mstr[k];
                                    bigm = mobile;
                                }
                            }
                        }
                        else
                        {
                            mobile = mobileStr;
                        }

                    }
                }
                //return;
                mobile = mobile.Replace(",,", "");
                string s = mobile.Substring(0, 1);
                string l = mobile.Substring(mobile.Length - 1, 1);
                if (s.IndexOf(',') != -1)
                {
                    mobile = mobile.Substring(1, mobile.Length-1);
                }
                if (l.IndexOf(',') != -1)
                {
                    mobile = mobile.Substring(0, mobile.Length - 1);
                }


                StringBuilder sb = new StringBuilder();
                sb.Append("<form id=\"formData\">");
                sb.Append("<input type=\"hidden\" name=\"mobiles\" value=\"" + mobile + "\">");
                sb.Append("<input type=\"hidden\" name=\"Action\" value=\"cbkuser\">");
                sb.Append("<input type=\"hidden\" name=\"groupNames\" value=\"" + hidgroupNames.Value.ToString() + "\">");
                sb.Append("</form>");
                sb.Append("<script language= 'javascript'>");
                sb.Append("document.getElementById('formData').method = 'post';");
                sb.Append("document.getElementById('formData').action ='user_sms.aspx';");
                sb.Append("document.getElementById('formData').submit();");
                sb.Append("</script>");
                Response.Write(sb.ToString());
                
              
                //JscriptMsg("选择成功" ,
                //    Utils.CombUrlTxt("user_sms.aspx", "mobiles={0}&Action={1}&groupNames={2}", mobile, "cbkuser", groupNames), "Success");
            }
            catch (Exception)
            {

                StringBuilder sb = new StringBuilder();
                sb.Append("<form id=\"formData\">");
                sb.Append("<input type=\"hidden\" name=\"mobiles\" value=\"" + mobile + "\">");
                sb.Append("<input type=\"hidden\" name=\"Action\" value=\"cbkuser\">");
                sb.Append("<input type=\"hidden\" name=\"groupNames\" value=\"" + hidgroupNames.Value.ToString() + "\">");
                sb.Append("</form>");
                sb.Append("<script language= 'javascript'>");
                sb.Append("document.getElementById('formData').method = 'post';");
                sb.Append("document.getElementById('formData').action ='user_sms.aspx';");
                sb.Append("document.getElementById('formData').submit();");
                sb.Append("</script>");
                Response.Write(sb.ToString());
                //JscriptMsg("设置失败！",
                //   Utils.CombUrlTxt("user_sms.aspx", "", ""), "Failure");
            }
        }

        protected string GetGroupName()
        {
            return context.dt_user_groups.Single(g => g.id == groupId).title;
        }

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
    }
}