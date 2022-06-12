using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CsvHelper;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;

namespace ClarenceHouse.Discovery
{
    public class ClarenceHouseProductDiscoverer : IProductDiscoverer<ClarenceHouseVendor>
    {
        private const string ProductUrl = "http://customers.clarencehouse.com/readitem.asp?action=read&acct=01025851";
        private const string ProductDetailUrl = "http://customers.clarencehouse.com/";
        private readonly IPageFetcher<ClarenceHouseVendor> _pageFetcher;
        private readonly IVendorScanSessionManager<ClarenceHouseVendor> _sessionManager; 
        private readonly IStorageProvider<ClarenceHouseVendor> _storageProvider;

        public ClarenceHouseProductDiscoverer(IPageFetcher<ClarenceHouseVendor> pageFetcher, IVendorScanSessionManager<ClarenceHouseVendor> sessionManager, IStorageProvider<ClarenceHouseVendor> storageProvider)
        {
            _pageFetcher = pageFetcher;
            _sessionManager = sessionManager;
            _storageProvider = storageProvider;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var fileProducts = ReadProductsCSVFile();
            var discoveredProducts = new List<DiscoveredProduct>();
            await _sessionManager.ForEachNotifyAsync("Discovering products", fileProducts, async (i, product) =>
            {
                var mpn = product[ScanField.ManufacturerPartNumber];
                var pattern = product[ScanField.Pattern];

                var nvCol = new NameValueCollection();
                nvCol.Add("ID", pattern.Replace("'", "''"));
                nvCol.Add("ltype", "1");

                var page = await _pageFetcher.FetchAsync(ProductUrl, CacheFolder.Search, pattern, nvCol);
                if (page == null) return;

                var itemLinks = page.QuerySelectorAll("a[href^='itemdetail.asp?acct=']").ToList();
                var correctProduct = itemLinks.SingleOrDefault(x => x.InnerText.Trim() == mpn);

                if (correctProduct == null)
                    return;

                var productDetailUrl = ProductDetailUrl + correctProduct.Attributes["href"].Value;
                var productGroup = product.ContainsKey(ScanField.Group) ? product[ScanField.Group] : string.Empty;
                product.DetailUrl = new Uri(productDetailUrl);
                product[ScanField.ProductGroup] = productGroup;
                discoveredProducts.Add(new DiscoveredProduct(product) { DetailUrl = product.DetailUrl });
            });
            return discoveredProducts;
        }

        public List<ScanData> ReadProductsCSVFile()
        {
            var products = new List<ScanData>();
            var csvFile = _storageProvider.GetProductsFileStaticPath(ProductFileType.Xlsx);
            csvFile = csvFile.Replace(".xlsx", ".csv");
            using (var reader = new StreamReader(File.OpenRead(csvFile)))
            {
                var csv = new CsvReader(reader);
                while (csv.Read())
                {
                    var product = new ScanData();
                    product[ScanField.ManufacturerPartNumber] = csv.GetField("Item No.");
                    product[ScanField.Group] = csv.GetField("Collection");
                    product[ScanField.Width] = csv.GetField("Width");
                    product[ScanField.Pattern] = csv.GetField("Pattern Desc");
                    products.Add(product);
                }
            }
            return products;
        }
    }
}