using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace RalphLauren.Details
{
    public class RalphLaurenStockChecker : StockChecker<RalphLaurenVendor>
    {
        private const string SearchUrl = "http://customers.folia-fabrics.com/readitem.asp?acct={0}&action=read&ltype=1";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var detailUrl = await FindDetailUrlAsync(webClient, check);
            if (string.IsNullOrWhiteSpace(detailUrl)) return new ProductStockInfo(StockCheckStatus.Discontinued);

            var status = StockCheckStatus.OutOfStock;
            var detailsPage = await webClient.DownloadPageAsync(detailUrl);
            var stockText = detailsPage.GetFieldValue("td:contains('Stock Available') + td");
            var stock = stockText == "None" ? 0 : stockText.TakeOnlyFirstIntegerToken();

            if (check.GetPackaging() == "Double Roll") stock /= 2;

            if (stock == 0) status = StockCheckStatus.OutOfStock;
            else if (stock >= check.Quantity) status = StockCheckStatus.InStock;
            else if (stock < check.Quantity) status = StockCheckStatus.PartialStock;

            return new ProductStockInfo(status, stock) {ProductDetailUrl = detailUrl};
        }

        private async Task<string> FindDetailUrlAsync(IWebClientEx webClient, StockCheck check)
        {
            if (!string.IsNullOrWhiteSpace(check.DetailUrl)) return check.DetailUrl;
            var nvCol = new NameValueCollection();
            nvCol.Add("ID", check.MPN);
            nvCol.Add("ltype", "1");

            var vendor = new RalphLaurenVendor();
            var searchUrl = string.Format(SearchUrl, vendor.Username);
            var page = await webClient.DownloadPageAsync(searchUrl, nvCol);
            if (page.OuterHtml.Contains("No product found, please try again")) return string.Empty;

            //var itemLink = page.QuerySelector("table td a[href^='itemdetailnew.asp?acct=']");
            var itemLink = page.QuerySelector("table td a[href]");
            if (itemLink == null) return string.Empty;
            return "http://customers.folia-fabrics.com/" + itemLink.Attributes["href"].Value.Trim();
        }
    }
}