using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace AndrewMartin
{
    public class AndrewMartinMetadataCollector : IMetadataCollector<AndrewMartinVendor>
    {
        private readonly IPageFetcher<AndrewMartinVendor> _pageFetcher;
        private readonly IProductFileLoader<AndrewMartinVendor> _productFileLoader;
        private readonly IVendorScanSessionManager<AndrewMartinVendor> _sessionManager; 
        private const string SearchUrl = "http://www.andrewmartin.co.uk/fabric-showroom/fabric-search.php";
        private const string SearchPageUrl = "http://www.andrewmartin.co.uk/fabric-showroom/fabric-search.php?{0}={1}";

        public AndrewMartinMetadataCollector(IPageFetcher<AndrewMartinVendor> pageFetcher, 
            IProductFileLoader<AndrewMartinVendor> productFileLoader,
            IVendorScanSessionManager<AndrewMartinVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _productFileLoader = productFileLoader;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search");
            var colorsData = await ScanMetadataTypeAsync(searchPage, 0, "fabric_colour_group");
            var designData = await ScanMetadataTypeAsync(searchPage, 1, "fabric_design_group");
            var compositionData = await ScanMetadataTypeAsync(searchPage, 2, "fabric_composition_group");

            var fileProducts = await _productFileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                var patternName = product[ScanField.PatternName].Replace("-", " ").ToLower();
                var match = fileProducts.FirstOrDefault(x => x[ScanField.PatternName].ToLower()
                    .StartsWith(patternName));
                if (match == null)
                {
                    continue;
                }

                product.Cost = match[ScanField.Cost].Replace("$", "").ToDecimalSafe();
                product[ScanField.FireCode] = match[ScanField.FireCode];
                product[ScanField.Railroaded] = match[ScanField.Railroaded];
                product[ScanField.ColorGroup] = FindData(colorsData, patternName);
                product[ScanField.Design] = FindData(designData, patternName);
                product[ScanField.Category] = FindData(compositionData, patternName);
            }
            return products;
        }

        private string FindData(Dictionary<string, string> data, string patternName)
        {
            foreach (var d in data)
            {
                if (d.Key.ContainsIgnoreCase(patternName))
                    return d.Value;
            }
            return null;
        }

        private async Task<Dictionary<string, string>> ScanMetadataTypeAsync(HtmlNode searchPage, int index, string key)
        {
            var data = new Dictionary<string, string>();
            var designs = searchPage.QuerySelectorAll("dd").ToList()[index].QuerySelectorAll("li a");

            foreach (var design in designs)
            {
                var designId = design.Attributes["href"].Value.CaptureWithinMatchedPattern(string.Format(@"{0}=(?<capture>(\d+))", key));
                var designGroup = design.InnerText;
                var searchUrl = string.Format(SearchPageUrl, key, designId);
                var designPage = await _pageFetcher.FetchAsync(searchUrl, CacheFolder.Search, designGroup);

                var total = Convert.ToInt32(designPage.InnerText.CaptureWithinMatchedPattern(@"of (?<capture>(\d+)) total"));
                var numPages = total/9 + 1;
                await _sessionManager.ForEachNotifyAsync("Scanning pages for " + designGroup, Enumerable.Range(1, numPages), async pageNum =>
                {
                    var url = string.Format("{0}&p={1}", searchUrl, pageNum);
                    var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, string.Format("{0}-{1}", designId, pageNum));
                    var patterns = page.QuerySelectorAll(".category-products .products-grid li a").Select(x => x.Attributes["title"].Value).Distinct().ToList();
                    patterns.ForEach(x => { if (!data.ContainsKey(x)) data.Add(x, designGroup); });
                });
            };
            return data;
        }
    }
}