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

namespace RyanStudio
{
    public class RyanStudioProductScraper : ProductScraper<RyanStudioVendor>
    {
        public RyanStudioProductScraper(IPageFetcher<RyanStudioVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var name = url.Replace("https://www.ryanstudio.biz/ProductDetails.asp?ProductCode=", "");
            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, name);

            var scanData = new ScanData();
            scanData[ScanField.ManufacturerPartNumber] = name;

            var variantData = page.QuerySelector("span[itemprop='description']").InnerText;

            var pricing = page.QuerySelector("script:contains('TCN_addContent')").InnerText;
            ParsePricing(pricing);

            var image = page.QuerySelector("#product_photo_zoom_url2").Attributes["src"].Value;
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image));
            return new List<ScanData> {scanData};
        }

        private void ParsePricing(string pricing)
        {
            //TCN_makeComboGroup("SELECT___ANIMAUX___9", "SELECT___ANIMAUX___8");
            //TCN_makeSelValueGroup("", "");
            //var separator = "+#+";
            //TCN_addContent("22 x 22[Add $120.00]+#+55+#+Eggplant+#+1575");
            //TCN_addContent("22 x 22[Add $120.00]+#+55+#+Jungle+#+1198");
            //TCN_addContent("14 x 20[Add $95.00]+#+57+#+Eggplant+#+1575");
            //TCN_addContent("14 x 20[Add $95.00]+#+57+#+Jungle+#+1198");
            //TCN_reload();
            //setDefault();

            var rows = pricing.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var priceRows = rows.Where(x => x.Contains("TCN_addContent")).Select(ParsePriceRow).ToList();
            
        }

        private RyanStudioPrice ParsePriceRow(string row)
        {
            row = row.Replace("TCN_addContent(\"", "").Replace("\");", "");
            var size = row.Substring(0, row.IndexOf("["));
            var price = row.Substring(row.IndexOf("[") + 1, row.IndexOf("]") - row.IndexOf("[")).Replace("Add $", "").ToDoubleSafe();

            return new RyanStudioPrice(size, price, "");
        }
    }

    public class RyanStudioPrice
    {
        public string Size { get; set; }
        public double Price { get; set; }
        public string Color { get; set; }

        public RyanStudioPrice(string size, double price, string color)
        {
            Size = size;
            Price = price;
            Color = color;
        }
    }
}