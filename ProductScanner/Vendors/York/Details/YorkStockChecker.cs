using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.Checkers;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace York.Details
{
    public class YorkStockChecker : StockChecker<YorkVendor>
    {
        private string _searchUrl = "http://www.yorkwall.com/CGI-BIN/lansaweb?webapp=WBRAND3+webrtn=BRANDD3+ml=LANSA:XHTML+partition=YWP+language=ENG+sid=";
        private string _resultsUrl = "http://www.yorkwall.com/CGI-BIN/lansaweb?webapp=WPSE+webrtn=RESULTS+ml=LANSA:XHTML+partition=YWP+language=ENG+sid=";
        public YorkStockChecker() : base(checkPreflight: true) { }

        public async override Task<ProductStockInfo> CheckStockAsync(StockCheck check, IWebClientEx webClient)
        {
            var detailUrl = await FindDetailUrlAsync(check, webClient);
            if (string.IsNullOrWhiteSpace(detailUrl)) return new ProductStockInfo(StockCheckStatus.Discontinued);

            var unit = check.PublicProperties[ProductPropertyType.UnitOfMeasure.DescriptionAttr()];

            var detailPage = await webClient.DownloadPageAsync(detailUrl);
            var stockText = detailPage.InnerText.CaptureWithinMatchedPattern("Total Number Available: (?<capture>(.*)) S");
            var stock = GetStockValue(stockText);

            // everything sold by roll is sold as double rolls
            if (unit == "Roll") stock /= 2;

            if (stock == 0) return new ProductStockInfo(StockCheckStatus.OutOfStock);
            if (stock < check.Quantity) return new ProductStockInfo(StockCheckStatus.PartialStock, stock);
            return new ProductStockInfo(StockCheckStatus.InStock, stock);
        }

        private async Task<string> FindDetailUrlAsync(StockCheck check, IWebClientEx webClient)
        {
            if (!string.IsNullOrWhiteSpace(check.DetailUrl)) return check.DetailUrl;
            var namedElements = new[]
            {
                "STDRENTRY",
                "STDSESSID",
                "STDWEBUSR",
                "STDWEBC01",
                "STDTABFLR",
                "STDROWNUM",
                "STDUSERST",
                "STDUSRTYP",
                "LW3VARFLD",
                "STDNXTFUN",
                "STDPRVFUN",
                "LW3SITTOT",
                "LW3SITCNT",
                "LW3EASTAT",
                "LW3CUSIND",
                "STDCUSIND",
                "LW3PROCID",
                "LW3VNDNME",
                "STDLISTID",
                "STD_ADLIN",
                "PRIMCOLR",
                "ACNTCOLR",
                "PATTERN",
                "KEYWORD",
                "WW3SUBSIT",
                "PRESS",
                "NEWNBRPAG",
                "CLKNBRPAG",
                "NBRPAG",
                "_SERVICENAME",
                "_WEBAPP",
                "_WEBROUTINE",
                "_PARTITION",
                "_LANGUAGE",
                "_LW3TRCID"
            };

            var searchPage = await webClient.DownloadPageAsync(_searchUrl);
            var nvCol = new NameValueCollection();
            foreach (var item in searchPage.OwnerDocument.GetFormPostValuesByName(namedElements))
                nvCol.Add(item.Key, item.Value);
            nvCol["PATTERN"] = check.MPN;

            var resultsPage = await webClient.DownloadPageAsync(_resultsUrl, nvCol);
            var table = resultsPage.QuerySelector("table[class='prdList']");

            if (table == null)
                return null;

            var productCells = table.QuerySelectorAll("td[width='230']").ToList();

            if (!productCells.Any())
                return null;

            var cell = productCells.First();
            var linkElement = cell.QuerySelector("a");

            var onclick = linkElement.GetAttributeValue("onclick", string.Empty);
            var detailUrl = onclick.CaptureWithinMatchedPattern(@"window.location.href\s=\s'(?<capture>(.*))';");
            detailUrl = "http://www.yorkwall.com" + detailUrl;
            return detailUrl;
        }
    }
}