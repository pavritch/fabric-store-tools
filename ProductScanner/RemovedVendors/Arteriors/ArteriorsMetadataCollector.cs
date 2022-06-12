using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace Arteriors
{
    public class ArteriorsMetadataCollector : IMetadataCollector<ArteriorsVendor>
    {
        private const string SearchUrl = "http://www.arteriorshome.com/shop?p=5000";
        private readonly IPageFetcher<ArteriorsVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<ArteriorsVendor> _sessionManager;

        public ArteriorsMetadataCollector(IPageFetcher<ArteriorsVendor> pageFetcher, IVendorScanSessionManager<ArteriorsVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all");
            await SearchMetadata(products, searchPage, "filter_color", "color", ScanField.ColorGroup);
            await SearchMetadata(products, searchPage, "filter_certification", "certification", ScanField.Country);
            await SearchMetadata(products, searchPage, "filter_material", "material", ScanField.Content);

            return products;
        }

        public async Task SearchMetadata(List<ScanData> products, HtmlNode searchPage, string htmlId, string key, ScanField field)
        {
            var options = searchPage.QuerySelectorAll(string.Format("#{0} input", htmlId)).Select(x => x.Attributes["id"].Value);
            await _sessionManager.ForEachNotifyAsync("Searching by " + key.TitleCase(), options, async s =>
            {
                var searchResults = await _pageFetcher.FetchAsync(SearchUrl + "&" + key + "=" + s, CacheFolder.Search, s);
                var mpns = searchResults.QuerySelectorAll("ul.products > li").Select(x => x.QuerySelector("a").Attributes["data-product-id"].Value).ToList();
                var matches = products.Where(x => mpns.Contains(x[ScanField.ManufacturerPartNumber])).ToList();
                matches.ForEach(x => x[field] = s);
            });
        }
    }
}