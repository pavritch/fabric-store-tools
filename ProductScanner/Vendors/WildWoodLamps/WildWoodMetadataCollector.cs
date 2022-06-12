using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace WildWoodLamps
{
    public class WildWoodMetadataCollector : IMetadataCollector<WildWoodLampsVendor>
    {
        private const string SearchUrlWW = "https://supercat.supercatsolutions.com/wwjc/e/wwfc-2/products?page={0}";
        private const string SearchUrlCH = "https://supercat.supercatsolutions.com/wwjc/e/ch-2/products?page={0}";
        private const string SearchUrlJC = "https://supercat.supercatsolutions.com/wwjc/e/jc-2/products?page={0}";
        private readonly IPageFetcher<WildWoodLampsVendor> _pageFetcher;

        private readonly WildWoodLampsPriceFileLoader _wwPriceFileLoader;
        private readonly ChelseaHousePriceFileLoader _chPriceFileLoader;

        public WildWoodMetadataCollector(IPageFetcher<WildWoodLampsVendor> pageFetcher, WildWoodLampsPriceFileLoader wwPriceFileLoader, ChelseaHousePriceFileLoader chPriceFileLoader)
        {
            _pageFetcher = pageFetcher;
            _wwPriceFileLoader = wwPriceFileLoader;
            _chPriceFileLoader = chPriceFileLoader;
        }

        private async Task<Dictionary<string, string>> CollectStock(string url, string code)
        {
            var stockDictionary = new Dictionary<string, string>();
            var i = 1;
            while (true)
            {
                var page = await _pageFetcher.FetchAsync(string.Format(url, i), CacheFolder.Search, code + i);
                i++;

                var pageProducts = page.QuerySelectorAll(".catalog-item").ToList();
                pageProducts.ForEach(x => stockDictionary[x.GetFieldValue(".catalog-item-number")] = x.GetFieldValue("li:contains('available')"));

                if (!pageProducts.Any()) break;
            }

            return stockDictionary;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var stockDictionary = await CollectStock(SearchUrlWW, "ww");
            (await CollectStock(SearchUrlCH, "ch")).ToList().ForEach(x => stockDictionary[x.Key] = x.Value);
            (await CollectStock(SearchUrlJC, "jc")).ToList().ForEach(x => stockDictionary[x.Key] = x.Value);

            products.ForEach(x =>
            {
                var mpn = x[ScanField.ManufacturerPartNumber];
                x[ScanField.StockCount] = stockDictionary.ContainsKey(mpn) ? stockDictionary[mpn] : "";
            });

            await CollectCategories(products, SearchUrlWW, "wwfc");
            await CollectCategories(products, SearchUrlCH, "ch");
            await CollectCategories(products, SearchUrlJC, "jc");

            var wwPrices = _wwPriceFileLoader.LoadInventoryData();
            var chPrices = _chPriceFileLoader.LoadInventoryData();

            foreach (var price in wwPrices.Concat(chPrices))
            {
                var match = products.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == price[ScanField.ManufacturerPartNumber]);
                if (match != null)
                {
                    match.Cost = price[ScanField.MAP].ToDecimalSafe() / 2.3m;
                    match[ScanField.MAP] = price[ScanField.MAP];
                }
            }

            return products;
        }

        private async Task CollectCategories(List<ScanData> products, string url, string siteCode)
        {
            var mainPage = await _pageFetcher.FetchAsync(string.Format(url, "1"), CacheFolder.Search, siteCode);
            var typesContainer = mainPage.QuerySelector("span:contains('Product Types')");
            if (typesContainer == null) return;

            var types = mainPage.QuerySelector("span:contains('Product Types')").NextSibling.NextSibling.QuerySelectorAll("a").ToList();
            var categories = types
                .Where(x => x.Attributes["href"].Value.Contains("category_id"))
                .Select(x => new Tuple<string, string>(x.Attributes["href"].Value.Split('=').Last(), x.InnerText))
                .ToList();

            var tasks = categories.Select(x => SetCategories(x.Item1, x.Item2, siteCode, products));
            await Task.WhenAll(tasks);
        }

        private async Task SetCategories(string categoryId, string category, string siteCode, List<ScanData> products)
        {
            var matches = await SearchCategories("https://catalog.wildwoodlamps.com/wwjc/e/{2}-2/products?category_id={0}&page={1}", categoryId.ToIntegerSafe(), siteCode);
            var matchingProducts = products.Where(x => matches.Contains(x[ScanField.ManufacturerPartNumber])).ToList();
            matchingProducts.ForEach(x => x[ScanField.Category] = category);
        }

        public async Task<List<string>> SearchCategories(string url, int categoryId, string siteCode)
        {
            var products = new List<string>();
            var pageNum = 1;
            while (true)
            {
                var page = await _pageFetcher.FetchAsync(string.Format(url, categoryId, pageNum, siteCode), CacheFolder.Search, 
                    categoryId + siteCode + "-" + pageNum);
                var pageProducts = page.QuerySelectorAll(".catalog-item").ToList();

                products.AddRange(pageProducts.Select(x => x.GetFieldValue(".catalog-item-number")).ToList());

                pageNum++;
                if (!pageProducts.Any()) break;
            }
            return products;
        }
    }
}