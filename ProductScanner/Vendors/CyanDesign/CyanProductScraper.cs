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
using Utilities.Extensions;

namespace CyanDesign
{
    public class CyanProductScraper : ProductScraper<CyanDesignVendor>
    {
        private const string StockUrl = "http://www.cyandesign.info/products/qsearchresults.aspx?ITEM={0}&Q=500";
        private const string DetailUrl = "http://www.cyandesign.biz/index.cfm?fuseaction=app.detail&ProductNumber={0}&CategoryID=";

        private const string SearchUrl = "http://cyan.design/search_result.asp?params=ENumber:{0};";
        private const string ApiUrl = "http://cyan.design/usmanajax_search.asp?params=ENumber:{0};&rand=20654959400517";
        private const string ApiUrl2 = "http://cyan.design/usmanajax_result.asp?page=number&number=1&qitems=24&orderind=0&menu=fffffffffffff&regSL=0&rand=235310280275931";

        public CyanProductScraper(IPageFetcher<CyanDesignVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var mpn = product.MPN.PadLeft(5, '0').Replace("/", "-");

            var searchPage = await PageFetcher.FetchAsync(string.Format(ApiUrl, mpn), CacheFolder.Search, mpn);
            var searchPage2 = await PageFetcher.FetchAsync(string.Format(ApiUrl2, mpn), CacheFolder.Search, mpn + "-2");
            var test = searchPage.QuerySelector("#XAnchorIMG1");
            if (test == null) return new List<ScanData>();

            var stockPage = await PageFetcher.FetchAsync(string.Format(StockUrl, mpn), CacheFolder.Stock, mpn);
            var detailsPage = await PageFetcher.FetchAsync(string.Format(DetailUrl, mpn), CacheFolder.Details, mpn);

            // images from public site? http://images.cyandesign.biz.s3.amazonaws.com/large/04395-default.jpg
            var scanData = new ScanData(product.ScanData);
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, string.Format("http://images.cyandesign.biz.s3.amazonaws.com/large/{0}-default.jpg", mpn)));
            //scanData.AddImage(new ScannedImage(ImageVariantType.Other, string.Format("http://images.cyandesign.biz.s3.amazonaws.com/large/{0}-lit.jpg", mpn)));

            if (stockPage.InnerText.ContainsIgnoreCase("Item is Obsolete")) return new List<ScanData>();
            if (stockPage.InnerText.ContainsIgnoreCase("Invalid Item Number")) return new List<ScanData>();
            if (stockPage.InnerText.ContainsIgnoreCase("There has been a problem in proccessing your request")) return new List<ScanData>();

            if (detailsPage.InnerText.ContainsIgnoreCase("The system has encountered an error")) return new List<ScanData>();
            if (detailsPage.InnerText.ContainsIgnoreCase("Beautiful Objects for Beautiful Lives")) return new List<ScanData>();

            var availableStock = stockPage.QuerySelector("#dgQSearch tr.DGItem").QuerySelector("td").InnerText;
            var netPrice = stockPage.QuerySelector("#dgPSearch tr.DGItem").QuerySelectorAll("td").ToList()[2].InnerText;
            scanData.Cost = netPrice.ToDecimalSafe();

            //scanData[ScanField.TempContent1] = stockPage.GetFieldValue("#lblNumberOfData");
            //scanData[ScanField.TempContent3] = stockPage.GetFieldValue("#lblSeriesData");
            scanData[ScanField.Packaging] = stockPage.GetFieldValue("#lblCanShipData");
            scanData[ScanField.Dimensions] = stockPage.GetFieldValue("#lblDimensionsData");
            scanData[ScanField.ProductName] = stockPage.GetFieldValue("#lblDescData");
            scanData[ScanField.ProductType] = stockPage.GetFieldValue("#lblProductTypeData");
            scanData[ScanField.StockCount] = availableStock;
            scanData.DetailUrl = new Uri(string.Format(DetailUrl, mpn));

            var values = detailsPage.QuerySelectorAll(".product_description p");
            scanData[ScanField.Color] = values.Last().InnerText;

            return new List<ScanData> { scanData };
        }
    }
}