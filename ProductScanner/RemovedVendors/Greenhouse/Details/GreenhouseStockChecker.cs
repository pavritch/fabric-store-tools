using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace Greenhouse.Details
{
    public class GreenhouseStockChecker : StockChecker<GreenhouseVendor>
    {
        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            // Note: some of these products return 403 Access Denied (99006-spice)
            var url = "https://www.greenhousefabrics.com/fabric/" + check.MPN;
            var page = await webClient.DownloadPageAsync(url);
            if (page.InnerText.Contains("please call or email")) return new ProductStockInfo(StockCheckStatus.InStock);

            var inventory = page.GetFieldValue(".field-fabric-inventory");
            var stockCount = ParseInventory(inventory);
            if (stockCount <= 0.5) return new ProductStockInfo(StockCheckStatus.OutOfStock, stockCount);
            if (stockCount < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, stockCount);
            return new ProductStockInfo(StockCheckStatus.InStock, stockCount);
        }

        private float ParseInventory(string inventory)
        {
            if (inventory == null) inventory = "0";
            inventory = inventory.Replace(" yards in stock", "");
            inventory = inventory.Replace(" yard", "");
            inventory = inventory.Replace("Â", "");
            inventory = inventory.Replace("¼", ".25");
            inventory = inventory.Replace("½", ".5");
            inventory = inventory.Replace("¾", ".75").Trim();
            return GetStockValue(inventory);
        }
    }
}