using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace FSchumacher.Details
{
    public class FSchumacherStockChecker : StockChecker<FSchumacherVendor>
    {
        private const string ProductUrl = "https://www.fschumacher.com/item/{0}";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var mpn = check.MPN;
            var stockUrl = string.Format(ProductUrl, mpn);
            var detailPage = await webClient.DownloadPageAsync(stockUrl);

            var stock = 0d;
            if (detailPage.InnerText.ContainsIgnoreCase("This item ships directly from the mill. Please contact Customer Service or your Schumacher Sales Representative for availability."))
            {
                stock = 1;
            }
            else
            {
                var stockInfo = detailPage.GetFieldValue("div.row:contains('STOCK INFORMATION') ~ div.row")
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var sum = stockInfo.Select(x => x.Trim().TakeOnlyFirstDecimalToken()).Sum();
                stock = Convert.ToDouble(sum);
            }

            if (check.GetPackaging() == "Double Roll") stock /= 2;
            if (check.GetPackaging() == "Triple Roll") stock /= 3;

            if (stock >= check.Quantity)
                return new ProductStockInfo(StockCheckStatus.InStock);
            if (stock > 0)
                return new ProductStockInfo(StockCheckStatus.PartialStock);
            return new ProductStockInfo(StockCheckStatus.OutOfStock);
        }
    }
}