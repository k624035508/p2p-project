using System;
using System.Text;
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
    public partial class mortgage_type_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;
        Lip2pDataContext context = new Lip2pDataContext();

        protected string keywords = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            pageSize = GetPageSize(10); //每页数量
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_mortgage_types", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind();
            }
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            var query = context.li_mortgage_types.Where(q => q.name.Contains(txtKeywords.Text));

            totalCount = query.Count();
            rptList.DataSource = query.Skip(pageSize*(page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("mortgage_type_list.aspx", "keywords={0}&page={1}", txtKeywords.Text, "__id__");
            PageContent.InnerHtml = Utils.OutPageList(pageSize, page, totalCount, pageUrl, 8);
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

        //关健字查询
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(Utils.CombUrlTxt("mortgage_type_list.aspx", "keywords={0}", txtKeywords.Text));
        }

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
            Response.Redirect(Utils.CombUrlTxt("mortgage_type_list.aspx", "keywords={0}", txtKeywords.Text));
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("loan_mortgage_types", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    var preDel = context.li_mortgage_types.FirstOrDefault(q => q.id == id);
                    if (preDel != null)
                    {
                        sucCount += 1;
                        context.li_mortgage_types.DeleteOnSubmit(preDel);
                    }
                    else errorCount += 1;
                }
            }
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除标的物" + sucCount + "条，失败" + errorCount + "条"); //记录日志
                JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！", Utils.CombUrlTxt("mortgage_type_list.aspx", "keywords={0}", txtKeywords.Text), "Success");
            }
            catch (Exception)
            {
                JscriptMsg("删除失败！", Utils.CombUrlTxt("mortgage_type_list.aspx", "keywords={0}", txtKeywords.Text), "Failure");
            }
        }
    }
}