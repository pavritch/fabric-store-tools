using System;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace ClarenceHouse.Details
{
    public class ClarenceHouseStockChecker : StockChecker<ClarenceHouseVendor>
    {
        private const string StockUrl = "http://customers.clarencehouse.com/itemdetail.asp?acct=01025851&pattno={0}&pattcd={1}&pattco={2}&itemno={3}";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var webItemNumber = check.PublicProperties["Web Item Number"];
            var pattCo = check.PublicProperties["Color Number"];
            var pattCd = check.PublicProperties["Alternate Item Number"];
            var itemNum = check.MPN;

            var detailUrl = string.Format(StockUrl, webItemNumber, pattCd, pattCo, itemNum);

            var status = StockCheckStatus.OutOfStock;
            var detailsPage = await webClient.DownloadPageAsync(detailUrl);
            var rows = detailsPage.QuerySelectorAll(".productinfoboxheadderraw").Skip(1).ToList();
            var stockText = string.Empty;
            foreach (var row in rows)
            {
                var values = row.InnerText.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                var fieldKey = values.First();
                var fieldValue = values.Last();
                if (fieldKey == "Stock Available now")
                    stockText = fieldValue;
            }
            var stock = stockText == "None" ? 0 : stockText.TakeOnlyFirstIntegerToken();

            if (check.GetPackaging() == "Double Roll")
                stock /= 2;

            if (stock == 0) status = StockCheckStatus.OutOfStock;
            else if (stock >= check.Quantity) status = StockCheckStatus.InStock;
            else if (stock < check.Quantity) status = StockCheckStatus.PartialStock;

            if (stockText.Contains("Stocked at mill")) status = StockCheckStatus.InStock;

            return new ProductStockInfo(status, stock);
        }
    }
}