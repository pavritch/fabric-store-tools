using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Nourison
{
    public class NourisonMetadataCollector : IMetadataCollector<NourisonVendor>
    {
        private const string SearchUrl = "http://www.nourison.com/products";
        private readonly IProductFileLoader<NourisonVendor> _fileLoader;
        private readonly IPageFetcher<NourisonVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<NourisonVendor> _sessionManager; 
        private readonly NourisonImapFileLoader _nourisonFileLoader;

        public NourisonMetadataCollector(IProductFileLoader<NourisonVendor> fileLoader, IPageFetcher<NourisonVendor> pageFetcher, IVendorScanSessionManager<NourisonVendor> sessionManager, NourisonImapFileLoader nourisonFileLoader)
        {
            _fileLoader = fileLoader;
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
            _nourisonFileLoader = nourisonFileLoader;
        }

        // Still need to work with the pricing sheet
        // a lot of products not matching up
        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            await SearchColors(products);
            await SearchCategories(products);

            var priceData = await _fileLoader.LoadProductsAsync();
            var imapData = _nourisonFileLoader.LoadInventoryData();
            foreach (var product in products)
            {
                foreach (var variant in product.Variants)
                {
                    var size = variant[ScanField.Size].Replace(" ", "").Replace("'", ".").Replace("\"", "").Replace(".x", ".0x").ToLower();
                    if (size.EndsWith(".")) size += "0";

                    var pattern = product[ScanField.PatternNumber];
                    var priceMatch = priceData.FirstOrDefault(x => x[ScanField.PatternNumber] == pattern &&
                                                                   x[ScanField.Size].ToLower().Contains(size));
                    if (priceMatch != null)
                    {
                        variant[ScanField.Cost] = priceMatch[ScanField.Cost];
                    }
                    else
                    {
                        //Debug.WriteLine(product[ScanField.ManufacturerPartNumber]);
                    }

                    var imapMatch = imapData.FirstOrDefault(x => x[ScanField.Pattern].ToLower().Contains(product[ScanField.Collection].Replace(":", "").ToLower()) &&
                                                                 x[ScanField.Size].Contains(size));
                    if (imapMatch != null)
                    {
                        variant[ScanField.MAP] = imapMatch[ScanField.Cost];
                    }
                    else
                    {
                        Debug.WriteLine(product[ScanField.Collection]);
                    }
                }
            }
            return products;
        }

        private async Task SearchCategories(List<ScanData> products)
        {
            var catSearchUrl = "http://www.nourison.com/{0}?p={1}";
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-all");
            var categories = searchPage.QuerySelectorAll(".child-cat-13 li a")
                .Select(x => x.Attributes["href"].Value.Replace("http://www.nourison.com/", "").Split(new []{'/'}).First()).ToList();
            await _sessionManager.ForEachNotifyAsync("Searching by Category", categories, async category =>
            {
                var allFound = new List<string>();
                var pageNum = 1;
                while (true)
                {
                    var url = string.Format(catSearchUrl, category, pageNum);
                    var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "category-" + category + "-" + pageNum);
                    var foundProducts = page.QuerySelectorAll(".product-image-wrapper a").Select(x => x.Attributes["href"].Value).ToList();
                    var matches = products.Where(x => foundProducts.Contains(x[ScanField.DetailUrlTEMP])).ToList();
                    matches.ForEach(x => x[ScanField.Category] = category);

                    if (foundProducts.Count < 12 || allFound.Contains(foundProducts[0])) break;

                    allFound.AddRange(foundProducts);
                    pageNum++;
                }
            });
        }

        private async Task SearchColors(List<ScanData> products)
        {
            var colorSearchUrl = "http://www.nourison.com/products/filter/color/{0}?p={1}";
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search-products");
            var colors = searchPage.QuerySelector("#dropdown-color").ParentNode.QuerySelectorAll("li")
                .Select(x => x.InnerText.Trim().ToLower()).ToList();
            await _sessionManager.ForEachNotifyAsync("Searching by Color", colors, async color =>
            {
                var allFound = new List<string>();
                var pageNum = 1;
                while (true)
                {
                    var url = string.Format(colorSearchUrl, color, pageNum);
                    var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, "color-" + color + pageNum);
                    var foundProducts = page.QuerySelectorAll(".product-image-wrapper a").Select(x => x.Attributes["href"].Value).ToList();
                    var matches = products.Where(x => foundProducts.Contains(x[ScanField.DetailUrlTEMP])).ToList();
                    matches.ForEach(x => x[ScanField.ColorGroup] = color);

                    if (foundProducts.Count < 12 || allFound.Contains(foundProducts[0])) break;

                    allFound.AddRange(foundProducts);
                    pageNum++;
                }
            });
        }
    }
}