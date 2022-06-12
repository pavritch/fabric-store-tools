using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities;

namespace Nourison
{
    public class NourisonResponse
    {
        public List<NourisonJsonProduct> Products { get; set; }
        public int Count { get; set; }
    }

    public class NourisonJsonProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Url { get; set; }
        public string Img_Uri { get; set; }
        public string Sku { get; set; }
        public bool Limited { get; set; }
        public bool New { get; set; }
        public string Bv_Id { get; set; }
    }

    public class NourisonProductDiscoverer : IProductDiscoverer<NourisonVendor>
    {
        private const string SearchUrl = "http://www.nourison.com/area-rugs";
        private const string ApiEndpoint = "https://www.nourison.com/filter_product.php";
        //private const string NicoSearchUrl = "http://www.nourison.com/nico-home";
        private readonly IPageFetcher<NourisonVendor> _pageFetcher;

        public NourisonProductDiscoverer(IPageFetcher<NourisonVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all");
            var collectionIds = searchPage.QuerySelectorAll(".image-product").Select(x => x.Attributes["id"].Value).ToList();

            //var nicoSearchPage = await _pageFetcher.FetchAsync(NicoSearchUrl, CacheFolder.Search, "search-nico");
            //collectionUrls.AddRange(nicoSearchPage.QuerySelectorAll(".image-product a").Select(x => x.Attributes["href"].Value));

            var productUrls = new List<string>();
            foreach (var collectionId in collectionIds)
            {
                var postData = new NameValueCollection();
                postData.Add("page", "1");
                postData.Add("page_size", "200");
                postData.Add("main_cat", collectionId);
                postData.Add("shapes", "");
                postData.Add("sizes", "");
                postData.Add("colors", "");
                postData.Add("styles", "");
                postData.Add("collns", "");
                postData.Add("sort", "asc");

                var collectionResults = await _pageFetcher.FetchAsync(ApiEndpoint, CacheFolder.Search, collectionId, postData);
                var products = collectionResults.InnerText.FromJSON<NourisonResponse>();
                if (products == null) continue;

                productUrls.AddRange(products.Products.Select(x => x.Url).ToList());
            }
            return productUrls.Select(x => new DiscoveredProduct(new Uri("https://www.nourison.com/" + x))).ToList();
        }
    }
}