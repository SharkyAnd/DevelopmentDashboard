using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;

namespace DevelopmentDashboardWeb
{
    public class ActiveSessionsHub : Hub
    {
        public void Update()
        {
            Clients.All.update();
        }
    }
}
