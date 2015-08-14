using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.admin.dialog
{
    public partial class dialog_mortgage_setting : Web.UI.ManagePage
    {
        Lip2pDataContext context = new Lip2pDataContext();
        private int risk_id;
        private string channel_id;
        private string project_id;
        private string pageName;

        protected void Page_Load(object sender, EventArgs e)
        {
            risk_id = DTRequest.GetQueryInt("risk_id");
            channel_id = DTRequest.GetQueryString("channel_id");
            project_id = DTRequest.GetQueryString("project_id");
            pageName = DTRequest.GetQueryString("pageName");

            if (channel_id == "" || project_id == "")
            {
                JscriptMsg("传输参数不正确！", "back", "Error");
                return;
            }
            
            if (!Page.IsPostBack)
            {
                var model = context.li_risks.FirstOrDefault(q => q.id == risk_id) ?? new li_risks();
                ShowInfo(model);
            }
        }

        #region 赋值操作=================================
        private void ShowInfo(li_risks risk)
        {
            ddlLoaner.Items.Clear();
            var models =
                context.li_loaners.OrderByDescending(l => l.last_update_time)
                    .Select(l => new ListItem(l.name, l.id.ToString())).ToArray();
            ddlLoaner.Items.AddRange(models);
            if (risk.loaner != null)
            {
                ddlLoaner.SelectedValue = risk.loaner.ToString();
            }
            var selectedValue = ddlLoaner.SelectedValue;
            LoadMortgageList(Convert.ToInt32(selectedValue));
        }
        #endregion

        public class MortgageItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public string typeName { get; set; }
            public decimal valuation { get; set; }
            public byte status { get; set; }
            public bool check { get; set; }
            public bool enable { get; set; }
        }

        protected void ddlLoaner_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedValue = ddlLoaner.SelectedValue;
            var loanerId = Convert.ToInt32(selectedValue);
            LoadMortgageList(loanerId);
        }

        private void LoadMortgageList(int loanerId)
        {
            // status: 抵押物是否被其他风控信息使用, check: 抵押物是否被当前风控信息使用
            // 未关联风控信息的抵押物
            var allMortgages =
                from m in context.li_mortgages
                where m.owner == loanerId
                orderby m.last_update_time descending
                select new MortgageItem
                {
                    id = m.id,
                    name = m.name,
                    typeName = m.li_mortgage_types.name,
                    valuation = m.valuation,
                    status = (byte) Lip2pEnums.MortgageStatusEnum.Mortgageable,
                    check = false,
                    enable = true
                };
            // 已关联风控信息的抵押物，如果是别的风控信息，禁用；否则可设置关联
            var mortgageInUse =
                (from m in context.li_mortgages
                    from rm in context.li_risk_mortgage
                    from r in context.li_risks
                    where
                        loanerId == m.owner && m.id == rm.mortgage && rm.risk == r.id
                        && r.id == risk_id // 新加的条件，仅显示当前的风控信息相关的绑定，暂时不考虑项目的问题
                        /*&& r.li_projects.Any(
                            p =>
                                p.status != (int) Lip2pEnums.ProjectStatusEnum.WanCheng)*/ // 有项目未完成，其他项目就不可以用此项目正在使用的抵押物
                    select new MortgageItem
                    {
                        id = m.id,
                        name = m.name,
                        typeName = m.li_mortgage_types.name,
                        valuation = m.valuation,
                        status = (byte)Lip2pEnums.MortgageStatusEnum.Mortgaged,
                        check = r.id == risk_id,
                        enable = r.id == risk_id
                    }).GroupBy(m => m.id).ToDictionary(m => m.Key, m => m.First()); // 旧数据中可能会有一个抵押物多次绑定多个未完成的风控信息的情况，这里只取第一个

            rptList.DataSource = allMortgages.Select(m => mortgageInUse.ContainsKey(m.id) ? mortgageInUse[m.id] : m);
            rptList.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int insCount = 0, delCount = 0;
            var risk = risk_id == 0 ? new li_risks() : context.li_risks.First(r => r.id == risk_id);

            var selectedLoaner = Convert.ToInt32(ddlLoaner.SelectedValue);
            if (risk.loaner == null)
            {
                risk.loaner = selectedLoaner;
                risk.last_update_time = DateTime.Now;
            }
            else if (risk.loaner != selectedLoaner) // 更换借款人后，之前的抵押物绑定需要全部删除
            {
                var dtRiskMortgages = context.li_risk_mortgage.Where(rm => rm.risk == risk_id).ToList();
                delCount += dtRiskMortgages.Count;
                context.li_risk_mortgage.DeleteAllOnSubmit(dtRiskMortgages);

                risk.loaner = selectedLoaner;
                risk.last_update_time = DateTime.Now;
            }

            for (int i = 0; i < rptList.Items.Count; i++)
            {
                int mortgageId = Convert.ToInt32(((HiddenField)rptList.Items[i].FindControl("hidId")).Value);
                CheckBox cb = (CheckBox)rptList.Items[i].FindControl("chkId");
                if (!cb.Enabled) continue;
                var riskMortgage = context.li_risk_mortgage.FirstOrDefault(rm => rm.risk == risk_id && rm.mortgage == mortgageId);
                if (cb.Checked) // 绑定抵押物
                {
                    if (riskMortgage != null) continue;
                    riskMortgage = new li_risk_mortgage
                    {
                        mortgage = mortgageId,
                        //risk = risk_id,
                        last_update_time = DateTime.Now,
                        li_risks = risk

                    };
                    context.li_risk_mortgage.InsertOnSubmit(riskMortgage);
                    insCount += 1;
                }
                else // 解除绑定
                {
                    if (riskMortgage == null) continue;
                    context.li_risk_mortgage.DeleteOnSubmit(riskMortgage);
                    delCount += 1;
                }
            }
            var action = project_id == "0" ? "Add" : "Edit";
            try
            {                
                context.SubmitChanges();
                AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "绑定标的物 " + insCount + " 条，取消绑定 " + delCount + " 条"); //记录日志

                JscriptMsg("绑定标的物 " + insCount + " 条，取消绑定 " + delCount + " 条！", Utils.CombUrlTxt("../project/project_edit_" + pageName + ".aspx", "action={0}&" +
                    "risk_id={1}&channel_id={2}&id={3}&show_risk={4}", action, risk.id.ToString(), channel_id, project_id, "true"), "Success");
            }
            catch (Exception ex)
            {
                JscriptMsg("设置失败！ " + FriendlyDBError.HandleDeleteError(ex), Utils.CombUrlTxt("../project/project_edit_" + pageName + ".aspx", "action={0}&" +
                    "risk_id={1}&channel_id={2}&id={3}&show_risk={4}", action, risk.id.ToString(), channel_id, project_id, "true"), "Failure");
            }
        }

        protected string QueryUsingProject(int mortgageId)
        {
            var mortgage = context.li_mortgages.Single(m => m.id == mortgageId);
            var projs = mortgage.li_risk_mortgage.Select(rm => rm.li_risks)
                .SelectMany(r => r.li_projects.Where(p => p.status != (int) Lip2pEnums.ProjectStatusEnum.WanCheng)).ToList();
            var projectNames = projs.Select(p => p.title).ToList();
            var riskCount = projs.GroupBy(p => p.risk_id).Count();
            return string.Join(",", projectNames) + (riskCount <= 1 ? "" : " 警告：此抵押物被多个风控信息关联");
        }
    }
}