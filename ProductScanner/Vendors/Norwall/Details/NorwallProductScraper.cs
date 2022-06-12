using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Norwall.Details
{
    public class NorwallProductScraper : ProductScraper<NorwallVendor>
    {
        private const int StockCheckQuantity = 2;
        public NorwallProductScraper(IPageFetcher<NorwallVendor> pageFetcher) : base(pageFetcher) { }
        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            var mpn = context.MPN;
            var detailUrl = string.Format("http://norwall.net/collection_search_result.php?sku={0}", mpn);
            var detailPage = await PageFetcher.FetchAsync(detailUrl, CacheFolder.Details, mpn);

            // this is not working correctly - I think I need to post first?
            var url = string.Format("http://www.pattonwallcoverings.net/product_details.php?pageID=15&prtno={0}&qty={1}&lotno=&rowno=0&op=inq", 
                mpn, StockCheckQuantity);
            var stockXml = await PageFetcher.FetchAsync(url, CacheFolder.Stock, mpn);

            var newProduct = CreateProduct(detailPage, stockXml, mpn, detailUrl);
            return new List<ScanData> {newProduct};
        }

        private ScanData CreateProduct(HtmlNode detailPage, HtmlNode stockPage, string sku, string detailUrl)
        {
            var product = new ScanData();
            var values = detailPage.QuerySelector(".simple_list .text_01").InnerHtml;

            var collection = values.CaptureWithinMatchedPattern("</strong> (?<capture>(.*))\n");
            var match = values.CaptureWithinMatchedPattern("Match: (?<capture>(.*)) <br>");
            var repeat = values.CaptureWithinMatchedPattern("Repeat: (?<capture>(.*))\"");
            var coverage = values.CaptureWithinMatchedPattern("Coverage:</strong><br>\n\t\t\t\t\t\t(?<capture>(.*))<br>");

            var imageUrl = detailPage.QuerySelector("#rightcolumn img").Attributes["src"].Value;
            product.AddImage(new ScannedImage(ImageVariantType.Primary, "http://norwall.net/" + imageUrl.Replace("medium", "larg")));
            product.DetailUrl = new Uri(detailUrl);
            product[ScanField.ManufacturerPartNumber] = sku;
            product[ScanField.AdditionalInfo] = values;
            product[ScanField.Collection] = collection;
            product[ScanField.Match] = match;
            product[ScanField.Repeat] = repeat;
            product[ScanField.Coverage] = coverage;

            var available = stockPage.QuerySelector("availqty").InnerText;
            var message = stockPage.QuerySelector("message").InnerText;
            var unitValue = stockPage.QuerySelector("soldin").InnerText;
            var price = stockPage.QuerySelector("unitprice").InnerText;

            var unit = "Roll";
            if (unitValue == "BLT")
                unit = "Each";

            product[ScanField.StockCount] = available == "Available" ? "999999" : "0";
            product[ScanField.UnitOfMeasure] = unit;
            product[ScanField.Ignore] = message;
            // I should just pass 'unitValue' in as UnitOfMeasure, and then do the processing in the ProductBuilder
            product[ScanField.Bullet1] = unitValue;
            product.Cost = price.ToDecimalSafe();
            return product;
        }
    }
}