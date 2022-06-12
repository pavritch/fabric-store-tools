using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace Emissary
{
    public class EmissaryProductDiscoverer : IProductDiscoverer<EmissaryVendor>
    {
        private const string SearchUrl = "https://www.emissaryusa.com/products.html?limit=100&p={0}";
        private readonly IPageFetcher<EmissaryVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<EmissaryVendor> _session;

        public EmissaryProductDiscoverer(IPageFetcher<EmissaryVendor> pageFetcher, IVendorScanSessionManager<EmissaryVendor> session)
        {
            _pageFetcher = pageFetcher;
            _session = session;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var items = new List<Uri>();
            await _session.ForEachNotifyAsync("Discovering Products", Enumerable.Range(1, 40), async i =>
            {
                var page = await _pageFetcher.FetchAsync(string.Format(SearchUrl, i), CacheFolder.Search, "search-" + i);
                items.AddRange(page.QuerySelectorAll("a.product-image").Select(x => new Uri(x.Attributes["href"].Value)));
            });
            return items.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}