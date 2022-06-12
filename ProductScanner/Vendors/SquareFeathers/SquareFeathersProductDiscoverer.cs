using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace SquareFeathers
{
    public class SquareFeathersProductDiscoverer : IProductDiscoverer<SquareFeathersVendor>
    {
        private const string SearchUrl = "https://www.squarefeathers.com/shop/";
        private readonly IPageFetcher<SquareFeathersVendor> _pageFetcher;

        public SquareFeathersProductDiscoverer(IPageFetcher<SquareFeathersVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var mainSearch = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search");
            var links = mainSearch.QuerySelectorAll("dd a").Select(x => x.Attributes["href"].Value).ToList();

            var urls = new List<Uri>();
            foreach (var collection in links)
            {
                var searchPage = await _pageFetcher.FetchAsync(collection + "?product_count=400", CacheFolder.Search, new Uri(collection).LocalPath);
                urls.AddRange(searchPage.QuerySelectorAll(".product-images").Select(x => new Uri(x.Attributes["href"].Value)));
            }
            return urls.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}