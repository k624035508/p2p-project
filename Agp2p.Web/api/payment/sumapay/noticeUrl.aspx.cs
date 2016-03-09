using System;
using System.Collections.Generic;
using System.IO;
using Agp2p.Common;
using Agp2p.Linq2SQL;
using System.Linq;
using System.Text;
using Agp2p.Core.Message;
using Agp2p.Core.Message.PayApiMsg;
using Newtonsoft.Json;

namespace Agp2p.Web.api.payment.sumapay
{
    public partial class noticeUrl : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string reqStr = ReadReqStr();
            var sd = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(reqStr);
            if (!string.IsNullOrEmpty(sd["requestId"]))
            {
                Agp2pDataContext context = new Agp2pDataContext();
                //保存响应日志
                var responseLog = new li_pay_response_log()
                {
                    request_id = sd["requestId"],
                    result = sd["result"],
                    status = (int)Agp2pEnums.SumapayResponseEnum.Return,
                    response_time = DateTime.Now,
                    response_content = reqStr
                };
                context.li_pay_response_log.InsertOnSubmit(responseLog);
                context.SubmitChanges();
                //发送响应消息
                MessageBus.Main.PublishAsync(new BaseRespMsg(responseLog.request_id, responseLog.result, responseLog.response_content));
            }
            else
            {
                //TODO 记录错误信息
            }
        }

        /// <summary>
        /// 从request中读取流，组成字符串返回
        /// </summary>
        /// <returns></returns>
        public string ReadReqStr()
        {
            StringBuilder sb = new StringBuilder();
            //Stream inputStream = Request.GetBufferlessInputStream();
            Stream inputStream = Request.InputStream;
            StreamReader reader = new StreamReader(inputStream, System.Text.Encoding.UTF8);

            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                sb.Append(line);
            }
            reader.Close();
            return sb.ToString();

        }
    }
}
