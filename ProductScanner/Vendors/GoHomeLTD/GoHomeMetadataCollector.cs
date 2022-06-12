using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace GoHomeLTD
{
    public class GoHomeMetadataCollector : IMetadataCollector<GoHomeVendor>
    {
        private readonly GoHomeSearcher _searcher;
        private readonly IPageFetcher<GoHomeVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<GoHomeVendor> _sessionManager;

        public GoHomeMetadataCollector(GoHomeSearcher searcher, IPageFetcher<GoHomeVendor> pageFetcher, IVendorScanSessionManager<GoHomeVendor> sessionManager)
        {
            _searcher = searcher;
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var searchPage = await _pageFetcher.FetchAsync("http://www.gohomeltd.com/Store/Search.aspx", CacheFolder.Search, "collections");
            var collections = searchPage.QuerySelectorAll("a[href*='Collection']").ToList();
            await _sessionManager.ForEachNotifyAsync("Searching by Collection", collections, async node =>
            {
                var name = node.InnerText;
                var id = node.Attributes["href"].Value.Split('=').Last().ToIntegerSafe();
                var collectionProducts = (await _searcher.SearchByCollection(id));
                var matches = products.Where(x => collectionProducts.Contains(x[ScanField.ManufacturerPartNumber].ToIntegerSafe()));
                matches.ForEach(x => x[ScanField.Collection] = name);
            });

            var categories = searchPage.QuerySelectorAll("a[href*='Category']")
                .Where(x => !x.Attributes["href"].Value.Contains("Subcategory"))
                .Where(x => x.InnerText != "View All").ToList();
            await _sessionManager.ForEachNotifyAsync("Searching by Category", categories, async node =>
            {
                var name = node.InnerText;
                var id = node.Attributes["href"].Value.Split('=').Last().ToIntegerSafe();
                var collectionProducts = (await _searcher.SearchByCategory(id));
                var matches = products.Where(x => collectionProducts.Contains(x[ScanField.ManufacturerPartNumber].ToIntegerSafe()));
                matches.ForEach(x => x[ScanField.Category] = name);
            });

            var subcategories = searchPage.QuerySelectorAll("a[href*='Subcategory']");
            await _sessionManager.ForEachNotifyAsync("Searching by Subcategory", subcategories, async node =>
            {
                var name = node.InnerText;
                var url = node.Attributes["href"].Value;
                var subcategoryId = url.Split('=').Last().ToIntegerSafe();
                var categoryId = url.CaptureWithinMatchedPattern(@"Category=(?<capture>(\d+))").ToIntegerSafe();
                var collectionProducts = (await _searcher.SearchByCategoryAndSubcategory(categoryId, subcategoryId));
                var matches = products.Where(x => collectionProducts.Contains(x[ScanField.ManufacturerPartNumber].ToIntegerSafe()));
                matches.ForEach(x => x[ScanField.Category2] = name);
            });

            return products;
        }
    }
}