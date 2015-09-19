using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Lip2p.Common;

namespace Lip2p.Web.Plugin.LineService.admin
{
    public partial class group_list : Lip2p.Web.UI.ManagePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ChkAdminLevel("plugin_lineservice_group", DTEnums.ActionEnum.View.ToString()); //检查权限
                RptBind("", "sort_id asc,id desc");
            }
        }

        #region 数据绑定=================================
        private void RptBind(string _strWhere, string _orderby)
        {
            BLL.online_service_group bll = new BLL.online_service_group();
            this.rptList.DataSource = bll.GetList(0, _strWhere, _orderby);
            this.rptList.DataBind();
        }
        #endregion

        //设置操作
        protected void rptList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            /*int id = Convert.ToInt32(((HiddenField)e.Item.FindControl("hidId")).Value);
            BLL.online_service_group bll = new BLL.online_service_group();
            Model.online_service_group model = bll.GetModel(id);
            switch (e.CommandName.ToLower())
            {
                case "ibtnlock":
                    if (model.is_lock == 1)
                        bll.UpdateField(id, "is_lock=0");
                    else
                        bll.UpdateField(id, "is_lock=1");
                    break;
            }
            this.RptBind("", "sort_id asc,id desc");*/
        }

        //批量删除
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ChkAdminLevel("plugin_lineservice_group", DTEnums.ActionEnum.Delete.ToString()); //检查权限
            int sucCount = 0;
            int errorCount = 0;
            BLL.online_service_group bll = new BLL.online_service_group();
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
            AddAdminLog(DTEnums.ActionEnum.Delete.ToString(), "删除客服分组成功" + sucCount + "条，失败" + errorCount + "条"); //记录日志
            JscriptMsg("删除成功" + sucCount + "条，失败" + errorCount + "条！","group_list.aspx", "Success");
        }

    }
}