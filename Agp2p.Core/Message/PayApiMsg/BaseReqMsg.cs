using System;
using System.Collections.Generic;
using TinyMessenger;

namespace Agp2p.Core.Message.PayApiMsg
{
    public class BaseReqMsg : ITinyMessage
    {
        public int Api { get; set; }
        public string ApiInterface { get; set; }
        public string RequestId { get; set; }
        public int? UserId { get; set; }
        public string ProjectCode { get; set; }
        public string RequestContent { get; set; }
        public string Remarks { get; set; }//1、手动还款：记录是否提前还款、还款计划id isEarly=false&repayTaskId="1" 

        public BaseReqMsg()
        {
        }

        public object Sender
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        public virtual string GetSignature()
        {
            throw new NotImplementedException();
        }
    }
}