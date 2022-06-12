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

namespace WinfieldThybony.Details
{
    public class WinfieldThybonyProductScraper : ProductScraper<WinfieldThybonyVendor>
    {
        private const string WTDetailUrl = "http://www.winfieldthybony.com/home/products/details?sku={0}";
        public WinfieldThybonyProductScraper(IPageFetcher<WinfieldThybonyVendor> pageFetcher) : base(pageFetcher) { }

        private bool ValidatePage(HtmlNode page)
        {
            return !page.InnerText.Contains("Sitefinity trial version");
        }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN;
            var wtDetailsPage = await PageFetcher.FetchAsync(string.Format(WTDetailUrl, mpn), CacheFolder.Details, mpn + "-wt", pageValidator:ValidatePage);

            var colorElement = wtDetailsPage.QuerySelector("#ctl00_mainContent_C001_pName");
            var pattern = colorElement.PreviousSibling;

            var relatedProducts = wtDetailsPage.QuerySelectorAll("#ctl00_mainContent_C001_divAddColors img").Select(x => x.Attributes["alt"].Value).ToList();
            var packaging = wtDetailsPage.QuerySelector("h4:contains('Package Information')");
            var packagingInfo = string.Empty;
            if (packaging != null)
                packagingInfo = packaging.ParentNode.QuerySelector("li").InnerText;

            var scanData = new ScanData();
            scanData.RelatedProducts = relatedProducts;
            scanData[ScanField.Backing] = wtDetailsPage.GetFieldValue("strong:contains('Backing'):select-parent");
            scanData[ScanField.OrderInfo] = wtDetailsPage.GetFieldValue("strong:contains('Roll Length'):select-parent");
            scanData[ScanField.Repeat] = wtDetailsPage.GetFieldValue("strong:contains('Repeat'):select-parent");
            scanData[ScanField.Match] = wtDetailsPage.GetFieldValue("strong:contains('Pattern Match'):select-parent");
            scanData[ScanField.FireCode] = wtDetailsPage.GetFieldValue("strong:contains('Fire Rating'):select-parent");
            scanData[ScanField.Description] = wtDetailsPage.QuerySelector(".detailDescription p").InnerText;
            scanData[ScanField.Packaging] = packagingInfo;
            scanData[ScanField.Width] = wtDetailsPage.GetFieldValue("strong:contains('Width'):select-parent");

            scanData[ScanField.ManufacturerPartNumber] = mpn;
            scanData[ScanField.Color] = colorElement.InnerText;
            scanData[ScanField.Pattern] = pattern.InnerText;
            scanData.DetailUrl = new Uri(string.Format(WTDetailUrl, mpn));
            //scanData.AddImage(new ScannedImage(ImageVariantType.Primary, "http:" + image));
            return new List<ScanData> {scanData};
        }
    }
}