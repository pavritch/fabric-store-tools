using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace LegendOfAsia
{
    public class LegendOfAsiaProductDiscoverer : IProductDiscoverer<LegendOfAsiaVendor>
    {
        private const string SearchUrl = "http://www.legendofasia.com/";
        private readonly IPageFetcher<LegendOfAsiaVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<LegendOfAsiaVendor> _sessionManager;

        public LegendOfAsiaProductDiscoverer(IPageFetcher<LegendOfAsiaVendor> pageFetcher, IVendorScanSessionManager<LegendOfAsiaVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search");
            var categories = searchPage.QuerySelectorAll(".main-navigation-container li a").Select(x => x.Attributes["href"].Value)
                .Where(x => !x.Contains("drop-ship"));
            var products = new List<string>();

            await _sessionManager.ForEachNotifyAsync("Searching by Category", categories, async cat =>
            {
                var catName = cat.Replace("http://www.legendofasia.com/", "");
                int pageNum = 1;
                while (true)
                {
                    var url = cat + "?page=" + pageNum;
                    var pageResults = await _pageFetcher.FetchAsync(url, CacheFolder.Search, catName + "-" + pageNum);
                    var items = pageResults.QuerySelectorAll(".grid-item").Select(x => x.QuerySelector(".grid-item-overlay-link").Attributes["href"].Value).ToList();
                    products.AddRange(items);
                    pageNum++;

                    if (items.Count != 12) break;
                }
            });

            products = products.Distinct().ToList();
            return products.Select(x => new DiscoveredProduct(new Uri(x))).ToList();
        }
    }
}