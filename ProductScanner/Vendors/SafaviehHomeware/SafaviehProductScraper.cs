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

namespace SafaviehHomeware
{
    public class SafaviehProductScraper : ProductScraper<SafaviehHomewareVendor>
    {
        private readonly Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "SKU:", ScanField.Ignore},
            { "Category:", ScanField.Category},
            { "Color:", ScanField.Color},
            { "Dimensions (W*D*H):", ScanField.Dimensions},
            { "Seat Dimensions (W*D*H):", ScanField.SeatDimensions},
            { "Drawer/Shelf Dimensions:", ScanField.DrawerDimensions},
            { "Metal Type:", ScanField.Material},
            { "Material:", ScanField.Material},
            { "Quantity Of Drawers:", ScanField.Ignore},
            { "Quantity Of Shelves:", ScanField.Ignore},
            { "Weight:", ScanField.Weight},
            { "Weight Capacity:", ScanField.Ignore},
            { "Metal Color:", ScanField.Ignore},
            { "Wood Color:", ScanField.Ignore},
            { "Lamp Color:", ScanField.Ignore},
            { "Shade Color:", ScanField.Ignore},
            { "Finial Color:", ScanField.Ignore},
            { "Harp Color:", ScanField.Ignore},
            { "Wood Content:", ScanField.Ignore},
            { "Upholstery Content:", ScanField.Content},
            { "Assembly Required:", ScanField.Ignore},
            { "Shade Fabric:", ScanField.Ignore},
            { "Body Material:", ScanField.Ignore},
            { "Shade Dimensions:", ScanField.Ignore},
            { "Harp Height:", ScanField.Ignore},
            { "Bottom Base Dimension:", ScanField.Ignore},
            { "Lamp Body Dimension:", ScanField.Ignore},
            { "Body Dimension W/O Shade & Neck:", ScanField.Ignore},
            { "Cord Length:", ScanField.Ignore},
            { "Lighting Switch Type:", ScanField.Ignore},
            { "Lumen:", ScanField.Ignore},
            { "Harp Style:", ScanField.Ignore},
            { "Light Bulb Base Type:", ScanField.Ignore},
            { "Recommended Bulb Type:", ScanField.Ignore},
            { "Fill Material:", ScanField.Ignore},
            { "Back Cover:", ScanField.Ignore},
            { "Closure:", ScanField.Ignore},
            { "Recommended Bulb Wattage:", ScanField.Ignore},
            { "# of Bulbs:", ScanField.Ignore},
            { "Disclaimer:", ScanField.Ignore},
        }; 

        private const string SearchUrl = "http://safavieh.com/?s={0}";

        public SafaviehProductScraper(IPageFetcher<SafaviehHomewareVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN;
            var url = string.Format(SearchUrl, mpn);

            var resultsPage = await PageFetcher.FetchAsync(url, CacheFolder.Search, mpn);
            var results = resultsPage.QuerySelectorAll(".item").ToList();

            if (!results.Any()) return new List<ScanData>();

            var match = results.First().QuerySelector("a").Attributes["href"].Value;
            var detailPage = await PageFetcher.FetchAsync("http://safavieh.com" + match, CacheFolder.Details, mpn);

            var scanData = new ScanData(product.ScanData);
            //scanData[ScanField.Category] = string.Join(", ", detailPage.QuerySelectorAll(".breadcrumbs a").Select(x => x.InnerText));
            scanData[ScanField.Collection] = detailPage.GetFieldValue(".collection");
            scanData[ScanField.Color] = detailPage.GetFieldValue(".color");
            scanData[ScanField.Description] = detailPage.GetFieldValue(".desc");
            scanData.DetailUrl = new Uri(url);

            var table = detailPage.QuerySelector("#productDetails .col66 table");
            if (table == null) return new List<ScanData>();

            var rows = table.QuerySelectorAll("tr").ToList();
            foreach (var row in rows)
            {
                var cells = row.QuerySelectorAll("td").ToList();
                var label = cells.First().InnerText;
                var value = cells.Last().InnerText;

                var field = _fields[label];
                scanData[field] = value;
            }

            var primaryImage = detailPage.QuerySelector("#mainimage");
            if (primaryImage != null)
            {
                //scanData.AddImage(new ScannedImage(ImageVariantType.Primary, primaryImage.Attributes["src"].Value));
            }

            var images = detailPage.QuerySelectorAll("#productImages .thumb a").Select(x => x.Attributes["href"].Value).ToList();
            if (images.Any())
            {
                scanData.AddImage(new ScannedImage(ImageVariantType.Primary, images.First()));
                //images.Skip(1).ToList().ForEach(x => scanData.AddImage(new ScannedImage(ImageVariantType.Other, x)));
            }
            return new List<ScanData> { scanData };
        }
    }
}