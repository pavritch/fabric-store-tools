using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Metadata
{
    public class NullMetadataCollector<T> : IMetadataCollector<T> where T : Vendor
    {
        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            return Task.FromResult(products);
        }
    }
}