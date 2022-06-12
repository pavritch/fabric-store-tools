using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Dalyn
{
    public class DalynVariantMerger : IVariantMerger<DalynVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants.GroupBy(x => x[ScanField.PatternNumber] + "-" + x[ScanField.Color])
                .Select(x => new ScanData {Variants = x.ToList()}).ToList();
        }
    }
}