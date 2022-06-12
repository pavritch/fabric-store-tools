using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Capel.Metadata
{
    public class CapelVariantMerger : IVariantMerger<CapelVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants.GroupBy(x => x[ProductPropertyType.SKU]).Select(x => new ScanData {Variants = x.ToList()}).ToList();
        }
    }
}