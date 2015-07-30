using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.SessionState;
using Lip2p.BLL;
using Lip2p.Common;
using Lip2p.Core;
using Lip2p.Linq2SQL;

namespace Lip2p.Web.tools
{
    /// <summary>
    /// calc_stand_guard_fee 的摘要说明
    /// </summary>
    public class calc_stand_guard_fee : IHttpHandler
    {
        Lip2pDataContext context = new Lip2pDataContext();

        public void ProcessRequest(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.TrySkipIisCustomErrors = true;

            CalcStandGuardFee((statusCode, handlingFee, msg) =>
            {
                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new {fee = msg, value = handlingFee}));
            });
        }

        protected void CalcStandGuardFee(Action<int, decimal, string> callback)
        {
            var userId = DTRequest.GetQueryInt("user_id");
            var withdrawValue = DTRequest.GetQueryDecimal("withdraw_value", 0);

            if (withdrawValue <= 0)
            {
                callback((int) HttpStatusCode.BadRequest, 0, "请先填写正确的提现金额");
                return;
            }
            try
            {
                var standGuardFee = context.CalcStandGuardFee(userId, Convert.ToDecimal(withdrawValue));
                if (standGuardFee == 0)
                {
                    callback((int)HttpStatusCode.OK, TransactionFacade.DefaultHandlingFee, "提现手续费 " + TransactionFacade.DefaultHandlingFee.ToString("c"));
                }
                else
                {
                    var finalValue = standGuardFee < TransactionFacade.DefaultHandlingFee ? TransactionFacade.DefaultHandlingFee : standGuardFee;
                    callback((int)HttpStatusCode.OK, finalValue, "提现手续费 " + finalValue.ToString("c"));
                }
            }
            catch (Exception ex)
            {
                callback((int) HttpStatusCode.InternalServerError, 0, ex.Message);
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}