using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace AndrewMartin
{
    public class AndrewMartinProductDiscoverer : IProductDiscoverer<AndrewMartinVendor>
    {
        private const string FabricSearchUrl = "http://www.andrewmartin.co.uk/fabric-showroom/fabric-designs.php?p={0}";
        private const string WallpaperSearchUrl = "http://www.andrewmartin.co.uk/wallpaper-showroom/wallpaper-designs.php?p={0}";
        private readonly IPageFetcher<AndrewMartinVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<AndrewMartinVendor> _sessionManager;

        public AndrewMartinProductDiscoverer(IPageFetcher<AndrewMartinVendor> pageFetcher,
            IVendorScanSessionManager<AndrewMartinVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var urls = await DiscoverWallpaperProducts();
            urls.AddRange(await DiscoverFabricProducts());
            return urls.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<Uri>> DiscoverWallpaperProducts()
        {
            var url = string.Format(WallpaperSearchUrl, 1);
            var firstPage = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "wallpaper-patterns-1");
            var patternUrls = GetPatternUrls(firstPage);
            var total = Convert.ToInt32(firstPage.InnerText.CaptureWithinMatchedPattern(@"of (?<capture>(\d+)) total"));
            var numPages = total / 9 + 1;
            await _sessionManager.ForEachNotifyAsync("Scanning wallpaper patterns.", Enumerable.Range(2, numPages - 1), async i =>
            {
                url = string.Format(WallpaperSearchUrl, i);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "wallpaper-patterns-" + i);
                patternUrls.AddRange(GetPatternUrls(page));
            });
            return patternUrls.Distinct().ToList();
        }

        private async Task<List<Uri>> DiscoverFabricProducts()
        {
            var url = string.Format(FabricSearchUrl, 1);
            var firstPage = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "fabric-patterns-1");
            var patternUrls = GetPatternUrls(firstPage);
            var total = Convert.ToInt32(firstPage.InnerText.CaptureWithinMatchedPattern(@"of (?<capture>(\d+)) total"));
            var numPages = total / 9 + 1;
            await _sessionManager.ForEachNotifyAsync("Scanning fabric patterns", Enumerable.Range(2, numPages - 1), async i =>
            {
                url = string.Format(FabricSearchUrl, i);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "fabric-patterns-" + i);
                patternUrls.AddRange(GetPatternUrls(page));
            });
            return patternUrls.Distinct().ToList();
        }

        private List<Uri> GetPatternUrls(HtmlNode page)
        {
            return page.QuerySelectorAll(".category-products .products-grid li a").Select(x => new Uri(x.Attributes["href"].Value)).ToList();
        }
    }
}