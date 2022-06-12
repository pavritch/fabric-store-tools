using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace AllstateFloral
{
    public class AllstateSearch
    {
        public KeyValuePair<string, string> CategoryCode { get; set; }
        public KeyValuePair<string, string> GroupCode { get; set; }
        public KeyValuePair<string, string> DisplayClassCode { get; set; }

        public AllstateSearch(KeyValuePair<string, string> categoryCode, KeyValuePair<string, string> groupCode, KeyValuePair<string, string> displayClassCode)
        {
            CategoryCode = categoryCode;
            GroupCode = groupCode;
            DisplayClassCode = displayClassCode;
        }
    }

    public class AllstateFloralMetadataCollector : IMetadataCollector<AllstateFloralVendor>
    {
        private const string SearchUrl = "https://www.allstatefloral.com/pro_i/";
        private const string SearchTwoUrl = "https://www.allstatefloral.com/pro_i/index.cfm?SEARCH=1";
        private const string FullSearchUrl = "https://www.allstatefloral.com/pro_i/index.cfm?1=1&list_f=1&showpic=y&KeyWord=&CategoryCode={0}&ColorNumber=&GroupCode={1}&SizeCode=&DisplayClassCode={2}&PRICE_FM=&PRICE_TO=&page={3}";

        private readonly IPageFetcher<AllstateFloralVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<AllstateFloralVendor> _sessionManager;

        public AllstateFloralMetadataCollector(IPageFetcher<AllstateFloralVendor> pageFetcher, IVendorScanSessionManager<AllstateFloralVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        // CategoryCode
        // GroupCode
        // DisplayClassCode
        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var searches = await GetSearchCombos();
            foreach (var search in searches)
            {
                var url = string.Format(FullSearchUrl, search.CategoryCode.Key, search.GroupCode.Key, search.DisplayClassCode.Key, 1);
                var pageOne = await _pageFetcher.FetchAsync(url, CacheFolder.Search, string.Format("search-{0}-{1}-{2}-{3}", search.CategoryCode.Value, search.GroupCode.Value, search.DisplayClassCode.Value, 1));
                if (pageOne.InnerText.ContainsIgnoreCase("No data matched")) continue;

                var numProducts = pageOne.QuerySelector("td:contains('items matched') font").InnerText.ToIntegerSafe();
                var numPages = numProducts/50 + 1;

                var foundProducts = new List<string>();
                await _sessionManager.ForEachNotifyAsync("Discovering Products", Enumerable.Range(1, numPages), async i =>
                {
                    url = string.Format(FullSearchUrl, search.CategoryCode.Key, search.GroupCode.Key, search.DisplayClassCode.Key, i);
                    var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, string.Format("search-{0}-{1}-{2}-{3}", search.CategoryCode.Value, search.GroupCode.Value, search.DisplayClassCode.Value, i));
                    var urls = page.QuerySelectorAll("a.font3").Select(x => x.Attributes["href"].Value.Replace("../", "http://www.allstatefloral.com/")).ToList();
                    foundProducts.AddRange(urls);
                });
                foundProducts = foundProducts.Select(x => x.Replace("http://www.allstatefloral.com/Productitemdetail.cfm?", "").Replace("&Banner=1", "").Replace("ItemNumber=", "")).ToList();
                foundProducts = foundProducts.Select(HttpUtility.UrlDecode).ToList();

                var matches = products.Where(x => foundProducts.Contains(x[ScanField.ManufacturerPartNumber])).ToList();
                matches.ForEach(x => x.SetFieldIfNotSet(ScanField.ProductType, search.CategoryCode.Value));
                matches.ForEach(x => x.SetFieldIfNotSet(ScanField.Group, search.GroupCode.Value));
                matches.ForEach(x => x.SetFieldIfNotSet(ScanField.Code, search.DisplayClassCode.Value));
            }

            return products;
        }

        public async Task<List<AllstateSearch>> GetSearchCombos()
        {
            var pageOne = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-main");
            var categoryKvps = GetKeyValues(pageOne, "CategoryCode");

            var combinations = new List<AllstateSearch>();
            foreach (var category in categoryKvps)
            {
                combinations.Add(new AllstateSearch(category, new KeyValuePair<string, string>(), new KeyValuePair<string, string>()));
                var values = new NameValueCollection();
                values.Add("CategoryCode", category.Key);
                values.Add("Keyword", "");
                values.Add("ColorNumber", "");
                values.Add("SizeCode", "");
                values.Add("PRICE_FM", "");
                values.Add("PRICE_TO", "");
                values.Add("showpic", "1");
                var groupPage = await _pageFetcher.FetchAsync(SearchTwoUrl, CacheFolder.Search, "cat-" + category.Value, values);
                var groupKvps = GetKeyValues(groupPage, "GroupCode");
                foreach (var group in groupKvps)
                {
                    combinations.Add(new AllstateSearch(category, group, new KeyValuePair<string, string>()));
                    values.Add("GroupCode", group.Key);
                    var classPage = await _pageFetcher.FetchAsync(SearchTwoUrl, CacheFolder.Search, "group-" + group.Value, values);
                    var classKvps = GetKeyValues(classPage, "DisplayClassCode");
                    foreach (var classCode in classKvps)
                    {
                        combinations.Add(new AllstateSearch(category, group, classCode));
                    }
                }
            }
            return combinations;
        }

        private IEnumerable<KeyValuePair<string, string>> GetKeyValues(HtmlNode page, string name)
        {
            var selectList = page.QuerySelectorAll(string.Format("select[name='{0}'] option", name)).Skip(1).ToList();
            var categories = selectList.Select(x => x.NextSibling.InnerText.Trim()).ToList();
            var categoryKeys = selectList.Select(x => x.Attributes["value"].Value).ToList();
            return categories.Zip(categoryKeys, (value, key) => new KeyValuePair<string, string>(key, value)).ToList();
        }
    }
}