using System.Collections.Generic;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace GoHomeLTD
{
    public class GoHomeProductScraper : ProductScraper<GoHomeVendor>
    {
        private const string DetailUrl = "http://www.gohomeltd.com/Store/Style.aspx?Id={0}";

        public GoHomeProductScraper(IPageFetcher<GoHomeVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN;
            var detailPage = await PageFetcher.FetchAsync(string.Format(DetailUrl, mpn), CacheFolder.Details, mpn);

            var scanData = new ScanData();
            scanData[ScanField.ProductName] = detailPage.GetFieldValue("#ContentPlaceHolder1_lblItemName");
            scanData[ScanField.ManufacturerPartNumber] = mpn;
            scanData[ScanField.Dimensions] = detailPage.GetFieldValue("#ContentPlaceHolder1_lblDim");
            scanData[ScanField.Note] = detailPage.GetFieldValue("#ContentPlaceHolder1_lblNotes14");
            scanData[ScanField.Material] = detailPage.GetFieldValue("#ContentPlaceHolder1_lblMaterial");
            scanData[ScanField.Finish] = detailPage.GetFieldValue("#ContentPlaceHolder1_lblFinish");
            scanData[ScanField.OrderIncrement] = detailPage.GetFieldValue("#ContentPlaceHolder1_lblMinimumUnits1");
            scanData.Cost = detailPage.GetFieldValue("#lblOriginalPrice").Replace("$", "").ToDecimalSafe();
            scanData[ScanField.DetailUrlTEMP] = string.Format(DetailUrl, mpn);

            var stock = detailPage.QuerySelector("div[style='display:block']");
            scanData[ScanField.StockCount] = stock.Id;

            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, "http://www.gohomeltd.com" +
                detailPage.QuerySelector("#ContentPlaceHolder1_Zoom").Attributes["href"].Value.Replace("..", "")));
            return new List<ScanData> { scanData };
        }
    }
}