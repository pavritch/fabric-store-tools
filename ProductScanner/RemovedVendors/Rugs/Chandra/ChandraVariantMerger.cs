using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Chandra
{
    public class ChandraVariantMerger : IVariantMerger<ChandraVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            var groupedVariants = variants.GroupBy(x => x[ScanField.PatternNumber]).Select(x => new ScanData {Variants = x.ToList()}).ToList();
            return groupedVariants;
        }
    }
}
