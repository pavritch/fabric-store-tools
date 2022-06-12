using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Dynamic
{
    public class DynamicVariantMerger : IVariantMerger<DynamicVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants.GroupBy(x => x[ScanField.Design] + x[ScanField.Color]).Select(x => new ScanData
            {
                Variants = x.ToList().DistinctBy(v => v[ScanField.Size].ToLower()).ToList()
            }).ToList();
        }
    }
}