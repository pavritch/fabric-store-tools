using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities;
using Utilities.Extensions;

namespace Nomi.Metadata
{
    public class NomiMetadataCollector : IMetadataCollector<NomiVendor>
    {
        private readonly IPageFetcher<NomiVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<NomiVendor> _sessionManager; 

        public NomiMetadataCollector(IPageFetcher<NomiVendor> pageFetcher, IVendorScanSessionManager<NomiVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var categories = await ScrapeCategory("http://www.nomiinc.com/outdoor/index-landing.php");
            await _sessionManager.ForEachNotifyAsync("Loading metadata", products, async product =>
            {
                var colorName = product[ProductPropertyType.ColorName];
                var patternName = product[ProductPropertyType.PatternName];
                var url = string.Format("http://www.nomiinc.com/outdoor/index.php?category=Patterns&pattern={0}&colorway={1}&page=1", patternName, colorName);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Details, product[ProductPropertyType.ManufacturerPartNumber]);

                product.AddImage(new ScannedImage(ImageVariantType.Primary, string.Format("http://www.nomiinc.com/outdoor/outdoor/{0}/{0}_{1}.jpg",
                    patternName.Replace(" ", "-").TitleCase(), colorName)));

                product[ProductPropertyType.Style] = GetCategory(categories, patternName);
            });
            return products;
        }

        private string GetCategory(Dictionary<string, string> categories, string patternName)
        {
            return categories[patternName.Replace("É", "E").Replace("’", "").TitleCase().Replace(" ", "-")];
        }

        private async Task<Dictionary<string, string>> ScrapeCategory(string rootUrl)
        {
            var patternsByCategory = new Dictionary<string, string>();
            var rootPage = await _pageFetcher.FetchAsync(rootUrl, CacheFolder.Search, "root");
            var categories = rootPage.QuerySelectorAll(".roll").Select(x => x.InnerText);
            foreach (var category in categories)
            {
                var patterns = await ScrapePatterns(category);
                foreach (var pattern in patterns)
                {
                    if (patternsByCategory.ContainsKey(pattern)) patternsByCategory[pattern] += ", " + category;
                    else patternsByCategory.Add(pattern, category);
                }
            }
            return patternsByCategory;
        }

        private async Task<List<string>> ScrapePatterns(string categoryName)
        {
            var patterns = new List<string>();
            for (var i = 1; ; i++)
            {
                var url = string.Format("http://www.nomiinc.com/outdoor/index.php?category={0}&page={1}", categoryName, i);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Details, categoryName + i);
                patterns.AddRange(page.QuerySelectorAll(".pattern1, .pattern2").Select(x =>
                    x.Attributes["src"].Value.CaptureWithinMatchedPattern("outdoor/(?<capture>(.*))/")));
                if (!page.InnerText.Contains("next")) break;
            }
            return patterns;
        }
    }
}