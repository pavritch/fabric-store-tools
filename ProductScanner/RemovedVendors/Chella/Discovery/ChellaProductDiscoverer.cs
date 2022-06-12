using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Chella.Discovery
{
    public class ChellaProductDiscoverer : IProductDiscoverer<ChellaVendor>
    {
        private const string SearchUrl = "http://www.chellatextiles.com/fabric-collections/search-fabrics/";
        private readonly IPageFetcher<ChellaVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<ChellaVendor> _sessionManager; 
        public ChellaProductDiscoverer(IPageFetcher<ChellaVendor> pageFetcher, IVendorScanSessionManager<ChellaVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "Search");

            var productColors = await ScanProductsByTypeAsync(FindColorOptions(searchPage), "fab_s_color[]");
            var productCategories = await ScanProductsByTypeAsync(FindCategoryOptions(searchPage), "fab_category[]");

            var ids = productColors.Select(x => x.Key)
                .Concat(productCategories.Select(x => x.Key))
                .Distinct()
                .ToList();

            var products = new List<DiscoveredProduct>();
            foreach (var id in ids)
            {
                var properties = new ScanData();
                properties[ProductPropertyType.Category] = productCategories.ContainsKey(id) ? productCategories[id] : null;
                properties[ProductPropertyType.Color] = productColors.ContainsKey(id) ? productColors[id] : null;
                properties[ProductPropertyType.ManufacturerPartNumber] = id;
                properties[ProductPropertyType.ProductDetailUrl] = FindDetailUrl(searchPage, id);
                products.Add(new DiscoveredProduct(properties));
            }
            return products;
        }

        private string FindDetailUrl(HtmlNode searchPage, string sku)
        {
            var urls = searchPage.QuerySelectorAll("#fabrics_all a").Select(x => x.Attributes["href"].Value).Distinct().ToList();
            return urls.SingleOrDefault(x => x.ContainsIgnoreCase(sku));
        }

        private async Task<Dictionary<string, string>> ScanProductsByTypeAsync(IEnumerable<Tuple<string, string>> options, string postKey)
        {
            var productData = new Dictionary<string, string>();
            await _sessionManager.ForEachNotifyAsync("Scanning products by " + postKey, options, async option =>
            {
                var values = new NameValueCollection();
                values.Add("search_name", "");
                values.Add("search_sku", "");
                values.Add(postKey, option.Item1);

                var page = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, option.Item2, values);
                var skus = page.QuerySelectorAll("#fabrics_all div img").Select(x => x.Attributes["alt"].Value.Split(new []{','}).First()).ToList();
                foreach (var sku in skus)
                {
                    if (productData.ContainsKey(sku)) productData[sku] += ", " + option.Item2;
                    else productData.Add(sku, option.Item2);
                }
            });
            return productData;
        }

        private IEnumerable<Tuple<string, string>> FindColorOptions(HtmlNode page)
        {
            return page.QuerySelectorAll("input[name*='fab_s_color']").Select(x => new Tuple<string, string>(
                x.Attributes["value"].Value, x.NextSibling.InnerText));
        }

        private IEnumerable<Tuple<string, string>> FindCategoryOptions(HtmlNode page)
        {
            return page.QuerySelectorAll("input[name*='fab_category']").Select(x => new Tuple<string, string>(
                x.Attributes["value"].Value, x.NextSibling.InnerText));
        }
    }
}