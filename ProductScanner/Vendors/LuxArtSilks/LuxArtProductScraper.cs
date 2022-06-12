using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace LuxArtSilks
{
    public class LuxArtProductScraper : ProductScraper<LuxArtSilksVendor>
    {
        public LuxArtProductScraper(IPageFetcher<LuxArtSilksVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var modelCost = url.Replace("http://luxartsilks.com/products-2/?model_number=", "");
            var split = modelCost.Split('-').ToList();
            var costStr = split.Last();
            if (costStr.StartsWith("0")) costStr = costStr.Substring(1);

            var model = string.Join("-", split.Take(split.Count - 1));
            if (model == string.Empty) return new List<ScanData>();

            var details = await PageFetcher.FetchAsync(url, CacheFolder.Details, model);
            var image = details.QuerySelector(".ec_details_main_image img").Attributes["src"].Value;

            var scanData = new ScanData();
            scanData.DetailUrl = product.DetailUrl;
            scanData[ScanField.ManufacturerPartNumber] = modelCost;

            //using (StreamWriter w = File.AppendText(@"c:\mpns.txt"))
            //{
                //var query = "update ProductVariant set ManufacturerPartNumber = '{0}' where ProductVariant.ManufacturerPartNumber = '{1}' and ProductID in (select ProductID from ProductManufacturer where ManufacturerID=161)";
                //w.WriteLine(query, modelCost, model);
            //}

            scanData[ScanField.ProductName] = details.GetFieldValue(".ec_details_title");
            scanData[ScanField.Description] = string.Join(", ", details.GetFieldValue(".ec_details_description").Split(new[] {"<br>"}, 
                StringSplitOptions.RemoveEmptyEntries));
            scanData[ScanField.Category] = details.GetFieldValue(".ec_details_categories");
            //scanData[ScanField.Brand] = details.GetFieldValue(".ec_details_manufacturer");
            scanData[ScanField.AdditionalInfo] = string.Join(", ", details.QuerySelectorAll(".ec_details_description_content p").Select(x => x.InnerText));
            scanData.Cost = string.Concat(costStr.Reverse()).ToIntegerSafe();
            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image));
            return new List<ScanData> { scanData };
        }
    }
}