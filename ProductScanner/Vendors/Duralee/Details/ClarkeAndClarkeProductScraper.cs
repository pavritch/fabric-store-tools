using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Duralee.Details
{
    // for Clarke and Clarke, they don't have stock counts coming back from the same endpoint,
    // so we need to scrape details pages instead
    public class ClarkeAndClarkeProductScraper : ProductScraper<ClarkeAndClarkeVendor>
    {
        private const string StockCheckUrl = "http://www.duralee.com/admin/code/postback/getCCAvailability.ashx?pattern={0}&color={1}&i=1";
        public ClarkeAndClarkeProductScraper(IPageFetcher<ClarkeAndClarkeVendor> pageFetcher)
            : base(pageFetcher) { }

        private readonly Dictionary<string, ScanField> _keys = new Dictionary<string, ScanField>
        {
            { "Brand:", ScanField.Brand},
            { "Collection:", ScanField.Collection},
            { "Fabric Content:", ScanField.Content},
            { "Pattern Repeat:", ScanField.Repeat},
            { "Width:", ScanField.Ignore},
            { "Usage:", ScanField.ProductUse},
            { "Durability:", ScanField.Durability},
            { "Origin:", ScanField.Country},
            { "Roll Size:", ScanField.Ignore},
            { "Care:", ScanField.Cleaning},
            { "Attribute:", ScanField.Drop},
            { "Finish:", ScanField.Finish},
            { "&nbsp;", ScanField.Ignore},
        };  

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN;
            var url = string.Format("https://www.duralee.com/Clarke-Clarke/Clarke-Clarke-Fabric/Product/{0}.htm", mpn);
            if (mpn.StartsWith("W"))
            {
                url = string.Format("https://www.duralee.com/Clarke-Clarke/Clarke-Clarke-Wallcoverings/Product/{0}.htm", mpn);
            }
            var detailPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);

            if (detailPage.InnerText.ContainsIgnoreCase("Sorry, we could not find the product you are looking for."))
                return new List<ScanData>();

            if (detailPage.InnerText.ContainsIgnoreCase("Oops, an error has occurred"))
                return new List<ScanData>();

            var rows = detailPage.QuerySelectorAll(".productInfoStats .row").ToList();
            foreach (var row in rows)
            {
                var first = row.QuerySelector(".grid_4of12");
                var key = first.InnerText.Trim();
                var value = row.QuerySelector(".grid_8of12").InnerText.Trim();
                var prop = _keys[key];
                product.ScanData[prop] = value;
            }

            var mpnSplit = mpn.Split(new[] {'-'});
            var stockUrl = string.Format(StockCheckUrl, mpnSplit.First(), mpnSplit.Last());
            var stockPage = await PageFetcher.FetchAsync(stockUrl, CacheFolder.Stock, mpn);
            var jsonString = stockPage.InnerText;
            var data = JsonConvert.DeserializeObject<CCStockData>(jsonString);
            var page = new HtmlDocument();
            page.LoadHtml(data.Qtyhtml);
            var rootNode = page.DocumentNode.Clone();
            var stock = rootNode.QuerySelector(".available-qty").InnerText;
            product.ScanData[ScanField.StockCount] = stock.Trim();

            product.ScanData.Cost = detailPage.GetFieldValue(".is-wholesale span").Remove("$").ToDecimalSafe();
            return new List<ScanData> { product.ScanData };
        }
    }

    public class CCStockData
    {
        public string Qtyhtml { get; set; }
        public string Piecehtml { get; set; }
    }
}