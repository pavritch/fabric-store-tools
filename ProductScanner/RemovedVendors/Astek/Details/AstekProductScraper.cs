using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Astek.Details
{
    public class AstekProductScraper : ProductScraper<AstekVendor>
    {
        public AstekProductScraper(IPageFetcher<AstekVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var pageName = product.DetailUrl.AbsoluteUri.Replace("http://www.designyourwall.com/store/", "").Replace(".html", "");
            var page = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri, CacheFolder.Details, pageName);

            if (page.InnerText.ContainsIgnoreCase("This product is no longer available")) return new List<ScanData>();
            // some of the product pages redirect out to the home page
            if (page.InnerText.ContainsIgnoreCase("Shop Categories")) return new List<ScanData>();

            var productBox = page.QuerySelector("#product-page-right-box");
            // blank page
            if (productBox == null || productBox.QuerySelector("h1") == null) return new List<ScanData>();

            var breadcrumbs = page.QuerySelector("#breadcrumbs-header").QuerySelectorAll("a").Skip(1).ToList();
            var categories = string.Join(", ", breadcrumbs.Select(x => x.InnerText));
            var sku = page.QuerySelector(".product-info, .model").InnerText.Replace("SKU: ", "").Replace("&#039;", "'").Replace(".tif", "");
            var retail = page.QuerySelector(".uc-price-product").InnerText.Replace("Price: $", "");
            var properties = GetAggregatedField(page, ".properties_list li");
            var extraFields = GetAggregatedField(page, ".extra-fields li");
            var dimensionFields = GetAggregatedField(page, ".product-dimensions li");
            var collections = GetAggregatedField(page, ".product-detail-tags a");
            var productName = productBox.QuerySelector("h1").InnerText;

            // exclude Graham and Brown
            if (productName.ContainsIgnoreCase("Graham and Brown") || categories.ContainsIgnoreCase("Graham and Brown"))
            {
                return new List<ScanData>();
            }

            // exclude custom items
            if (page.InnerText.ContainsIgnoreCase("Custom Dimensions"))
            {
                return new List<ScanData>();
            }

            if (productName.ContainsIgnoreCase("Shipping Upgrade")) return new List<ScanData>();
            if (productName.ContainsIgnoreCase("Design Services")) return new List<ScanData>();
            if (productName.ContainsIgnoreCase("Gift Certificate")) return new List<ScanData>();

            var scanData = new ScanData();
            scanData[ProductPropertyType.Category] = categories;
            scanData[ProductPropertyType.ProductDetailUrl] = product.DetailUrl.AbsoluteUri;
            scanData[ProductPropertyType.ProductName] = productName;
            scanData[ProductPropertyType.Description] = productBox.GetFieldValue("p");
            scanData[ProductPropertyType.Dimensions] = productBox.GetFieldValue("#dimensions_wrapper_customer");
            scanData[ProductPropertyType.ManufacturerPartNumber] = sku;
            scanData[ProductPropertyType.RetailPrice] = retail;
            scanData[ProductPropertyType.TempContent1] = properties;
            scanData[ProductPropertyType.TempContent2] = extraFields;
            scanData[ProductPropertyType.TempContent3] = dimensionFields;
            scanData[ProductPropertyType.TempContent4] = collections;

            var images = page.QuerySelectorAll("li.additional-prod-image");
            var imageUrls = images.Select(x => x.QuerySelector("img").Attributes["src"].Value).ToList();
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, FindBestImage(imageUrls).Replace("product_page_thumb", "product_full")));
            
            //Exclude any custom-made/special items.

            // looks like color search is just searching name and description
            // we could just create a list of colors and see if they are in any of those locations
            return new List<ScanData> {scanData};
        }

        private bool ShouldRemove(string url)
        {
            var removeKeywords = new List<string> {"scale", "set", "room", "closeup", "office", "den", "study", "_tile", "pop", "big"};
            return (removeKeywords.Any(url.Contains));
        }

        private string FindBestImage(List<string> imageUrls)
        {
            for (var i = imageUrls.Count - 1; i >= 0; i--)
            {
                // never remove the last one - something is better than nothing
                if (imageUrls.Count == 1) return imageUrls.First();

                if (ShouldRemove(imageUrls[i])) imageUrls.RemoveAt(i);
            }

            // the image we want is 'usually' the last one
            return imageUrls.Last();
        }

        private string GetAggregatedField(HtmlNode page, string selector)
        {
            var elements = page.QuerySelectorAll(selector);
            if (!elements.Any()) return string.Empty;
            return elements.Select(x => x.InnerText).Aggregate((a, b) => a + "," + b);
        }
    }
}