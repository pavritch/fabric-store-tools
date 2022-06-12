using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace NewGrowthDesigns
{
    public class NewGrowthDesignsProductDiscoverer : IProductDiscoverer<NewGrowthDesignsVendor>
    {
        private const string CollectionsUrl = "https://newgrowthdesigns.com/collections/";
        private const string BaseUrl = "https://newgrowthdesigns.com";
        private readonly IPageFetcher<NewGrowthDesignsVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<NewGrowthDesignsVendor> _sessionManager;

        public NewGrowthDesignsProductDiscoverer(IPageFetcher<NewGrowthDesignsVendor> pageFetcher, IVendorScanSessionManager<NewGrowthDesignsVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var searchUrls = await GetSearchUrls();
            var products = new List<Uri>();
            await _sessionManager.ForEachNotifyAsync("Discovering Products", searchUrls, async s =>
            {
                var collectionPage = await _pageFetcher.FetchAsync(BaseUrl + s, CacheFolder.Search, "collection-" + s.Replace("/collections/", ""));
                products.AddRange(collectionPage.QuerySelectorAll(".grid-product__image-link").Select(x => new Uri(BaseUrl + x.Attributes["href"].Value)));
            });
            return products.Distinct().Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<string>> GetSearchUrls()
        {
            var categoryPage = await _pageFetcher.FetchAsync(CollectionsUrl, CacheFolder.Search, "collections");
            var collections = categoryPage.QuerySelectorAll(".collection-grid__item-link").Select(x => x.Attributes["href"].Value).ToList();

            var searchUrls = new List<string>(collections);
            await _sessionManager.ForEachNotifyAsync("Discovering Collections", collections, async s =>
            {
                var collectionPage = await _pageFetcher.FetchAsync(BaseUrl + s, CacheFolder.Search, "collection-" + s.Replace("/collections/", ""));
                var pagination = collectionPage.QuerySelectorAll(".pagination .page").LastOrDefault();
                if (pagination != null)
                {
                    var numPages = pagination.InnerText.ToIntegerSafe();
                    searchUrls.AddRange(Enumerable.Range(2, numPages - 1).Select(x => s + "?page=" + x).ToList());
                }
            });
            return searchUrls;
        }
    }
}