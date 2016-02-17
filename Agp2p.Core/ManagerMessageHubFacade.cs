using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Agp2p.Core
{
    // 参考：http://www.asp.net/signalr/overview/getting-started/tutorial-server-broadcast-with-signalr
    public class ManagerMessageHubFacade
    {
        // Singleton instance
        private static readonly Lazy<ManagerMessageHubFacade> _instance =
            new Lazy<ManagerMessageHubFacade>(
                () => new ManagerMessageHubFacade(GlobalHost.ConnectionManager.GetHubContext<ManagerMessageHub>().Clients));

        private IHubConnectionContext<dynamic> Clients { get; set; }

        private ManagerMessageHubFacade(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        public static ManagerMessageHubFacade Instance => _instance.Value;

        public void OnNewMsg()
        {
            Clients.All.onNewMsg();
        }

        public void OnManagerReadDelete(string managerUserName)
        {
            Clients.User(managerUserName).onMsgDelete();
        }

        public void OnManagerReadMsg(string managerUserName)
        {
            Clients.User(managerUserName).onMsgRead();
        }
    }
}