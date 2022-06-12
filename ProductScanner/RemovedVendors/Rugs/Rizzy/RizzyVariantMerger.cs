using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Rizzy
{
    public class RizzyVariantMerger : IVariantMerger<RizzyVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants.GroupBy(x => x[ScanField.PatternNumber]).Select(x => new ScanData {Variants = x.ToList()}).ToList();
        }
    }
}