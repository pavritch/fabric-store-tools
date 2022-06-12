using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities;
using Utilities.Extensions;

namespace PeggyPlatner.Details
{
    public class PeggyPlatnerProductScraper : ProductScraper<PeggyPlatnerVendor>
    {
        public PeggyPlatnerProductScraper(IPageFetcher<PeggyPlatnerVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct discoveredProduct)
        {
            var pattern = discoveredProduct.GetPatternName();
            var url = string.Format("http://www.peggyplatnercollection.com/products/{0}", pattern.Replace(" ", "-"));
            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, pattern);
            var colors = page.QuerySelectorAll(".prod-name").Select(x => x.Attributes["data-sku"].Value).Distinct();
            return colors.Select(color => CreateProduct(color, page, pattern, url)).ToList();
        }

        private ScanData CreateProduct(string sku, HtmlNode page, string patternName, string detailUrl)
        {
            var product = new ScanData();
            product[ProductPropertyType.ManufacturerPartNumber] = sku;
            product[ProductPropertyType.PatternName] = patternName;
            product[ProductPropertyType.ProductDetailUrl] = detailUrl;

            product[ProductPropertyType.Description] = page.QuerySelector(".right-side p").InnerText;
            product.AddImage(new ScannedImage(ImageVariantType.Primary, 
                string.Format("http://cdn.shopify.com/s/files/1/0269/1977/files/{0}.jpg", sku)));

            var code = page.InnerText.CaptureWithinMatchedPattern("Fabric code:(?<capture>(.*))");
            if (code.Contains("/"))
            {
                var patternNumber = code.Split(new[] {'/'}).First();
                product[ProductPropertyType.PatternNumber] = patternNumber;
            }
            return product;
        }
    }
}