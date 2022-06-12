using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace SuryaHomeware
{
    public class SuryaProductScraper : ProductScraper<SuryaHomewareVendor>
    {
        private const string SearchUrl = "http://www.surya.com/search/?searchtext={0}";

        public SuryaProductScraper(IPageFetcher<SuryaHomewareVendor> pageFetcher) : base(pageFetcher) { }

        public override async Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            var searchText = product.ScanData[ScanField.PatternNumber];
            var searchPage = await PageFetcher.FetchAsync(string.Format(SearchUrl, searchText), CacheFolder.Search, searchText);
            var results = searchPage.QuerySelectorAll(".product-name a").ToList();
            if (searchPage.InnerText.ContainsIgnoreCase("No results.")) return new List<ScanData>();
            if (results.Count != 1) { return new List<ScanData>(); }

            var detailUrl = results.First().Attributes["href"].Value;
            var detailsPage = await PageFetcher.FetchAsync(detailUrl, CacheFolder.Details, searchText);
            if (!detailsPage.InnerText.ContainsIgnoreCase("Tessa"))
            {
                PageFetcher.RemoveCachedFile(CacheFolder.Details, product.ScanData[ScanField.UPC]);
                throw new AuthenticationException();
            }
            if (detailsPage.InnerText.ContainsIgnoreCase("Oops, we ran into an issue"))
                return new List<ScanData>();

            var productInfo = detailsPage.QuerySelector("div[data-info=product]");
            if (productInfo == null)
                return new List<ScanData>();

            var fileVariants = product.ScanData.Variants;

            var scanData = new ScanData(product.ScanData);
            scanData.DetailUrl = new Uri(detailUrl);
            var image = detailsPage.QuerySelector(".main-image a");
            if (image == null) return new List<ScanData>();

            scanData.AddImage(new ScannedImage(ImageVariantType.Primary, image.Attributes["href"].Value));

            var rows = detailsPage.QuerySelectorAll(".skusChart tr").Skip(1).ToList();
            foreach (var row in rows)
            {
                var specs = row.QuerySelector(".sku").InnerHtml.Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Where(x => !x.ContainsIgnoreCase("img")).ToList();
                var size = specs.First().Trim();
                var sku = specs.Last().Trim();

                var stock = row.QuerySelector(".stock").InnerText;
                var price = row.QuerySelectorAll("td").ToList()[6].InnerText.Replace("$", "");

                var variantMatch = fileVariants.FirstOrDefault(x => x[ScanField.ManufacturerPartNumber] == sku);
                if (variantMatch == null)
                {
                    // this is for the pillow that isn't listed in the file
                    // we want to take all of the data from the first variant, and then just fill in the extra info
                    variantMatch = !fileVariants.Any() ? new ScanData() : new ScanData(fileVariants.First());
                }

                //variantMatch.Cost = variantMatch[ScanField.Cost].ToDecimalSafe();
                variantMatch.Cost = price.ToDecimalSafe();
                variantMatch[ScanField.Size] = size;
                variantMatch[ScanField.SKU] = sku;
                variantMatch[ScanField.ManufacturerPartNumber] = sku.Trim();
                variantMatch[ScanField.StockCount] = stock.Trim();
                //variant.AddImage(new ScannedImage(GetImageVariant(sizeFormatted), string.Format("http://suryas1.blob.core.windows.net/images/512x512/large/skus/{0}.png", sku.Trim().ToLower())));
                if (!scanData.Variants.Any(x => x[ScanField.ManufacturerPartNumber] == sku.Trim()))
                    scanData.Variants.Add(variantMatch);
            }
            return new List<ScanData> { scanData };
        }
    }
}