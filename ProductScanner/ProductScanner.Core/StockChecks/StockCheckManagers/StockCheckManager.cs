using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Monitoring;
using ProductScanner.Core.StockChecks.Aggregators;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.StockChecks.StockCheckManagers
{
    public class StockCheckManager<T> : IStockCheckManager<T> where T : Store
    {
        private readonly IVendorStockCheckMediator _stockCheckMediator;

        public StockCheckManager(IVendorStockCheckMediator stockCheckMediator)
        {
            _stockCheckMediator = stockCheckMediator;
        }

        public async Task<List<StockCheckResult>> CheckStockAsync(List<StockCheck> stockChecks)
        {
            var invalidChecks = stockChecks.Where(x => x.MPN == null).ToList();
            stockChecks = stockChecks.Where(x => x.MPN != null).ToList();

            var vendorGroups = stockChecks.GroupBy(x => x.Vendor).ToList();
            var res = new List<Task<List<ProductStockInfo>>>();
            foreach (var group in vendorGroups)
            {
                var key = group.Key;
                var vendorStockChecks = group.ToList();
                var stockCheckAggregator = GetStockCheckAggregator(key);
                res.Add(stockCheckAggregator.CheckStockAsync(vendorStockChecks));
            }

            try
            {
                await Task.WhenAll(res);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // only include completed checks
            res = res.Where(x => x.Status == TaskStatus.RanToCompletion).ToList();
            var validResults = res.SelectMany(x => x.Result)
                .Zip(stockChecks, (stockInfo, check) => CreateStockCheckResult(check, stockInfo, check.Vendor)).ToList();
            validResults.AddRange(CreateInvalidResults(invalidChecks));
            return validResults;
        }

        private List<StockCheckResult> CreateInvalidResults(List<StockCheck> stockChecks)
        {
            return stockChecks.Select(x => new StockCheckResult
            {
                VariantId = x.VariantId,
                StockCheckStatus = StockCheckStatus.InvalidProduct
            }).ToList();
        }

        private StockCheckResult CreateStockCheckResult(StockCheck stockCheck, ProductStockInfo stockInfo, Vendor vendor)
        {
            var stockChecker = GetStockCheckAggregator(vendor);
            return new StockCheckResult(stockInfo, vendor, stockCheck.MPN, stockCheck.VariantId, vendor.StockCapabilities);
        }

        protected IStockCheckAggregator GetStockCheckAggregator(Vendor vendor)
        {
            return _stockCheckMediator.GetStockChecker(vendor);
        }
    }
}