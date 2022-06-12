using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Config;
using ProductScanner.Core.Monitoring;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using Utilities;

namespace ProductScanner.Core.StockChecks.Aggregators
{
    public class StockCheckAggregator<T> : IStockCheckAggregator<T> where T : Vendor
    {
        private readonly IStockChecker<T> _stockChecker;
        private readonly IAppSettings _appSettings;
        private readonly IVendorPerformanceMonitor<T> _performanceMonitor;
        private readonly IVendorSessionManager<T> _sessionManager; 
        private static readonly AsyncLock MyLock = new AsyncLock();
        public StockCheckAggregator(IStockChecker<T> stockChecker, IAppSettings appSettings, 
            IVendorPerformanceMonitor<T> performanceMonitor, IVendorSessionManager<T> sessionManager)
        {
            _stockChecker = stockChecker;
            _appSettings = appSettings;
            _performanceMonitor = performanceMonitor;
            _sessionManager = sessionManager;
        }

        public async Task<List<ProductStockInfo>> CheckStockAsync(List<StockCheck> checks)
        {
            var results = new List<ProductStockInfo>();
            foreach (var check in checks)
            {
                try
                {
                    using (await MyLock.LockAsync())
                    {
                        _performanceMonitor.BumpApiRequest();
                        var webClient = _sessionManager.CreateWebClient();
                        var result = await _stockChecker.CheckStockAsync(check, webClient);
                        results.Add(result);
                        _performanceMonitor.SaveNotification(check.VariantId, check.MPN, result.StockCheckStatus);
                        Console.WriteLine("Checked stock for " + typeof(T).Name + " product " + check.MPN + ": " + result.StockCheckStatus);
                        await Task.Delay(_appSettings.VendorQueryDelayInMilliseconds);
                    }
                }
                catch (Exception)
                {
                    _performanceMonitor.SaveNotification(check.VariantId, check.MPN, StockCheckStatus.Unavailable);
                    results.Add(ProductStockInfo.Invalid());
                }
            }

            // if any of the results have AuthenticationFailed, invalidate the session
            if (results.Any(x => x.StockCheckStatus == StockCheckStatus.AuthenticationFailed))
                _sessionManager.InvalidateSession();
            return results;
        }

        public bool CheckPreflight { get { return _stockChecker.CheckPreflight; } }
    }
}