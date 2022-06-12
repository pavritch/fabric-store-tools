using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace ClassicHome
{
    public class ClassicHomeMetadataCollector : IMetadataCollector<ClassicHomeVendor>
    {
        private readonly IPageFetcher<ClassicHomeVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<ClassicHomeVendor> _sessionManager;

        public ClassicHomeMetadataCollector(IPageFetcher<ClassicHomeVendor> pageFetcher, IVendorScanSessionManager<ClassicHomeVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var furniturePage = await _pageFetcher.FetchAsync(string.Empty, CacheFolder.Search, "furniture");
            var textilePage = await _pageFetcher.FetchAsync(string.Empty, CacheFolder.Search, "textiles");

            var searches = GetSearches(furniturePage, "Category", ScanField.Category).ToList();
            searches.AddRange(GetSearches(furniturePage, "Room Type", ScanField.RoomType));
            searches.AddRange(GetSearches(furniturePage, "Style", ScanField.Style));
            searches.AddRange(GetSearches(furniturePage, "Wood type", ScanField.WoodType));

            searches.AddRange(GetSearches(textilePage, "Category", ScanField.Category));
            searches.AddRange(GetSearches(textilePage, "Style", ScanField.Style));
            searches.AddRange(GetSearches(textilePage, "Category", ScanField.Category));

            await _sessionManager.ForEachNotifyAsync("Collecting Metadata", searches, async search =>
            {
                var prefix = search.SearchUrl.AbsoluteUri.ContainsIgnoreCase("furniture") ? "furniture" : "textiles";
                var searchResults = await _pageFetcher.FetchAsync(search.SearchUrl.AbsoluteUri, CacheFolder.Search, prefix + "-" + search.Value);
                var resultProducts = searchResults.QuerySelectorAll("a.product-image").Select(x => x.Attributes["href"].Value).ToList();
                var matches = products.Where(x => resultProducts.Contains(x.GetDetailUrl())).ToList();
                matches.ForEach(x => x[search.ScanField] = search.Value);
            });
            return products;
        }

        private IEnumerable<ClassicHomeSearch> GetSearches(HtmlNode page, string heading, ScanField field)
        {
            return page.QuerySelector(string.Format("dt:contains('{0}') ~ dd", heading)).QuerySelectorAll("li a")
                .Select(x => new ClassicHomeSearch(new Uri(x.Attributes["href"].Value.Replace("&amp;", "&")), field, x.InnerText)).ToList();
        }
    }
}