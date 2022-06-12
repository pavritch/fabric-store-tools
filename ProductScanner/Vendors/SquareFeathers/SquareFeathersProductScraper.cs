using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace SquareFeathers
{
    public class SquareFeathersProductScraper : ProductScraper<SquareFeathersVendor>
    {
        public SquareFeathersProductScraper(IPageFetcher<SquareFeathersVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var detailPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, product.DetailUrl.LocalPath);

            var image = detailPage.QuerySelector(".images a").Attributes["href"].Value;
            var price = detailPage.GetFieldValue(".amount").Replace("$", "").Replace("&#36;", "");
            var name = detailPage.GetFieldValue(".product_title");
            var bullets = string.Join(", ", detailPage.QuerySelector(".post-content p").Children().Select(x => x.InnerText));
            var categories = string.Join(", ", detailPage.QuerySelectorAll(".posted_in a").Select(x => x.InnerText));
            var tags = string.Join(", ", detailPage.QuerySelectorAll(".tagged_as a").Select(x => x.InnerText));

            var scanData = new ScanData();
            scanData[ScanField.ManufacturerPartNumber] = name;
            scanData[ScanField.Bullets] = bullets;
            scanData[ScanField.Category] = categories;
            scanData[ScanField.Tags] = tags;
            scanData[ScanField.Breadcrumbs] = detailPage.GetFieldValue(".fusion-page-title-secondary");
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image));
            scanData[ScanField.RetailPrice] = price;
            scanData.DetailUrl = product.DetailUrl;

            var variations = detailPage.QuerySelector(".variations_form");
            if (variations != null)
            {
                var encodedJson = variations.Attributes["data-product_variations"].Value;
                var json = HttpUtility.HtmlDecode(encodedJson);
                dynamic obj = JsonConvert.DeserializeObject<dynamic>(json);
                var container = (obj as JContainer);
                foreach (var item in container)
                {
                    if (item["display_price"].ToString() == string.Empty) continue;

                    var itemPrice = Convert.ToInt32(item["display_price"]);
                    var attributeSize = Convert.ToString(item["attributes"]["attribute_size"]);
                    if (attributeSize == string.Empty) continue;

                    var variant = new ScanData();
                    variant[ScanField.RetailPrice] = itemPrice.ToString();
                    variant[ScanField.Size] = attributeSize;
                    if (scanData.Variants.Any(x => x[ScanField.Size] == attributeSize)) continue;
                    scanData.Variants.Add(variant);
                }
            }
            return new List<ScanData> { scanData };
        }
    }
}