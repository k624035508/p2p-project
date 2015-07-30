using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Lip2p.API.Payment.Baofoo
{
    public class Service
    {
        #region 字段
        //商户编号
        private string _partner = "";

        private string _terminal_id = "";
        private string _interface_version = "";
        private string _key_type = "";
        //商户MD5密钥
        private string _key = "";
        //字符编码格式
        private string _input_charset = "utf-8";
        //页面跳转同步返回页面文件路径
        private string _return_url = "";
        //服务器通知的页面文件路径
        private string _notify_url = "";
        //支付网关地址
        private string _gateway = "";
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public Service()
        {
            _partner = Config.partner.Trim();
            _terminal_id = Config.terminal_id.Trim();
            _key = Config.key.Trim();
            _key_type = Config.key_type.Trim();
            _gateway = Config.gateway;
            _return_url = Config.return_url.Trim();
            _notify_url = Config.notify_url.Trim();
            _interface_version = Config.interface_version.Trim();
        }

        /// <summary>
        /// 构造提交表单HTML数据
        /// </summary>
        /// <param name="dicPara">请求参数数组</param>
        /// <param name="strMethod">提交方式。两个值可选：post、get</param>
        /// <param name="strButtonValue">确认按钮显示文字</param>
        /// <returns>提交表单HTML文本</returns>
        public string BuildFormHtml(string _no_order, string _dt_order, string _order_money, string _pay_id)
        {
            StringBuilder sbHtml = new StringBuilder();

            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("MemberID", _partner);//商户号
            sParaTemp.Add("TerminalID", _terminal_id);//终端号
            sParaTemp.Add("InterfaceVersion", _interface_version);//接口版本
            sParaTemp.Add("KeyType", _key_type);//加密类型
            sParaTemp.Add("PayID", _pay_id);//功能id 空就是不选银行
            sParaTemp.Add("TradeDate", _dt_order);//订单日期
            sParaTemp.Add("TransID", _no_order);//订单号
            sParaTemp.Add("OrderMoney", _order_money);//订单金额
            sParaTemp.Add("ProductName", "充值");//产品名称
            sParaTemp.Add("Amount", "1");//产品数量
            sParaTemp.Add("Username", "");//用户名称
            sParaTemp.Add("AdditionalInfo", "");//附加字段
            sParaTemp.Add("NoticeType", "1");//通知类型 0 不跳转 1 会跳转
            sParaTemp.Add("PageUrl", _notify_url);//支付结果异步通知回调地址
            sParaTemp.Add("ReturnUrl", _return_url);//支付结果主动通知地址
            sParaTemp.Add("Md5Sign", _key);//支付结果主动通知地址
            //交易签名
            sParaTemp.Add("Signature", GetMd5Sign(_partner, _pay_id, _dt_order, _no_order, _order_money, _notify_url, _return_url, "1", _key));

            sbHtml.Append("<form id='baofoosubmit' name='baofoosubmit' action='" + _gateway + "' encoding=" + _input_charset + "' method='post'>");
            foreach (KeyValuePair<string, string> temp in sParaTemp)
            {
                sbHtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
            }

            sbHtml.Append("<input type='submit' value='submit' style='display:none;'></form>");
            sbHtml.Append("<script>document.forms['baofoosubmit'].submit();</script>");
            return sbHtml.ToString();
        }

        //md5签名
        private string GetMd5Sign(string _MerchantID, string _PayID, string _TradeDate, string _TransID,
            string _OrderMoney, string _Page_url, string _Return_url, string _NoticeType, string _Md5Key)
        {
            string mark = "|";
            string str = _MerchantID + mark
                        + _PayID + mark
                        + _TradeDate + mark
                        + _TransID + mark
                        + _OrderMoney + mark
                        + _Page_url + mark
                        + _Return_url + mark
                        + _NoticeType + mark
                        + _Md5Key;
            return Helper.Md5Encrypt(str);
        }
    }
}
