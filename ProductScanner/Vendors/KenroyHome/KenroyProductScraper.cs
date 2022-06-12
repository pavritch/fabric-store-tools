using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace KenroyHome
{
    public class KenroyProductScraper : ProductScraper<KenroyHomeVendor>
    {
        private const string SearchUrl = "https://catalog.kenroyhome.com/khl/e/2/products?utf8=%E2%9C%93&query={0}";

        private Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "Available", ScanField.StockCount },
            { "Open Box Inventory", ScanField.Ignore },
            { "In Stock Date", ScanField.Finish },
            { "Next Receipt QTY", ScanField.Ignore },
            { "QTY On Backorder", ScanField.Ignore },
            { "QTY on PO", ScanField.Ignore },
            { "Finish", ScanField.Finish },
            { "Dimensions", ScanField.Dimensions },
            { "Shade", ScanField.Ignore },
            { "Bulb", ScanField.Ignore },
            { "Product Weight", ScanField.Ignore },
            { "Shipping Weight", ScanField.Ignore },
            { "Product Cycle", ScanField.Ignore },
            { "Suggested Retail", ScanField.Ignore },
            { "Instructions", ScanField.Ignore },
            { "Specifications", ScanField.Ignore },
            { "Lamp Closeouts", ScanField.Ignore },
            { "Classic Closeouts", ScanField.Ignore },
            { "Vegas Market Specials", ScanField.Ignore },
            { "New Only", ScanField.Ignore },
            { "Summer Blowout", ScanField.Ignore },
            { "Accent Tables", ScanField.Ignore },
            { "Dallas Introductions", ScanField.Ignore },
            { "Fixture Closeouts", ScanField.Ignore },
            { "UPC", ScanField.UPC },
            { "Catalog Pending", ScanField.Ignore },
            { "Top 250", ScanField.Ignore },
            { "Summer Specials", ScanField.Ignore },
        };

        public KenroyProductScraper(IPageFetcher<KenroyHomeVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN;
            var searchPage = await PageFetcher.FetchAsync(string.Format(SearchUrl, mpn), CacheFolder.Details, mpn + "-search");
            var results = searchPage.QuerySelectorAll(".catalog-item").ToList();
            if (results.Count != 1)
            {
                return new List<ScanData>();
            }
            var detailLink = results.First().QuerySelector(".modal-trigger").Attributes["href"].Value;
            var detailPage = await PageFetcher.FetchAsync("https://catalog.kenroyhome.com" + detailLink, CacheFolder.Details, mpn);

            var scanData = new ScanData(product.ScanData);
            scanData[ScanField.ProductName] = detailPage.GetFieldValue(".item-name h1");
            scanData.DetailUrl = new Uri("https://catalog.kenroyhome.com" + detailLink);

            var labels = detailPage.QuerySelectorAll(".tab-content dt").Select(x => x.InnerText).ToList();
            var values = detailPage.QuerySelectorAll(".tab-content dd").Select(x => x.InnerText).ToList();
            for (int i = 0; i < labels.Count; i++)
            {
                var label = labels[i];
                if (label == string.Empty) continue;

                var value = values[i];
                scanData[_fields[label]] = value;
            }

            scanData.Cost = scanData[ScanField.Cost].ToDecimalSafe();
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, string.Format("https://supercatcdn.global.ssl.fastly.net/data/35/image/{0}.png", mpn)));
            return new List<ScanData> { scanData };
        }
    }
}