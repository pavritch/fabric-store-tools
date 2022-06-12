using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace TrendLighting
{
    public class TrendLightingProductDiscoverer : IProductDiscoverer<TrendLightingVendor>
    {
        private const string SearchPage = "http://www.tlighting.com/retail/filter.html?limit=all";
        private readonly IPageFetcher<TrendLightingVendor> _pageFetcher;

        public TrendLightingProductDiscoverer(IPageFetcher<TrendLightingVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var search = await _pageFetcher.FetchAsync(SearchPage, CacheFolder.Search, "all");
            var products = search.QuerySelectorAll(".item a").Select(x => new Uri(x.Attributes["href"].Value)).ToList();
            return products.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}