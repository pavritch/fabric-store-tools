using System;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace BlueMountain.Details
{
    public class BlueMountainStockChecker : StockChecker<BlueMountainVendor>
    {
        private const string StockUrl = "https://extranet.itgeneration.ca/EXTRANET-BMW_BPP/CustomerService/InventoryInquiry/inventory.aspx?part_no={0}";
        public BlueMountainStockChecker() : base(StockCapabilities.ReportOnHand) { } 

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var stockPage = await webClient.DownloadPageAsync(string.Format(StockUrl, check.MPN));
            var stockText = stockPage.GetFieldValue("#ContentPlaceHolder1_lblInventoryBAvailable");
            var obsolete = stockPage.GetFieldValue("#ContentPlaceHolder1_lblObsolete");
            var stock = GetStockValue(stockText);
            return new ProductStockInfo(GetStockCheckStatus(stockText, obsolete, check.Quantity), stock);
        }

        private StockCheckStatus GetStockCheckStatus(string stockText, string obsolete, float quantity)
        {
            if (stockText.Contains("BAK")) return StockCheckStatus.OutOfStock;

            var stock = GetStockValue(stockText);
            if (stock == 0)
            {
                if (obsolete == "YES") return StockCheckStatus.Discontinued;
                return StockCheckStatus.OutOfStock;
            }
            if (stock < quantity) return StockCheckStatus.PartialStock;
            if (stock >= quantity) return StockCheckStatus.InStock;
            return StockCheckStatus.OutOfStock;
        }
    }
}