using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Kaleen
{
    public class KaleenVariantMerger : IVariantMerger<KaleenVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants.GroupBy(x => x[ScanField.Image1]).Select(x => new ScanData { Variants = x.ToList() }).ToList();
        }
    }
}