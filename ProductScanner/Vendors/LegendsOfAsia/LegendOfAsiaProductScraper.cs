using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace LegendOfAsia
{
    public class LegendOfAsiaProductScraper : ProductScraper<LegendOfAsiaVendor>
    {
        public LegendOfAsiaProductScraper(IPageFetcher<LegendOfAsiaVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var pageFilename = product.DetailUrl.LocalPath.Replace("/", "");
            var details = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri, CacheFolder.Details, pageFilename);
            if (details.InnerText.ContainsIgnoreCase("You do not have permission to access this page"))
            {
                return new List<ScanData>();
            }

            var scanData = new ScanData();
            scanData[ScanField.ProductName] = details.GetFieldValue(".product-title");
            scanData[ScanField.ManufacturerPartNumber] = details.GetFieldValue(".product-tab-details-item-value");
            scanData[ScanField.StockCount] = details.GetFieldValue(".product-stock-count");
            scanData.Cost = details.GetFieldValue(".price-value").Replace("$", "").ToDecimalSafe();

            var image = details.QuerySelector(".product-image-slide").Attributes["href"].Value;
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image));
            /*
            scanData[ScanField.Description] = string.Join(", ", details.GetFieldHtml("div[itemprop='description']").Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries));
            */
            return new List<ScanData> { scanData };
        }
    }
}