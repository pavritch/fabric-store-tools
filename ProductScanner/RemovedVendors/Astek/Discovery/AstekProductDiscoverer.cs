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

namespace Astek.Discovery
{
    public class AstekProductDiscoverer : IProductDiscoverer<AstekVendor>
    {
        private const string SearchUrl = "http://www.designyourwall.com/search/content?page={0}&filters=fs_uc_sell_price%3A%5B0%20TO%202000%5D";
        private readonly IPageFetcher<AstekVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<AstekVendor> _sessionManager;

        public AstekProductDiscoverer(IPageFetcher<AstekVendor> pageFetcher, IVendorScanSessionManager<AstekVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var pageOne = await _pageFetcher.FetchAsync(string.Format(SearchUrl, 1), CacheFolder.Search, "search");
            var numItems = Convert.ToInt32(pageOne.InnerText.CaptureWithinMatchedPattern(@"Search found (?<capture>(\d+)) items"));
            var numPages = numItems/24 + 1;
            var urls = new List<string>();
            await _sessionManager.ForEachNotifyAsync("Discovering products", Enumerable.Range(0, numPages - 1), async pageNum =>
            {
                var url = string.Format(SearchUrl, pageNum);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "search-" + pageNum);
                urls.AddRange(page.QuerySelectorAll(".catalog-grid-image a").Select(x => x.Attributes["href"].Value));
            });
            return urls.Select(x => new DiscoveredProduct(new Uri("http://www.designyourwall.com" + x))).ToList();
        }
    }
}
