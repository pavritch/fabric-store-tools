using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace FSchumacher.Metadata
{
    public class FSchumacherMetadataCollector : IMetadataCollector<FSchumacherVendor>
    {
        private const string SearchPageUrl = "https://www.fschumacher.com/browse/fabrics";
        private const string KeywordSearchUrl = "http://www.fschumacher.com/search/SearchResults.aspx?KeyWords={0}&ResultsPerPage=ViewAll";
        private const string ColorSearchUrl = "http://www.fschumacher.com/search/SearchResults.aspx?sColors={0}&ResultsPerPage=ViewAll";

        private readonly IVendorScanSessionManager<FSchumacherVendor> _sessionManager;
        private readonly IPageFetcher<FSchumacherVendor> _pageFetcher;
        public FSchumacherMetadataCollector(IVendorScanSessionManager<FSchumacherVendor> sessionManager, IPageFetcher<FSchumacherVendor> pageFetcher)
        {
            _sessionManager = sessionManager;
            _pageFetcher = pageFetcher;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchPageUrl, CacheFolder.Search, "search-page");
            //var colorOptions = 

//            var keywordOptions = searchPage.QuerySelectorAll("#CBL_KEYWORDS label").Select(x => x.InnerText).ToList();
//            var colorOptions = searchPage.QuerySelectorAll("#CBL_COLORS span").Select(x => x.Attributes["xvalue"].Value).ToList();

//            await _sessionManager.ForEachNotifyAsync("Searching by Keyword", keywordOptions, async keywordOption =>
//            {
//                var keywordPage = await _pageFetcher.FetchAsync(string.Format(KeywordSearchUrl, keywordOption), CacheFolder.Search, keywordOption);
//                var skus = keywordPage.QuerySelectorAll("a.borderArt").Select(x => x.Attributes["href"].Value)
//                    .Select(x => x.Substring(x.IndexOf("=") + 1)).ToList();
//                products.Where(x => skus.Contains(x[ProductPropertyType.ManufacturerPartNumber])).ForEach(x => x[ProductPropertyType.Style] = keywordOption);
//            });

//            await _sessionManager.ForEachNotifyAsync("Searching by Color", colorOptions, async colorOption =>
//            {
//                var colorPage = await _pageFetcher.FetchAsync(string.Format(ColorSearchUrl, colorOption), CacheFolder.Search, colorOption);
//                var skus = colorPage.QuerySelectorAll("a.borderArt").Select(x => x.Attributes["href"].Value)
//                    .Select(x => x.Substring(x.IndexOf("=") + 1)).ToList();
//                products.Where(x => skus.Contains(x[ProductPropertyType.ManufacturerPartNumber])).ForEach(x => x[ProductPropertyType.ColorGroup] = colorOption);
//            });

            return products;
        }
    }
}