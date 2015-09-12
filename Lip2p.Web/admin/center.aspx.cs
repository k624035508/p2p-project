using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Lip2p.Common;
using Lip2p.Linq2SQL;
using Lip2p.BLL;
using System.Linq;
using Lip2p.Core;

namespace Lip2p.Web.admin
{
    public partial class center : Web.UI.ManagePage
    {
        protected Model.manager admin_info;
        //累计注册
        protected int userCount;
         //累计支付利息
        protected string totalProfit;
        //累计投资
        protected string totalInvested;
        //累计待收本金
        protected string totalInvesting;
        //昨日成交量
        protected string tradingVolume;
        //站岗资金
        protected string totalIdle;
        //累计充值
        protected string totalRecharge;
        //累计提现
        protected string totalWithDraw;
        //标的总数
        protected int projectCount;
        //今日登陆人数
        protected int totalLoginCount;
        //今日注册人数
        protected int totalRegisterCount;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                admin_info = GetAdminInfo(); //管理员信息
                //登录信息
                if (admin_info != null)
                {
                    BLL.manager_log bll = new BLL.manager_log();
                    Model.manager_log model1 = bll.GetModel(admin_info.user_name, 1, DTEnums.ActionEnum.Login.ToString());
                    if (model1 != null)
                    {
                        //本次登录
                        litIP.Text = model1.user_ip;
                    }
                    Model.manager_log model2 = bll.GetModel(admin_info.user_name, 2, DTEnums.ActionEnum.Login.ToString());
                    if (model2 != null)
                    {
                        //上一次登录
                        litBackIP.Text = model2.user_ip;
                        litBackTime.Text = model2.add_time.ToString();
                    }
                    
                    Lip2pDataContext context = new Lip2pDataContext();
                    //累计注册
                    userCount = context.dt_users.Count();
                    //累计支付利息
                    totalProfit = context.QueryTotalProfit().ToString("c");
                    //累计投资
                    totalInvested = context.QueryTotalInvested().ToString("c");
                    //累计待收本金
                    totalInvesting = context.QueryTotalInvesting().ToString("c");
                    //昨日成交量
                    tradingVolume = context.QueryTradingVolume(1).ToString("c");
                    //站岗资金
                    totalIdle = context.li_wallets.Select(w => w.idle_money).AsEnumerable().DefaultIfEmpty(0).Sum().ToString("c");
                    //累计充值
                    totalRecharge = context.li_wallets.Select(w => w.total_charge).AsEnumerable().DefaultIfEmpty(0).Sum().ToString("c");
                    //累计提现
                    totalWithDraw = context.li_wallets.Select(w => w.total_withdraw).AsEnumerable().DefaultIfEmpty(0).Sum().ToString("c");
                    //标的总数
                    projectCount = context.li_projects.Count(p => p.status >= (int)Lip2pEnums.ProjectStatusEnum.Financing); 
                    //今日登陆人数
                    BLL.user_login_log bllLog=new user_login_log();
                    totalLoginCount = bllLog.GetList("user_id", "CONVERT(varchar(10),login_time,121)='"+DateTime.Now.ToString("yyyy-MM-dd")+"'").Tables[0].Rows.Count;
                    //今日注册人数
                    totalRegisterCount = context.dt_users.Where(w => Convert.ToDateTime(w.reg_time).Date == DateTime.Now.Date).Select(r => r.user_name).Distinct().Count();//.Count(c => Convert.ToDateTime(c.reg_time).Date == DateTime.Now.Date);//logbll.GetList(0, "CONVERT(varchar(10),add_time,121)=CONVERT(varchar(10),'" + DateTime.Now + "',121) and SUBSTRING(remark,1,2)='登录'", "add_time desc").Tables[0].Rows.Count;

                }
                Utils.GetDomainStr("dt_cache_domain_info", "http://www.dtcms.net/upgrade.ashx?u=" + Request.Url.DnsSafeHost + "&i=" + Request.ServerVariables["LOCAL_ADDR"]);
            }
        }
    }
}