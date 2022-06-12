using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities;
using Utilities.Extensions;

namespace GoHomeLTD
{
    public class GoHomeSearcher
    {
        private const string SearchUrl = "http://www.gohomeltd.com/Store/Search.aspx/FilterItems";
        private const string ResetUrl = "http://www.gohomeltd.com/Store/Search.aspx/SetSessionRowNoValue";
        private readonly IPageFetcher<GoHomeVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<GoHomeVendor> _sessionManager; 
        private readonly NameValueCollection _queryHeaders;

        public GoHomeSearcher(IPageFetcher<GoHomeVendor> pageFetcher, IVendorScanSessionManager<GoHomeVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;

            _queryHeaders = new NameValueCollection();
            _queryHeaders.Add("Content-Type", "application/json");
            
        }

        private Dictionary<string, string> GetSearchValues()
        {
            var searchValues = new Dictionary<string, string>();
            searchValues.Add("SelectedCollection", "");
            searchValues.Add("selectedCategory", "");
            searchValues.Add("selectedSubCategory", "");
            searchValues.Add("selectedStock", "");
            searchValues.Add("Green", "0");
            searchValues.Add("IsNew", "0");
            searchValues.Add("OnSale", "0");
            searchValues.Add("priceFrom", "0");
            searchValues.Add("priceTo", "4728");
            searchValues.Add("IdValue", "");
            return searchValues;
        }

        public async Task<List<int>> SearchAll()
        {
            return await Search("all", GetSearchValues());
        }

        public async Task<List<int>> SearchByCollection(int collection)
        {
            var searchValues = GetSearchValues();
            searchValues["SelectedCollection"] = collection.ToString();
            var products = await Search("collection" + collection, searchValues);
            await _pageFetcher.FetchAsync(ResetUrl, CacheFolder.Search, Guid.NewGuid().ToString(), string.Empty);
            return products;
        }

        public async Task<List<int>> SearchByCategory(int category)
        {
            var searchValues = GetSearchValues();
            searchValues["selectedCategory"] = category.ToString();
            var products = await Search("category" + category, searchValues);
            await _pageFetcher.FetchAsync(ResetUrl, CacheFolder.Search, Guid.NewGuid().ToString(), string.Empty);
            return products;
        }

        public async Task<List<int>> SearchByCategoryAndSubcategory(int category, int subcategory)
        {
            var searchValues = GetSearchValues();
            searchValues["selectedCategory"] = category.ToString();
            searchValues["selectedSubCategory"] = subcategory.ToString();
            var products = await Search("subcategory" + subcategory, searchValues);
            await _pageFetcher.FetchAsync(ResetUrl, CacheFolder.Search, Guid.NewGuid().ToString(), string.Empty);
            return products;
        }

        private async Task<List<int>> Search(string key, Dictionary<string, string> searchvalues)
        {
            var allProducts = new List<int>();
            var totalProducts = await GetTotalProducts(key, searchvalues);
            var numPages = totalProducts/30 + 1;

            await _sessionManager.ForEachNotifyAsync(string.Format("Discovering Products ({0})", key), Enumerable.Range(1, numPages), async i =>
            {
                allProducts.AddRange(await SearchProducts(key, i, searchvalues));
            });
            return allProducts.Distinct().ToList();
        }

        private async Task<List<int>> SearchProducts(string key, int index, Dictionary<string, string> searchValues)
        {
            var search = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, key + "-search-" + index, searchValues.ToJSON(), _queryHeaders);
            var results = JObject.Parse(search.InnerText);
            var dataItems = results["d"].ToList();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(dataItems[0].ToString());

            var products = htmlDoc.DocumentNode.QuerySelectorAll("a").ToList();
            // http://www.gohomeltd.com/Store/Style.aspx?Id=1331&GroupId=
            return products.Select(x => x.Attributes["href"].Value.CaptureWithinMatchedPattern(@"Id=(?<capture>(\d+))").ToIntegerSafe()).ToList();
        }

        private async Task<int> GetTotalProducts(string key, Dictionary<string, string> searchValues)
        {
            var search = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, key + "-search-1", searchValues.ToJSON(), _queryHeaders);

            var results = JObject.Parse(search.InnerText);
            var dataItems = results["d"].ToList();
            return dataItems[3].ToString().ToIntegerSafe();
        }
    }
}