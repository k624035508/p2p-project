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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Agp2p.API.Payment.Lianlianpay;
using Agp2p.BLL;
using System.Linq;
using Agp2p.Core;

/// <summary>
/// 功能：服务器异步通知页面
/// 说明：
/// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
/// ///////////////////页面功能说明///////////////////
/// 创建该页面文件时，请留心该页面文件中无任何HTML代码及空格。
/// 该页面不能在本机电脑测试，请到服务器上做测试。请确保外部可以访问该页面。
/// </summary>
namespace Agp2p.Web.api.payment.lianlianpay
{
    public partial class notify_url : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SortedDictionary<string, string> sPara = GetRequestPost();
            if (sPara.Count > 0)//判断是否有带返回参数
            {

                Console.WriteLine("接收支付异步通知数据：【" + sPara.ToString() + "】");

                if (!YinTongUtil.checkSign(sPara, PartnerConfig.YT_PUB_KEY, //验证失败
                    PartnerConfig.MD5_KEY))
                {
                    Response.Write(@"{""ret_code"":""9999"",""ret_msg"":""验签失败""}");
                    Console.WriteLine("支付异步通知验签失败");
                    return;
                }
                else
                {
                    Response.Write(@"{""ret_code"":""0000"",""ret_msg"":""交易成功""}");
                    Response.Write("支付异步通知数据接收处理成功");
                }

                try
                {
                    var context = new Agp2p.Linq2SQL.Agp2pDataContext();
                    var order = context.li_bank_transactions.FirstOrDefault(b => b.no_order == sPara["no_order"]);
                    if (order != null)
                    {
                        context.ConfirmBankTransaction(order.id, null);
                    }
                }
                catch (Exception ex)
                {                    
                    Response.Write(@"{""ret_code"":""9999"",""ret_msg"":"+ex.Message+"}");
                }
            }
            else
            {
                Response.Write(@"{""ret_code"":""9999"",""ret_msg"":""交易失败""}");
            }           
        }

        /// <summary>
        /// 获取POST过来通知消息，并以“参数名=参数值”的形式组成字典
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestPost()
        {
            string reqStr = readReqStr();

            SortedDictionary<string, string> sArray = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(reqStr);
            return sArray;
        }


        //从request中读取流，组成字符串返回
        public String readReqStr()
        {
            StringBuilder sb = new StringBuilder();
            //Stream inputStream = Request.GetBufferlessInputStream();
            Stream inputStream = Request.InputStream;
            StreamReader reader = new StreamReader(inputStream, System.Text.Encoding.UTF8);

            String line = null;
            while ((line = reader.ReadLine()) != null)
            {
                sb.Append(line);
            }
            reader.Close();
            return sb.ToString();

        }
    }
}


