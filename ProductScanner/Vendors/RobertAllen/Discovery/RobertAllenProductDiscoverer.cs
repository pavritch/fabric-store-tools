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

namespace RobertAllen.Discovery
{
    public class RobertAllenProductDiscoverer : RobertAllenBaseProductDiscoverer<RobertAllenVendor>
    {
        private const string FabricSearchUrl = "https://www.robertallendesign.com/all-products/fabric/shopby/manufacturer-robert_allen_athome-robert_allen_contract-robert_allen-other/rad_lifecycle-outlet-discontinued-active?limit=48&p={0}";
        private const string TrimSearchUrl = "https://www.robertallendesign.com/all-products/trim/shopby/manufacturer-robert_allen/rad_lifecycle-outlet-discontinued-active?limit=48&p={0}";
        private const string FabricOutletUrl = "http://www.robertallenoutlet.com/searches/search_results_graphic.aspx?tp=Finder&type=Fabric&brand=1,4,5&color=0&usage=0&cat=0&style=0&design=0&performance=0&finish=0&pageSize=100000";
        private const string TrimOutletUrl = "http://www.robertallenoutlet.com/searches/search_results_graphic.aspx?tp=Finder&type=Trim&brand=1,4,5&color=0&ttype=0&tcoll=0&pageSize=10000";
        public RobertAllenProductDiscoverer(IPageFetcher<RobertAllenVendor> pageFetcher, IVendorScanSessionManager<RobertAllenVendor> sessionManager)
            : base(pageFetcher, sessionManager, FabricSearchUrl, TrimSearchUrl, FabricOutletUrl, TrimOutletUrl) { }
    }

    public class BeaconHillProductDiscoverer : RobertAllenBaseProductDiscoverer<BeaconHillVendor>
    {
        private const string FabricSearchUrl = "https://www.robertallendesign.com/all-products/fabric/shopby/manufacturer-beacon_hill/rad_lifecycle-outlet-discontinued-active?limit=48&p={0}";
        private const string TrimSearchUrl = "https://www.robertallendesign.com/all-products/trim/shopby/manufacturer-beacon_hill/rad_lifecycle-outlet-discontinued-active?limit=48&p={0}";
        private const string FabricOutletUrl = "http://www.robertallenoutlet.com/searches/search_results_graphic.aspx?tp=Finder&type=Fabric&brand=2&color=0&usage=0&cat=0&style=0&design=0&performance=0&finish=0&pageSize=100000";
        private const string TrimOutletUrl = "http://www.robertallenoutlet.com/searches/search_results_graphic.aspx?tp=Finder&type=Trim&brand=2&color=0&ttype=0&tcoll=0&pageSize=10000";
        public BeaconHillProductDiscoverer(IPageFetcher<BeaconHillVendor> pageFetcher, IVendorScanSessionManager<BeaconHillVendor> sessionManager)
            : base(pageFetcher, sessionManager, FabricSearchUrl, TrimSearchUrl, FabricOutletUrl, TrimOutletUrl) { }
    }

    public class RobertAllenBaseProductDiscoverer<T> : IProductDiscoverer<T> where T : Vendor
    {
        // most of the outlet products come back in searches for robertallendesign.com, but not all of them
        // so we use the outlet site for discovery as well to fill in the gaps
        private readonly string _fabricSearchUrl;
        private readonly string _trimSearchUrl;
        private readonly string _fabricOutletUrl;
        private readonly string _trimOutletUrl;

        private readonly IPageFetcher<T> _pageFetcher;
        private readonly IVendorScanSessionManager<T> _sessionManager;
        public RobertAllenBaseProductDiscoverer(IPageFetcher<T> pageFetcher, IVendorScanSessionManager<T> sessionManager, string fabricSearchUrl, string trimSearchUrl, string fabricOutletUrl, string trimOutletUrl)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;

            _fabricSearchUrl = fabricSearchUrl;
            _trimSearchUrl = trimSearchUrl;
            _fabricOutletUrl = fabricOutletUrl;
            _trimOutletUrl = trimOutletUrl;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            return (await DiscoverOutletProducts(_fabricOutletUrl, "fabric"))
                .Concat(await DiscoverOutletProducts(_trimOutletUrl, "trim"))
                .Concat(await DetectAvailableProductsByGroup(_fabricSearchUrl, "fabric"))
                .Concat(await DetectAvailableProductsByGroup(_trimSearchUrl, "trim")).ToList();
        }

        private async Task<List<DiscoveredProduct>> DetectAvailableProductsByGroup(string searchUrl, string productGroup)
        {
            _sessionManager.VendorSessionStats.CurrentTask = "Downloading product list: " + productGroup;

            var discoveredProducts = new List<DiscoveredProduct>();
            var pageOne = await _pageFetcher.FetchAsync(searchUrl, CacheFolder.Search, productGroup + "-1");
            var total = pageOne.GetFieldValue(".pager-results").TakeOnlyLastIntegerToken();
            var numPages = total/48 + 1;
            await _sessionManager.ForEachNotifyAsync("Discovering " + productGroup, Enumerable.Range(1, numPages), async i =>
            {
                var page = await _pageFetcher.FetchAsync(string.Format(searchUrl, i), CacheFolder.Search, productGroup + "-" + i);
                var products = page.QuerySelectorAll("h3.catalog-item-name a").ToList();
                foreach (var product in products)
                {
                    var mpn = product.Attributes["href"].Value.TakeOnlyLastIntegerToken().ToString().PadLeft(6, '0');
                    var name = product.Attributes["title"].Value.Replace(" | ", "-").Replace(" ", "-").ToLower();
                    var url = string.Format("https://www.robertallendesign.com/{0}-{1}", name, mpn);
                    discoveredProducts.Add(new DiscoveredProduct(new Uri(url), mpn, productGroup));
                }
            });
            return discoveredProducts.OrderBy(x => x.MPN).ToList();
        }

        private async Task<List<DiscoveredProduct>> DiscoverOutletProducts(string searchUrl, string productGroup)
        {
            var searchResults = await _pageFetcher.FetchAsync(searchUrl, CacheFolder.Search, "outlet-" + productGroup);
            var products = searchResults.QuerySelectorAll(".results a").ToList();
            var discoveredProducts = new List<DiscoveredProduct>(); 
            foreach (var product in products)
            {
                var mpn = product.Attributes["href"].Value.TakeOnlyLastIntegerToken().ToString().PadLeft(6, '0');
                var name = product.QuerySelector("span[id$='_lblProduct']").InnerText;
                name = name.Replace(" / ", "-").Replace(" ", "-").ToLower();
                var url = string.Format("https://www.robertallendesign.com/{0}-{1}", name, mpn);
                discoveredProducts.Add(new DiscoveredProduct(new Uri(url), mpn, productGroup));
            }
            return discoveredProducts;
        }
    }
}