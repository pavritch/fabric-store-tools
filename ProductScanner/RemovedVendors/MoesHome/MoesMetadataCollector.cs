using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace MoesHome
{
    public class MoesMetadataCollector : IMetadataCollector<MoesHomeVendor>
    {
        private readonly MoesInventoryFileLoader _fileLoader;

        public MoesMetadataCollector(MoesInventoryFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var stockData = _fileLoader.LoadInventoryData();
            foreach (var product in products)
            {
                var match = stockData.FirstOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ManufacturerPartNumber]);
                if (match != null) product[ScanField.StockCount] = match[ScanField.StockCount];
            }
            return Task.FromResult(products);
        }
    }
}