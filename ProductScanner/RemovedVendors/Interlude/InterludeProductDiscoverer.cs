using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Interlude
{
    public class InterludeProductDiscoverer : IProductDiscoverer<InterludeVendor>
    {
        private const string SearchUrl = "http://www.interludehome.com/searchadv.aspx?searchterm=product+search&pagesize=5000";
        private readonly IPageFetcher<InterludeVendor> _pageFetcher;

        public InterludeProductDiscoverer(IPageFetcher<InterludeVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all");
            var products = searchPage.QuerySelectorAll(".img_thumb_border a").Select(x => x.Attributes["href"].Value).ToList();
            return products.Select(x => new DiscoveredProduct(new Uri("http://www.interludehome.com/" + x))).ToList();
        }
    }
}