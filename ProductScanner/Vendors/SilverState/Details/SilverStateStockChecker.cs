using System;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace SilverState.Details
{
    public class SilverStateStockChecker : StockChecker<SilverStateVendor>
    {
        // TODO: It looks like I need to do searches here, because there's no way for me to know that this is the correct URL:
        // https://www.silverstatetextiles.com/storefrontCommerce/itemLookup.jsp?itemNum=*MARRAKECH+MOSAIC+CI
        // VariantID: 2116375

        private const string ProductUrl = "https://www.silverstatetextiles.com/storefrontCommerce/itemLookup.jsp?itemNum={0}";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var urlMpn = check.MPN.Replace("-", "+");
            var url = string.Format(ProductUrl, urlMpn);
            var page = await webClient.DownloadPageAsync(url);
            var estimatedAvailability = GetEstimatedAvailability(page);

            if (page.InnerText.ContainsIgnoreCase("out of stock")) 
                return new ProductStockInfo(StockCheckStatus.OutOfStock, moreExpectedOn: estimatedAvailability);

            var rows = page.QuerySelectorAll(".item_available_display tr").Skip(1);
            var availableCells = rows.Select(x => x.QuerySelectorAll(".item_available_display_line").Last()).ToList();

            var totalAvailable = availableCells.Select(x => GetStockValue(x.InnerText)).Sum();
            if (totalAvailable <= 0) return new ProductStockInfo(StockCheckStatus.OutOfStock, moreExpectedOn: estimatedAvailability);
            if (totalAvailable < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, totalAvailable);
            return new ProductStockInfo(StockCheckStatus.InStock, totalAvailable);
        }

        private DateTime? GetEstimatedAvailability(HtmlNode page)
        {
            if (!page.InnerText.ContainsIgnoreCase("estimated availability")) return null;
            var bTags = page.QuerySelectorAll("b");
            foreach (var bTag in bTags)
            {
                DateTime result;
                if (DateTime.TryParse(bTag.InnerText, out result))
                    return result;
            }
            return null;
        }
    }
}