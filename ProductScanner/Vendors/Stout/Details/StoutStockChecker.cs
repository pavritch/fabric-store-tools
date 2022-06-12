using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Stout.Details
{
    public class StoutStockChecker : StockChecker<StoutVendor>
    {
        private const string ProductUrl = "http://www.estout.com/details.asp?sku={0}";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var url = string.Format(ProductUrl , check.MPN);
            var page = await webClient.DownloadPageAsync(url);
            var stock = (float) page.QuerySelector("#avail").Attributes["value"].Value.ToDecimalSafe();
            if (stock <= 0) return new ProductStockInfo(StockCheckStatus.OutOfStock);
            if (stock < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, stock);
            return new ProductStockInfo(StockCheckStatus.InStock, stock);
        }
    }
}