using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace ArtAndFrameSource
{
    public class ArtAndFrameSearcher
    {
        private readonly IPageFetcher<ArtAndFrameSourceVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<ArtAndFrameSourceVendor> _sessionManager;

        public ArtAndFrameSearcher(IPageFetcher<ArtAndFrameSourceVendor> pageFetcher, IVendorScanSessionManager<ArtAndFrameSourceVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<string>> SearchSectionAsync(string baseUrl, string category)
        {
            var firstPage = await _pageFetcher.FetchAsync(baseUrl, CacheFolder.Search, "search-" + category);
            if (firstPage.InnerText.ContainsIgnoreCase("LOGIN OR CREATE AN ACCOUNT"))
            {
                // TODO: Some type of retry after login?
                // does MetadataCollector support that?
                return new List<string>();
            }
            var count = firstPage.QuerySelector(".amount").InnerText.Replace("1-60 of ", "").ToIntegerSafe();
            var numPages = count/60 + 1;
            var productUrls = new List<string>();
            await _sessionManager.ForEachNotifyAsync(string.Format("Discovering {0} Products", category), Enumerable.Range(1, numPages), async i =>
            {
                var url = baseUrl + "&p=" + i;
                var results = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "search-" + category + "-" + i.ToString());
                productUrls.AddRange(results.QuerySelectorAll("a.product-image").Select(x => x.Attributes["href"].Value).ToList());
            });
            return productUrls;
        }
    }
}