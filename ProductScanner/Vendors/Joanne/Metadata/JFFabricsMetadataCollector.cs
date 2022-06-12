using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace JFFabrics.Metadata
{
    public class JFFabricsMetadataCollector : IMetadataCollector<JFFabricsVendor>
    {
        private const string SearchUrl = "http://www.jffabrics.com/types/fabric/";
        private readonly IPageFetcher<JFFabricsVendor> _pageFetcher;
        private readonly IProductFileLoader<JFFabricsVendor> _fileLoader;
        private readonly JFFabricsSearcher _jfFabricsSearcher;

        public JFFabricsMetadataCollector(JFFabricsSearcher joanneSearcher, IPageFetcher<JFFabricsVendor> pageFetcher, IProductFileLoader<JFFabricsVendor> fileLoader)
        {
            _jfFabricsSearcher = joanneSearcher;
            _pageFetcher = pageFetcher;
            _fileLoader = fileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var fileProducts = await _fileLoader.LoadProductsAsync();
            PopulateFileData(products, fileProducts);

            var searchPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "search");
            var colors = searchPage.QuerySelectorAll("a.color-select").Select(x => x.Attributes["data-filter"].Value).Where(x => !x.Contains("*")).ToList();
            foreach (var color in colors)
            {
                var byColor = await _jfFabricsSearcher.SearchFabricWithFilter(color.Replace("+", "%2B"));
                AppendProperty(products, ScanField.ColorGroup, byColor, color.Replace("color+", ""));
            }

            var designs = searchPage.QuerySelectorAll("#filters-design li a").Skip(1).ToList();
            foreach (var design in designs)
            {
                var filter = design.Attributes["data-filter"].Value;
                var designValue = design.InnerText.Replace("<!--", "").Replace("-->", "").Trim();
                var byDesign = await _jfFabricsSearcher.SearchFabricWithFilter(filter.Replace("+", "%2B"));
                AppendProperty(products, ScanField.Design, byDesign, designValue);
            }

            // right now everything is being returned for every category
            //var categories = searchPage.QuerySelectorAll("#filters-category li a").Skip(1).ToList();
            //foreach (var category in categories)
            //{
            //    var filter = category.Attributes["data-filter"].Value;
            //    var categoryValue = category.InnerText.Replace("<!--", "").Replace("-->", "").Trim();
            //    var byCategory = await _jfFabricsSearcher.SearchFabricWithFilter(filter.Replace("+", "%2B"));
            //    AppendProperty(products, ScanField.Category, byCategory, categoryValue);
            //}

            return products;
        }

        private void PopulateFileData(List<ScanData> products, List<ScanData> fileProducts)
        {
            foreach (var product in products)
            {
                var associatedFileProduct = fileProducts.SingleOrDefault(x => x[ScanField.PatternName].Equals(product[ScanField.PatternName], 
                    StringComparison.OrdinalIgnoreCase));
                if (associatedFileProduct != null)
                {
                    product[ScanField.ProductGroup] = associatedFileProduct[ScanField.ProductGroup];
                    product[ScanField.UnitOfMeasure] = associatedFileProduct[ScanField.UnitOfMeasure];

                    // if we didn't find a wholesale price in the scan, use the one from the file
                    if (product[ScanField.Cost] == string.Empty)
                    {
                        product[ScanField.Cost] = associatedFileProduct[ScanField.Cost].Replace("$", "");
                    }
                }
            }
        }

        private void AppendProperty(List<ScanData> products, ScanField property, List<string> productUrls, string value)
        {
            foreach (var productUrl in productUrls)
            {
                var associatedProduct = products.SingleOrDefault(x => x.DetailUrl == new Uri(productUrl));
                if (associatedProduct != null)
                    associatedProduct[property] += value + ",";
            }
        }
    }
}