using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace RMCoco.Details
{
    public class RmCocoStockChecker : StockChecker<RMCocoVendor>
    {
        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var url = string.Format("https://rmcoco.com/product/{0}", check.MPN.Replace("-", "_"));
            var page = await webClient.DownloadPageAsync(url);
            var stock = (float)page.QuerySelector("#Qty").Attributes["value"].Value.ToDoubleSafe();
            if (stock <= 0.5) return new ProductStockInfo(StockCheckStatus.OutOfStock, stock);
            if (stock < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, stock);
            return new ProductStockInfo(StockCheckStatus.InStock, stock);
        }
    }
}