using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace LacefieldDesigns
{
    public class LacefieldProductDiscoverer : IProductDiscoverer<LacefieldDesignsVendor>
    {
        private const string SearchUrl = "http://www.lacefielddesigns.com/all-pillows/";
        private readonly IPageFetcher<LacefieldDesignsVendor> _pageFetcher;

        public LacefieldProductDiscoverer(IPageFetcher<LacefieldDesignsVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var resultsPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search");
            var products = resultsPage.QuerySelectorAll(".products .product-small a").Select(x => x.Attributes["href"].Value).Distinct();
            return products.Select(x => new DiscoveredProduct(new Uri(x))).ToList();
        }
    }
}