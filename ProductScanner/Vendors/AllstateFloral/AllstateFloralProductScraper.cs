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

namespace AllstateFloral
{
    public class AllstateFloralProductScraper : ProductScraper<AllstateFloralVendor>
    {
        public AllstateFloralProductScraper(IPageFetcher<AllstateFloralVendor> pageFetcher, IStorageProvider<AllstateFloralVendor> storageProvider) : base(pageFetcher)
        {
        }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var mpn = url.Replace("http://www.allstatefloral.com/Productitemdetail.cfm?", "").Replace("&Banner=1", "").Replace("ItemNumber=", "");
            var detailPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, mpn);

            var scanData = new ScanData();
            scanData.DetailUrl = product.DetailUrl;

            var descriptionHtml = detailPage.GetFieldHtml("td:contains('Description') + td font");
            var split = descriptionHtml.Split(new[] {"&nbsp;"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            scanData[ScanField.Description] = split.First();
            if (split.Count > 1 && !split.Last().Contains("w/") && !split.Last().Contains("("))
                scanData[ScanField.ColorName] = split.Last();

            scanData[ScanField.UnitOfMeasure] = detailPage.GetFieldValue("td:contains('Uom') + td");
            scanData[ScanField.MinimumQuantity] = detailPage.GetFieldValue("td:contains('MinQty') + td");
            //scanData[ScanField.Ignore] = detailPage.GetFieldValue("td:contains('BoxQty') + td");
            scanData[ScanField.StockCount] = detailPage.GetFieldValue("td:contains('Avail.') + td");
            scanData[ScanField.Length] = detailPage.GetFieldValue("td:contains('ProdLength') + td");
            scanData[ScanField.Weight] = detailPage.GetFieldValue("td:contains('ProdWeight') + td");
            scanData[ScanField.ShippingWeight] = detailPage.GetFieldValue("td:contains('BoxWeight') + td");
            //scanData[ScanField.Ignore] = detailPage.GetFieldValue("td:contains('CsWeight') + td");
            scanData[ScanField.Dimensions] = detailPage.GetFieldValue("td:contains('Box_LxWxH') + td");
            scanData[ScanField.Packaging] = detailPage.GetFieldValue("td:contains('Case_LxWxH') + td");
            scanData[ScanField.UPC] = detailPage.GetFieldValue("td:contains('UPC') + td");

            scanData[ScanField.Classification] = detailPage.GetFieldValue("td:contains('Class') + td");
            scanData[ScanField.ColorGroup] = detailPage.GetFieldValue("td:contains('ColorGrp') + td");
            scanData[ScanField.Season] = detailPage.GetFieldValue("td:contains('Season') + td");
            //scanData[ScanField.Ignore] = detailPage.GetFieldValue("td:contains('Oversize') + td");
            scanData[ScanField.AlternateItemNumber] = detailPage.GetFieldValue("td:contains('Alt-Item') + td");
            scanData[ScanField.Category] = detailPage.GetFieldValue("td:contains('Category') + td");

            scanData[ScanField.ManufacturerPartNumber] = detailPage.QuerySelector("input[name='ItemNumber']").Attributes["value"].Value;

            scanData.Cost = Math.Round(detailPage.GetFieldValue("td:contains('BasePrice') + td").Replace("$", "").ToDecimalSafe() * .8m, 2);

            var image = detailPage.QuerySelector("td.box1 img");
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, string.Format("http://scanner.insidefabric.com/vendors/AllstateFloral/{0}.jpg", mpn)));

            return new List<ScanData> { scanData };
        }
    }
}