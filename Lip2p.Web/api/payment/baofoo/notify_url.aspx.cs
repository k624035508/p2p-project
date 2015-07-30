using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.Configuration;
using Lip2p.API.Payment.Baofoo;

namespace Lip2p.Web.api.payment.baofoo
{
    public partial class notify_url : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string MemberID = Request.Params["MemberID"];//商户号
            string TerminalID = Request.Params["TerminalID"];//商户终端号
            string TransID = Request.Params["TransID"];//商户流水号
            string Result = Request.Params["Result"];//支付结果(1:成功,0:失败)
            string ResultDesc = Request.Params["ResultDesc"];//支付结果描述
            string FactMoney = Request.Params["FactMoney"];//实际成交金额
            string AdditionalInfo = Request.Params["AdditionalInfo"];//订单附加消息
            string SuccTime = Request.Params["SuccTime"];//交易成功时间
            string Md5Sign = Request.Params["Md5Sign"].ToLower();//md5签名
            string Md5Key = ConfigurationManager.AppSettings["Md5key"];//密钥 双方约定
            String mark = "~|~";//分隔符

            string _Md5Key = WebConfigurationManager.AppSettings["Md5key"];

            string _WaitSign = "MemberID=" + MemberID + mark + "TerminalID=" + TerminalID + mark + "TransID=" + TransID + mark + "Result=" + Result + mark + "ResultDesc=" + ResultDesc + mark
                 + "FactMoney=" + FactMoney + mark + "AdditionalInfo=" + AdditionalInfo + mark + "SuccTime=" + SuccTime
                 + mark + "Md5Sign=" + Md5Key;


            if (Md5Sign.ToLower() == Helper.Md5Encrypt(_WaitSign).ToLower())
            {

                lbMoney.Text = (float.Parse(FactMoney) / 100).ToString() + " 元";
                lbDate.Text = SuccTime;
                lbFlag.Text = Helper.GetErrorInfo(Result, ResultDesc) + "-====";
                lbOrderID.Text = TransID;
            }
            else
            {
                Response.Write("校验失败");
            }
        }

    }
}
