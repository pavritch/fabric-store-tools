using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Momeni
{
    public class MomeniVariantMerger : IVariantMerger<MomeniVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants.GroupBy(x => x[ScanField.SKU]).Select(x => new ScanData {Variants = x.ToList()}).ToList();
        }
    }
}