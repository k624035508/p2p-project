using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            app.MapSignalR();

            var idProvider = new MyIdProvider();
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);
        }
    }
}
