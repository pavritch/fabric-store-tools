using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace AndrewMartin
{
    public class AndrewMartinProductScraper : ProductScraper<AndrewMartinVendor>
    {
        private readonly Dictionary<string, ScanField> _dataFields = new Dictionary<string, ScanField>
        {
            { "Pattern Book", ScanField.Collection },
            { "Collection", ScanField.Collection },
            { "Sold By", ScanField.UnitOfMeasure },
            { "Width (cm)", ScanField.Width },
            { "Vertical Repeat (cm)", ScanField.VerticalRepeat },
            { "Roll Length (m)", ScanField.Length },
            { "Composition", ScanField.Content },
            { "Type", ScanField.Content },
            { "Fire Standards", ScanField.FireCode },
            { "End Use", ScanField.ProductUse },
            { "Flam Code", ScanField.FlameRetardant },
            { "Martindale", ScanField.Durability },
            { "After Care", ScanField.Cleaning },
            { "Delivery Time", ScanField.LeadTime },
            { "Application Method", ScanField.Ignore },
        };

        public AndrewMartinProductScraper(IPageFetcher<AndrewMartinVendor> pageFetcher) : base(pageFetcher) {}
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl;
            var patternPage = await PageFetcher.FetchAsync(url.AbsoluteUri, CacheFolder.Details, url.AbsoluteUri.Replace("http://www.andrewmartin.co.uk/", "").Replace(".php", ""));
            var colorways = patternPage.QuerySelectorAll("#super-product-table li .simple-item-colour").Select(x => x.InnerText.Trim()).ToList();
            var colorwaysImages = patternPage.QuerySelectorAll("#super-product-table .cloud-zoom-gallery").Select(x => x.Attributes["href"].Value).ToList();
            var newProducts = new List<ScanData>();
            for (var i = 0; i < colorways.Count; i++)
                newProducts.Add(CreateProduct(patternPage, colorways[i], colorwaysImages[i], url.AbsoluteUri));
            return newProducts;
        }

        private ScanData CreateProduct(HtmlNode patternPage, string colorway, string imageUrl, string detailUrl)
        {
            var data = new ScanData();
            var patternName = patternPage.QuerySelector(".product").InnerText.Trim();
            var colorName = colorway.Replace(" &amp; ", " and ").Replace(" ", "-");
            var description = patternPage.QuerySelector(".std").InnerText.Trim();

            patternName = patternName.
                Replace(" Fabric", "").
                Replace(" Wallpaper", "").
                Replace(" Stripe", "").
                Replace(" ", "-").SkuTweaks();
            data[ScanField.PatternName] = patternName;
            data[ScanField.Description] = description;
            data[ScanField.ColorName] = colorName;
            data[ScanField.ManufacturerPartNumber] = patternName + "-" + colorName.ToUpper();
            data.DetailUrl = new Uri(detailUrl);

            if (!imageUrl.Contains("placeholder"))
                data.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));

            var dataRows = patternPage.QuerySelectorAll("#product-attribute-specs-table tr").ToList();
            foreach (var row in dataRows)
            {
                var label = row.QuerySelector(".label").InnerText;
                var fieldData = row.QuerySelector(".data").InnerText;
                var property = _dataFields[label];
                data[property] = fieldData;
            }

            return data;
        }
    }
}