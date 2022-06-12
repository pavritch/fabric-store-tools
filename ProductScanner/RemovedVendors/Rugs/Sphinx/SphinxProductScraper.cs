using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Sphinx
{
    public class SphinxProductScraper : ProductScraper<SphinxVendor>
    {
        public SphinxProductScraper(IPageFetcher<SphinxVendor> pageFetcher) : base(pageFetcher) { }

        public async override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            // search by pattern 
            // http://www.owrugs.com/camscgip/aa2.pgm?sstring=42105
            var pattern = product.ScanData[ScanField.Pattern];
            var searchUrl = string.Format("http://www.owrugs.com/camscgip/aa2.pgm?sstring={0}", pattern);
            var searchPage = await PageFetcher.FetchAsync(searchUrl, CacheFolder.Details, pattern + "-search");
            var itemUrls = searchPage.QuerySelectorAll("a.a_item").Select(x => x.Attributes["href"].Value).ToList();

            var products = new List<ScanData>();
            foreach (var url in itemUrls)
            {
                var patternNumber = url.CaptureWithinMatchedPattern(@"Pat=(?<capture>(.*))&");
                var detailsPage = await PageFetcher.FetchAsync(url, CacheFolder.Details, patternNumber);

                // two fields with the fiber class
                var fiberFields = detailsPage.QuerySelectorAll(".a_item_fiber").ToList();
                var content = fiberFields.First().InnerText;
                var construction = fiberFields.Last().InnerText;

                var scanData = new ScanData(product.ScanData);
                scanData[ScanField.ManufacturerPartNumber] = product.ScanData[ScanField.UPC];
                scanData[ScanField.Content] = content;
                scanData[ScanField.Construction] = construction;
                scanData.DetailUrl = new Uri(url);

                scanData[ScanField.Country] = detailsPage.QuerySelector(".a_item_madeinusa") != null ? "USA" : string.Empty;

                var mainImg = detailsPage.QuerySelector("#a_product_page_img_zoom").Attributes["data-zoom-image"].Value;
                if (!mainImg.ContainsIgnoreCase("_cs"))
                    scanData.AddImage(new ScannedImage(GetImageType(mainImg), mainImg));

                var altImages = detailsPage.QuerySelectorAll("#a_product_page_alternateviews img")
                    .Select(x => x.Attributes["src"].Value.Replace("alt_thumb", "alt_high"))
                    // _cs is some kind of promo image that we don't want
                    .Where(x => !x.ContainsIgnoreCase("_cs")).ToList();
                altImages.ForEach(x => scanData.AddImage(new ScannedImage(GetImageType(x), x.Replace("_runner.png", "_runner.jpg"))));

                products.Add(scanData);
            }
            return products;
        }

        private ImageVariantType GetImageType(string url)
        {
            if (url.ContainsIgnoreCase("_runner")) return ImageVariantType.Runner;
            if (url.ContainsIgnoreCase("2x8")) return ImageVariantType.Runner;
            if (url.ContainsIgnoreCase("2x10")) return ImageVariantType.Runner;
            if (url.ContainsIgnoreCase("_rnd")) return ImageVariantType.Round;
            if (url.ContainsIgnoreCase("_rs")) return ImageVariantType.Scene;
            if (url.ContainsIgnoreCase("_sq")) return ImageVariantType.Square;
            return ImageVariantType.Rectangular;
        }
    }
}