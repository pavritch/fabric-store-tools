using System.Collections.Generic;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Variants
{
    public class NullVariantMerger<T> : IVariantMerger<T> where T : Vendor
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants;
        }
    }
}