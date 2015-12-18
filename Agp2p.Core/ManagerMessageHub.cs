using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Agp2p.Core
{
    // 参考：http://www.asp.net/signalr/overview/getting-started/tutorial-server-broadcast-with-signalr
    public class ManagerMessageHub : Hub
    {
        private readonly ManagerMessageHubFacade _facade;

        public ManagerMessageHub() : this(ManagerMessageHubFacade.Instance) { }

        public ManagerMessageHub(ManagerMessageHubFacade facade)
        {
            _facade = facade;
        }

    }
}