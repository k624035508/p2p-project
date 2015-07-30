using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.Advert.admin
{
    public partial class bar_list : Lip2p.Web.UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected int aid = 0;
        protected string keywords = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.aid = DTRequest.GetQueryInt("aid");
            //
            this.keywords = DTRequest.GetQueryString("keywords");

            this.pageSize = GetPageSize(10); //每页数量
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("plugin_advert_bar", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                TreeBind(); //绑定广告位
                RptBind("id>0" + CombSqlTxt(this.aid, txtKeywords.Text), "is_lock asc,sort_id asc,add_time desc");
            }
        }

        #region 绑定广告位===============================
        private void TreeBind()
        {
            BLL.advert bll = new BLL.advert();
            DataTable dt = bll.GetList("").Tables[0];

            this.ddlAdvertId.Items.Clear();
            this.ddlAdvertId.Items.Add(new ListItem("所有广告位", ""));
            foreach (DataRow dr in dt.Rows)
            {
                this.ddlAdvertId.Items.Add(new ListItem(dr["title"].ToString(), dr["id"].ToString()));
            }
        }
        #endregion

        #region 数据绑定=================================
        private void RptBind(string _strWhere, string _orderby)
        {
            this.page = DTRequest.GetQueryInt("page", 1);
            //this.txtKeywords.Text = this.keywords;
            if (this.aid > 0)
            {
                this.ddlAdvertId.SelectedValue = this.aid.ToString();
            }
            BLL.advert_banner bll = new BLL.advert_banner();
            this.rptList.DataSource = bll.GetList(this.pageSize, this.page, _strWhere, _orderby, out this.totalCount);
            this.rptList.DataBind();
            //绑定页码
            txtPageNum.Text = this.pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("bar_list.aspx", "aid={0}&keywords={1}&page={2}", this.aid.ToString(), txtKeywords.Text, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(this.pageSize, this.page, this.totalCount, pageUrl, 8);
        }
        #endregion

        #region 组合SQL查询语句==========================
        protected string CombSqlTxt(int _aid, string _keywords)
        {
            StringBuilder strTemp = new StringBuilder();
            if (_aid > 0)
            {
                strTemp.Append(" and aid=" + _aid);
            }
            _keywords = _keywords.Replace("'", "");
            if (!string.IsNullOrEmpty(_keywords))
            {
                strTemp.Append(" and title like '%" + _keywords + "%'");
            }
            return strTemp.ToString();
        }
        #endregion

        #region 返回图文每页数量=========================
        private int GetPageSize(int _default_size)
        {
            int _pagesize;
            if (int.TryParse(Utils.GetCookie("advert_bar_page_size"), out _pagesize))
            {
                if (_pagesize > 0)
                {
                    return _pagesize;
                }
            }
            return _default_size;
        }
        #endregion

        #region 返回广告条状态===========================
        protected string GetState(string strLock, string startTime, string endTime)
        {
            if (int.Parse(strLock) == 1)
            {
                return "<font color=\"#999999\">暂停</font>";
            }
            else if (DateTime.Compare(DateTime.Parse(startTime), DateTime.Today) > 0)
            {
                return "<font color=\"#009900\">未生效</font>";
            }
            else if (DateTime.Compare(DateTime.Parse(endTime), DateTime.Today) == -1)
            {
                return "<font color=\"#FF0000\">已过期</font>";
            }
            else
            {
                return "<font color=\"#009900\">正常</font>";
            }
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
                    Utils.WriteCookie("advert_bar_page_size", _pagesize.ToString(), 14400);
                }
            }
            Response.Redirect(Utils.CombUrlTxt("bar_list.aspx", "aid={0}&keywords={1}", this.aid.ToString(), txtKeywords.Text));
        }

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("bar_list.aspx", "aid={0}&keywords={1}", this.aid.ToString(), txtKeywords.Text));
        }

        //筛选广告位
        protected void ddlAdvertId_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("bar_list.aspx", "aid={0}&keywords={1}", ddlAdvertId.SelectedValue, txtKeywords.Text));
        }

        //保存排序
        protected void btnSave_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_advert_bar", DTEnums.ActionEnum.Edit.ToString()); //检查权限
            BLL.advert_banner bll = new BLL.advert_banner();
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                int sortId;
                if (!int.TryParse(((TextBox)rptList.Items[i].FindControl("txtSortId")).Text.Trim(), out sortId))
                {
                    sortId = 99;
                }
                bll.UpdateField(id, "sort_id=" + sortId.ToString());
            }
            AddAdminLog(DTEnums.ActionEnum.Edit.ToString(), "修改广告内容排序"); //记录日志
            JscriptMsg("保存排序成功！", Utils.CombUrlTxt("bar_list.aspx", "aid={0}&keywords={1}", this.aid.ToString(), txtKeywords.Text), "Success");
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_advert_bar", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            BLL.advert_banner bll = new BLL.advert_banner();
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    if (bll.Delete(id))
                    {
                        sucCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
            }
            AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除广告内容成功" + sucCount + "条，失败" + errorCount + "条"); //记录日志
            JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！",
                Utils.CombUrlTxt("bar_list.aspx", "aid={0}&keywords={1}", this.aid.ToString(), txtKeywords.Text), "Success");
        }

    }
}