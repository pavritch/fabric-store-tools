using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using Newtonsoft.Json.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Maxwell.Details
{
    public class MaxwellProductScraper : ProductScraper<MaxwellVendor>
    {
        public MaxwellProductScraper(IPageFetcher<MaxwellVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var url = context.DetailUrl.AbsoluteUri;
            var mpn = url.Replace("http://www.maxwellfabrics.com/p/", "");

            var page = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);
            if (page.InnerText.ContainsIgnoreCase("Page not found"))
                return new List<ScanData>();

            if (page.InnerText.ContainsIgnoreCase("Access denied"))
                return new List<ScanData>();

            if (!page.InnerText.ContainsIgnoreCase("Logout") ||
                page.InnerText.ContainsIgnoreCase("You must be logged in"))
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Details, mpn);
                throw new AuthenticationException();
            }

            var stock = page.GetFieldValue("div.microstockcheck");
            if (stock == null && !page.InnerText.ContainsIgnoreCase("only available in Canada"))
            {
                var inStock = await RunStockCheck(mpn);
                stock = inStock.ToString();
            }

            var product = new ScanData(context.ScanData);
            product.DetailUrl = new Uri(url);
            product[ScanField.ManufacturerPartNumber] = mpn;
            product[ScanField.ColorName] = page.GetFieldValue("div.pattern-color");
            product[ScanField.Country] = page.GetFieldValue("div.field-label:contains('Country') + div.field-items > div.field-item");
            product[ScanField.ProductUse] = page.GetFieldValue("div.field-label:contains('Usage') + div.field-items");
            product[ScanField.Cleaning] = page.GetFieldValue("div.field-label:contains('Cleaning') + div.field-items > div.field-item");
            product[ScanField.StockCount] = stock;
            product[ScanField.Ignore] = page.GetFieldValue("div.field-label:contains('Color Comment') + div.field-items > div.field-item");
            product[ScanField.AdditionalInfo] = page.GetFieldValue("div.field-label:contains('Notes') + div.field-items > div.field-item");
            product[ScanField.PatternName] = page.GetFieldValue("div.field-label:contains('Pattern Name') + div.field-items > div.field-item");
            product[ScanField.Note] = page.GetFieldValue(".us-only");

            product[ScanField.Dimensions] = page.GetFieldValue("div.field-label:contains('Roll Size') + div.field-items > div.field-item");
            product[ScanField.Repeat] = page.GetFieldValue("div.field-label:contains('Repeat') + div.field-items > div.field-item");
            product[ScanField.Match] = page.GetFieldValue("div.field-label:contains('Match') + div.field-items > div.field-item");
            product[ScanField.Type] = page.GetFieldValue("div.field-label:contains('Hanging type') + div.field-items > div.field-item");
            product[ScanField.Book] = page.GetFieldValue("div.field-label:contains('Book') + div.field-items > div.field-item");

            if (stock.ContainsIgnoreCase("Limited"))
                product.IsClearance = true;

            var tests = GetProperty(page, "Tests:");
            product[ScanField.Durability] = GetWhenFound(tests, new List<string> {"DOUBLE RUBS", "WYZENBEEK", "MARTINDALE"});
            product[ScanField.FlameRetardant] = GetWhenFound(tests, new List<string> { "UFAC", "FLAME", "NFPA", "CAL 117" });

            var cost = page.GetFieldValue("div.field-label:contains('Price') + div.field-items");
            var salePrice = page.QuerySelector("div.field-label:contains('Price') + div.field-items > strong");
            if (salePrice != null)
            {
                cost = salePrice.InnerText;
            }
            product.Cost = Convert.ToDecimal(cost.CaptureWithinMatchedPattern(@"^\$(?<capture>(\d+\.\d+))/"));

            if (!page.OuterHtml.ContainsIgnoreCase("maxwell480.png"))
            {
                var imageUrl = "http://www.maxwellfabrics.com" + page.QuerySelector(".image-download-link a").Attributes["href"].Value;
                product.AddImage(new ScannedImage(ImageVariantType.Primary, imageUrl));
            }

            if (page.InnerText.ContainsIgnoreCase("Unable to check stock at this time"))
            {
                product.IsSkipped = true;
            }
            return new List<ScanData> { product }; 
        }

        // given a data label (Content: or Tests:, etc), create a list of all
        // lines of text associated with that label. Some kinds of data are divided
        // over multiple lines.
        private List<string> GetProperty(HtmlNode topNode, string label)
        {
            var list = new List<string>();
            var selector = string.Format("div.field-label:contains('{0}')", label);
            var labelNode = topNode.QuerySelector(selector);
            if (labelNode == null)
                return list;

            var parentNode = labelNode.ParentNode;
            var valueNodes = parentNode.QuerySelectorAll("div.field-item").ToList();
            if (!valueNodes.Any())
                return list;

            list.AddRange(valueNodes.Select(node => node.InnerHtml.TrimToNull()).Where(value => value != null));
            return list;
        }

        private string GetWhenFound(IEnumerable<string> lines, IEnumerable<string> keys)
        {
            foreach (var line in lines.Where(line => keys.Any(line.ContainsIgnoreCase)))
            {
                return line;
            }
            return string.Empty;
        }

        private async Task<double> RunStockCheck(string item)
        {
            var url = "https://www.maxwellfabrics.com/maxwell_cart/lookup";
            var values = new NameValueCollection();
            values.Add("already_checked", "");
            values.Add("check_stock_status", "1");
            values.Add("product_details", "");
            values.Add("discontinued", "");
            values.Add("increment", "0.5");
            values.Add("minimum_order", "2.00");
            values.Add("price", "");
            values.Add("productAction", "checkstock");
            values.Add("minimum_reserve", "3");
            values.Add("getitem", item);
            values.Add("qty", "2");
            values.Add("qty_set[1]", "");
            values.Add("cut_set[1]", "");
            values.Add("qty_set[2]", "");
            values.Add("cut_set[2]", "");
            values.Add("qty_set[3]", "");
            values.Add("cut_set[3]", "");
            values.Add("order_actions", "");
            values.Add("check_stock_status", "");
            values.Add("order_action_status", "");
            values.Add("form_build_id", "form-JZ3CqG8ntKwf-yWMHZtlRt76L0ao6anLHawxfyVMcSc");
            values.Add("form_id", "maxwell_cart_add_to_cart_form");

            var stockData = await PageFetcher.FetchAsync(url, CacheFolder.Stock, item, values);
            dynamic data = JObject.Parse(stockData.OuterHtml);
            return data.MemoInv;
        }
    }
}