using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Sphinx
{
    public class SphinxVariantMerger : IVariantMerger<SphinxVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants.GroupBy(x => x[ScanField.Collection] + x[ScanField.Pattern])
                .Select(x => new ScanData {Variants = x.ToList()}).ToList();
        }
    }
}