using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace LuxArtSilks
{
    public class LuxArtProductDiscoverer : IProductDiscoverer<LuxArtSilksVendor>
    {
        private const string SearchUrl = "http://luxartsilks.com/products-2/?perpage=50&pagenum={0}";

        private readonly IPageFetcher<LuxArtSilksVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<LuxArtSilksVendor> _sessionManager;

        public LuxArtProductDiscoverer(IPageFetcher<LuxArtSilksVendor> pageFetcher, IVendorScanSessionManager<LuxArtSilksVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var firstPage = await _pageFetcher.FetchAsync(string.Format(SearchUrl, 1), CacheFolder.Search, "search-1");
            var count = firstPage.GetFieldValue(".ec_product_page_showing");
            var results = count.Replace("Showing 50 of ", "").Replace("Results", "").Trim().ToIntegerSafe();
            var numPages = results/50 + 1;
            var urls = new List<string>();
            await _sessionManager.ForEachNotifyAsync("Discovering Products", Enumerable.Range(0, numPages), async i =>
            {
                var url = string.Format(SearchUrl, i + 1);
                var pageResults = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "search-" + (i + 1));
                var items = pageResults.QuerySelectorAll("a.ec_image_link_cover").ToList();
                urls.AddRange(items.Select(x => x.Attributes["href"].Value).Distinct().ToList());
            });
            return urls.Select(x => new DiscoveredProduct(new Uri(x))).ToList();
        }
    }
}