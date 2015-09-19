using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agp2p.Common;

namespace Agp2p.API.Payment.Lianlianpay
{
    public class LianlianpayService
    {
        #region 字段
        //商户编号
        private string _partner = "";
        //商户编号
        private string _busi_partner = "";
        //商户MD5密钥
        private string _key = "";
        //接口版本号
        string _version = "";						
        //字符编码格式
        private string _input_charset = "";
        //页面跳转同步返回页面文件路径
        private string _return_url = "";
        //服务器通知的页面文件路径
        private string _notify_url = "";
        //支付网关地址
        private string _gateway = "";
        //签名类型：RSA/MD5  
        private string _sign_type = "";
        // 订单有效期 单位分钟，可以为空，默认7天	      
        private string _valid_order = "10080";								
        #endregion

        public LianlianpayService()
        {
            _partner = PartnerConfig.OID_PARTNER;
            _busi_partner = PartnerConfig.BUSI_PARTNER;
            _key = PartnerConfig.MD5_KEY;
            _version = PartnerConfig.VERSION;	
            _return_url = PartnerConfig.URL_RETURN.Trim();
            _notify_url = PartnerConfig.NOTIFY_URL.Trim();
            _sign_type = PartnerConfig.SIGN_TYPE;
        }

        public string BuildReChargeFormHtml(SortedDictionary<string, string> sParaTemp, string _no_order, string _dt_order, string _money_order, string _user_id, string _userreq_ip)
        {
            GetBaseParam(sParaTemp, _no_order, _dt_order, _money_order, _user_id, _userreq_ip);

            StringBuilder sbHtml = new StringBuilder();
            sbHtml.Append("<form id='payBillForm' action='" + ServerURLConfig.PAY_URL + "' method='post'>");
            foreach (KeyValuePair<string, string> temp in sParaTemp)
            {
                sbHtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
            }
            //submit按钮控件不要含有name属性
            sbHtml.Append("<input type='submit' value='tijiao' style='display:none;'></form>");
            sbHtml.Append("<script>document.forms['payBillForm'].submit();</script>");
            return sbHtml.ToString();
        }

        public string BuildReChargeFormBankGateWayHtml(SortedDictionary<string, string> sParaTemp, string _no_order, string _dt_order, string _money_order, string _user_id, string _userreq_ip, string _bankCode)
        {
            sParaTemp.Add("bank_code", _bankCode);
            GetBaseParam(sParaTemp, _no_order, _dt_order, _money_order, _user_id, _userreq_ip);            

            StringBuilder sbHtml = new StringBuilder();
            sbHtml.Append("<form id='payBillForm' action='" + ServerURLConfig.BANK_PAY_URL + "' method='post'>");
            foreach (KeyValuePair<string, string> temp in sParaTemp)
            {
                sbHtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
            }
            //submit按钮控件不要含有name属性
            sbHtml.Append("<input type='submit' value='tijiao' style='display:none;'></form>");
            sbHtml.Append("<script>document.forms['payBillForm'].submit();</script>");
            return sbHtml.ToString();
        }

        public string BuildReChargeFormWapHtml(string _no_order, string _dt_order, string _money_order, string _user_id, string _id_no, string _acct_name)
        {
            string paramJson = GetWapParamJson(_no_order, _dt_order, _money_order, _user_id, _id_no, _acct_name);

            StringBuilder sbHtml = new StringBuilder();
            sbHtml.Append("<form id='payBillForm' action='" + ServerURLConfig.WAP_PAY_URL + "' method='post'>");
            sbHtml.Append("<input type='hidden' name='req_data' value='" + paramJson + "'/>");
            sbHtml.Append("<button name='next_btn' class='btn' type='submit' id='next_btn'>连连支付</button></form>");
            sbHtml.Append("<script>document.forms['payBillForm'].submit();</script>");
            return sbHtml.ToString();
        }

        private string GetWapParamJson(string _no_order, string _dt_order, string _money_order, string _user_id, string _id_no, string _acct_name)
        {
            //添加基本参数            
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string,string>();
            sParaTemp.Add("version", _version);
            sParaTemp.Add("oid_partner", _partner);
            sParaTemp.Add("user_id", _user_id);
            sParaTemp.Add("app_request", "3");
            sParaTemp.Add("sign_type", _sign_type);
            sParaTemp.Add("busi_partner", _busi_partner);
            sParaTemp.Add("no_order", _no_order);
            sParaTemp.Add("dt_order", _dt_order);
            sParaTemp.Add("name_goods", "充值");
            sParaTemp.Add("info_order", "用户充值");
            sParaTemp.Add("money_order", _money_order);
            sParaTemp.Add("notify_url", _notify_url);
            sParaTemp.Add("url_return", _return_url);
            sParaTemp.Add("no_agree", "");
            sParaTemp.Add("valid_order", _valid_order);
            sParaTemp.Add("id_type", "0");
            sParaTemp.Add("id_no", _id_no);
            sParaTemp.Add("acct_name", _acct_name);
            sParaTemp.Add("risk_item", createRiskItem());
            //sParaTemp.Add("card_no", "");
            //加签
            string sign = YinTongUtil.addSign(sParaTemp, PartnerConfig.TRADER_PRI_KEY, PartnerConfig.MD5_KEY);
            sParaTemp.Add("sign", sign);
            sParaTemp["risk_item"] = createRiskItem().Replace("\"", "\\\"");

            return YinTongUtil.dictToJson(sParaTemp);
        }

        private void GetBaseParam(SortedDictionary<string, string> sParaTemp, string _no_order, string _dt_order, string _money_order, string _user_id, string _userreq_ip)
        {
            //添加基本参数            
            string timestamp = YinTongUtil.getCurrentDateTimeStr();		//时间戳

            sParaTemp.Add("version", _version);
            sParaTemp.Add("oid_partner", _partner);
            sParaTemp.Add("user_id", _user_id);
            sParaTemp.Add("sign_type", _sign_type);
            sParaTemp.Add("busi_partner", _busi_partner);
            sParaTemp.Add("no_order", _no_order);
            sParaTemp.Add("dt_order", _dt_order);
            sParaTemp.Add("name_goods", "充值");
            sParaTemp.Add("info_order", "用户充值");
            sParaTemp.Add("money_order", _money_order);
            sParaTemp.Add("notify_url", _notify_url);
            sParaTemp.Add("url_return", _return_url);
            sParaTemp.Add("userreq_ip", _userreq_ip);
            sParaTemp.Add("url_order", "");
            sParaTemp.Add("valid_order", _valid_order);
            sParaTemp.Add("timestamp", timestamp);
            sParaTemp.Add("risk_item", createRiskItem());
            //加签
            string sign = YinTongUtil.addSign(sParaTemp, PartnerConfig.TRADER_PRI_KEY, PartnerConfig.MD5_KEY);

        }

        public string PostQueryOrder(string _no_order, string _dt_order)
        {
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("oid_partner", _partner);
            sParaTemp.Add("sign_type", _sign_type);
            sParaTemp.Add("no_order", _no_order);
            sParaTemp.Add("dt_order", _dt_order);
            string sign = YinTongUtil.addSign(sParaTemp, PartnerConfig.TRADER_PRI_KEY, PartnerConfig.MD5_KEY);
            sParaTemp.Add("sign", sign);

            string reqJson = YinTongUtil.dictToJson(sParaTemp);
            string responseJSON = Utils.HttpPostByte(ServerURLConfig.QUERY_ORDER, reqJson);
            return responseJSON;
        }

        /**
         * 根据连连支付风控部门要求的参数进行构造风控参数
         * @return
         */
        private String createRiskItem()
        {
            return "{\"frms_ware_category\":\"1999\",\"user_info_full_name\":\"金汇丰会员\"}";
        }
    }
}
