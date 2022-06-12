using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace TrendLighting
{
    public class TrendLightingMetadataCollector : IMetadataCollector<TrendLightingVendor>
    {
        private const string SearchPage = "http://www.tlighting.com/retail/filter.html?limit=all";
        private readonly IPageFetcher<TrendLightingVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<TrendLightingVendor> _sessionManager;

        public TrendLightingMetadataCollector(IPageFetcher<TrendLightingVendor> pageFetcher, IVendorScanSessionManager<TrendLightingVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var searches = await GetSearches();
            await RunSearches(products, searches);
            return products;
        }

        private async Task<List<TrendSearch>> GetSearches()
        {
            var search = await _pageFetcher.FetchAsync(SearchPage, CacheFolder.Search, "all");
            var searchOptions = search.QuerySelectorAll("#narrow-by-list span").ToList();
            var searches = new List<TrendSearch>();
            foreach (var option in searchOptions)
            {
                var category = option.GetFieldValue("dt");
                var listItems = option.QuerySelectorAll("dd li").ToList();
                foreach (var item in listItems)
                {
                    var url = GetLink(item);
                    var param = url.Split(new[] {'&'}).Last();
                    var splitParam = param.Split(new[] {'='});
                    searches.Add(new TrendSearch(category, item.InnerText.Trim(), splitParam.First(), splitParam.Last().ToIntegerSafe()));
                }
            }
            return searches;
        }

        private async Task RunSearches(IEnumerable<ScanData> products, IEnumerable<TrendSearch> searches)
        {
            await _sessionManager.ForEachNotifyAsync("Collecting Metadata", searches, async trendSearch =>
            {
                var searchResults = await _pageFetcher.FetchAsync(trendSearch.GetSearchUrl(), CacheFolder.Search, trendSearch.UrlValue.ToString());
                var resultProducts = searchResults.QuerySelectorAll(".item a").Select(x => new Uri(x.Attributes["href"].Value)).Distinct().ToList();
                var matchingProducts = products.Where(x => resultProducts.Contains(x.DetailUrl)).ToList();
                matchingProducts.ForEach(x => x[trendSearch.GetScanField()] = trendSearch.SearchValue);
            });
        }

        private string GetLink(HtmlNode item)
        {
            var link = item.QuerySelector("a");
            if (link != null) return link.Attributes["href"].Value;

            var input = item.QuerySelector("input");
            var onclick = input.Attributes["onclick"].Value.Replace("location.href = '", "").Replace("'", "");
            return onclick;
        }
    }
}