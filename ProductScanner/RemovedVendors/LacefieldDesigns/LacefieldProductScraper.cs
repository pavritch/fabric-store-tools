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

namespace LacefieldDesigns
{
    public class LacefieldProductScraper : ProductScraper<LacefieldDesignsVendor>
    {
        public LacefieldProductScraper(IPageFetcher<LacefieldDesignsVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var mpn = url.Replace("http://www.lacefielddesigns.com/product/", "").Trim('/').TitleCase();
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);

            var scanData = new ScanData(product.ScanData);
            var image = detailsPage.QuerySelector(".slide img");
            if (image != null)
                scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image.Attributes["src"].Value));

            if (detailsPage.InnerText.ContainsIgnoreCase("Available on backorder"))
                scanData[ScanField.StockCount] = "Out";

            var parts = detailsPage.QuerySelector(".short-description").InnerText
                .Split('\n').ToList();

            scanData.DetailUrl = new Uri(url);
            scanData[ScanField.ProductName] = parts[0];
            foreach (var part in parts.Skip(1))
            {
                if (part.Contains("%")) scanData[ScanField.Content] = part;
                if (part.Contains("Made")) scanData[ScanField.Country] = part;
                if (part.Contains("Back")) scanData[ScanField.Material] = part;
                if (part.Contains("x")) scanData[ScanField.Dimensions] = part;
            }
            scanData[ScanField.Category] = detailsPage.GetFieldValue(".posted_in");
            scanData[ScanField.Tags] = detailsPage.GetFieldValue(".tagged_as");
            scanData[ScanField.ManufacturerPartNumber] = mpn;
            var retailPrice = detailsPage.QuerySelector("meta[itemprop='price']").Attributes["content"].Value.ToDecimalSafe();
            scanData.Cost = retailPrice/(decimal) 2.5;

            return new List<ScanData>{scanData};
        }
    }
}