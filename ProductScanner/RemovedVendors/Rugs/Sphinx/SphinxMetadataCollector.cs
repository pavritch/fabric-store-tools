using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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

namespace Sphinx
{
    public class SphinxMetadataCollector : IMetadataCollector<SphinxVendor>
    {
        private const string SearchUrlTemplate = "http://www.owrugs.com/camscgip/aa2.pgm?product=&Srt=1&Psize=21&Coll=&Sstring=&Fiber=&Weave=&Size=&{0}&apage={1}";

        private readonly IVendorScanSessionManager<SphinxVendor> _sessionManager;
        private readonly IPageFetcher<SphinxVendor> _pageFetcher;
        private readonly IStorageProvider<SphinxVendor> _storageProvider;

        public SphinxMetadataCollector(IVendorScanSessionManager<SphinxVendor> sessionManager, IPageFetcher<SphinxVendor> pageFetcher, IStorageProvider<SphinxVendor> storageProvider)
        {
            _sessionManager = sessionManager;
            _pageFetcher = pageFetcher;
            _storageProvider = storageProvider;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            await UpdateStockInfo(products);
            LoadPriceInfo(products);

            await ScrapeMetadata(products);
            return products;
        }

        private async Task ScrapeMetadata(List<ScanData> products)
        {
            var mainSearchUrl = "http://www.owrugs.com/camscgip/login.pgm";
            var mainPage = await _pageFetcher.FetchAsync(mainSearchUrl, CacheFolder.Search, "search");

            await ScrapeKey(products, mainPage, "Style", ScanField.Style);
            await ScrapeKey(products, mainPage, "Color", ScanField.ColorGroup);
            await ScrapeKey(products, mainPage, "Pattern", ScanField.Design);

            //var keyValue = 
            //http://www.owrugs.com/camscgip/aa2.pgm?Style=Pantone&apage=02    
        }

        private async Task ScrapeKey(List<ScanData> products, HtmlNode mainPage, string key, ScanField property)
        {
            var options = mainPage.QuerySelectorAll(string.Format("input[name='{0}']", key)).Select(x => x.Attributes["value"].Value).ToList();
            foreach (var option in options)
            {
                var keyValue = string.Format("{0}={1}", key, option);
                var url = string.Format(SearchUrlTemplate, keyValue, "01");
                var pageOne = await _pageFetcher.FetchAsync(url, CacheFolder.Search, key + "-" + option + "-" + 1);
                var totalProducts = pageOne.QuerySelector("#a_items_found").InnerText.ToIntegerSafe();
                var numPages = Math.Floor((double) (totalProducts - 1)/21) + 1;
                for (int i = 2; i <= numPages; i++)
                {
                    url = string.Format(SearchUrlTemplate, keyValue, i.ToString().PadLeft(2, '0'));
                    var page = await _pageFetcher.FetchAsync(url, CacheFolder.Search, key + "-" + option + "-" + i);
                    var itemsUrls = page.QuerySelectorAll(".a_item").Select(x => x.Attributes["href"].Value).ToList();
                    var patterns = itemsUrls.Select(x => x.CaptureWithinMatchedPattern("Pat=(?<capture>(.*))&")).ToList();

                    var matchingProducts = products.Where(x => patterns.Contains(x[ScanField.Pattern])).ToList();
                    matchingProducts.ForEach(x => x[property] = option);
                }
            }
        }

        private void LoadPriceInfo(List<ScanData> products)
        {
            var filePath = _storageProvider.GetProductsFileStaticPath(ProductFileType.Xlsx);
            var excelFileLoader = new ExcelFileLoader();
            var rows = excelFileLoader.Load(filePath);
            var headers = rows.First().Select(x => RugParser.ParseDimensions(x) == null ? "" : RugParser.ParseDimensions(x).GetDescription()).ToList();

            foreach (var variant in products)
            {
                var match = rows.SingleOrDefault(x => x[0].Equals(variant[ScanField.Collection].Replace(" TOMMY BAHAMA", ""), StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    Debug.WriteLine("Collection Missing Prices: " + variant[ScanField.Collection]);
                    continue;
                }

                if (variant[ScanField.Description].ContainsIgnoreCase("SET")) continue;

                var rugDimensions = RugParser.ParseDimensions(variant[ScanField.Description]);
                if (rugDimensions == null) continue;

                var formattedDescription = rugDimensions.GetDescription();

                var priceCol = headers.IndexOf(formattedDescription);
                if (priceCol != -1 && match[priceCol] != null)
                {
                    var price = match[priceCol].Split(new []{':'});
                    variant[ScanField.Cost] = price.First();
                    if (price.Count() > 1)
                        variant[ScanField.MAP] = price.Last();
                }
                else
                {
                    Debug.WriteLine(formattedDescription + "-" + variant[ScanField.Collection]);
                }
            }
        }

        private async Task UpdateStockInfo(List<ScanData> products)
        {
            var collections = products.Select(x => x[ScanField.Code]).Distinct().ToList();
            await _sessionManager.ForEachNotifyAsync("Scanning inventory", collections, async collection =>
            {
                var stockInfo = await GetCollectionStock(collection);
                var rows = stockInfo.QuerySelectorAll("tr[align='center']").ToList();
                foreach (var row in rows)
                {
                    var cells = row.QuerySelectorAll("td").ToList();
                    var upc = cells[0].QuerySelector("input[name=UPCCode]").Attributes["value"].Value;
                    var stock = cells[5].InnerText.Trim().Replace("+", "");

                    var match = products.SingleOrDefault(x => x[ScanField.UPC] == upc);
                    if (match != null) match[ScanField.StockCount] = stock;
                }
            });
        }

        private async Task<HtmlNode> GetCollectionStock(string collection)
        {
            var sessionUrl = _sessionManager.GetLoginUrl();

            var referUrl = sessionUrl.Replace("/main.mbr/start", "/inventory.mbr/display?Style=") + collection;
            await _pageFetcher.FetchAsync(referUrl, CacheFolder.Stock, collection + "-1");
            var stockFirst = await _pageFetcher.FetchAsync(referUrl, CacheFolder.Stock, collection + "-1");
            if (stockFirst.InnerText.ContainsIgnoreCase("encountered errors"))
            {
                await _sessionManager.Reauthenticate();
                _pageFetcher.RemoveCachedFile(CacheFolder.Stock, collection + "-1");
                _pageFetcher.RemoveCachedFile(CacheFolder.Stock, collection + "-2");
                stockFirst = await _pageFetcher.FetchAsync(referUrl, CacheFolder.Stock, collection + "-1");
            }

            // https://www.owrugs.net/cgi-bin/BD600DFC/qsys.lib/web.lib/macros.file/main.mbr/start
            var url = sessionUrl.Replace("/main.mbr/start", "/inventory.mbr/display");
            var nvCol = new NameValueCollection();
            nvCol.Add("Color", "");
            nvCol.Add("Size", "");
            nvCol.Add("Back", "");
            nvCol.Add("Action", "View Inventory");
            return await _pageFetcher.FetchAsync(url, CacheFolder.Stock, collection + "-2", nvCol, new NameValueCollection { {"Referer", referUrl}});
        }
    }
}