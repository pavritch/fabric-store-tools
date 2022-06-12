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

namespace CurreyCo
{
    public class CurreyProductScraper : ProductScraper<CurreyVendor>
    {
        private readonly Dictionary<string, ScanField> _fields = new Dictionary<string, ScanField>
        {
            { "Bulb Type", ScanField.Bulbs},
            { "Chain Length", ScanField.ChainLength},
            { "Dimensions", ScanField.Dimensions},
            { "Finish", ScanField.Finish},
            { "Freight Information", ScanField.Packaging},
            { "Item Wt", ScanField.Weight},
            { "Material", ScanField.Material},
            { "Number of Lights", ScanField.NumberOfLights},
            { "Pkg Wt", ScanField.ShippingWeight},
            { "Product Name", ScanField.ProductName},
            { "Shades", ScanField.Shade},
            { "Stock Status", ScanField.StockCount},
            { "Total Wattage", ScanField.Wattage},
            { "Wattage Per Light", ScanField.WattagePerLight},
            { "Extra Chain SKU", ScanField.Ignore},
        }; 

        public CurreyProductScraper(IPageFetcher<CurreyVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, url.Replace("https://www.curreyandcompany.com/", ""));

            var breadcrumbs = detailsPage.QuerySelectorAll("a.SectionTitleText").Select(x => x.InnerText).ToList();

            var sku = detailsPage.GetFieldValue(".sku");
            if (string.IsNullOrWhiteSpace(sku)) return new List<ScanData>();

            var scanData = new ScanData(product.ScanData);
            scanData.DetailUrl = product.DetailUrl;
            scanData[ScanField.Category] = string.Join(", ", breadcrumbs);
            scanData[ScanField.Description] = detailsPage.GetFieldValue(".overviewDetails");
            scanData[ScanField.ManufacturerPartNumber] = detailsPage.GetFieldValue(".sku").Replace("#", "");
            scanData[ScanField.SKU] = detailsPage.GetFieldValue(".sku").Replace("#", "");

            var price = detailsPage.GetFieldValue(".price");
            if (price == null)
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Details, url.Replace("https://www.curreyandcompany.com/", ""));
                throw new AuthenticationException();
            }

            scanData.Cost = price.Replace("&nbsp;", "").Replace("$", "").ToDecimalSafe();

            // MAP & Retail (identical) are 2.5x the cost that we pull
            scanData[ScanField.MAP] = (scanData.Cost*2.5m).ToString();

            var bullets = detailsPage.QuerySelectorAll(".specsDetails ul li").ToList();
            foreach (var bullet in bullets)
            {
                var split = bullet.InnerText.Split(new[] {':'}).ToList();
                var key = split.First();
                var value = split.Last();

                var field = _fields[key];
                scanData[field] = value;
            }

            var image = detailsPage.QuerySelector(".highRes a");
            if (image != null)
                scanData.AddImage(new ScannedImage(ImageVariantType.Primary, "https://www.curreyandcompany.com/" + image.Attributes["onclick"].Value.Replace("startDownload('", "").Replace("')", "")));

            return new List<ScanData> { scanData };
        }
    }
}