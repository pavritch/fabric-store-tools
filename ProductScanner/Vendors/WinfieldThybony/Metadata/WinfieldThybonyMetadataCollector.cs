using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using ServiceStack.Text;
using Utilities.Extensions;

namespace WinfieldThybony.Metadata
{
    public class WinfieldThybonyMetadataCollector : IMetadataCollector<WinfieldThybonyVendor>
    {
        private const bool EnablePriceScraping = false;
        private const string StockUrl = "https://www.e-designtrade.com/stkinq.asp?pat={0}&clr=WT";

        private readonly IPageFetcher<WinfieldThybonyVendor> _pageFetcher;
        private readonly IStorageProvider<WinfieldThybonyVendor> _storageProvider; 
        private readonly IVendorScanSessionManager<WinfieldThybonyVendor> _sessionManager;
        private readonly IProductFileLoader<WinfieldThybonyVendor> _fileLoader;

        public WinfieldThybonyMetadataCollector(IPageFetcher<WinfieldThybonyVendor> pageFetcher, IStorageProvider<WinfieldThybonyVendor> storageProvider, IVendorScanSessionManager<WinfieldThybonyVendor> sessionManager, IProductFileLoader<WinfieldThybonyVendor> fileLoader)
        {
            _pageFetcher = pageFetcher;
            _storageProvider = storageProvider;
            _sessionManager = sessionManager;
            _fileLoader = fileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            // this should only be done on demand, since we're pulling from Kravet, and they have limits on connections
            if (EnablePriceScraping)
            {
                //await BuildPricingFileAsync(products);
            }

            await PopulatePricingDataAsync(products);
            return products;
        }

        private async Task PopulatePricingDataAsync(List<ScanData> products)
        {
            var fileProducts = await _fileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                var match = fileProducts.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ManufacturerPartNumber]);
                if (match != null)
                {
                    product.AddFields(match);
                }
            }
        }

        private async Task BuildPricingFileAsync(List<ScanData> products)
        {
            var priceData = new List<Dictionary<string, string>>();
            await _sessionManager.ForEachNotifyAsync("Scanning for pricing", products, async product =>
            {
                var mpn = product[ScanField.ManufacturerPartNumber];
                var url = string.Format(StockUrl, mpn);
                var stockPage = await _pageFetcher.FetchAsync(url, CacheFolder.Stock, mpn);

                if (stockPage.InnerText.ContainsIgnoreCase("An error occurred in the execution of this ASP page:"))
                {
                    _pageFetcher.RemoveCachedFile(CacheFolder.Stock, mpn);
                    return;
                }

                var retail = stockPage.GetFieldValue("b:contains('Retail'):select-parent + td").Replace("&nbsp;", "");
                var wholesale = stockPage.GetFieldValue("b:contains('Wholesale'):select-parent + td").Replace("&nbsp;", "");
                var unit = retail.Contains("YARD") ? UnitOfMeasure.Yard : UnitOfMeasure.Roll;

                var data = new Dictionary<string, string>();
                data["MPN"] = mpn;
                data["RetailPrice"] = retail.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
                data["Unit"] = unit.ToString();
                data["WholesalePrice"] = wholesale.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
                priceData.Add(data);
            });

            // create pricing file
            var csv = CsvSerializer.SerializeToCsv(priceData);
            var path = _storageProvider.GetProductsFileStaticPath(ProductFileType.Xls);
            FileExtensions.ConvertCSVToXLS(csv, path);
        }
    }
}