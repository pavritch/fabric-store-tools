using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Capel.Metadata
{
    public class CapelMetadataCollector : IMetadataCollector<CapelVendor>
    {
        public Task<List<ScanData>> PopulateMetadata(List<ScanData> variants)
        {
            return Task.FromResult(variants);
        }
    }
}