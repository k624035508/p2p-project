using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using Lip2p.REST.Bindings;
using Lip2p.REST.Filters;

namespace Lip2p.REST
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services 全部请求都要走的 filter，有分先后顺序
            //config.Filters.Add(new RequireHttpsAttribute());
            //config.Filters.Add(new IdentityBasicAuthenticationAttribute());

            // Web API routes
            config.MapHttpAttributeRoutes();

            // 默认路径，不设置的话就必须设置 [Route("...")] 才能使用 api
            /*config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );*/

            //config.Formatters.Add(new FormUrlEncodedMediaTypeFormatter());
            /*config.ParameterBindingRules.Insert(0, descriptor =>
                descriptor.ParameterType == typeof(string) ? new MultipleParameterFromBodyParameterBinding(descriptor) : null);*/

            // http://stackoverflow.com/questions/9847564/how-do-i-get-asp-net-web-api-to-return-json-instead-of-xml-using-chrome
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

#if DEBUG
            // http://www.asp.net/web-api/overview/testing-and-debugging/tracing-in-aspnet-web-api
            config.EnableSystemDiagnosticsTracing();
#endif
        }
    }
}
