using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace CurreyCo
{
    public class CurreyMetadataCollector : IMetadataCollector<CurreyVendor>
    {
        private readonly CurreyDiscontinuedFileLoader _productFileLoader;

        public CurreyMetadataCollector(CurreyDiscontinuedFileLoader productFileLoader)
        {
            _productFileLoader = productFileLoader;
        }

        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var discontinued = _productFileLoader.LoadPriceData();
            products.ForEach(x =>
            {
                var match = discontinued.SingleOrDefault(disc => disc[ScanField.ItemNumber] == x[ScanField.ManufacturerPartNumber]);
                if (match != null) x.IsDiscontinued = true;
            });
            return Task.FromResult(products);
        }
    }
}