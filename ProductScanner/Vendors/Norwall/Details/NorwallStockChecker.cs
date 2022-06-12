using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Norwall.Details
{
    public class NorwallStockChecker : StockChecker<NorwallVendor>
    {
        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            if (check.PublicProperties[ProductPropertyType.Packaging.DescriptionAttr()] == "Double Roll") check.Quantity /= 2;

            var url = string.Format("http://www.pattonwallcoverings.net/product_details.php?pageID=15&prtno={0}&qty={1}&lotno=&rowno=0&op=inq", 
                check.MPN, check.Quantity);
            var page = await webClient.DownloadPageAsync(url);
            var available = page.QuerySelector("availqty").InnerText;
            if (available == "Available") return new ProductStockInfo(StockCheckStatus.InStock);
            return new ProductStockInfo(StockCheckStatus.OutOfStock);
        }
    }
}