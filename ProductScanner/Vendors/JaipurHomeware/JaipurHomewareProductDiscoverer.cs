using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace JaipurHomeware
{
    public class JaipurHomewareProductDiscoverer : IProductDiscoverer<JaipurHomewareVendor>
    {
        private const string AllPillowsThrowsUrl = "https://www.jaipurliving.com/pillows?pagenumber=1&pagesize=500&orderby=0";
        private const string AllPoufsUrl = "https://www.jaipurliving.com/poufs?pagenumber=1&pagesize=192&orderby=0";
        private const string AllThrowsUrl = "https://www.jaipurliving.com/throws?pagenumber=1&pagesize=192&orderby=0";

        private readonly IPageFetcher<JaipurHomewareVendor> _pageFetcher;

        public JaipurHomewareProductDiscoverer(IPageFetcher<JaipurHomewareVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var pillows = await _pageFetcher.FetchAsync(AllPillowsThrowsUrl, CacheFolder.Search, "allPillowsThrows");
            var productBlocks = pillows.QuerySelectorAll(".row .col-lg-3").ToList();
            var links = productBlocks.Select(x => x.QuerySelector("a").Attributes["href"].Value).ToList();

            var poufs = await _pageFetcher.FetchAsync(AllPoufsUrl, CacheFolder.Search, "allPoufs");
            productBlocks = poufs.QuerySelectorAll(".row .col-lg-3").ToList();
            links.AddRange(productBlocks.Select(x => x.QuerySelector("a").Attributes["href"].Value).ToList());

            var throws = await _pageFetcher.FetchAsync(AllThrowsUrl, CacheFolder.Search, "allThrows");
            productBlocks = throws.QuerySelectorAll(".row .col-lg-3").ToList();
            links.AddRange(productBlocks.Select(x => x.QuerySelector("a").Attributes["href"].Value).ToList());
            return links.Select(x => new DiscoveredProduct(new Uri(x))).ToList();
        }
    }
}