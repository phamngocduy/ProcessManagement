using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace ProcessManagement
{
    public class NotificationHub : Hub
    {
        [HubMethodName("SendNotify")]
        public static void SendNotify()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.All.updateNotify();
        }
    }
}