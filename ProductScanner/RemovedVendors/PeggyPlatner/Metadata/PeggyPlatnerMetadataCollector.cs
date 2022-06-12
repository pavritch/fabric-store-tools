using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace PeggyPlatner.Metadata
{
    public class PeggyPlatnerMetadataCollector : IMetadataCollector<PeggyPlatnerVendor>
    {
        private const string CollectionsUrl = "http://www.peggyplatnercollection.com/collections/all/";
        private readonly IPageFetcher<PeggyPlatnerVendor> _pageFetcher;
        private readonly IProductFileLoader<PeggyPlatnerVendor> _fileLoader;
        private readonly IVendorScanSessionManager<PeggyPlatnerVendor> _sessionManager;

        public PeggyPlatnerMetadataCollector(IPageFetcher<PeggyPlatnerVendor> pageFetcher, IProductFileLoader<PeggyPlatnerVendor> fileLoader, IVendorScanSessionManager<PeggyPlatnerVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _fileLoader = fileLoader;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var patternMetadata = await LoadPatternMetadata();
            var fileProducts = await _fileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                var pattern = product[ProductPropertyType.PatternName];
                var fileProduct = fileProducts.SingleOrDefault(x => x[ProductPropertyType.PatternName] == pattern);
                if (fileProduct == null) continue;

                var metadata = patternMetadata[pattern];

                product[ProductPropertyType.WholesalePrice] = fileProduct[ProductPropertyType.WholesalePrice].Replace("$", "");
                product[ProductPropertyType.Content] = fileProduct[ProductPropertyType.Content];
                product[ProductPropertyType.Width] = fileProduct[ProductPropertyType.Width];
                product[ProductPropertyType.Durability] = fileProduct[ProductPropertyType.Durability];
                product[ProductPropertyType.CountryOfOrigin] = fileProduct[ProductPropertyType.CountryOfOrigin];
                product[ProductPropertyType.Use] = metadata.Application.ToCommaDelimitedList();
                product[ProductPropertyType.Material] = metadata.Composition.ToCommaDelimitedList();
                product[ProductPropertyType.Style] = metadata.Style.ToCommaDelimitedList();
            }
            return products;
        }

        private async Task<Dictionary<string, PPPatternMetadata>> LoadPatternMetadata()
        {
            var patterns = await GetAllPatterns();
            var search = await _pageFetcher.FetchAsync(CollectionsUrl, CacheFolder.Search, "SearchPage");
            var dictionary = patterns.ToDictionary(x => x, v => new PPPatternMetadata());

            // application
            var applicationOptions = new List<string> {"Commercial", "Residential"};
            foreach (var option in applicationOptions)
            {
                var matchedPatterns = await GetPatternsByCategory(option);
                matchedPatterns.ForEach(x => dictionary[x].Application.Add(option));
            }

            // composition
            var compositionOptions = search.QuerySelectorAll(".span4").First().QuerySelectorAll("a").Skip(2).Select(x => x.InnerText);
            foreach (var option in compositionOptions)
            {
                var matchedPatterns = await GetPatternsByCategory(option);
                matchedPatterns.ForEach(x => dictionary[x].Composition.Add(option));
            }

            // style
            var styleOptions = search.QuerySelectorAll(".span4").ToList()[1].QuerySelectorAll("a").Select(x => x.InnerText);
            foreach (var option in styleOptions)
            {
                var matchedPatterns = await GetPatternsByCategory(option);
                matchedPatterns.ForEach(x => dictionary[x].Style.Add(option));
            }

            // color
            var colorOptions = search.QuerySelectorAll(".span4").Last().QuerySelectorAll("a").Select(x => x.InnerText);
            foreach (var option in colorOptions)
            {
                var matchedPatterns = await GetPatternsByCategory(option);
                matchedPatterns.ForEach(x => dictionary[x].Color.Add(option));
            }
            return dictionary;
        }

        private async Task<List<string>> GetAllPatterns()
        {
            const string url = "http://www.peggyplatnercollection.com/collections/all";
            var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "all");
            return page.QuerySelectorAll(".prod-name").Select(x => x.InnerText).ToList();
        }

        private async Task<List<string>> GetPatternsByCategory(string option)
        {
            var url = string.Format("http://www.peggyplatnercollection.com/collections/all/{0}", option);
            var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, option);
            return page.QuerySelectorAll(".prod-name").Select(x => x.InnerText).ToList(); 
        }
    }
}