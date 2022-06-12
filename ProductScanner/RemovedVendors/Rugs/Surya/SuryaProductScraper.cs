using System;
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
using Utilities.Extensions;

namespace Surya
{
    public class SuryaProductScraper : ProductScraper<SuryaVendor>
    {
        public SuryaProductScraper(IPageFetcher<SuryaVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var detailUrl = product.DetailUrl.AbsoluteUri;
            var detailsPage = await PageFetcher.FetchAsync(detailUrl, CacheFolder.Details, detailUrl.Replace("http://www.surya.com/Rugs/", "").Replace("/", "-"));

            if (!detailsPage.InnerText.ContainsIgnoreCase("Tessa")) 
                throw new AuthenticationException();

            if (detailsPage.InnerText.ContainsIgnoreCase("Oops, we ran into an issue")) return new List<ScanData>();

            var productInfo = detailsPage.QuerySelector("div[data-info=product]");
            if (productInfo == null) return new List<ScanData>();

            var heading = productInfo.QuerySelector("h1");
            var mpn = heading.QuerySelector("span").InnerText;
            var dataList = productInfo.QuerySelectorAll("ul li").ToList();

            var scanData = new ScanData();
            scanData.DetailUrl = new Uri(detailUrl);
            // first one is always content - after that it's not consistent
            scanData[ScanField.Content] = dataList[0].InnerText.GetEverythingBeforeNewline();
            scanData[ScanField.Bullet2] = dataList[1].InnerText.GetEverythingBeforeNewline();
            scanData[ScanField.Bullet3] = dataList[2].InnerText.GetEverythingBeforeNewline();
            scanData[ScanField.Bullet4] = dataList[3].InnerText.GetEverythingBeforeNewline();
            if (dataList.Count > 4) scanData[ScanField.Bullet5] = dataList[4].InnerText.GetEverythingBeforeNewline();
            if (dataList.Count > 5) scanData[ScanField.Bullet6] = dataList[5].InnerText.GetEverythingBeforeNewline();
            if (dataList.Count > 6) scanData[ScanField.Bullet7] = dataList[6].InnerText.GetEverythingBeforeNewline();
            if (dataList.Count > 7) scanData[ScanField.Bullet8] = dataList[7].InnerText.GetEverythingBeforeNewline();

            scanData[ScanField.ManufacturerPartNumber] = mpn;

            // collection - looks like it's on the page
            // designer - shown on the pages

            // skip the first three and leave out the last two
            var rows = detailsPage.QuerySelectorAll(".skus tr").Skip(3).ToList();
            rows = rows.Take(rows.Count() - 2).ToList();
            foreach (var row in rows)
            {
                var size = row.QuerySelector(".size").InnerText;
                var sku = row.QuerySelector(".sku").InnerText;
                var stock = row.QuerySelector(".stock").InnerText;
                var price = row.QuerySelectorAll("td").ToList()[5].InnerText.Replace("$", "");
                var variant = new ScanData();
                var sizeFormatted = size.Trim().Split(new string[] {"&nbsp;", "\n"}, StringSplitOptions.RemoveEmptyEntries).First().Trim();
                variant[ScanField.Size] = sizeFormatted;
                variant[ScanField.SKU] = sku.Trim();
                variant[ScanField.StockCount] = stock.Trim();
                variant[ScanField.Cost] = price.Trim();
                variant.AddImage(new ScannedImage(GetImageVariant(sizeFormatted), string.Format("http://suryas1.blob.core.windows.net/images/512x512/large/skus/{0}.png", sku.Trim().ToLower())));

                if (sizeFormatted.Contains("Swatch") ||
                    sizeFormatted.Contains("Ringset") ||
                    sizeFormatted.Contains("Sample Blanket") ||
                    sizeFormatted.Contains("CUSTOM") ||
                    sizeFormatted.Contains("3PC")) continue;

                if (!scanData.Variants.Any(x => x[ScanField.SKU] == sku.Trim()))
                    scanData.Variants.Add(variant);
            }
            if (!scanData.Variants.Any()) return new List<ScanData>();

            return new List<ScanData> { scanData };
        }

        private ImageVariantType GetImageVariant(string size)
        {
            var dimensions = RugParser.ParseDimensions(size);
            if (dimensions == null) return ImageVariantType.Rectangular;
            return dimensions.Shape.ToImageVariantType();
        }
    }
}
