using System;
using Agp2p.Linq2SQL;
using Agp2p.BLL;
using Agp2p.Common;
using System.Collections.Generic;
using Agp2p.API.Payment.Lianlianpay;
using System.Text;
using Agp2p.Core;

namespace Agp2p.Web.api.payment.lianlianpay
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = DTRequest.GetFormString("action");
            if (action.Equals("recharge") || action.Equals("bankgateway") || action.Equals("wap"))//测试充值
            {
                int user_id = Utils.StrToInt(DTRequest.GetFormString("user_id"), 0);
                string amount = DTRequest.GetFormString("amount");                
                //创建充值订单
                var context = new Agp2pDataContext();
                var charge_order = context.Charge(user_id, decimal.Parse(amount), (byte)Agp2pEnums.PayApiTypeEnum.Lianlianpay);

                //请求连连快捷支付接口                
                var service = new LianlianpayService();
                string sHtmlText = "";
                if (action.Equals("recharge"))
                {
                    var sParaTemp = getParamDict(user_id);                    
                    sHtmlText = service.BuildReChargeFormHtml(sParaTemp, charge_order.no_order, charge_order.create_time.ToString("yyyyMMddHHmmss"), amount, user_id.ToString(), Request.UserHostAddress);
                }
                else if (action.Equals("wap"))
                {
                    //手机端wap支付
                    var sParaTemp = getParamDict(user_id);
                    sHtmlText = service.BuildReChargeFormWapHtml(charge_order.no_order, charge_order.create_time.ToString("yyyyMMddHHmmss"), amount, user_id.ToString(), sParaTemp["id_no"], sParaTemp["acct_name"]);
                }
                else
                {
                    //获取银行编号
                    string bankName = DTRequest.GetFormString("bankcode");
                    int bankCode = (int)(Agp2pEnums.LianlianpayBankCodeEnum)Enum.Parse(typeof(Agp2pEnums.LianlianpayBankCodeEnum), bankName);

                    SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                    sHtmlText = service.BuildReChargeFormBankGateWayHtml(sParaTemp, charge_order.no_order, charge_order.create_time.ToString("yyyyMMddHHmmss"), amount, user_id.ToString(), Request.UserHostAddress, "0" + bankCode);
                }
                Response.Write(sHtmlText);
            }
            else//测试查询支付订单
            {
                var service = new LianlianpayService();
                string returnText = service.PostQueryOrder(DTRequest.GetFormString("no_order"), DTRequest.GetFormString("dt_order"));
            }            
        }

        /**
         * 卡前置模式 额外需要的参数
         */
        private SortedDictionary<string, string> getParamDict(int _user_id)
        {
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            //查询会员信息
            //var user = new BLL.users().GetModel(_user_id);
            //if (user != null)
            //{
                sParaTemp.Add("id_type", "0");				    //证件类型
                sParaTemp.Add("id_no", DTRequest.GetFormString("id_card_no"));  //身份证
                sParaTemp.Add("acct_name", DTRequest.GetFormString("card_username"));
                sParaTemp.Add("flag_modify", "1");
                sParaTemp.Add("back_url", PartnerConfig.URL_BACK);

                string no_agree = "";
                if (!YinTongUtil.isnull(Request["no_agree"]))
                    no_agree = Request["no_agree"].Trim();

                if (!YinTongUtil.isnull(no_agree))
                {
                    sParaTemp.Add("pay_type", "D");
                    sParaTemp.Add("no_agree", no_agree);
                }
                else
                {
                    string card_no = Request["card_no"].Trim();
                    sParaTemp.Add("card_no", card_no);
                }
            //}
            //else
            //    throw new InvalidOperationException("找不到会员信息！会员id为：" + _user_id);

            return sParaTemp;
        }
    }
}