using System;
using System.ComponentModel;
using System.Linq;
using Agp2p.API.Payment.Ecpss;
using Agp2p.Core;

namespace Agp2p.Web.api.payment.ecpss
{
    public partial class return_url : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string billNo = Request.Params["BillNo"];//商户流水号
            string amount = Request.Params["Amount"];//实际成交金额
            string succeed = Request.Params["Succeed"];//状态码
            string result = Request.Params["Result"];//支付结果描述
            string signMD5info = Request.Params["SignMD5info"];//md5签名

            if (Helper.CheckReturnMD5(billNo, amount, succeed, signMD5info))
            {
                lbMoney.Text = amount.ToString() + " 元";
                lbFlag.Text = Helper.GetResultInfo(succeed) + "-====";
                lbOrderID.Text = billNo;
                if (succeed.Equals("88"))
                {
                    try
                    {
                        var context = new Agp2p.Linq2SQL.Agp2pDataContext();
                        var order = context.li_bank_transactions.FirstOrDefault(b => b.no_order == billNo);
                        //如果异步通知没收到，则在返回页面处理订单
                        if (order != null && order.status == (int)Agp2p.Common.Agp2pEnums.BankTransactionStatusEnum.Acting)
                        {
                            context.ConfirmBankTransaction(order.id, null);
                            new BLL.manager_log().Add(1, "admin", "ReCharge", "支付成功（" + billNo + "）！来自同步通知。");
                        }                        
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidEnumArgumentException("确认充值信息失败（" + billNo + "）：" + ex.Message);
                    }
                    Response.Redirect(new Web.UI.BasePage().linkurl("mytrade","mytrade"));
                }
                else
                    new BLL.manager_log().Add(1, "admin", "ReCharge", "支付失败（" + billNo + "）：" + Helper.GetResultInfo(succeed));
            }
            else
            {
                new BLL.manager_log().Add(1, "admin", "ReCharge", "支付失败（" + billNo + "），MD5验证不通过！");
                Response.Write("校验失败");
            }
        }

    }
}
