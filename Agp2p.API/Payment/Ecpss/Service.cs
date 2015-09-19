using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using System.Xml;
using Agp2p.Common;

namespace Agp2p.API.Payment.Ecpss
{
    public class Service
    {
        #region 字段
        //商户编号
        private string _partner = "";
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
            _key = Config.key.Trim();
            _gateway = Config.gateway;
            _return_url = Config.return_url.Trim();
            _notify_url = Config.notify_url.Trim();
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
            sParaTemp.Add("MerNo", _partner);//商户号
            sParaTemp.Add("BillNo", _no_order);//订单号
            sParaTemp.Add("Amount", _order_money);//订单金额
            sParaTemp.Add("ReturnURL", _return_url);//支付结果主动通知地址
            sParaTemp.Add("AdviceURL", _notify_url);//支付结果异步通知回调地址
            sParaTemp.Add("orderTime", _dt_order);//订单日期
            sParaTemp.Add("defaultBankNumber", _pay_id);//银行代码
            sParaTemp.Add("products", "在线充值");//产品名称         
            sParaTemp.Add("Remark", "");//备注
            //交易签名
            sParaTemp.Add("SignInfo", GetMd5Sign(_partner, _no_order, _order_money, _return_url, _key));

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
        private string GetMd5Sign(string _MerNo, string _BillNo, string _Amount, string _ReturnURL, string _Md5Key)
        {
            string mark = "&";
            string str = _MerNo + mark
                        + _BillNo + mark
                        + _Amount + mark
                        + _ReturnURL + mark
                        + _Md5Key;
            return Helper.Md5Encrypt(str);
        }

        /// <summary>
        /// 检查充值是否成功
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public bool CheckRechargeOrder(string orderNo)
        {
            string postXml = Convert.ToBase64String(Encoding.Default.GetBytes(BuildSearchOrderXml(orderNo)));
            var result = Utils.HttpPost(Config.queryUrl, "requestDomain=" + postXml);
            //解析返回的xml
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            XmlElement root = (XmlElement)doc.SelectSingleNode("root");
            //请求成功resultCode==00并且能查询到交易数据resultCount大于0
            if (root != null && root.SelectSingleNode("resultCode").InnerText == "00" && Convert.ToInt32(root.SelectSingleNode("resultCount").InnerText) > 0)
            {
                //订单状态为成功orderStatus=1
                XmlElement orderStatus = (XmlElement)root.SelectSingleNode("lists/list/orderStatus");
                return orderStatus != null && orderStatus.InnerText == "1";
            }            

            return false;
        }

        private string BuildSearchOrderXml(string orderNo)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);
            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);
            root.SetAttribute("tx", "1001");
            XmlElement merCode = doc.CreateElement("merCode");
            merCode.InnerText = Config.partner;
            root.AppendChild(merCode);
            XmlElement order = doc.CreateElement("orderNumber");
            order.InnerText = orderNo;
            root.AppendChild(order);
            XmlElement sign = doc.CreateElement("sign");
            sign.InnerText = Helper.Md5Encrypt(Config.partner + Config.key);
            root.AppendChild(sign);

            return doc.OuterXml;
        }
    }
}
