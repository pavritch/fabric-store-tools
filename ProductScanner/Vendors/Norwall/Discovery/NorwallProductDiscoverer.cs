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

namespace Norwall.Discovery
{
    public class NorwallProductDiscoverer : IProductDiscoverer<NorwallVendor>
    {
        private readonly IPageFetcher<NorwallVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<NorwallVendor> _sessionManager; 
        public NorwallProductDiscoverer(IPageFetcher<NorwallVendor> pageFetcher, IVendorScanSessionManager<NorwallVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var collectionIds = await GetCollectionIds();
            var skus = await ScanCollectionPages(collectionIds);
            return skus.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<string>> ScanCollectionPages(IEnumerable<string> collectionIds)
        {
            var skus = new List<string>();
            await _sessionManager.ForEachNotifyAsync("Scanning collection pages", collectionIds, async collectionId =>
            {
                var collectionUrl = string.Format("http://norwall.net/collection_search_result.php?clctnid={0}", collectionId);
                var collectionPage = await _pageFetcher.FetchAsync(collectionUrl, CacheFolder.Search, "collection-" + collectionId);
                var numPages = Convert.ToInt32(collectionPage.InnerText.CaptureWithinMatchedPattern(@"(?<capture>(\d+)) Page"));
                for (int pageNum = 1; pageNum <= numPages; pageNum++)
                {
                    var pageUrl = string.Format("http://norwall.net/collection_search_result.php?clctnid={0}&page={1}", collectionId, pageNum);
                    var page = await _pageFetcher.FetchAsync(pageUrl, CacheFolder.Search, string.Format("collection-{0}-{1}", collectionId, pageNum));
                    skus.AddRange(page.QuerySelectorAll("#leftcolumn a").Select(x =>
                        x.Attributes["href"].Value.CaptureWithinMatchedPattern("sku=(?<capture>(.*))&")));
                }
            });
            return skus;
        }

        private async Task<List<string>> GetCollectionIds()
        {
            var url = "http://norwall.net/Search_By_Collection.php?pageid=16";
            var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "collections");
            return page.QuerySelectorAll(".searchLeftLink").Select(x =>
                x.Attributes["href"].Value.CaptureWithinMatchedPattern("clctnid=(?<capture>(.*))")).ToList();
        }
    }
}