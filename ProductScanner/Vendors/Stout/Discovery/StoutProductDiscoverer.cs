using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Stout.Discovery
{
    public class StoutItem
    {
        public string id { get; set; }
        public string d { get; set; }
        public string c { get; set; }
        public string s { get; set; }
    }

    public class StoutProductDiscoverer : IProductDiscoverer<StoutVendor>
    {
        private const string DiscoveryUrl = "https://www.estout.com/api/search?api=c6564b6b-d6ee-414b-b7a6-213e3d87d918&pp=48&page={0}";
        private readonly IPageFetcher<StoutVendor> _pageFetcher;

        public StoutProductDiscoverer(IPageFetcher<StoutVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var firstPage = await _pageFetcher.FetchAsync(string.Format(DiscoveryUrl, 1), CacheFolder.Search, "search-1");

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(firstPage.InnerText);
            int total = Convert.ToInt32(jsonResponse.total);

            var skus = new List<string>();
            var numPages = total/48 + 1;
            for (int i = 1; i <= numPages; i++)
            {
                var productsData = await _pageFetcher.FetchAsync(string.Format(DiscoveryUrl, i), CacheFolder.Search, "search-" + i);
                dynamic data = JsonConvert.DeserializeObject<dynamic>(productsData.InnerText);
                
                foreach (var item in data.result)
                {
                    skus.Add(item["sku"].Value);
                }
            }
            return skus.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}