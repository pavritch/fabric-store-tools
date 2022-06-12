using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace JFFabrics.Details
{
    public class JFFabricsProductScraper : ProductScraper<JFFabricsVendor>
    {
        private const string StockUrl = "http://67.211.122.138:450/rpgsp/JF_E_ST_1.pgm";
        private const int StockCheckQty = 2;
        public JFFabricsProductScraper(IPageFetcher<JFFabricsVendor> pageFetcher) : base(pageFetcher) { }

        private readonly Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "Width:", ScanField.Width},
            { "Embroidered Width:", ScanField.EmbroideredWidth},
            { "Vertical:", ScanField.VerticalRepeat},
            { "Horizontal:", ScanField.HorizontalRepeat},
            { "Railroaded:", ScanField.Railroaded},
            { "Flammability:", ScanField.Flammability},
            { "Finish:", ScanField.Finish},
            { "Surface Abrasion:", ScanField.Durability},
            { "Seam Slippage:", ScanField.Ignore},
            { "Breaking Strength:", ScanField.Ignore},
            { "Tear Strength:", ScanField.Ignore},
            { "Colorfastness to Water:", ScanField.Ignore},
            { "Colorfastness to Solvent:", ScanField.Ignore},
            { "Colorfastness to Light:", ScanField.Ignore},
            { "Crocking:", ScanField.Ignore},
            { "Pilling:", ScanField.Ignore},
            { "Quality:", ScanField.Ignore},
            { "Width Extendable:", ScanField.Ignore},
            { "Coverage Area:", ScanField.Coverage},
            { "Match type:", ScanField.Match},
            { "Coverage Area per Double Roll:", ScanField.Ignore},
        }; 

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var urlId = string.Format("{0}_{1}", product.ScanData[ScanField.Pattern], product.ScanData[ScanField.ColorNumber]).ToLower();
            var url = string.Format("https://www.jffabrics.com/products/{0}", urlId);
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, urlId);
            var title = detailsPage.QuerySelector("title").InnerText;
            if (title.ContainsIgnoreCase("Page not found") || title.ContainsIgnoreCase("Maintenance")) 
                return new List<ScanData>();

            var actualUrl = detailsPage.QuerySelector("meta[property='og:url']").Attributes["content"].Value;
            if (actualUrl.ContainsIgnoreCase("lang=fr")) 
                return new List<ScanData>();

            var patternColor = detailsPage.GetFieldValue("h3");
            var split = patternColor.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

            var scanData = new ScanData();
            var usage = detailsPage.QuerySelector("h4:contains('Usage') ~ ul");

            scanData[ScanField.Book] = detailsPage.GetFieldValue("h4:contains('Book') ~ p");
            if (usage != null)
                scanData[ScanField.Use] = string.Join(", ", usage.QuerySelectorAll("li").Select(x => x.InnerText));
            scanData[ScanField.Content] = detailsPage.GetFieldValue("h4:contains('Content') ~ p");
            scanData[ScanField.Country] = detailsPage.GetFieldValue("h4:contains('Country') ~ p");
            scanData[ScanField.Cleaning] = detailsPage.GetFieldValue("h4:contains('Care Code') ~ p");
            scanData[ScanField.ManufacturerPartNumber] = urlId;
            scanData[ScanField.PatternName] = split.First();
            scanData[ScanField.Color] = split.Last();

            scanData.DetailUrl = new Uri(url);

            var moreDetails = detailsPage.QuerySelectorAll(".more-details-info");
            foreach (var detailField in moreDetails)
            {
                var labelField = detailField.QuerySelector("strong");
                var label = labelField.InnerText;
                var property = _fields[label];
                var value = labelField.NextSibling.InnerText.Trim();
                scanData[property] = value;
            }

            scanData[ScanField.ProductGroup] = detailsPage.GetFieldValue(".collection-title");

            var stockPage = await GetStockPageAsync(split.First(), string.Join("", split.Skip(1)));

            // assuming all of the wallpaper products are in stock
            var inStock = stockPage.InnerText.ContainsIgnoreCase("In Stock") || urlId.StartsWithDigit();
            var priceCell = stockPage.QuerySelector("td:contains('Price') + td");
            var wholesalePrice = "";
            if (priceCell != null)
            {
                var priceText = priceCell.ParentNode.InnerText;   
                wholesalePrice = priceText.Replace("Includes Discount", "").Replace("Price:", "").Replace("$", "").Trim();
                if (wholesalePrice == ".00") wholesalePrice = "";
            }
            scanData[ScanField.StockCount] = inStock.ToString();
            scanData.Cost = wholesalePrice.ToDecimalSafe();

            var imageElement = detailsPage.QuerySelector(".product-etalage img");
            if (imageElement != null)
            {
                scanData.AddImage(new ScannedImage(ImageVariantType.Primary, imageElement.Attributes["src"].Value));
            }
            return new List<ScanData> { scanData };
        }

        private async Task<HtmlNode> GetStockPageAsync(string pattern, string color)
        {
            var values = new NameValueCollection();
            values.Add("PATTNO", pattern);
            values.Add("MCOLBK", color);
            values.Add("QTYORD", StockCheckQty.ToString());
            values.Add("UNITDESC", "YD");

            var pageOne = await PageFetcher.FetchAsync(StockUrl, CacheFolder.Stock, pattern + "-" + color, values);
            if (pageOne.InnerText.ContainsIgnoreCase("currently undergoing maintenance"))
                throw new Exception("Stock Check Failed: Currently Undergoing Maintenance");

            values.Add("show_price_button", "_");
            var pageTwo = await PageFetcher.FetchAsync(StockUrl, CacheFolder.Stock, pattern + "-" + color + "-price", values);
            return pageTwo;
        }
    }
}