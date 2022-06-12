using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Arteriors
{
    public class ArteriorsProductDiscoverer : IProductDiscoverer<ArteriorsVendor>
    {
        private List<string> Urls = new List<string>
        {
            "http://www.arteriorshome.com/shop/lighting/table-lamps?p={0}",
            "http://www.arteriorshome.com/shop/lighting/floor-lamps?p={0}",
            "http://www.arteriorshome.com/shop/lighting/pendants?p={0}",
            "http://www.arteriorshome.com/shop/lighting/sconces?p={0}",
            "http://www.arteriorshome.com/shop/lighting/chandeliers?p={0}",
            "http://www.arteriorshome.com/shop/lighting/pipes-and-chains?p={0}",
            "http://www.arteriorshome.com/shop/furniture/cabinets-shelving",
            "http://www.arteriorshome.com/shop/furniture/screens?p={0}",

            "http://www.arteriorshome.com/shop/furniture/seating/upholstered-seating-1?p={0}",
            "http://www.arteriorshome.com/shop/furniture/seating/muslin-seating?p={0}",
            "http://www.arteriorshome.com/shop/furniture/seating/accent-chairs?p={0}",
            "http://www.arteriorshome.com/shop/furniture/seating/bar-and-counter-stools?p={0}",
            "http://www.arteriorshome.com/shop/furniture/seating/stools-and-benches?p={0}",
            "http://www.arteriorshome.com/shop/furniture/seating/ottomans?p={0}",

            "http://www.arteriorshome.com/shop/furniture/tables/coffee-and-cocktail-tables?p={0}",
            "http://www.arteriorshome.com/shop/furniture/tables/accent-side-end-and-occassional-tables?p={0}",
            "http://www.arteriorshome.com/shop/furniture/tables/dining-and-entry-tables?p={0}",
            "http://www.arteriorshome.com/shop/furniture/tables/consoles?p={0}",
            "http://www.arteriorshome.com/shop/furniture/tables/desks?p={0}",

            "http://www.arteriorshome.com/shop/accessories/candles?p={0}",
            "http://www.arteriorshome.com/shop/accessories/fireplace?p={0}",
            "http://www.arteriorshome.com/shop/accessories/new-accessories?p={0}",
            "http://www.arteriorshome.com/shop/accessories/trays?p={0}",
            "http://www.arteriorshome.com/shop/accessories/barware-and-entertaining?p={0}",
            "http://www.arteriorshome.com/shop/accessories/objects-sculptures-and-bookends?p={0}",
            "http://www.arteriorshome.com/shop/accessories/centerpieces-containers-and-vases?p={0}",
            "http://www.arteriorshome.com/shop/accessories/decorative-boxes?p={0}",

            "http://www.arteriorshome.com/shop/wall/decorative?p={0}",
            "http://www.arteriorshome.com/shop/wall/mirrors?p={0}",
            "http://www.arteriorshome.com/shop/new?p={0}",
            "http://www.arteriorshome.com/shop/on-sale?p={0}",
        }; 
        private readonly IVendorScanSessionManager<ArteriorsVendor> _sessionManager; 
        private readonly IPageFetcher<ArteriorsVendor> _pageFetcher;

        public ArteriorsProductDiscoverer(IPageFetcher<ArteriorsVendor> pageFetcher, IVendorScanSessionManager<ArteriorsVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var urls = new List<Uri>();
            foreach (var url in Urls)
            {
                var category = new Uri(url.Replace("p=/{0/}", "")).GetDocumentName();
                urls.AddRange(await GetProducts(url, category));
            }
            return urls.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<Uri>> GetProducts(string url, string category)
        {
            var urls = new List<Uri>();
            var first = await _pageFetcher.FetchAsync(string.Format(url, 1), CacheFolder.Search, category + "-1");
            var count = first.GetFieldValue(".amount").Replace(" items", "").Trim().ToIntegerSafe();
            var numPages = (count/20) + 1;
            await _sessionManager.ForEachNotifyAsync("Discovering Products", Enumerable.Range(1, numPages), async i =>
            {
                var page = await _pageFetcher.FetchAsync(string.Format(url, i), CacheFolder.Search, category + "-" + i);
                var items = page.QuerySelectorAll(".products-grid .item > a").Select(x => x.Attributes["href"].Value).Distinct().ToList();
                urls.AddRange(items.Select(x => new Uri(x)));
            });
            return urls;
        }
    }
}