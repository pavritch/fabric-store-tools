using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using InsideFabric.Data;
using Newtonsoft.Json;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace InnovationsUsa.Discovery
{
    public class InnovationsUsaProductDiscoverer : IProductDiscoverer<InnovationsUsaVendor>
    {
        private const string CollectionsListUrl = "https://www.innovationsusa.com/ajax/searchHandler.php";
        private readonly Dictionary<string, ScanField> _specs = new Dictionary<string, ScanField>
        {
            {"Composition:", ScanField.Content},
            {"Backing:", ScanField.Backing},
            {"Total Weight:", ScanField.Weight},
            {"Width:", ScanField.Width},
            {"Repeat:", ScanField.Repeat},
            {"Full Roll:", ScanField.Length},
            {"Fire Rating:", ScanField.FireCode},
            {"Type:", ScanField.Type},
            {"Origin:", ScanField.Country},
            {"Lightfastness:", ScanField.Lightfastness},
            {"Testing:", ScanField.Ignore},
        }; 

        private readonly IPageFetcher<InnovationsUsaVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<InnovationsUsaVendor> _sessionManager;

        public InnovationsUsaProductDiscoverer(IPageFetcher<InnovationsUsaVendor> pageFetcher, IVendorScanSessionManager<InnovationsUsaVendor> sessionManager)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var urls = await GetCollectionUrls();

            var products = new List<ScanData>();
            await _sessionManager.ForEachNotifyAsync("Discovering Collection", urls, async s =>
            {
                var collectionPage = await _pageFetcher.FetchAsync(s, CacheFolder.Search, s.Replace("https://www.innovationsusa.com/s/", ""));

                var template = new ScanData();
                var specs = collectionPage.QuerySelectorAll(".skuSpec li").ToList();
                foreach (var spec in specs)
                {
                    var key = spec.GetFieldValue(".specTitle");
                    var value = spec.GetFieldValue(".specVal");
                    var match = _specs[key];
                    template[match] = value;
                }

                var skuTitle = collectionPage.GetFieldValue(".skuTitle");
                var colors = collectionPage.QuerySelectorAll(".skuUL li").ToList();
                foreach (var color in colors)
                {
                    var sku = color.Attributes["data-sku"].Value;
                    var skuName = color.QuerySelector(".skuName").InnerText;
                    var href = color.QuerySelector("a").Attributes["href"].Value;
                    var image = collectionPage.QuerySelector(href + " img").Attributes["src"].Value;

                    var product = new ScanData(template);
                    product[ScanField.ManufacturerPartNumber] = sku;
                    product[ScanField.Color] = skuName;
                    product[ScanField.PatternName] = skuTitle;
                    product.DetailUrl = new Uri(s);
                    product.AddImage(new ScannedImage(ImageVariantType.Primary, image));
                    products.Add(product);
                }
            });

            return products.Select(x => new DiscoveredProduct(x)).ToList();
        }

        private async Task<List<string>> GetCollectionUrls()
        {
            var items = new List<InnovationsItem>();
            int pageNum = 0;
            while (true)
            {
                var list = await _pageFetcher.FetchAsync(CollectionsListUrl, CacheFolder.Search, "search-" + pageNum, GetFormData(pageNum));
                var result = JsonConvert.DeserializeObject<List<InnovationsItem>>(list.InnerText);
                if (result.Count == 0) break;

                items.AddRange(result);
                pageNum++;
            }
            var urls = items.Select(x => string.Format("https://www.innovationsusa.com/s/{0}:{1}",
                x.subCategoryID.ToLower().Replace(" ", "-").Replace("sheers/drapery", "sheers_drapery"), x.fabricName.ToLower().Replace(" ", "+")));
            return urls.ToList();
        }

        private NameValueCollection GetFormData(int page)
        {
            var collection = new NameValueCollection();
            collection.Add("ajaxRows", "1");
            collection.Add("pageTitle", "product");
            collection.Add("lazyCount", page.ToString());
            collection.Add("gotoCount", "0");
            collection.Add("mainBucket", "wallcovering");
            collection.Add("clickPath", "../");
            return collection;
        }
    }
}