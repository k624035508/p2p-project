using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Agp2p.Common;
using Agp2p.Linq2SQL;

namespace Agp2p.Web.admin.loaner
{
    public partial class creditor_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;

        protected string keywords = string.Empty;
        private Agp2pDataContext context = new Agp2pDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");

            pageSize = GetPageSize(10); //每页数量
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_creditor", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                RptBind(keywords);
            }
        }

        class Creditor
        {
            public int user_id { get; set; }
            public string real_name { get; set; }
            public string tel { get; set; }
            public string id_card_number { get; set; }
            public string email { get; set; }
            public string att_creditor_id_card { get; set; }
            public DateTime last_update_time { get; set; }
        }

        #region 数据绑定=================================
        private void RptBind(string _keyword)
        {
            var options = new DataLoadOptions();
            options.LoadWith<li_creditors>(w => w.dt_users); // 类似 EF 的 Include
            context.LoadOptions = options;

            page = DTRequest.GetQueryInt("page", 1);
            //txtKeywords.Text = keywords;
            var query = from c in context.li_creditors
                        where c.real_name.Contains(txtKeywords.Text) || c.dt_users.mobile.Contains(txtKeywords.Text) || c.id_card_number.Contains(txtKeywords.Text)
                        select new Creditor
                        {
                            user_id = c.user_id,
                            real_name = c.real_name,
                            tel = c.dt_users.mobile,
                            id_card_number = c.id_card_number,
                            email = c.dt_users.email,
                            last_update_time = c.last_update_time
                        };

            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.last_update_time).Skip(pageSize * (page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("creditor_list.aspx", "keywords={0}&page={1}", txtKeywords.Text, "__id__");
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
            Response.Redirect(Utils.CombUrlTxt("creditor_list.aspx", "keywords={0}", txtKeywords.Text));
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
            Response.Redirect(Utils.CombUrlTxt("creditor_list.aspx", "keywords={0}", txtKeywords.Text));
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("loan_creditor", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    var preDel = context.li_creditors.FirstOrDefault(q => q.user_id == id);
                    if (preDel != null)
                    {
                        sucCount += 1;
                        context.li_creditors.DeleteOnSubmit(preDel);
                    }
                    else errorCount += 1;
                }
            }
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除债权人" + sucCount + "条，失败" + errorCount + "条"); //记录日志
                JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！", Utils.CombUrlTxt("creditor_list.aspx", "keywords={0}", txtKeywords.Text), "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("删除失败！" + FriendlyDBError.HandleDeleteError(ex), Utils.CombUrlTxt("creditor_list.aspx", "keywords={0}", txtKeywords.Text), "Failure");
            }
        }

        protected string formatTel(object eval)
        {
            return string.Equals(eval, "") ? "" : string.Format("{0:### #### ####}", long.Parse(eval.ToString()));
        }
    }
}