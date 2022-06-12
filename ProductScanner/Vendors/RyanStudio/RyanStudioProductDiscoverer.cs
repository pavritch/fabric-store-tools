using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace RyanStudio
{
    public class RyanStudioProductDiscoverer : IProductDiscoverer<RyanStudioVendor>
    {
        private const string SearchUrl = "https://www.ryanstudio.biz/SearchResults.asp?searching=Y&sort=7&cat=22&show=320&page=1";
        private readonly IPageFetcher<RyanStudioVendor> _pageFetcher;

        public RyanStudioProductDiscoverer(IPageFetcher<RyanStudioVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var pillowsList = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "pillows");
            var pillows = pillowsList.QuerySelectorAll("a[href*='ProductDetails']").ToList();
            var pillowUrls = pillows.Select(x => x.Attributes["href"].Value).Distinct().ToList();
            return pillowUrls.Select(x => new DiscoveredProduct(new Uri("https://www.ryanstudio.biz" + x))).ToList();
        }
    }
}
