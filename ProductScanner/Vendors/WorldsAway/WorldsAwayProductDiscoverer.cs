using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace WorldsAway
{
    public class WorldsAwayProductDiscoverer : IProductDiscoverer<WorldsAwayVendor>
    {
        private const string SearchUrl = "http://www.worlds-away.com/collection/";
        private readonly IPageFetcher<WorldsAwayVendor> _pageFetcher;

        public WorldsAwayProductDiscoverer(IPageFetcher<WorldsAwayVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var mainSearch = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all");
            var collections = mainSearch.QuerySelector(".sidebarBlock")
                .QuerySelectorAll(".navList-item a")
                .Select(x => x.Attributes["href"].Value).ToList();
            var products = new List<Uri>();
            foreach (var collection in collections)
            {
                var searchSubcategories = await _pageFetcher.FetchAsync(collection, CacheFolder.Search, "search-" + collection.Replace("https://www.worlds-away.com/collection/", "").Trim('/'));
                var subcategories = searchSubcategories.QuerySelector(".sidebarBlock:not(#facetedSearch)").QuerySelectorAll(".navList-item a").Select(x => x.Attributes["href"].Value).ToList();

                foreach (var subcategory in subcategories)
                {
                    products.AddRange(await DiscoverProducts(subcategory));
                }

                if (!subcategories.Any())
                {
                    products.AddRange(await DiscoverProducts(collection));
                }
            }

            return products.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<Uri>> DiscoverProducts(string searchUrl)
        {
            var pageNum = 1;
            var products = new List<Uri>();
            while (true)
            {
                var category = searchUrl.Replace("https://www.worlds-away.com/collection/", "").Trim('/');
                var search = await _pageFetcher.FetchAsync(searchUrl + "?page=" + pageNum, CacheFolder.Search, "search-" + category + pageNum);
                pageNum++;

                var pageProducts = search.QuerySelectorAll(".product").ToList();
                products.AddRange(pageProducts.Select(x => new Uri(x.QuerySelector("a").Attributes["href"].Value)));

                var nextBtn = search.QuerySelector(".pagination-item--next");

                if (pageProducts.Count < 20 || nextBtn == null)
                    break;
            }
            return products;
        }
    }
}