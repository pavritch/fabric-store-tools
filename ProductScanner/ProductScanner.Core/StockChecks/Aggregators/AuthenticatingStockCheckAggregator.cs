using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.StockChecks.DTOs;
using Utilities;

namespace ProductScanner.Core.StockChecks.Aggregators
{
    public class AuthenticatingStockCheckAggregator<T> : IStockCheckAggregator<T> where T : Vendor, new()
    {
        private readonly IStockCheckAggregator<T> _stockCheckAggregator;
        private readonly IVendorAuthenticator<T> _authenticator;
        private readonly IVendorSessionManager<T> _sessionManager;
        private static readonly AsyncLock MyLock = new AsyncLock();

        public AuthenticatingStockCheckAggregator(IStockCheckAggregator<T> stockCheckAggregator, IVendorAuthenticator<T> authenticator, 
            IVendorSessionManager<T> sessionManager)
        {
            _stockCheckAggregator = stockCheckAggregator;
            _authenticator = authenticator;
            _sessionManager = sessionManager;
        }

        public async Task<List<ProductStockInfo>> CheckStockAsync(List<StockCheck> stockChecks)
        {
            var vendor = new T();
            if (vendor.StockCapabilities == StockCapabilities.None || !vendor.IsStockCheckerFunctional) return AllNotSupported(stockChecks);

            try
            {
                var auth = await RunAuthentication();
                if (!auth) return AllAuthFailed(stockChecks);
            }
            catch (Exception)
            {
                _sessionManager.SetVendorUnavailable();
                return AllAuthFailed(stockChecks);
            }

            return await _stockCheckAggregator.CheckStockAsync(stockChecks);
        }

        private async Task<bool> RunAuthentication()
        {
            // Make sure we're only attempting one login at a time (per vendor)
            using (await MyLock.LockAsync())
            {
                // checked prior to each request/vendor batch
                _sessionManager.CheckForTimeout();
                _sessionManager.SetLastRequest();

                var session = _sessionManager.GetVendorSession();
                if (session.IsValid) return true;

                Console.WriteLine("Authenticating to " + typeof(T).Name);
                var authResult = await _authenticator.LoginAsync();
                if (!authResult.IsSuccessful)
                {
                    Console.WriteLine("Authenticating to " + typeof(T).Name + " failed");
                    _sessionManager.SetVendorUnavailable();
                    return false;
                }

                _sessionManager.SaveVendorSession(authResult.Cookies);
                return authResult.IsSuccessful;
            }
        }

        protected List<ProductStockInfo> AllAuthFailed(List<StockCheck> stockChecks)
        {
            return stockChecks.Select(x => new ProductStockInfo(StockCheckStatus.AuthenticationFailed)).ToList();
        }

        protected List<ProductStockInfo> AllNotSupported(List<StockCheck> stockChecks)
        {
            return stockChecks.Select(x => new ProductStockInfo(StockCheckStatus.NotSupported)).ToList();
        }

        public bool CheckPreflight { get { return _stockCheckAggregator.CheckPreflight; } }
    }
}