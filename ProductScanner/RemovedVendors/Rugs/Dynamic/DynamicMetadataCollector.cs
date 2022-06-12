using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Dynamic
{
    public class DynamicMetadataCollector : IMetadataCollector<DynamicVendor>
    {
        private readonly IProductFileLoader<DynamicVendor> _productFileLoader;

        public DynamicMetadataCollector(IProductFileLoader<DynamicVendor> productFileLoader)
        {
            _productFileLoader = productFileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            return await _productFileLoader.LoadProductsAsync();
        }
    }
}