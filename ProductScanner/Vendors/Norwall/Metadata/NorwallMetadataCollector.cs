using System;
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
using Utilities.Extensions;

namespace Norwall.Metadata
{
    public class NorwallMetadataCollector : IMetadataCollector<NorwallVendor>
    {
        private readonly IPageFetcher<NorwallVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<NorwallVendor> _sessionManager; 
        private const string SearchUrl = "http://www.norwall.net/Product_Search.php?pageid=15";

        public NorwallMetadataCollector(IPageFetcher<NorwallVendor> pageFetcher, IVendorScanSessionManager<NorwallVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            await _sessionManager.ForEachNotifyAsync("Scanning colorways", products, async product => 
            {
                var sku = product[ScanField.ManufacturerPartNumber];
                var colorwayUrl = string.Format("http://norwall.net/difColorListing.php?sku={0}", sku);
                var colorwayPage = await _pageFetcher.FetchAsync(colorwayUrl, CacheFolder.Colorways, sku);

                var otherSkus = colorwayPage.QuerySelectorAll("#leftcolumn img").Select(x =>
                    x.Attributes["src"].Value.CaptureWithinMatchedPattern("/thumb/(?<capture>(.*)).jpg")).ToList();
                otherSkus.Add(sku);
                product.RelatedProducts = otherSkus;
            });

            await _sessionManager.ForEachNotifyAsync("Scanning coordinating products", products, async product => 
            {
                var sku = product[ScanField.ManufacturerPartNumber];
                var coordinateUrl = string.Format("http://norwall.net/coordinatesListing.php?sku={0}", sku);
                var coordinatesPage = await _pageFetcher.FetchAsync(coordinateUrl, CacheFolder.Colorways, sku + "-coord");

                var otherSkus = coordinatesPage.QuerySelectorAll("#leftcolumn img").Select(x =>
                    x.Attributes["src"].Value.CaptureWithinMatchedPattern("/thumb/(?<capture>(.*)).jpg")).ToList();
                otherSkus.Add(sku);
                product[ScanField.Coordinates] = string.Join(", ", otherSkus);
            });

            /*var searches = await GetMetadataSearchInfo();
            await _sessionManager.ForEachNotifyAsync("Running Metadata Searches", searches, async search =>
            {
                var mpns = await RunSearch(search);
                var matchingProducts = products.Where(x => mpns.Contains(x[ScanField.ManufacturerPartNumber]));
                matchingProducts.ForEach(x => x[search.AssociatedProperty] = search.Name);
            });*/

            return products;
        }

        private async Task<List<NorwallSearch>> GetMetadataSearchInfo()
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-metadata");
            var productTypes = GetSearchData("product_type", searchPage);
            var roomTypes = GetSearchData("room_type", searchPage);
            var colors = GetSearchData("product_colour", searchPage);
            var styles = GetSearchData("product_style", searchPage);
            var designs = GetSearchData("design_type", searchPage);

            var searches = new List<NorwallSearch>();
            searches.AddRange(productTypes.Select(x => new NorwallSearch("srl_type", x.Item1, x.Item2, ScanField.ProductType)));
            searches.AddRange(roomTypes.Select(x => new NorwallSearch("slr_room", x.Item1, x.Item2, ScanField.Ignore)));
            searches.AddRange(colors.Select(x => new NorwallSearch("srl_color", x.Item1, x.Item2, ScanField.Color)));
            searches.AddRange(styles.Select(x => new NorwallSearch("srl_type", x.Item1, x.Item2, ScanField.Style)));
            searches.AddRange(designs.Select(x => new NorwallSearch("srl_design", x.Item1, x.Item2, ScanField.Design)));
            return searches;
        }

        private IEnumerable<Tuple<string, int>> GetSearchData(string key, HtmlNode page)
        {
            return page.QuerySelectorAll(string.Format("input[name='{0}[]']", key)).Select(x => 
                new Tuple<string, int>(x.NextSibling.InnerText.Replace("&nbsp;", "").Trim(), x.Attributes["value"].Value.ToIntegerSafe())).ToList();
        }

        private async Task<List<string>> RunSearch(NorwallSearch search)
        {
            var resultOne = await _pageFetcher.FetchAsync(search.GetUrl(1), CacheFolder.Search, string.Format("{0}-{1}-1", search.Key, search.Value));
            var ids = resultOne.QuerySelectorAll("#leftcolumn img").Select(x => x.Attributes["src"].Value).ToList();
            ids = ids.Select(x => x.Substring(x.LastIndexOf("/", StringComparison.Ordinal) + 1).Replace(".jpg", "")).ToList();

            var pageInfoElement = resultOne.QuerySelector("td.text_01:contains('Page')");
            if (pageInfoElement == null) return new List<string>();

            var numItems = pageInfoElement.InnerText.Replace("Total", "").Trim().TakeOnlyFirstIntegerToken();
            var numPages = Math.Ceiling((double)numItems/10);
            for (var i = 2; i <= numPages; i++)
            {
                var nextPage = await _pageFetcher.FetchAsync(search.GetUrl(i), CacheFolder.Search, string.Format("{0}-{1}-{2}", search.Key, search.Value, i));
                var mpns = nextPage.QuerySelectorAll("#leftcolumn img").Select(x => x.Attributes["src"].Value).ToList();
                mpns = mpns.Select(x => x.Substring(x.LastIndexOf("/", StringComparison.Ordinal) + 1).Replace(".jpg", "")).ToList();
                ids.AddRange(mpns);
            }
            return ids;
        }
    }
}