using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using Newtonsoft.Json;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace FSchumacher.Details
{
    public class FSchumacherLotInfo
    {
        public string LotNumber { get; set; }
        public double Inventory { get; set; }
        public string Warehouse { get; set; }
        public string SoldByUnit { get; set; }
        //{\"LotNumber\":\"90014160\",\"Inventory\":24.20,\"Warehouse\":\"WHSE20\",\"SoldByUnit\":\"YARD\"}
    }

    public class FSchumacherProductScraper : ProductScraper<FSchumacherVendor>
    {
        private const string ProductUrl = "https://www.fschumacher.com/item/{0}";
        private readonly IPageFetcher<FSchumacherVendor> _pageFetcher;

        private readonly Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "WIDTH", ScanField.Width },
            { "CONTENT", ScanField.Content },
            { "HORIZONTAL REPEAT", ScanField.HorizontalRepeat },
            { "VERTICAL REPEAT", ScanField.VerticalRepeat },
            { "PERFORMANCE", ScanField.Durability },
            { "LIGHT FASTNESS", ScanField.Ignore },
            { "MATCH", ScanField.Match },
            { "COUNTRY OF FINISH", ScanField.Country },
            { "YARDS PER ROLL", ScanField.Length },
            { "CANADIAN PRICE", ScanField.Ignore },
            { "DESIGNER", ScanField.Ignore },
        }; 

        public FSchumacherProductScraper(IPageFetcher<FSchumacherVendor> pageFetcher)
            : base(pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var key = context.MPN;
            var detailUrl = string.Format(ProductUrl, key);
            var detailPage = await PageFetcher.FetchAsync(detailUrl, CacheFolder.Details, key);

            if (detailPage.InnerText.ContainsIgnoreCase("An unexpected error occurred"))
            {
                return new List<ScanData>();
            }

            var product = new ScanData();

            if (detailPage.QuerySelector("#modalAvailability .large-10") == null)
            {
                return new List<ScanData>();
            }

            var availability = detailPage.QuerySelector("#modalAvailability .large-10").InnerText
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToList();
            var costText = availability.Single(x => x.Contains("$"));

            product[ScanField.Cost] = costText.Replace("$", "").Replace(" per yard", "").Replace(" per panel", "").Replace(" per single roll", "").Trim();
            if (detailPage.InnerText.ContainsIgnoreCase("minimum order of 8.00 yards"))
            {
                product[ScanField.MinimumOrder] = "8";
            }

            if (detailPage.InnerText.ContainsIgnoreCase("minimum order of 30.00 yards"))
            {
                product[ScanField.MinimumOrder] = "30";
            }

            if (detailPage.InnerText.ContainsIgnoreCase("8.00 yard increments"))
            {
                product[ScanField.OrderIncrement] = "8";
            }

            var unit = costText.Contains("yard") ? UnitOfMeasure.Yard :
                costText.Contains("panel") ? UnitOfMeasure.Panel :
                    costText.Contains("roll") ? UnitOfMeasure.Roll : UnitOfMeasure.Each;

            if (detailPage.InnerText.ContainsIgnoreCase("This item ships directly from the mill. Please contact Customer Service or your Schumacher Sales Representative for availability."))
            {
                product[ScanField.StockCount] = "1";
            }
            else
            {
                var stockInfo = detailPage.GetFieldValue("div.row:contains('STOCK INFORMATION') ~ div.row")
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var sum = stockInfo.Select(x => x.Trim().TakeOnlyFirstDecimalToken()).Sum();
                product[ScanField.StockCount] = sum.ToString();
            }

            if (detailPage.InnerText.ContainsIgnoreCase("Made to order"))
                product.MadeToOrder = true;

            product[ScanField.ManufacturerPartNumber] = key;
            product[ScanField.UnitOfMeasure] = unit.ToString();

            var details = detailPage.QuerySelector("div.large-4:contains('PRODUCT INFO')").ParentNode.ParentNode.QuerySelectorAll(".row").ToList();
            foreach (var row in details.Skip(1))
            {
                var cells = row.QuerySelectorAll("div.large-4").ToList();
                foreach (var cell in cells)
                {
                    var fieldKey = cell.ChildNodes.First().InnerText;
                    var value = cell.ChildNodes.Last().InnerText;

                    var field = _fields[fieldKey];
                    product[field] = value;
                }
            }

            var breadcrumbs = detailPage.QuerySelector(".page-title-bar > div > div").Children().Where(x => x.Name == "a" || x.Name == "span")
                .Select(x => x.InnerText).ToList();
            product[ScanField.ProductGroup] = breadcrumbs[1].Replace("WALLCOVERINGS", "WALLCOVERING").Replace("FABRICS", "FABRIC");

            if (breadcrumbs.Count > 3)
            {
                product[ScanField.Collection] = breadcrumbs[2].Replace(" COLLECTION COLLECTION", "");
                product[ScanField.ProductName] = breadcrumbs[3];
            }
            else
            {
                product[ScanField.ProductName] = breadcrumbs[2];
            }

            var color = detailPage.GetFieldValue("span:contains('COLOR') ~ span");
            product[ScanField.ColorName] = color;

            product[ScanField.OrderInfo] = 
                detailPage.InnerText.ContainsIgnoreCase("Cut and sold by the triple roll") ? "3" : 
                detailPage.InnerText.ContainsIgnoreCase("Cut and sold by the double roll") ? "2" : "1";

            product.DetailUrl = new Uri(detailUrl);
            product.AddImage(new ScannedImage(ImageVariantType.Primary, string.Format("https://s3.amazonaws.com/schumacher-webassets/{0}.jpg", key)));
            return new List<ScanData> {product};
        }
    }
}