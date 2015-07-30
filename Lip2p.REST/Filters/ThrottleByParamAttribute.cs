using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Lip2p.REST.Common;

namespace Lip2p.REST.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ThrottleByParamAttribute : ThrottleAttribute
    {
        public string ParamName { get; set; }

        protected override string KeyParamGenerater(HttpActionContext actionContext)
        {
            return (string) actionContext.ActionArguments[ParamName];
        }
    }
}