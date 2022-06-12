using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace TrendLighting
{
    public class TrendLightingProductScraper : ProductScraper<TrendLightingVendor>
    {
        public TrendLightingProductScraper(IPageFetcher<TrendLightingVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var filename = System.IO.Path.GetFileName(product.DetailUrl.LocalPath);
            var detailPage = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri, CacheFolder.Details, filename);

            if (detailPage.InnerText.ContainsIgnoreCase("404 Not Found")) return new List<ScanData>();
            if (detailPage.InnerText.ContainsIgnoreCase("page you requested was not found")) return new List<ScanData>();

            var scanData = new ScanData(product.ScanData);
            scanData.DetailUrl = product.DetailUrl;
            scanData[ScanField.ProductName] = detailPage.GetFieldValue(".short-description");
            scanData[ScanField.ManufacturerPartNumber] = detailPage.GetFieldValue(".product-name");
            scanData[ScanField.StockCount] = detailPage.GetFieldValue(".availability");

            var fields = detailPage.GetFieldHtml(".description").Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            scanData[ScanField.Bullet1] = string.Join(", ", fields);

            scanData.Cost = detailPage.GetFieldValue(".price").Replace("$", "").ToDecimalSafe();

            var imageEl = detailPage.QuerySelector("#image");
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, imageEl.Attributes["src"].Value));

            return new List<ScanData> { scanData };
        }
    }
}