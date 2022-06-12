using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace FSchumacher.Discovery
{
    public class FSchumacherProductDiscoverer : IProductDiscoverer<FSchumacherVendor>
    {
        private const string FabricSearchUrl = "https://www.fschumacher.com/browse/getitems?categoryid=2&sort=&page={0}";
        private const string WallcoveringSearchUrl = "https://www.fschumacher.com/browse/getitems?categoryid=1&sort=&page={0}";
        private const string TrimSearchUrl = "https://www.fschumacher.com/browse/getitems?categoryid=3&sort=&page={0}";

        private readonly IPageFetcher<FSchumacherVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<FSchumacherVendor> _sessionManager;

        public FSchumacherProductDiscoverer(IPageFetcher<FSchumacherVendor> pageFetcher, IVendorScanSessionManager<FSchumacherVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var skus = await DiscoverFromWeb(FabricSearchUrl, "fabric");
            skus.AddRange(await DiscoverFromWeb(WallcoveringSearchUrl, "wallcovering"));
            skus.AddRange(await DiscoverFromWeb(TrimSearchUrl, "trim"));
            return skus.Distinct().Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<string>> DiscoverFromWeb(string url, string group)
        {
            var skus = new List<string>();
            var pageNum = 1;
            // I don't see any way to get rid of this infinite loop
            while (true)
            {
                _sessionManager.ThrowIfCancellationRequested();
                var page = await _pageFetcher.FetchAsync(string.Format(url, pageNum), CacheFolder.Search, group + "-" + pageNum);
                if (page.InnerText.ContainsIgnoreCase("Unexpected Error Occurred"))
                {
                    throw new Exception("Server side error occurred: try again shortly");
                }
                var foundSkus = page.QuerySelectorAll(".itemgrid-itemsku").Select(x => x.InnerText.Trim().Split(new[] {"&nbsp"}, StringSplitOptions.RemoveEmptyEntries).First().Trim()).ToList();
                skus.AddRange(foundSkus);
                if (!foundSkus.Any())
                    break;
                pageNum++;
            }
            return skus;
        }
    }
}