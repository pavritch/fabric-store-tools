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

namespace ClassicHome
{
    public class ClassicHomeProductScraper : ProductScraper<ClassicHomeVendor>
    {
        public ClassicHomeProductScraper(IPageFetcher<ClassicHomeVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, product.DetailUrl.GetDocumentName());

            var scanData = new ScanData(product.ScanData);
            scanData.DetailUrl = product.DetailUrl;
            scanData[ScanField.ProductName] = detailsPage.GetFieldValue(".product-name h1");
            scanData[ScanField.ManufacturerPartNumber] = detailsPage.GetFieldValue(".product-name .sku").Replace("SKU", "").Trim();
            scanData[ScanField.Status] = detailsPage.GetFieldValue(".product-name .syspro-status-new");

            var bullets = string.Join(", ", detailsPage.QuerySelectorAll(".desc-bullets").Select(x => x.InnerText));
            scanData[ScanField.Bullets] = bullets;

            var specs = string.Join(", ", detailsPage.QuerySelectorAll(".specification p").Select(x => x.InnerText));
            scanData[ScanField.AdditionalInfo] = specs;

            var stock = detailsPage.GetFieldValue(".available .item");
            scanData[ScanField.StockCount] = stock;

            //var msrp = detailsPage.GetFieldValue(".msrp-price span.price").Replace("$", "");
            //scanData[ScanField.RetailPrice] = msrp;

            var wholesale = detailsPage.GetFieldValue(".regular-price span.price") ?? detailsPage.GetFieldValue(".special-price span.price");
            scanData.Cost = wholesale.Replace("$", "").ToDecimalSafe();

            scanData[ScanField.Breadcrumbs] = string.Join(", ", detailsPage.QuerySelectorAll(".breadcrumbs-content").Select(x => x.InnerText));

            var images = detailsPage.QuerySelectorAll(".product-img-box a").Select(x => x.Attributes["href"].Value).Distinct().ToList();
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, images.First()));
            //images.Skip(1).ForEach(x => scanData.AddImage(new ScannedImage(ImageVariantType.Other, x)));
            return new List<ScanData> { scanData };
        }
    }
}