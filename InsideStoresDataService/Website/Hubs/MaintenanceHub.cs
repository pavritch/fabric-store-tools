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
    [HubName("maintenanceHub")]
    public class MaintenanceHub : Hub
    {
        #region Product Action

        public void CancelProductAction(string storeKey)
        {
            var store = MvcApplication.Current.GetWebStore(LibEnum.Parse<StoreKeys>(storeKey));
            store.CancelRunActionForAllProducts();
        }

        internal static void NotifyRunProductActionPctComplete(int pct)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MaintenanceHub>();
            context.Clients.All.runProductActionPctComplete(pct);
        }

        internal static void NotifyRunProductActionStatus(string msg)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MaintenanceHub>();
            context.Clients.All.runProductActionStatus(msg);
        }
        
        #endregion


        #region Store Action
        public void CancelStoreAction(string storeKey)
        {
            var store = MvcApplication.Current.GetWebStore(LibEnum.Parse<StoreKeys>(storeKey));
            store.CancelRunActionForStore();
        }

        internal static void NotifyRunStoreActionPctComplete(int pct)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MaintenanceHub>();
            context.Clients.All.runStoreActionPctComplete(pct);
        }

        internal static void NotifyRunStoreActionStatus(string msg)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MaintenanceHub>();
            context.Clients.All.runStoreActionStatus(msg);
        }

        #endregion

        public override System.Threading.Tasks.Task OnConnected()
        {
            Debug.WriteLine(string.Format("OnConnected: {0}", Context.ConnectionId));
            return base.OnConnected();
        }

    }
}