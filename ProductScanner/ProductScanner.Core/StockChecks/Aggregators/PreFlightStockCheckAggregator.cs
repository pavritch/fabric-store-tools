using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.StockChecks.Aggregators
{
    public class PreFlightStockCheckAggregator<T> : IStockCheckAggregator<T> where T : Vendor, new()
    {
        private readonly IStockCheckAggregator<T> _stockChecker;
        private readonly IPlatformDatabase _platformDatabase;
        public PreFlightStockCheckAggregator(IStockCheckAggregator<T> stockChecker, IPlatformDatabase platformDatabase)
        {
            _stockChecker = stockChecker;
            _platformDatabase = platformDatabase;
        }

        public async Task<List<ProductStockInfo>> CheckStockAsync(List<StockCheck> stockChecks)
        {
            // don't want to make SQL queries for vendors where we know we don't have preflight data saved
            if (!_stockChecker.CheckPreflight) return await _stockChecker.CheckStockAsync(stockChecks);

            // check if we have a detail url in the database for this product
            //fileProducts.AddRange(skus);
            //await PopulateDetailUrlAsync(stockChecks);

            // should always be a 1 to 1 relationship between ProductStockInfo and StockCheck at this point
            var results = await _stockChecker.CheckStockAsync(stockChecks);
            var zippedResults = results.Zip(stockChecks, (a, b) => new Tuple<ProductStockInfo, StockCheck>(a, b)).ToList();
            foreach (var result in zippedResults) { await SaveDetailUrl(result); }
            return results;
        }

        private async Task PopulateDetailUrlAsync(List<StockCheck> stockChecks)
        {
            var vendor = new T();
            var variantIds = stockChecks.Select(x => x.VariantId).ToList();
            var detailUrls = await _platformDatabase.GetDetailUrls(variantIds, vendor.Id, vendor.Store.ToString());
            foreach (var stockCheck in stockChecks)
            {
                var detail = detailUrls.FirstOrDefault(x => x.VariantId == stockCheck.VariantId);
                if (detail != null) stockCheck.DetailUrl = detail.Url;
            }
        }

        private async Task SaveDetailUrl(Tuple<ProductStockInfo, StockCheck> stockInfo)
        {
            var vendor = new T();
            if (string.IsNullOrWhiteSpace(stockInfo.Item1.ProductDetailUrl)) return;
            await _platformDatabase.SaveVendorProductDetailPageUrl(stockInfo.Item2.VariantId, vendor.Id, stockInfo.Item1.ProductDetailUrl, vendor.Store.ToString());
        }
        public bool CheckPreflight { get { return _stockChecker.CheckPreflight; } }
    }
}