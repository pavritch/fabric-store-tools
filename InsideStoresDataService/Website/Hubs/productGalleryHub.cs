using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Gen4.Util.Misc;

namespace Website
{
    [HubName("productGalleryHub")]
    public class productGalleryHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}