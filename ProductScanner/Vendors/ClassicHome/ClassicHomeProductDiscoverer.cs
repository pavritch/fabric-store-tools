using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace ClassicHome
{
    public class ClassicHomeProductDiscoverer : IProductDiscoverer<ClassicHomeVendor>
    {
        private const string FurnitureUrl = "http://www.classichome.com/products/furniture.html?limit=all";
        private const string AccessoriesUrl = "http://www.classichome.com/products/accessories.html?limit=all";
        private const string TextilesUrl = "http://www.classichome.com/products/textiles.html?limit=all";

        private readonly IPageFetcher<ClassicHomeVendor> _pageFetcher;

        public ClassicHomeProductDiscoverer(IPageFetcher<ClassicHomeVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var furniturePage = await _pageFetcher.FetchAsync(FurnitureUrl, CacheFolder.Search, "furniture");
            var accessoriesPage = await _pageFetcher.FetchAsync(AccessoriesUrl, CacheFolder.Search, "accessories");
            var textilesPage = await _pageFetcher.FetchAsync(TextilesUrl, CacheFolder.Search, "textiles");
            var products = furniturePage.QuerySelectorAll("a.product-image").Select(x => x.Attributes["href"].Value).ToList();
            products.AddRange(accessoriesPage.QuerySelectorAll("a.product-image").Select(x => x.Attributes["href"].Value));
            products.AddRange(textilesPage.QuerySelectorAll("a.product-image").Select(x => x.Attributes["href"].Value));
            return products.Select(x => new DiscoveredProduct(new Uri(x))).ToList();
        }
    }
}