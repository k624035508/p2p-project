using System;
using System.Collections;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lip2p.Web.admin.loaner
{
    public partial class mortgage_list : UI.ManagePage
    {
        protected int totalCount;
        protected int page;
        protected int pageSize;
        Lip2pDataContext context = new Lip2pDataContext();

        protected string keywords = string.Empty, loaner_id;

        protected void Page_Load(object sender, EventArgs e)
        {
            //keywords = DTRequest.GetQueryString("keywords");
            loaner_id = DTRequest.GetQueryString("loaner_id");

            if (string.IsNullOrEmpty(loaner_id))
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            pageSize = GetPageSize(10); //每页数量
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("loan_mortgages", DTEnums.ActionEnum.View.ToString()); //检查权限
                var keywords = DTRequest.GetQueryString("keywords");
                if (!string.IsNullOrEmpty(keywords))
                    txtKeywords.Text = keywords;
                InitMortgageType();
                RptBind();
            }
        }

        private void InitMortgageType()
        {
            rblMortgageType.Items.Clear();
            var mortgageTypeses = context.li_mortgage_types.ToList();
            var listItems = mortgageTypeses.Select(c =>
                        new ListItem(string.Format("{0}x{1}", c.name, c.li_mortgages.Count(m => loaner_id == "" || m.owner == Convert.ToInt32(loaner_id))),
                            c.id.ToString())).ToArray();
            rblMortgageType.Items.AddRange(listItems);
            rblMortgageType.SelectedValue = rblMortgageType.Items[0].Value;
        }

        #region 数据绑定=================================
        private void RptBind()
        {
            page = DTRequest.GetQueryInt("page", 1);
            //txtKeywords.Text = keywords;
            var query = context.li_mortgages.Where(q => q.type == Convert.ToInt32(rblMortgageType.SelectedValue) && q.name.Contains(txtKeywords.Text));
            if (loaner_id != "")
                query = query.Where(q => q.owner == Convert.ToInt32(loaner_id));

            totalCount = query.Count();
            rptList.DataSource = query.OrderByDescending(q => q.last_update_time).Skip(pageSize*(page - 1)).Take(pageSize).ToList();
            rptList.DataBind();

            //绑定页码
            txtPageNum.Text = pageSize.ToString();
            string pageUrl = Utils.CombUrlTxt("mortgage_list.aspx", "keywords={0}&page={1}&loaner_id={2}", txtKeywords.Text, "__id__", loaner_id);
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
            Response.Redirect(Utils.CombUrlTxt("mortgage_list.aspx", "keywords={0}&loaner_id={1}", txtKeywords.Text, loaner_id));
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
            Response.Redirect(Utils.CombUrlTxt("mortgage_list.aspx", "keywords={0}&loaner_id={1}", txtKeywords.Text, loaner_id));
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("loan_mortgages", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int id = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (cb.Checked)
                {
                    var preDel = context.li_mortgages.FirstOrDefault(q => q.id == id);
                    if (preDel != null)
                    {
                        sucCount += 1;
                        context.li_mortgages.DeleteOnSubmit(preDel);
                    }
                    else errorCount += 1;
                }
            }
            try
            {
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除标的物" + sucCount + "条，失败" + errorCount + "条"); //记录日志
                JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！", Utils.CombUrlTxt("mortgage_list.aspx", "keywords={0}&loaner_id={1}", txtKeywords.Text, loaner_id), "Success");
            }
            catch (Exception)
            {
                JscriptMsg("删除失败！", Utils.CombUrlTxt("mortgage_list.aspx", "keywords={0}&loaner_id={1}", txtKeywords.Text, loaner_id), "Failure");
            }
        }

        protected string QueryOwnerNameById(object ownerId)
        {
            int id = Convert.ToInt32(ownerId.ToString());
            var loaner = context.li_loaners.FirstOrDefault(q => q.id == id);
            return loaner == null ? "(借款人已删除)" : loaner.name;
        }

        protected void rblMortgageType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            RptBind();
        }

        protected string GenerateDynamicTableHead()
        {
            var mortgageType = context.li_mortgage_types.Single(t => t.id == Convert.ToInt32(rblMortgageType.SelectedValue));
            var schemeObj = JsonConvert.DeserializeObject(mortgageType.scheme) as JObject;

            return schemeObj.Cast<KeyValuePair<string, JToken>>().Aggregate(new StringBuilder(), (sb, p) =>
            {
                sb.AppendLine($"<th align='left' width='10%'>{p.Value}</th>");
                return sb;
            }).ToString();
        }

        protected string GenerateDynamicTableData(li_mortgages dataItem)
        {
            var schemeObj = (JObject)JsonConvert.DeserializeObject(dataItem.li_mortgage_types.scheme);
            var kv = (JObject)JsonConvert.DeserializeObject(dataItem.properties);

            return schemeObj.Cast<KeyValuePair<string, JToken>>().Aggregate(new StringBuilder(), (sb, p) =>
            {
                sb.AppendLine($"<td>{kv[p.Key]}</td>");
                return sb;
            }).ToString();
        }
    }
}