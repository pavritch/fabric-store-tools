using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace PhillipJeffries
{
    public class PhillipJeffriesProductDiscoverer : IProductDiscoverer<PhillipJeffriesVendor>
    {
        private readonly IPageFetcher<PhillipJeffriesVendor> _pageFetcher;
        private const string JsonSearchUrl = "https://www.phillipjeffries.com/api/search.json?term=&limit=100&offset={0}";

        private const string JsonSkewsUrl = "https://www.phillipjeffries.com/api/products/skews.json?limit=50&offset={0}";

        public PhillipJeffriesProductDiscoverer(IPageFetcher<PhillipJeffriesVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var i = 0;
            var paths = new List<string>();
            while (true)
            {
                var offset = i*50;
                var results = await _pageFetcher.FetchAsync(string.Format(JsonSkewsUrl, offset), CacheFolder.Search, "search-skews-" + (i + 1));
                dynamic data = JObject.Parse(results.InnerText);
                foreach (var x in data.items)
                {
                    var path = x.url;
                    paths.Add(path.Value);
                }

                i++;
                var batchCount = data.items.Count;
                if (batchCount != 50) break;
            }
            return paths.Select(x => new DiscoveredProduct(new Uri("https://www.phillipjeffries.com" + x))).ToList();

            /*
            var i = 0;
            var paths = new List<string>();
            while (true)
            {
                var offset = i*100;
                var results = await _pageFetcher.FetchAsync(string.Format(JsonSearchUrl, offset), CacheFolder.Search, "search-" + (i + 1));
                dynamic item = JObject.Parse(results.InnerText);
                foreach (var x in item.data.result.skews)
                {
                    var path = x.path;
                    paths.Add(path.Value);
                }

                i++;
                var batchCount = item.data.result.skews.Count;
                if (batchCount != 100) break;
            }
            return paths.Select(x => new DiscoveredProduct(new Uri("https://www.phillipjeffries.com" + x))).ToList();
            */
        }
    }
}