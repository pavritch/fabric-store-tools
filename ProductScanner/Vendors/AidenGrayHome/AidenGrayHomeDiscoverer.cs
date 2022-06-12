using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace AidanGrayHome
{
    public class AidenGrayHomeDiscoverer : IProductDiscoverer<AidanGrayHomeVendor>
    {
        private const string MainUrl = "http://www.aidangrayhome.com/wholesale/";
        private readonly PageFetcher<AidanGrayHomeVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<AidanGrayHomeVendor> _sessionManager; 

        public AidenGrayHomeDiscoverer(PageFetcher<AidanGrayHomeVendor> pageFetcher, IVendorScanSessionManager<AidanGrayHomeVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var mainPage = await _pageFetcher.FetchAsync(MainUrl, CacheFolder.Search, "search");
            var categories = mainPage.QuerySelectorAll(".level1 a").Select(x => x.Attributes["href"].Value).Distinct().ToList();

            var products = new List<DiscoveredProduct>();
            await _sessionManager.ForEachNotifyAsync("Discovering Products", categories, async s =>
            {
                var cat = s.Replace(MainUrl, "").Replace(".html", "");
                var detailPage = await _pageFetcher.FetchAsync(s + "?limit=all", CacheFolder.Search, cat);
                var links = detailPage.QuerySelectorAll(".product-image").ToList();
                products.AddRange(links.Select(x => new DiscoveredProduct(new Uri(x.Attributes["href"].Value))));
            });
            return products.ToList();
        }
    }
}