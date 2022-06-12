using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Duralee.Details
{
    public class BBergerProductScraper : DuraleeBaseProductScraper<BBergerVendor>
    {
        public BBergerProductScraper(IPageFetcher<BBergerVendor> pageFetcher)
            : base(pageFetcher, "https://www.duralee.com/Berger/Berger-Fabric/Product/{0}.htm") { }
    }

    public class DuraleeProductScraper : DuraleeBaseProductScraper<DuraleeVendor>
    {
        public DuraleeProductScraper(IPageFetcher<DuraleeVendor> pageFetcher)
            : base(pageFetcher, "https://www.duralee.com/Duralee/Duralee-Fabric/Product/{0}.htm") { }
    }

    public class HighlandCourtProductScraper : DuraleeBaseProductScraper<HighlandCourtVendor>
    {
        public HighlandCourtProductScraper(IPageFetcher<HighlandCourtVendor> pageFetcher)
            : base(pageFetcher, "https://www.duralee.com/Highland-Court/Highland-Court-Fabric/Product/{0}.htm") { }
    }

    public class DuraleeBaseProductScraper<T> : ProductScraper<T> where T : Vendor, new()
    {
        private readonly string _detailsUrl;
        private const string StockCheckUrl = "http://www.duralee.com/admin/code/postback/getAvailability.ashx?pattern={0}&color=0&i=1";

        public DuraleeBaseProductScraper(IPageFetcher<T> pageFetcher, string detailsUrl) : base(pageFetcher)
        {
            _detailsUrl = detailsUrl;
        }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct discoveredProduct)
        {
            var url = discoveredProduct.ScanData.GetDetailUrl();
            var detailPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, url.Split('/').Last().Replace(".htm", ""));

            if (detailPage.InnerText.ContainsIgnoreCase("Sorry, we could not find the product you are looking for."))
                return new List<ScanData>();

            if (detailPage.InnerText.ContainsIgnoreCase("Oops, an error has occurred"))
                return new List<ScanData>();

            var collection = detailPage.GetFieldValue(".bottom-data div.col:contains('Collection') + div");
            var attribute = detailPage.GetFieldValue(".bottom-data div.col:contains('Attribute') + div");
            var book = detailPage.GetFieldValue(".top-data-left a:contains('Book')");

            var patternNumber = discoveredProduct.ScanData[ScanField.PatternNumber];
            var stockUrl = string.Format(StockCheckUrl, patternNumber);
            var stockPage = await PageFetcher.FetchAsync(stockUrl, CacheFolder.Stock, patternNumber);
            var colorNodes = stockPage.QuerySelectorAll("td.color-code");

            foreach (var node in colorNodes)
            {
                var stock = node.ParentNode.QuerySelector("div.colorAvailability");
                var colorNumber = discoveredProduct.ScanData[ScanField.ColorNumber];
                var colorCode = node.InnerText;
                if (colorCode == colorNumber)
                    discoveredProduct.ScanData[ScanField.StockCount] = stock == null ? "0" : "999999";
            }
            var scanData = new ScanData(discoveredProduct.ScanData);
            scanData[ScanField.Collection] = collection;
            scanData[ScanField.Book] = book;
            scanData[ScanField.Railroaded] = attribute;

            scanData.Cost = detailPage.GetFieldValue(".is-wholesale span").Remove("$").ToDecimalSafe();
            return new List<ScanData> {scanData};
        }
    }
}