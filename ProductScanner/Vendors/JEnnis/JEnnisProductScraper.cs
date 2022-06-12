using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace JEnnis
{
    /*
    public class JEnnisProductScraper : ProductScraper<JEnnisVendor>
    {
        private Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            //{ "Description", ScanField.ProductName },
            { "Abrasion", ScanField.Durability },
            { "Backing Content", ScanField.Backing },
            { "Cautions", ScanField.Ignore },
            { "Cleaning Instruction", ScanField.Cleaning },
            { "Coating", ScanField.Ignore },
            { "Cold Crack", ScanField.Ignore },
            { "Colors Available", ScanField.Color },
            { "Construction", ScanField.Construction },
            { "Content", ScanField.Content },
            { "Description", ScanField.Description },
            { "Eco Friendly", ScanField.Ignore },
            { "Features", ScanField.AdditionalInfo },
            { "Fire Retardancy", ScanField.FlameRetardant },
            { "Frequently Asked Que", ScanField.Ignore },
            { "Hide Size", ScanField.Ignore },
            { "Mildew", ScanField.Ignore },
            { "Pilling", ScanField.Ignore },
            { "Protective Finish", ScanField.Finish },
            { "Railroaded Pattern", ScanField.Railroaded },
            { "Repeat Length", ScanField.VerticalRepeat },
            { "Repeat Width", ScanField.HorizontalRepeat },
            { "Roll Size", ScanField.AverageBolt },
            { "Seam Slippage", ScanField.Ignore },
            { "Shrinkage", ScanField.Ignore },
            { "Style", ScanField.Style },
            { "Tear Strength", ScanField.Ignore },
            { "Tensile Strength", ScanField.Ignore },
            { "Ultraviolet", ScanField.Ignore },
            { "Uses", ScanField.Use },
            { "Weight", ScanField.Weight },
            { "Width", ScanField.Width },
        }; 

        public JEnnisProductScraper(IPageFetcher<JEnnisVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN;
            var url = string.Format("https://www.jennisfabrics.com/jennis-web-core/productProfile.jef?prdCode={0}&invType=REG", mpn);
            var details = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);

            if (details.InnerText.ContainsIgnoreCase("Application Error"))
                return new List<ScanData>();

            var scanData = new ScanData();

            var specRows = details.QuerySelectorAll(".tblbrdr2 tr").ToList();
            foreach (var row in specRows)
            {
                var cells = row.QuerySelectorAll("td").ToList();
                if (cells.Count == 2)
                {
                    var field = cells[0].InnerText.Trim();
                    var value = cells[1].InnerText.Trim();
                    scanData[_fields[field]] = value;
                }
                if (cells.Count == 1)
                {
                    var productName = cells[0].InnerText.Trim();
                    scanData[ScanField.ManufacturerPartNumber] = productName;
                }
            }

            scanData[ScanField.ItemNumber] = mpn;

            var pricing = details.QuerySelectorAll(".tblpricing").First();
            var cost = pricing.QuerySelectorAll("td:contains('$')").Select(x => x.InnerText).First();

            scanData.Cost = cost.Replace("$", "").ToDecimalSafe();

            var stock = details.QuerySelector("b:contains('Available Stock')");
            var stockCount = stock.ParentNode.ParentNode.NextSibling.NextSibling.InnerText.Trim();
            scanData[ScanField.StockCount] = stockCount;

            var patternColor = details.QuerySelectorAll(".ptrn_hdng").Select(x => x.InnerText).ToList();
            scanData[ScanField.PatternName] = patternColor.First().Trim();
            scanData[ScanField.ColorName] = patternColor.Last().Trim();
            scanData.DetailUrl = new Uri(url);

            var image = details.QuerySelectorAll(".img_bg img").Select(x => x.Attributes["src"].Value).FirstOrDefault(x => x.Contains("large"));
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image.Replace("..", "https://www.jennisfabrics.com")));

            return new List<ScanData> { scanData };
        }
    }
    */
}