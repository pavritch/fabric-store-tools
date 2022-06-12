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

namespace AllstateFloral
{
    public class AllstateFloralProductDiscoverer : IProductDiscoverer<AllstateFloralVendor>
    {
        private const string SearchUrl = "https://www.allstatefloral.com/pro_i/index.cfm?1=1&list_f=1&showpic=y&KeyWord=&CategoryCode=&ColorNumber=&SizeCode=&PRICE_FM=&PRICE_TO=&page={0}";
        private readonly IPageFetcher<AllstateFloralVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<AllstateFloralVendor> _sessionManager;

        public AllstateFloralProductDiscoverer(IPageFetcher<AllstateFloralVendor> pageFetcher, IVendorScanSessionManager<AllstateFloralVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var pageOne = await _pageFetcher.FetchAsync(string.Format(SearchUrl, 1), CacheFolder.Search, "search-1");
            var numProducts = pageOne.QuerySelector("td:contains('items matched') font").InnerText.ToIntegerSafe();
            var numPages = numProducts/50 + 1;
            var products = new List<Uri>();
            await _sessionManager.ForEachNotifyAsync("Discovering Products", Enumerable.Range(1, numPages), async i =>
            {
                var page = await _pageFetcher.FetchAsync(string.Format(SearchUrl, i), CacheFolder.Search, "search-" + i);
                var urls = page.QuerySelectorAll("a.font3").Select(x => new Uri(x.Attributes["href"].Value.Replace("../", "http://www.allstatefloral.com/"))).ToList();
                products.AddRange(urls.ToList());
            });
            return products.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}