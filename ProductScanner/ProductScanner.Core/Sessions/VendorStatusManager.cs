using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.StockChecks.StockCheckManagers;

namespace ProductScanner.Core.Sessions
{
    public class VendorStatusManager<T> : IVendorStatusManager<T> where T : Store
    {
        private readonly IVendorSessionMediator _mediator;

        public VendorStatusManager(IVendorSessionMediator mediator)
        {
            _mediator = mediator;
        }

        public List<VendorInfo> GetAllCapabilities()
        {
            var fabricVendors = Vendor.GetByStore<T>().ToList();
            return fabricVendors.Select(x =>
            {
                var sessionManager = _mediator.GetSessionManager(x);
                sessionManager.RefreshStatus();
                return new VendorInfo(sessionManager.GetCapabilities(), x.DisplayName, x.Id);
            }).OrderBy(x => x.DisplayName).ToList();
        }

        public VendorInfo GetCapabilityByVendor(int vendorId)
        {
            var vendor = Vendor.GetById(vendorId);
            var sessionManager = _mediator.GetSessionManager(vendor);
            sessionManager.RefreshStatus();
            return new VendorInfo(sessionManager.GetCapabilities(), vendor.DisplayName, vendor.Id);
        }

        public List<VendorStatusData> GetAllStatuses()
        {
            var fabricVendors = Vendor.GetByStore<T>().ToList();
            return fabricVendors.Select(x =>
            {
                var sessionManager = _mediator.GetSessionManager(x);
                var performanceMonitor = _mediator.GetPerformanceMonitor(x);
                sessionManager.RefreshStatus();
                return new VendorStatusData()
                {
                    VendorId = x.Id,
                    VendorName = x.DisplayName,
                    StockCapabilities = x.StockCapabilities,
                    Available = sessionManager.GetCapabilities() != StockCapabilities.Unavailable,
                    UnavailableSince = sessionManager.WentUnavailable == DateTime.MinValue ? (DateTime?) null : sessionManager.WentUnavailable,
                    TotalQueries = performanceMonitor.ApiRequests.Total() + performanceMonitor.CacheHits.Total(),
                    LastSuccessfulQuery = performanceMonitor.LastSuccessfulQuery == DateTime.MinValue ? (DateTime?) null : performanceMonitor.LastSuccessfulQuery
                };
            }).OrderBy(x => x.VendorName).ToList();
        }
    }
}