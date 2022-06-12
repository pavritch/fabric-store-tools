using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace RobertAllen.Details
{
    public class RobertAllenStockChecker : RobertAllenBaseStockChecker<RobertAllenVendor> { }
    public class BeaconHillStockChecker : RobertAllenBaseStockChecker<BeaconHillVendor> { }

    public class RobertAllenBaseStockChecker<T> : StockChecker<T> where T : Vendor
    {
        private const string StockUrl = "http://www.robertallendesign.com/catalog/product/getAvailablePieces/";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var values = new NameValueCollection();
            values.Add("sku", check.MPN);
            var stockData = await webClient.DownloadPageAsync(StockUrl, values);

            // get every 4th cell (denotes available stock)
            var availableStock = (float)stockData.QuerySelectorAll("td").Where((x, i) => i%4 == 3).Sum(x => x.InnerText.ToDoubleSafe());
            if (availableStock <= 0) return new ProductStockInfo(StockCheckStatus.OutOfStock, availableStock);
            if (availableStock <= check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, availableStock);
            return new ProductStockInfo(StockCheckStatus.InStock, availableStock);
        }
    }
}