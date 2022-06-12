using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace TheRugMarket
{
    public class TheRugMarketMetadataCollector : IMetadataCollector<TheRugMarketVendor>
    {
        private const string ContemporaryUrl = "http://therugmarket.com/rugs/index.php/rugs/contemporary-area-rugs.html?limit=all";
        private const string TraditionalUrl = "http://therugmarket.com/rugs/index.php/rugs/traditional-area-rugs.html?limit=all";
        private const string OutdoorUrl = "http://therugmarket.com/rugs/index.php/rugs/outdoor-area-rugs.html?limit=all";
        private const string KidsUrl = "http://therugmarket.com/rugs/index.php/rugs/kids-area-rugs.html?limit=all";
        private const string ShagUrl = "http://therugmarket.com/rugs/index.php/rugs/shag-area-rugs.html?limit=all";

        private readonly IVendorScanSessionManager<TheRugMarketVendor> _sessionManager;
        private readonly IPageFetcher<TheRugMarketVendor> _pageFetcher;
        private readonly TheRugMarketInventoryFileLoader _inventoryFileLoader;

        public TheRugMarketMetadataCollector(IVendorScanSessionManager<TheRugMarketVendor> sessionManager, IPageFetcher<TheRugMarketVendor> pageFetcher, TheRugMarketInventoryFileLoader inventoryFileLoader)
        {
            _sessionManager = sessionManager;
            _pageFetcher = pageFetcher;
            _inventoryFileLoader = inventoryFileLoader;
        }

        private async Task UpdateProducts(List<ScanData> vendorVariants)
        {
            var contemporaryIndex = await _pageFetcher.FetchAsync(ContemporaryUrl, CacheFolder.Search, "contemporary");
            var traditionalIndex = await _pageFetcher.FetchAsync(TraditionalUrl, CacheFolder.Search, "traditional");
            var outdoorIndex = await _pageFetcher.FetchAsync(OutdoorUrl, CacheFolder.Search, "outdoor");
            var kidsIndex = await _pageFetcher.FetchAsync(KidsUrl, CacheFolder.Search, "kids");
            var shagIndex = await _pageFetcher.FetchAsync(ShagUrl, CacheFolder.Search, "shag");

            var products = GetUrls(contemporaryIndex, "Contemporary")
                .Concat(GetUrls(traditionalIndex, "Traditional"))
                .Concat(GetUrls(outdoorIndex, "Outdoor"))
                .Concat(GetUrls(kidsIndex, "Kids"))
                .Concat(GetUrls(shagIndex, "Shag"));

            await _sessionManager.ForEachNotifyAsync("Downloading Detail Pages", products, async p =>
            {
                var page = await DownloadDetailPage(p.Item1);
                var descriptionValue = page.GetFieldValue(".std");
                var description = descriptionValue == null ? string.Empty : descriptionValue.Trim();
                var sku = page.GetFieldValue(".sku").Replace("Product Code:", "").Trim();

                var matches = vendorVariants.Where(x => x[ScanField.ManufacturerPartNumber].StartsWith(sku)).ToList();
                if (matches.Any())
                {
                    matches.ForEach(x => x[ScanField.Description] = description);
                    matches.ForEach(x => x[ScanField.Category] += p.Item2 + "|");
                }
            });

            var inventoryData = _inventoryFileLoader.LoadStockData();
            foreach (var variant in vendorVariants)
            {
                var inventoryMatch = inventoryData.FirstOrDefault(x => x[ScanField.ManufacturerPartNumber] == variant[ScanField.ManufacturerPartNumber]);
                if (inventoryMatch != null)
                    variant[ScanField.StockCount] = inventoryMatch[ScanField.StockCount];
            }
        }

        private async Task<HtmlNode> DownloadDetailPage(string url)
        {
            return await _pageFetcher.FetchAsync(url, CacheFolder.Details, url.Replace("http://therugmarket.com/rugs/index.php/", ""));
        }

        private IEnumerable<Tuple<string, string>> GetUrls(HtmlNode page, string category)
        {
            return page.QuerySelectorAll("a.product-image").Select(x => new Tuple<string, string>(x.Attributes["href"].Value, category)).ToList();
        } 

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> vendorVariants)
        {
            await UpdateProducts(vendorVariants);
            return vendorVariants;
        }
    }
}