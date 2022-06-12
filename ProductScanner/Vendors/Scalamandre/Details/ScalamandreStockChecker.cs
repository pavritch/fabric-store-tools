using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Scalamandre.Details
{
    public class ScalamandreStockChecker : StockChecker<ScalamandreVendor>
    {
        private const string ProductUrl = "http://www.scalamandre.com/skuhtmlp2/{0}.html";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var url = string.Format(ProductUrl, check.MPN);
            var page = await webClient.DownloadPageAsync(url);
            var desc = page.GetFieldHtml("#desc");
            if (desc.ContainsIgnoreCase("out of stock")) return new ProductStockInfo(StockCheckStatus.OutOfStock);

            var stock = GetStockValue(desc.CaptureWithinMatchedPattern("Stock Quantity: (?<capture>(.*)) YD"));
            if (stock <= 0) stock = GetStockValue(desc.CaptureWithinMatchedPattern("Available Quantity: (?<capture>(.*)) YD"));

            if (check.GetPackaging() == "Double Roll") stock /= 2;

            if (stock <= 0) return new ProductStockInfo(StockCheckStatus.OutOfStock);
            if (stock < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, stock);
            return new ProductStockInfo(StockCheckStatus.InStock, stock);
        }
    }
}