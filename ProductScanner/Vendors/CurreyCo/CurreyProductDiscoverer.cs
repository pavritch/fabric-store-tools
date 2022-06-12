using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace CurreyCo
{
    public class CurreyProductDiscoverer : IProductDiscoverer<CurreyVendor>
    {
        private const string SearchUrl = "https://www.curreyandcompany.com/searchadv.aspx?searchterm=Product%20Search";
        private readonly IPageFetcher<CurreyVendor> _pageFetcher;

        public CurreyProductDiscoverer(IPageFetcher<CurreyVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var resp = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-get");
            var viewstate = resp.QuerySelector("#__VIEWSTATE").GetAttributeValue("value", "FALSE");

            var products = new List<string>();
            var i = 1;
            while (true)
            {
                var values = GetQueryData(i, viewstate);
                var results = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all-" + i, values);
                var matches = results.QuerySelectorAll(".thumb_list > a").Select(x => x.Attributes["href"].Value).ToList();
                products.AddRange(matches);

                i++;
                if (matches.Count < 200) break;
            }
            return products.Select(x => new DiscoveredProduct(new Uri("https://www.curreyandcompany.com" + x))).ToList();
        }

        private NameValueCollection GetQueryData(int pageNum, string viewstate)
        {
            var values = new NameValueCollection();
            values.Add("__EVENTTARGET", "");
            values.Add("__EVENTARGUMENT", "");
            values.Add("__VIEWSTATE", viewstate);
            values.Add("SearchTerm", "Product Search");
            values.Add("InMinHeight", "");
            values.Add("InMaxHeight", "");
            values.Add("InMinDepth", "");
            values.Add("InMaxDepth", "");
            values.Add("InMinWidth", "");
            values.Add("InMaxWidth", "");
            values.Add("pagenum", pageNum.ToString());
            values.Add("pagesize", "200");
            values.Add("maxWidth", "");
            values.Add("minWidth", "");
            values.Add("maxHeight", "");
            values.Add("minHeight", "");
            values.Add("maxDepth", "");
            values.Add("minDepth", "");
            values.Add("material", "");
            values.Add("finish", "");
            values.Add("curreyinahurry", "");
            values.Add("Isnew", "");
            values.Add("BestSeller", "");
            values.Add("pricerange", "");
            values.Add("searchtermform", "Product Search");
            return values;
        }
    }
}