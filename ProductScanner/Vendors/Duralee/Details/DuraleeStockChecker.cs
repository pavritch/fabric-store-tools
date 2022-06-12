using System;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductScanner.Core;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace Duralee.Details
{
    public class BBergerStockChecker : DuraleeBaseStockChecker<BBergerVendor> { }
    public class HighlandCourtStockChecker : DuraleeBaseStockChecker<HighlandCourtVendor> { }
    public class DuraleeStockChecker : DuraleeBaseStockChecker<DuraleeVendor> { }

    public class ClarkeAndClarkeStockChecker : StockChecker<ClarkeAndClarkeVendor>
    {
        private const string _stockUrl = "http://www.duralee.com/admin/code/postback/getCCAvailability.ashx?pattern={0}&color={1}&i=1";

        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck stockCheck, IWebClientEx webClient)
        {
            var splitMpn = stockCheck.MPN.Split(new[] {"-"}, StringSplitOptions.RemoveEmptyEntries);
            var patternNumber = splitMpn.First();
            var colorNumber = splitMpn.Last();
            var url = string.Format(_stockUrl, patternNumber, colorNumber);
            var page = await webClient.DownloadPageAsync(url);

            var result = JsonConvert.DeserializeObject<JObject>(page.InnerText);
            var qtyHtml = result["qtyhtml"].ToString();
            var node = new HtmlDocument();
            node.LoadHtml(qtyHtml);

            var stockElement = node.DocumentNode.QuerySelector(".available-qty a");
            return CreateStockInfo(stockElement, stockCheck.Quantity);
        }
    }

    public class DuraleeBaseStockChecker<T> : StockChecker<T> where T : Vendor
    {
        private const string _stockUrl = "http://www.duralee.com/admin/code/postback/getAvailability.ashx?pattern={0}&color={1}&i=1";
        public override async Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var splitMpn = check.MPN.Split(new[] {"-"}, StringSplitOptions.RemoveEmptyEntries);
            var patternNumber = splitMpn.First();
            var colorNumber = splitMpn.Last();
            var url = string.Format(_stockUrl, patternNumber, colorNumber);
            var page = await webClient.DownloadPageAsync(url);

            // find the color code for the given mpn
            var colorNode = page.QuerySelectorAll("td.color-code").SingleOrDefault(x => x.InnerText == colorNumber);
            if (colorNode != null)
            {
                var stock = colorNode.ParentNode.QuerySelector("div.colorAvailability .showColorPopup");
                return CreateStockInfo(stock, check.Quantity);
            }
            return new ProductStockInfo(StockCheckStatus.OutOfStock);
        }
    }
}