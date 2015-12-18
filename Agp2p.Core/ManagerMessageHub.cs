using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Agp2p.Core
{
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