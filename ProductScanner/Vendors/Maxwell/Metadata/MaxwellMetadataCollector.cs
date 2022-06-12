using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace Maxwell.Metadata
{
    public class MaxwellMetadataCollector : IMetadataCollector<MaxwellVendor>
    {
        private const string SearchUrl ="http://www.maxwellfabrics.com/collections?search=search";
        private readonly IPageFetcher<MaxwellVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<MaxwellVendor> _sessionManager; 

        public MaxwellMetadataCollector(IPageFetcher<MaxwellVendor> pageFetcher, IVendorScanSessionManager<MaxwellVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var productsByColor = await FindProductAssociations(await GetSearchOptions("#edit-vid-4"));
            var productsByConstruction = await FindProductAssociations(await GetSearchOptions("#edit-vid-6"));
            var productsByDesign = await FindProductAssociations(await GetSearchOptions("#edit-vid-3"));
            var productsByStyle = await FindProductAssociations(await GetSearchOptions("#edit-vid-9"));

            AddProperties(products, productsByColor, ScanField.ColorGroup);
            AddProperties(products, productsByConstruction, ScanField.Construction);
            AddProperties(products, productsByDesign, ScanField.Design);
            AddProperties(products, productsByStyle, ScanField.Style);
            return products;
        }

        private void AddProperties(List<ScanData> products, Dictionary<string, List<string>> groupedProducts, ScanField property)
        {
            foreach (var group in groupedProducts)
            {
                foreach (var mpn in group.Value)
                {
                    var product = products.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == mpn);
                    if (product == null) continue;
                    product[property] = group.Key;
                }
            }
        }

        private async Task<List<SearchCollection>> GetSearchOptions(string divId)
        {
            var docNode = await _pageFetcher.FetchAsync(SearchUrl, CacheFolder.Search, "Index");
            var checkboxParentNodes = docNode.QuerySelectorAll(divId + " div.form-type-checkbox").ToList();
            return checkboxParentNodes.Select(x => new SearchCollection(
                x.QuerySelector("label").InnerHtml.Trim(),
                // vid-4[18]
                x.QuerySelector("input").Attributes["name"].Value.CaptureWithinMatchedPattern(@"(?<capture>(vid\-\d+))\[\d+\]"),
                x.QuerySelector("input").Attributes["value"].Value)).ToList();
        }

        protected async Task<Dictionary<string, List<string>>> FindProductAssociations(List<SearchCollection> searchCollections)
        {
            // key is keyword, value is list of MPN
            var dicCollections = new Dictionary<string, List<string>>();

            await _sessionManager.ForEachNotifyAsync("Loading collection metadata", searchCollections, async collection =>
            {
                var memberProducts = new List<string>();
                var searchResultsUrl = string.Format("http://www.maxwellfabrics.com/product?{0}={1}", collection.QueryKey, collection.QueryValue);

                var page = await _pageFetcher.FetchAsync(searchResultsUrl, CacheFolder.Search, collection.QueryValue);
                var productNodes = page.QuerySelectorAll("img[src^='http://www.maxwellfabrics.com/sites/dev.maxwell.com/files/styles/thumbnail/public/']");

                foreach (HtmlNode node in productNodes)
                {
                    var href = node.ParentNode.Attributes["href"].Value;
                    var mpn = href.Replace("/p/", "");
                    if (!String.IsNullOrEmpty(mpn))
                    {
                        memberProducts.Add(mpn);
                    }
                }
                dicCollections.Add(collection.Name, memberProducts);
            });
            return dicCollections;
        }
    }
}