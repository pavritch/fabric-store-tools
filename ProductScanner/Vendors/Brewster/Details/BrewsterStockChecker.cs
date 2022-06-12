using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Brewster.Details
{
    public class BrewsterStockChecker : StockChecker<BrewsterVendor>
    {
        private const string SearchUrl = "https://dealer.brewsterwallcovering.com/Product/Search";
        private const string StockCheckUrl = "https://dealer.brewsterwallcovering.com/Inventory/Inquiry";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var values = new NameValueCollection();
            values.Add("SearchText", check.MPN);
            values.Add("Search", "Search");

            var stockPage = await webClient.DownloadPageAsync(SearchUrl, values);
            //if (stockPage.InnerText.ContainsIgnoreCase("Invalid Material Number")) return new List<ScanData>();
            var costElement = stockPage.GetFieldValue(".roomPricing tr:nth-child(3) td:nth-child(2)");
            var stockElement = stockPage.GetFieldValue(".productInventory tr:nth-child(3) td:nth-child(2)");

            var stockRows = stockPage.QuerySelector(".productInventory table").QuerySelectorAll(".data");
            var totalStock = 0;
            foreach (var row in stockRows)
            {
                totalStock += row.QuerySelectorAll("td").ToList()[1].InnerText.ToIntegerSafe();
            }

            if (totalStock <= 0) return new ProductStockInfo(StockCheckStatus.OutOfStock, totalStock);
            if (totalStock < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, totalStock);
            return new ProductStockInfo(StockCheckStatus.InStock, totalStock);
        }
    }
}