using System;
using System.Threading;
using Microsoft.AspNet.SignalR;
using ProductScanner.Core;
using ProductScanner.Core.Monitoring;
using ProductScanner.Core.Sessions;

namespace ProductScanner.Website.Hubs
{
    public class MonitorHub : Hub
    {
        private readonly int _updateInterval = 1000;
        private static Timer _timer;
        private static string _selectedVendor = "All";
        private readonly IStorePerformanceMonitor<InsideFabricStore> _fabricMonitor;
        private readonly IVendorStatusManager<InsideFabricStore> _fabricStatusManager;

        public MonitorHub(IStorePerformanceMonitor<InsideFabricStore> fabricMonitor, IVendorStatusManager<InsideFabricStore> fabricStatusManager)
        {
            _fabricMonitor = fabricMonitor;
            _fabricStatusManager = fabricStatusManager;
            _timer = new Timer(TimerCallback, null, _updateInterval, _updateInterval);
        }

        public void TimerCallback(object state)
        {
            Clients.All.pushNotifications(_fabricMonitor.GetCheckNotificationsInLast(_selectedVendor));

            var apiRequests = _fabricMonitor.GetApiRequests(_selectedVendor);
            Clients.All.pushRequests(apiRequests);

            var requestsPerMinute = apiRequests.RequestsLast5Minutes/5;
            Clients.All.pushServerInfo(new
            {
                RequestsPerMinute = requestsPerMinute >= 0 ? requestsPerMinute : 0,
                VendorSessionInfo = _fabricMonitor.GetVendorSessionInfo(_selectedVendor),
                AuthenticationFailures = _fabricMonitor.GetAuthenticationFailures()
            });

            Clients.All.pushStatuses(_fabricStatusManager.GetAllStatuses());
        }

        public void SetSelectedVendor(string vendor)
        {
            _selectedVendor = vendor;
        }
    }
}