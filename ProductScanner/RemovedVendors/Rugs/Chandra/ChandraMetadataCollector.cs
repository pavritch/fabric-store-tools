using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Chandra
{
    public class ChandraMetadataCollector : IMetadataCollector<ChandraVendor>
    {
        private readonly ChandraInventoryFileLoader _fileLoader;

        public ChandraMetadataCollector(ChandraInventoryFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var stockInfo = _fileLoader.LoadInventoryData();
            foreach (var stock in stockInfo)
            {
                var product = products.FirstOrDefault(x => x[ScanField.ManufacturerPartNumber] == stock[ScanField.ManufacturerPartNumber]);
                if (product != null)
                    product[ScanField.StockCount] = stock[ScanField.StockCount];
            }
            return Task.FromResult(products);
        }
    }
}