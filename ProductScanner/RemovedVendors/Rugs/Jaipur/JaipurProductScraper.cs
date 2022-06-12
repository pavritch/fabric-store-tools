using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Jaipur
{
    public class JaipurProductScraper : ProductScraper<JaipurVendor>
    {
        private const string ProductUrl = "http://www.jaipurliving.com/ShowProductDescription.aspx?code={0}";
        public JaipurProductScraper(IPageFetcher<JaipurVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN;
            var url = string.Format(ProductUrl, mpn);
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);
            //if (!detailsPage.InnerText.Contains("My Account")) throw new AuthenticationException();

            var scanData = new ScanData(product.ScanData);
            scanData[ScanField.ProductName] = detailsPage.GetFieldValue(".product-title");
            scanData[ScanField.Description] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblProductName");

            scanData.DetailUrl = new Uri(url);

            var flags = detailsPage.QuerySelectorAll(".product-info img[id*='imgAtt']").Select(x => x.Attributes["src"].Value).ToList();
            scanData[ScanField.AdditionalInfo] = string.Join(", ", flags);

            scanData[ScanField.SKU] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblSKU");
            scanData[ScanField.Design] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblDesign");
            scanData[ScanField.Color] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblColor");
            scanData[ScanField.Construction] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblConstruction");
            scanData[ScanField.Backing] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblBacking");
            scanData[ScanField.PileHeight] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblPile");
            scanData[ScanField.Style] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblstyle");
            scanData[ScanField.Content] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblFiber");
            scanData[ScanField.Country] = detailsPage.GetFieldValue("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_lblOrigin");

            scanData[ScanField.Cleaning] = detailsPage.GetFieldValue("#tab-2 .tab-body");
            scanData[ScanField.ManufacturerPartNumber] = mpn;
            //scanData[ScanField.PantoneTPX] = detailsPage.GetFieldValue("#ContentPlaceHolder1_uc_RugsDescriptionMain_lblPantone");

            var variants = new List<ScanData>();
            var variantRows = detailsPage.QuerySelectorAll("#ctl00_ContentPlaceHolder1_uc_RugsDescriptionMain_GVAddCart tr").Skip(1);
            if (!variantRows.Any()) return new List<ScanData>();

            foreach (var row in variantRows)
            {
                var columns = row.QuerySelectorAll("td").ToList();
                var size = columns[1].QuerySelector("span").InnerText.Trim();
                var shape = columns[2].QuerySelector("img").Attributes["title"].Value;
                //var inventory = columns[2].InnerText.Trim();
                var status = columns[3].InnerText.Trim();
                var price = columns[8].InnerText.Trim();

                var variant = new ScanData();
                variant[ScanField.Shape] = shape;
                variant[ScanField.Size] = size;
                variant[ScanField.StockCount] = status;
                variant[ScanField.ManufacturerPartNumber] = scanData[ScanField.SKU] + RugParser.ParseDimensions(size, shape.GetShape()).GetSkuSuffix();

                variant.Cost = Convert.ToDecimal(price.TakeOnlyLastIntegerToken());

                // some of the pages have duplicate size+shape
                if (variants.Any(x => x[ScanField.Shape] == shape && x[ScanField.Size] == size))
                    continue;

                variants.Add(variant);
            }
            scanData.Variants = variants;
            return new List<ScanData> {scanData};
        }
    }
}