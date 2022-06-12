using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace SilverState.Discovery
{
    public class SilverStateProductDiscoverer : IProductDiscoverer<SilverStateVendor>
    {
        private const string SearchUrl = "https://www.silverstatetextiles.com/index.php?p_action=filter&p_icp_pk=4&p_filters_advanced=yes&p_filters_icc_silverstate=yes&p_filters_page=1&p_filters_page_size=2000&p_resource=search";
        private readonly IPageFetcher<SilverStateVendor> _pageFetcher;
        public SilverStateProductDiscoverer(IPageFetcher<SilverStateVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-normal");
            var normalResults = await _pageFetcher.FetchAsync("https://www.silverstatetextiles.com/ajax_search.php", CacheFolder.Search, "normal-results");

            var products = normalResults.QuerySelectorAll(".resource_search_results_result_large").ToList();
            var productIdsWithImages = new List<Tuple<string, string>>();
            foreach (var product in products)
            {
                var productId = product.QuerySelector("a").Attributes["href"].Value.CaptureWithinMatchedPattern("itemNum=(?<capture>(.*))&p_resource");
                var imgUrl = product.QuerySelector("img").Attributes["src"].Value;
                productIdsWithImages.Add(new Tuple<string, string>(productId, imgUrl));
            }
            return productIdsWithImages.Select(x => new DiscoveredProduct
            {
                ScanData = new ScanData
                {
                    { ScanField.WebItemNumber, x.Item1 },
                }
            }).ToList();
        }
    }
}