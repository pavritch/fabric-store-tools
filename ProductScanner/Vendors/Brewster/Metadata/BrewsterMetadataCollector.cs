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

namespace Brewster.Metadata
{
    public class BrewsterMetadataCollector : IMetadataCollector<BrewsterVendor>
    {
        private const string StyleUrl = "http://www.brewsterwallcovering.com/wallpaper-styles.aspx";
        private const string TypeUrl = "http://www.brewsterwallcovering.com/wallpaper-product-types.aspx";
        private const string ThemeUrl = "http://www.brewsterwallcovering.com/wallpaper-themes.aspx";
        private const string ColorUrl = "http://www.brewsterwallcovering.com/wallpaper-colors.aspx";
        private readonly IPageFetcher<BrewsterVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<BrewsterVendor> _sessionManager;
        private readonly IProductFileLoader<BrewsterVendor> _fileLoader;
        private readonly BrewsterBoltFileLoader _boltFileLoader;

        public BrewsterMetadataCollector(IPageFetcher<BrewsterVendor> pageFetcher, IVendorScanSessionManager<BrewsterVendor> sessionManager, 
            IProductFileLoader<BrewsterVendor> fileLoader, BrewsterBoltFileLoader boltFileLoader)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
            _fileLoader = fileLoader;
            _boltFileLoader = boltFileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            await ScanWebsiteByPropertyAsync(StyleUrl, products, ScanField.Style1);
            await ScanWebsiteByPropertyAsync(TypeUrl, products, ScanField.Category);
            await ScanWebsiteByPropertyAsync(ThemeUrl, products, ScanField.Style2);
            await ScanWebsiteByPropertyAsync(ColorUrl, products, ScanField.ColorGroup);
            //await LoadRelatedProductsAsync(products.Where(x => x.DetailUrl != null).DistinctBy(x => x.GetDetailUrl()));

            var boltProducts = _boltFileLoader.LoadInventoryData();
            foreach (var product in products)
            {
                var match = boltProducts.FirstOrDefault(x => product[ScanField.ManufacturerPartNumber] == x[ScanField.ManufacturerPartNumber]);
                if (match != null)
                {
                    product.IsBolt = true;
                }
            }

            var fileProducts = await _fileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                var match = fileProducts.FirstOrDefault(x => product[ScanField.ManufacturerPartNumber] == x[ScanField.ManufacturerPartNumber]);
                if (match != null)
                {
                    product[ScanField.MAP] = match[ScanField.MAP];
                    product[ScanField.RetailPrice] = match[ScanField.RetailPrice];
                    product[ScanField.UnitOfMeasure] = match[ScanField.UnitOfMeasure];
                }
            }
            return products;
        }
        
        private async Task ScanWebsiteByPropertyAsync(string rootUrl, List<ScanData> products, ScanField property)
        {
            var rootPage = await _pageFetcher.FetchAsync(rootUrl, CacheFolder.Search, "root-" + property);
            var optionLinkElements = rootPage.QuerySelectorAll("#ctl00_ctl00_MainContent_uxCategory_BrewsterBanner_CATEGORIES_ctrlNavigationn1Nodes a").ToList();
            var options = optionLinkElements.Select(x => x.InnerText.Replace("&raquo;", "").Replace(" &amp; ", "-").Trim().Replace(" ", "-").ToLower()).ToList();
            var optionLinks = optionLinkElements.Select(x => x.Attributes["href"].Value).ToList();
            await _sessionManager.ForEachNotifyAsync("Scanning metadata for " + property, options, async (i, option) =>
            {
                var url = string.Format("http://www.brewsterwallcovering.com/{0}?page=1&size=10000", optionLinks[i]);

                var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, property + "-" + option);
                var productNodes = page.QuerySelectorAll("#ctl00_ctl00_MainContent_uxCategory_uxCategoryProductList_DataListProducts .ProductListItem").ToList();
                var mpns = productNodes.Select(x => x.Attributes["style"].Value.CaptureWithinMatchedPattern(@"/catalog/\d+/(?<capture>(.*)).jpg")).ToList();
                var urls = page.QuerySelectorAll(".boxed").Select(x => x.Attributes["href"].Value).ToList();
                for (int index = 0; index < mpns.Count; index++)
                {
                    var mpn = mpns[index];
                    var product = products.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == mpn);
                    if (product != null)
                    {
                        product[property] = option;
                        product.DetailUrl = new Uri(string.Format("http://www.brewsterwallcovering.com{0}", urls[index]));
                    }
                }
            });
        }

        private async Task LoadRelatedProductsAsync(IEnumerable<ScanData> products)
        {
            var list = products.ToList();
            await _sessionManager.ForEachNotifyAsync("Loading related products", list, async product =>
            {
                var productUrl = product.GetDetailUrl();
                var filename = productUrl.Replace("http://www.brewsterwallcovering.com/", "").Replace(".aspx", "").Replace("/", "");
                var page = await _pageFetcher.FetchAsync(productUrl, CacheFolder.Details, filename);
                CaptureCorrelatedPatterns(page);
                product[ScanField.Other] = page.InnerText.Contains("Roll Length") ? "Roll" : "";
                if (product[ScanField.Width] == string.Empty) 
                    product[ScanField.Width] = page.GetFieldValue("#ctl00_ctl00_MainContent_uxProduct_SampleDetails_brewProductProperties_lblRollWidth");
                if (product[ScanField.Length] == string.Empty)
                    product[ScanField.Length] = page.GetFieldValue("#ctl00_ctl00_MainContent_uxProduct_SampleDetails_brewProductProperties_lblRollLength");
                if (product[ScanField.Coverage] == string.Empty)
                    product[ScanField.Coverage] = page.GetFieldValue("#ctl00_ctl00_MainContent_uxProduct_SampleDetails_brewProductProperties_lblRollCoverage");
            });

            foreach (var product in list)
            {
                product.RelatedProducts = _patternGroups.FirstOrDefault(x => x.Contains(product[ScanField.ManufacturerPartNumber])) 
                    ?? new List<string>();
            }
        }

        private string GetMPNFromUrl(string href)
        {
            var end = href.NthIndexOf("-", 2);
            return href.Substring(0, end).Trim('/');

            /*
            var mpnWithDash = href.Substring(href.LastIndexOf("/") + 1).Replace(".jpg", "");

            // a lot of the mpns from the image have something like '-2514' at the end
            if (mpnWithDash.Contains("-"))
                return mpnWithDash.Substring(0, mpnWithDash.LastIndexOf("-"));
            return mpnWithDash;
            */
        }

        private readonly List<List<string>> _patternGroups = new List<List<string>>();
        private void CaptureCorrelatedPatterns(HtmlNode page)
        {
            var relatedList = page.QuerySelectorAll("#ctl00_ctl00_MainContent_uxProduct_divColorways a").ToList();
            if (!relatedList.Any()) return;

            // the first one in the list appears to always be the current mpn (with no dashes)
            //var currentId = relatedList.First().Attributes["href"].Value;
            //currentId = currentId.Substring(currentId.LastIndexOf("/") + 1).Replace(".jpg", "");
            var ids = relatedList.Select(x => GetMPNFromUrl(x.Attributes["href"].Value)).ToList();
            ids = ids.OrderBy(x => x).Distinct().ToList();
            //ids.Add(currentId);

            // some patterns don't have all of the relateds listed
            var group = FindGroup(ids);
            if (group == null)
            {
                _patternGroups.Add(ids);
            }

            if (group != null)
            {
                foreach (var id in ids)
                {
                    if (!group.Contains(id)) group.Add(id);
                }
            }
        }

        private List<string> FindGroup(List<string> mpns)
        {
            return _patternGroups.FirstOrDefault(group => group.Any(mpns.Contains));
        }
    }
}