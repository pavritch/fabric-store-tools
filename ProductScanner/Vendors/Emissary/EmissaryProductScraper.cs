using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Emissary
{
    public class EmissaryProductScraper : ProductScraper<EmissaryVendor>
    {
        public EmissaryProductScraper(IPageFetcher<EmissaryVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var page = url.Replace("https://www.emissaryusa.com/", "").Replace(".html", "");
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, page);
            var items = new List<ScanData>();

            var scanData = new ScanData(product.ScanData);
            scanData[ScanField.ProductName] = detailsPage.GetFieldValue(".product-name");
            //scanData[ScanField.SKU] = detailsPage.GetFieldValue(".pd_sku").Replace(scanData[ScanField.ProductName], "");
            scanData[ScanField.ManufacturerPartNumber] = detailsPage.GetFieldValue("span[itemprop='productID']").Replace("SKU", "");
            scanData[ScanField.ShippingMethod] = detailsPage.GetFieldValue(".attr-carrier").Replace("Carrier: ", "");

            if (detailsPage.InnerHtml.Contains("allProductData"))
            {
                var scriptData = detailsPage.QuerySelector("script:contains('allProductData')").InnerText;
                var lines = scriptData.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
                var skus = lines.Where(x => x.Contains("'sku'")).ToList();
                var images = lines.Where(x => x.Contains("'img'")).ToList();
                var names = lines.Where(x => x.Contains("'description'")).ToList();
                var dimensionElements = lines.Where(x => x.Contains("dimensions")).Skip(1).ToList();
                for (var i = 0; i < skus.Count; i++)
                {
                    var skuText = skus[i];
                    var imageText = images[i];
                    var nameText = names[i];
                    var dimText = dimensionElements[i];
                    var altProduct = new ScanData(product.ScanData);
                    var sku = skuText.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries).Last().Trim(' ', '\'', ';');
                    var imageUrl = imageText.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries).Last().Trim(' ', '\'', ';');
                    var name = nameText.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries).Last().Trim(' ', '\'', ';');
                    var dimensionHtml = dimText.Split(new[] {" = "}, StringSplitOptions.RemoveEmptyEntries).Last().Trim(' ', '\'', ';').ReplaceUnicode().Trim('\"');
                    var dimDoc = new HtmlDocument();
                    dimDoc.LoadHtml(dimensionHtml);
                    var dimensions = dimDoc.DocumentNode.GetFieldValue("p");
                    if (dimensions != null)
                        altProduct[ScanField.Dimensions] = dimensions.Replace("Dimensions: ", "");

                    altProduct[ScanField.SKU] = sku;
                    altProduct[ScanField.ProductName] = name;
                    altProduct[ScanField.ManufacturerPartNumber] = sku;
                    altProduct.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));
                    items.Add(altProduct);
                }
            }

            // these are in javascript and I'm not sure how to pull them out
            scanData[ScanField.Description] = detailsPage.GetFieldValue(".short-description");
            scanData[ScanField.Finish] = detailsPage.GetFieldValue(".finish");

            var dimensionsElement = detailsPage.GetFieldValue(".product-dimensions p");
            if (dimensionsElement != null)
                scanData[ScanField.Dimensions] = dimensionsElement.Replace("Dimensions: ", "");

            var image = detailsPage.QuerySelector(".MagicZoomPlus");
            if (image != null)
                scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image.Attributes["href"].Value));

            if (!items.Any())
                items.Add(scanData);
            return items;
        }
    }
}