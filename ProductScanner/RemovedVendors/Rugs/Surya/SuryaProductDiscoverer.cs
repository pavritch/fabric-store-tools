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

namespace Surya
{
    public class SuryaProductDiscoverer : IProductDiscoverer<SuryaVendor>
    {
        // I'm thinking discovery will be easiest by collection
        // http://www.surya.com/rugs/?shopby=1&page=2&n=0

        private const string CollectionUrl = "http://www.surya.com/rugs/?shopby=1&n=0";
        private const string CategoryUrl = "http://www.surya.com/rugs/?IsFiltered=1&shopfor=&category_id={0}";
        private readonly IPageFetcher<SuryaVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<SuryaVendor> _sessionManager;

        public SuryaProductDiscoverer(IPageFetcher<SuryaVendor> pageFetcher, IVendorScanSessionManager<SuryaVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var productUrls = new List<string>();
            var collectionList = await _pageFetcher.FetchAsync(CollectionUrl, CacheFolder.Search, "allCollections");
            var categoryIds = collectionList.QuerySelectorAll(".product-name a")
                .Select(x => x.Attributes["href"].Value.CaptureWithinMatchedPattern(@"category_id=(?<capture>(\d+))")).ToList();
            await _sessionManager.ForEachNotifyAsync("Discovering products by collection", categoryIds, async categoryId =>
            {
                var categoryUrl = string.Format(CategoryUrl, categoryId);
                var categoryPage = await _pageFetcher.FetchAsync(categoryUrl, CacheFolder.Search, "collection-" + categoryId);
                productUrls.AddRange(categoryPage.QuerySelectorAll(".product-image a").Select(x => x.Attributes["href"].Value));
            });
            return productUrls.Select(x => new DiscoveredProduct(new Uri("http://www.surya.com" + x))).ToList();
        }
    }
}