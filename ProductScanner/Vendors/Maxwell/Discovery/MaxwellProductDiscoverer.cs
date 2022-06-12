using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace Maxwell.Discovery
{
    public class MaxwellProductDiscoverer : IProductDiscoverer<MaxwellVendor>
    {
        private const string SearchUrl = "http://www.maxwellfabrics.com/product?page={0}";
        private readonly IPageFetcher<MaxwellVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<MaxwellVendor> _sessionManager; 
        public MaxwellProductDiscoverer(IPageFetcher<MaxwellVendor> pageFetcher, IVendorScanSessionManager<MaxwellVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = new List<Uri>();
            await _sessionManager.ForEachNotifyAsync("Discovering Products", Enumerable.Range(1, 200), async i =>
            {
                var page = await _pageFetcher.FetchAsync(string.Format(SearchUrl, i), CacheFolder.Search, i.ToString());
                products.AddRange(page.QuerySelectorAll(".node-product a").Select(x => new Uri("http://www.maxwellfabrics.com" + x.Attributes["href"].Value)));
            });

            products.AddRange(await SearchCollection("ROSEMORE"));
            products.AddRange(await SearchCollection("AVINGTON"));
            products.AddRange(await SearchCollection("CAMELLIA"));

            return products.Distinct().Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<Uri>> SearchCollection(string collection)
        {
            var url = "http://www.maxwellfabrics.com/book/" + collection;
            var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, collection);
            var products = page.QuerySelectorAll(".node-product");
            var links = products.Select(x => "http://www.maxwellfabrics.com" + x.QuerySelector("a").Attributes["href"].Value).ToList();
            return links.Select(x => new Uri(x)).ToList();
        }
    }
}