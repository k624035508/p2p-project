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
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Agp2p.API.Payment.Lianlianpay;
using Agp2p.Common;

namespace Agp2p.Web.api.payment.lianlianpay
{
    /// <summary>
    /// 用户已绑定银行卡信息查询 卡bin校验查询
    /// </summary>
    /// 
    public partial class infoQuery : System.Web.UI.Page
    {
        private static string QUERY_BANKCARD_LIST = "query_bankcard_list"; // 查询已绑定银行卡列表
        private static string QUERY_CARD_BIN = "query_card_bin";     // 查询银行卡卡bin信息

        protected void Page_Load(object sender, EventArgs e)
        {

            string operate = Request["operate"];
            string responseJson = "";
            if (QUERY_BANKCARD_LIST.Equals(operate))
            {
                responseJson = queryBankcardList();
            }
            else if (QUERY_CARD_BIN.Equals(operate))
            {
                responseJson = queryCardBin();
            }
            else
            {
                responseJson = "{\"ret_code\":\"9999\",\"ret_msg\":\"非法交易\"}";
            }

            Response.Write(responseJson);
        }

        /// <summary>
        /// 用户已绑定银行卡信息查询
        /// </summary>
        /// 
        public string queryBankcardList()
        {
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("oid_partner", PartnerConfig.OID_PARTNER);
            sParaTemp.Add("user_id", "1073");
            sParaTemp.Add("offset", "0");
            sParaTemp.Add("pay_type", "D");
            sParaTemp.Add("sign_type", PartnerConfig.SIGN_TYPE);
            string sign = YinTongUtil.addSign(sParaTemp, PartnerConfig.TRADER_PRI_KEY, PartnerConfig.MD5_KEY);
            sParaTemp.Add("sign", sign);

            string reqJson = YinTongUtil.dictToJson(sParaTemp);

            Console.WriteLine("用户已绑定银行列表查询-请求报文[" + reqJson + "]");
            string responseJSON = Utils.HttpPostByte(ServerURLConfig.QUERY_USER_BANKCARD_URL, reqJson);
            Console.WriteLine("用户已绑定银行列表查询-响应报文[" + responseJSON + "]");
            return responseJSON;
        }

        /// <summary>
        /// 卡bin校验查询
        /// </summary>
        /// 
        public string queryCardBin()
        {
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("oid_partner", PartnerConfig.OID_PARTNER);
            sParaTemp.Add("card_no", Request["card_no"]);
            sParaTemp.Add("sign_type", PartnerConfig.SIGN_TYPE);
            string sign = YinTongUtil.addSign(sParaTemp, PartnerConfig.TRADER_PRI_KEY, PartnerConfig.MD5_KEY);
            sParaTemp.Add("sign", sign);

            string reqJson = YinTongUtil.dictToJson(sParaTemp);

            Console.WriteLine("银行卡卡bin信息查询-请求报文[" + reqJson + "]");
            string responseJSON = Utils.HttpPostByte(ServerURLConfig.QUERY_BANKCARD_URL, reqJson);
            Console.WriteLine("银行卡卡bin信息查询-响应报文[" + responseJSON + "]");
            return responseJSON;
        }
    }

}


