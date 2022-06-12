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

namespace WorldsAway
{
    public class WorldsAwayProductScraper : ProductScraper<WorldsAwayVendor>
    {
        private Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "Diameter", ScanField.Diameter},
            { "Height", ScanField.Height},
            { "Width", ScanField.Width},
            { "Depth", ScanField.Depth},
        }; 

        public WorldsAwayProductScraper(IPageFetcher<WorldsAwayVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var detailPage = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri, CacheFolder.Details,
                product.DetailUrl.AbsoluteUri.Replace("https://www.worlds-away.com/", "").Remove("/"));

            var scanData = new ScanData();
            var price = detailPage.GetFieldValue(".price--withoutTax").Replace("$", "");

            scanData.Cost = price.ToDecimalSafe();
            scanData.DetailUrl = product.DetailUrl;

            var descriptions = detailPage.QuerySelectorAll(".productView-description p").ToList();
            var description = detailPage.QuerySelector(".productView-description").InnerText;
            scanData[ScanField.Description] = description;
            scanData[ScanField.ManufacturerPartNumber] = detailPage.GetFieldValue(".productView-title");
            scanData[ScanField.ProductName] = detailPage.GetFieldValue(".productView-title");
            scanData[ScanField.Breadcrumbs] = string.Join(",", detailPage.QuerySelectorAll(".breadcrumbs .breadcrumb").Select(x => x.InnerText.Trim()));

            var stock = detailPage.GetFieldValue(".productView-info-value");
            scanData[ScanField.StockCount] = stock;

            foreach (var item in descriptions.Skip(1))
            {
                var labelItem = item.QuerySelector("strong");
                if (labelItem == null || labelItem.InnerText == "&nbsp;")
                    continue;

                var label = labelItem.InnerText.Trim().Trim(':');
                if (label.ContainsIgnoreCase("Diameter"))
                {
                    scanData[ScanField.Diameter] = label.Replace("Diameter:", "");
                    continue;
                }

                var value = item.LastChild.InnerText;
                var field = _fields[label];
                scanData[field] = value;
            }

            var image = detailPage.QuerySelector(".productView-image-link");
            if (image != null)
                scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image.Attributes["href"].Value));

            return new List<ScanData> {scanData};
            /*
            var filename = System.IO.Path.GetFileName(product.DetailUrl.LocalPath);
            var detailPage = await PageFetcher.FetchAsync(product.DetailUrl.AbsoluteUri, CacheFolder.Details, filename);

            var name = detailPage.GetFieldValue(".product-name");
            var description = detailPage.QuerySelector(".short-description").Children().First().InnerText.TitleCase();

            var scanData = new ScanData();
            scanData[ScanField.Breadcrumbs] = detailPage.GetFieldValue(".breadcrumbs").Replace("&amp;", "&");
            scanData[ScanField.Color] = detailPage.GetFieldValue(".color_value");
            scanData[ScanField.ProductName] = name;
            scanData[ScanField.Dimensions] = detailPage.GetFieldValue(".dimen_value");
            scanData[ScanField.Description] = description;
            scanData[ScanField.ManufacturerPartNumber] = detailPage.GetFieldValue(".product-sku").Replace("SKU ", "");
            scanData[ScanField.StockCount] = detailPage.GetFieldValue(".availability");
            scanData[ScanField.ShippingMethod] = detailPage.GetFieldValue(".ship_value");

            var price = detailPage.GetFieldValue(".regular-price");
            if (string.IsNullOrWhiteSpace(price))
                price = detailPage.GetFieldValue(".special-price");

            scanData.Cost = price.Replace("Special Price:", "").Trim().Replace("$", "").ToDecimalSafe();
            scanData.DetailUrl = product.DetailUrl;

            var image = detailPage.QuerySelector(".MagicZoomPlus");
            if (image != null)
                scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image.Attributes["href"].Value));
            return new List<ScanData> { scanData };
            */
        }
    }
}