using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Safavieh
{
    public class SafaviehProductScraper : ProductScraper<SafaviehVendor>
    {
        public SafaviehProductScraper(IPageFetcher<SafaviehVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var collection = product.ScanData[ScanField.Collection];
            var sku = product.ScanData[ScanField.SKU];

            var url = string.Format("http://safavieh.com/rugs/{0}/{1}", collection.ToLower().Replace(" ", "-"), sku.ToLower());

            var scanData = new ScanData(product.ScanData);
            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, sku.ToLower());
            if (page.InnerText.ContainsIgnoreCase("Not found") || !page.InnerText.ContainsIgnoreCase("Rug Details"))
                return new List<ScanData> {scanData};

            var fiber = page.GetFieldValue("p:contains('Fiber Content')").Replace("Fiber Content: ", "");
            var style = page.GetFieldValue("p:contains('Style')").Replace("Style:", "")
                .Replace("Kids", " Kids")
                .Replace("Floral", " Floral")
                .Replace("Contemporary", " Contemporary");
            var care = page.GetFieldValue(".care");

            scanData.DetailUrl = new Uri(url);
            scanData[ScanField.Style] = style;
            scanData[ScanField.Content] = fiber;
            scanData[ScanField.Cleaning] = care;
            return new List<ScanData> { scanData };
        }
    }
}