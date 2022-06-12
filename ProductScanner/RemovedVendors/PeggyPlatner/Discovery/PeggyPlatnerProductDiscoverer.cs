using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace PeggyPlatner.Discovery
{
    public class PeggyPlatnerProductDiscoverer : IProductDiscoverer<PeggyPlatnerVendor>
    {
        private readonly IPageFetcher<PeggyPlatnerVendor> _pageFetcher;
        public PeggyPlatnerProductDiscoverer(IPageFetcher<PeggyPlatnerVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            const string url = "http://www.peggyplatnercollection.com/collections/all";
            var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "all");
            var patterns = page.QuerySelectorAll(".prod-name").Select(x => x.InnerText).ToList();
            return patterns.Select(DiscoveredProduct.WithPatternName).ToList();
        }
    }
}