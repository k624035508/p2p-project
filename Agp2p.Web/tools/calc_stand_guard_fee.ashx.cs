using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.SessionState;
using Agp2p.BLL;
using Agp2p.Common;
using Agp2p.Core;
using Agp2p.Linq2SQL;
using Agp2p.Web.UI;
using Newtonsoft.Json;

namespace Agp2p.Web.tools
{
    /// <summary>
    /// calc_stand_guard_fee 的摘要说明
    /// </summary>
    public class calc_stand_guard_fee : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.TrySkipIisCustomErrors = true;

            BasePage.IsUserLogin(); // 从 cookie 恢复 session

            CalcStandGuardFee((statusCode, handlingFee, msg) =>
            {
                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.Write(JsonConvert.SerializeObject(new {msg, handlingFee}));
            });
        }

        protected void CalcStandGuardFee(Action<int, decimal, string> callback)
        {
            Model.users model = HttpContext.Current.Session[DTKeys.SESSION_USER_INFO] as Model.users;
            if (model == null)
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                HttpContext.Current.Response.Write(JsonConvert.SerializeObject(new { msg = "登录超时，请重新登陆" }));
                return;
            }

            var userId = model.id;
            if (userId == 0)
            {
                var userInfo = BasePage.GetUserInfo();
                if (userInfo == null)
                {
                    callback((int)HttpStatusCode.Unauthorized, 0, "请先登录");
                    return;
                }
                userId = userInfo.id;
            }

            var withdrawValue = DTRequest.GetQueryDecimal("withdraw_value", 0);

            if (withdrawValue <= 0)
            {
                callback((int) HttpStatusCode.BadRequest, 0, "请先填写正确的提现金额");
                return;
            }
            try
            {
                /*var standGuardFee = new Agp2pDataContext().CalcStandGuardFee(userId, Convert.ToDecimal(withdrawValue));
                var finalValue = Math.Max(TransactionFacade.DefaultHandlingFee, standGuardFee);*/
                //TODO 暂时不收取
                //var finalValue = withdrawValue < 100 ? TransactionFacade.DefaultHandlingFee : 0;
                var finalValue = 0;
                callback((int)HttpStatusCode.OK, finalValue, "提现手续费 " + finalValue.ToString("c"));
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