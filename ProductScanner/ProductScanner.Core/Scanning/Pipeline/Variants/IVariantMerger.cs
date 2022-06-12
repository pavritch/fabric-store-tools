using System.Collections.Generic;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Variants
{
    // often times we have a big list of variants for rug vendors,
    // and we need to merge them into products
    public interface IVariantMerger<T> where T : Vendor
    {
        List<ScanData> MergeVariants(List<ScanData> variants);
    }
}