using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace JFFabrics
{
    public class JFFabricsSearcher
    {
        private const string FabricSearchUrl = "http://www.jffabrics.com/wp-admin/admin-ajax.php?action=filter_products&paged={0}&filters%5B%5D={1}&type=fabric";
        private const string WallcoveringSearchUrl = "http://www.jffabrics.com/wp-admin/admin-ajax.php?action=filter_products&paged={0}&filters%5B%5D={1}&type=wallcovering";
        private readonly IVendorScanSessionManager<JFFabricsVendor> _sessionManager;
        private readonly IPageFetcher<JFFabricsVendor> _pageFetcher;

        public JFFabricsSearcher(IVendorScanSessionManager<JFFabricsVendor> sessionManager, IPageFetcher<JFFabricsVendor> pageFetcher)
        {
            _sessionManager = sessionManager;
            _pageFetcher = pageFetcher;
        }

        public async Task<List<string>> SearchAllFabric() { return await SearchFabricWithFilter(); }
        public async Task<List<string>> SearchAllWallpaper() { return await SearchWallpaperWithFilter(); }

        public async Task<List<string>> SearchWallpaperWithFilter(string filter = "")
        {
            var firstPage = await _pageFetcher.FetchAsync(string.Format(WallcoveringSearchUrl, 1, filter), CacheFolder.Search, filter + "wp-page1");
            if (firstPage.InnerText.ContainsIgnoreCase("No results.")) return new List<string>();

            var numPages = 1;
            var pagination = firstPage.QuerySelector(".ajax-pagination");
            if (pagination != null)
                numPages = pagination.InnerText.TakeOnlyLastIntegerToken();
            var links = new List<string>();
            await _sessionManager.ForEachNotifyAsync("Discovering wallpaper", Enumerable.Range(1, numPages), async i =>
            {
                var url = string.Format(WallcoveringSearchUrl, i, filter);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, filter + "wp-page" + i);
                links.AddRange(page.QuerySelectorAll(".product a").Select(x => x.Attributes["href"].Value));
            });
            return links;
        }

        public async Task<List<string>> SearchFabricWithFilter(string filter = "")
        {
            var firstPage = await _pageFetcher.FetchAsync(string.Format(FabricSearchUrl, 1, filter), CacheFolder.Search, filter + "fab-page1");
            if (firstPage.InnerText.ContainsIgnoreCase("No results.")) return new List<string>();

            var numPages = 1;
            var pagination = firstPage.QuerySelector(".ajax-pagination");
            if (pagination != null)
                numPages = pagination.InnerText.TakeOnlyLastIntegerToken();
            var links = new List<string>();
            await _sessionManager.ForEachNotifyAsync("Discovering fabric", Enumerable.Range(1, numPages), async i =>
            {
                var url = string.Format(FabricSearchUrl, i, filter);
                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, filter + "fab-page" + i);
                links.AddRange(page.QuerySelectorAll(".product a").Select(x => x.Attributes["href"].Value));
            });
            return links;
        }
    }
}