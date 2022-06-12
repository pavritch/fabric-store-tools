using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElkGroup.FileLoaders;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ElkGroup
{
    public class ElkGroupMetadataCollector : IMetadataCollector<ElkGroupVendor>
    {
        private readonly ElkStockFileLoader _stockFileLoader;

        public ElkGroupMetadataCollector(ElkStockFileLoader stockFileLoader)
        {
            _stockFileLoader = stockFileLoader;
        }

        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var stockData = _stockFileLoader.LoadInventoryData();
            foreach (var product in products)
            {
                var match = stockData.FirstOrDefault(x => x[ScanField.UPC] == product[ScanField.UPC]);
                if (match != null)
                    product[ScanField.StockCount] = match[ScanField.StockCount];
            }
            return Task.FromResult(products);
        }
    }
}