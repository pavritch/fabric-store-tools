using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Nourison
{
    public class NourisonImageChecker : IImageChecker<NourisonVendor>
    {
        public bool CheckImage(HttpWebResponse response)
        {
            return response.StatusCode != HttpStatusCode.NotFound;
        }
    }

    public class NourisonProductScraper : ProductScraper<NourisonVendor>
    {
        public NourisonProductScraper(IPageFetcher<NourisonVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var url = product.DetailUrl.AbsoluteUri;
            var details = await PageFetcher.FetchAsync(url, CacheFolder.Details, url.Replace("https://www.nourison.com/", ""));

            var scanData = new ScanData(product.ScanData);
            var productName = details.GetFieldValue(".product-name");
            var dataItems = details.QuerySelectorAll(".description-bullet-list li").Select(x => x.InnerText).ToList();
            var image = details.QuerySelector("a.MagicZoomPlus");

            if (image != null)
            {
                scanData.AddImage(new ScannedImage(ImageVariantType.Rectangular, image.Attributes["href"].Value));
                scanData.AddImage(new ScannedImage(ImageVariantType.Rectangular, image.Attributes["href"].Value.Replace("/cache/2", "/cache/1")));
            }

            if (productName == null) return new List<ScanData>();

            var parts = productName.Split(new[] { Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Last()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (dataItems.Count > 0) scanData[ScanField.Content] = dataItems[0];
            if (dataItems.Count > 1) scanData[ScanField.Bullet1] = dataItems[1];
            if (dataItems.Count > 2) scanData[ScanField.Bullet2] = dataItems[2];
            if (dataItems.Count > 3) scanData[ScanField.Bullet3] = dataItems[3];

            scanData[ScanField.Description] = details.GetFieldValue(".description-std");
            scanData[ScanField.PatternNumber] = parts.First();
            scanData[ScanField.ColorName] = parts.Last();
            scanData[ScanField.ProductName] = productName;
            scanData[ScanField.ManufacturerPartNumber] = parts.First();
            scanData[ScanField.Collection] = productName.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).First();

            var variantRows = details.QuerySelectorAll(".details-table tr").Skip(1).ToList();

            foreach (var variantRow in variantRows)
            {
                var cells = variantRow.QuerySelectorAll("td").ToList();

                var scanDataVariant = new ScanData();
                scanDataVariant[ScanField.Size] = cells[0].GetFieldValue(".imperial-size").Replace(".", "'").Replace("''", "\"").Replace("SAMPLE", "");
                scanDataVariant[ScanField.StockCount] = cells[1].InnerText;
                scanDataVariant[ScanField.SKU] = cells[2].InnerText;
                if (cells[3].QuerySelector("img") != null)
                {
                    var shape = cells[3].QuerySelector("img").Attributes["title"].Value;
                    if (shape == "round") scanDataVariant[ScanField.Shape] = ProductShapeType.Round.ToString();
                    else if (shape == "rectangle") scanDataVariant[ScanField.Shape] = ProductShapeType.Rectangular.ToString();
                    else if (shape == "square") scanDataVariant[ScanField.Shape] = ProductShapeType.Square.ToString();
                    else if (shape == "runner") scanDataVariant[ScanField.Shape] = ProductShapeType.Runner.ToString();
                    else if (shape == "oval") scanDataVariant[ScanField.Shape] = ProductShapeType.Oval.ToString();
                    else if (shape == "octagon") scanDataVariant[ScanField.Shape] = ProductShapeType.Octagon.ToString();
                    else if (shape == "blanket") continue;
                    else if (shape == "free form") continue;
                    else continue;
                }
                else
                {
                    scanDataVariant[ScanField.RetailPrice] = cells[3].InnerText;
                }

                if (!scanData.Variants.Any(x => x[ScanField.Size] == scanDataVariant[ScanField.Size]) && scanDataVariant[ScanField.Shape] != "")
                    scanData.Variants.Add(scanDataVariant);
            }

            var moreImages = details.QuerySelectorAll(".MagicToolboxSelectorsContainer ul li a").Select(x => x.Attributes["href"].Value).ToList();
            moreImages.ForEach(x => scanData.AddImage(new ScannedImage(GetImageType(x, scanData.Variants), x)));

            return new List<ScanData> { scanData };
        }

        private ImageVariantType GetImageType(string url, List<ScanData> variants)
        {
            if (url.Contains("_1.png")) return ImageVariantType.Scene;
            if (url.Contains("_2.png")) return ImageVariantType.Scene;
            if (url.Contains("_3.png")) return ImageVariantType.Scene;

            if (url.Contains("_1_")) return ImageVariantType.Scene;
            if (url.Contains("_2_")) return ImageVariantType.Scene;
            if (url.Contains("_3_")) return ImageVariantType.Scene;

            var parts = url.Split(new[] {'_'}).ToList();
            if (parts.Count == 1) return ImageVariantType.Rectangular;

            var size = parts[parts.Count - 2];
            var match = variants.SingleOrDefault(x => x[ScanField.Size].Replace("'", "").Replace("\"", "") == size);
            if (match != null) return match[ScanField.Shape].ToImageVariantType();

            return ImageVariantType.Rectangular;
        }
    }
}