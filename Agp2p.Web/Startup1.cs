using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Agp2p.Web.Startup1))]

namespace Agp2p.Web
{
    // 参考：http://www.asp.net/signalr/overview/guide-to-the-api/mapping-users-to-connections
    class MyIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            if(!request.Cookies.ContainsKey("AdminName"))
                return request.User.Identity.Name;

            var cookie = request.Cookies["AdminName"];
            if (cookie == null)
            {
                return request.User.Identity.Name;
            }
            var keyValue = cookie.Value;
            var managerName = new Regex(@"Agp2p=(\w+)").Match(keyValue).Groups[1].ToString();
            return managerName;
        }
    }

    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888

            // TODO https://github.com/SignalR/SignalR/issues/3414 临时修正 iis 卡死的问题，等待 signalR 升级 2.2.1
            var originalUiCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            var originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            app.MapSignalR();

            var idProvider = new MyIdProvider();
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);


            // 还原设置
            System.Threading.Thread.CurrentThread.CurrentUICulture = originalUiCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
}
