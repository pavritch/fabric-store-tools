using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Safavieh
{
    public class SafaviehVariantMerger : IVariantMerger<SafaviehVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            var groupedVariants = variants.GroupBy(x => x[ScanField.SKU]).Select(x => new ScanData
            {
                // they occasionally have 2 products that look identical (same size and shape) - so we want to filter those out
                Variants = x.ToList().DistinctBy(v => v[ScanField.Size].ToLower()).ToList()
            }).ToList();
            return groupedVariants;
        }
    }
}