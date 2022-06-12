using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace NewGrowthDesigns
{
    public class NewGrowthDesignsProductScraper : ProductScraper<NewGrowthDesignsVendor>
    {
        public NewGrowthDesignsProductScraper(IPageFetcher<NewGrowthDesignsVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var detailsPage = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri, CacheFolder.Details, product.DetailUrl.GetDocumentName());
            if (detailsPage.InnerText.ContainsIgnoreCase("404 Page Not Found"))
                return new List<ScanData>();

            var scanData = new ScanData();
            scanData.DetailUrl = product.DetailUrl;

            var cost = detailsPage.QuerySelector("#ProductPrice");
            if (cost == null)
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Details, product.DetailUrl.GetDocumentName());
                throw new AuthenticationException();
            }
            scanData.Cost = detailsPage.QuerySelector("#ProductPrice").Attributes["content"].Value.ToDecimalSafe();
            scanData[ScanField.ManufacturerPartNumber] = detailsPage.QuerySelector(".variant-sku").InnerText.Replace("SKU: ", "");
            scanData[ScanField.ProductName] = detailsPage.QuerySelector(".product-single__title").InnerText;

            scanData[ScanField.StockCount] = detailsPage.InnerText.ContainsIgnoreCase("Sold Out") ? "0" : "999";
            scanData[ScanField.Description] = detailsPage.QuerySelector(".product-single__description").InnerText;

            var image = detailsPage.QuerySelector("#ProductPhotoImg").Attributes["src"].Value;
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, "https:" + image));

            return new List<ScanData> {scanData};
        }
    }
}