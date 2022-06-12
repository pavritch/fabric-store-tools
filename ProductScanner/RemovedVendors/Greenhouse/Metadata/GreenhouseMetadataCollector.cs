using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using Greenhouse.Discovery;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace Greenhouse.Metadata
{
    public class GreenhouseMetadataCollector : IMetadataCollector<GreenhouseVendor>
    {
        private const string SearchUrl = "https://www.greenhousefabrics.com/fabrics";
        private readonly IPageFetcher<GreenhouseVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<GreenhouseVendor> _sessionManager;
        private readonly GreenhouseJSONSearcher _jsonSearcher;
        private readonly IProductFileLoader<GreenhouseVendor> _productFileLoader;

        public GreenhouseMetadataCollector(IPageFetcher<GreenhouseVendor> pageFetcher, IVendorScanSessionManager<GreenhouseVendor> sessionManager, GreenhouseJSONSearcher jsonSearcher, IProductFileLoader<GreenhouseVendor> productFileLoader)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
            _jsonSearcher = jsonSearcher;
            _productFileLoader = productFileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var fabricsPage = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "fabrics");
            await UpdateProductsByQuery(GetMenuOptions(fabricsPage, "style"), "style", ScanField.Style, products);
            await UpdateProductsByQuery(GetMenuOptions(fabricsPage, "category"), "category", ScanField.Category, products);
            await UpdateProductsByQuery(GetMenuOptions(fabricsPage, "color"), "color", ScanField.Color, products);
            await UpdateProductsByQuery(GetMenuOptions(fabricsPage, "usage"), "usage", ScanField.ProductUse, products);
            await UpdateProductsByQuery(new List<string> { "true" }, "sale", ScanField.IsClearance, products);

            var leatherFilePrices = await _productFileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                var match = leatherFilePrices.FirstOrDefault(x => x[ScanField.ManufacturerPartNumber].ToLower() == product[ScanField.ManufacturerPartNumber].ToLower());
                if (match != null)
                {
                    product.Cost = match[ScanField.Cost].ToDecimalSafe();
                    product[ScanField.UnitOfMeasure] = "Square Foot";
                }
            }

            return products;
        }

        private IEnumerable<string> GetMenuOptions(HtmlNode page, string dataFacet)
        {
            return page.QuerySelectorAll(string.Format("li.facet-link[data-facet='{0}']", dataFacet))
                .Select(x => x.Attributes["data-machine-name"].Value);
        }

        private async Task UpdateProductsByQuery(IEnumerable<string> options, string keyword, ScanField property, List<ScanData> products)
        {
            await _sessionManager.ForEachNotifyAsync("Find metadata for " + keyword, options, async option =>
            {
                var matchingProducts = await _jsonSearcher.Search("&" + keyword + "=" + option);
                foreach (var mpn in matchingProducts)
                {
                    var product = products.SingleOrDefault(x => string.Equals(x[ScanField.ManufacturerPartNumber], mpn, StringComparison.OrdinalIgnoreCase));
                    if (product != null) product[property] = option;
                }
            });
        }
    }
}