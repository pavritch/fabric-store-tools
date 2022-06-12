using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace TheRugMarket
{
    public class TheRugMarketVariantMerger : IVariantMerger<TheRugMarketVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            var products = new List<ScanData>();
            var variantGroup = variants.GroupBy(x => x[ScanField.PatternNumber]);
            foreach (var group in variantGroup)
            {
                var product = new ScanData(group.First());
                // they occasionally have 2 products that look identical (same size and shape) - so we want to filter those out
                product.Variants = group.ToList().DistinctBy(x => x[ScanField.Size].Replace("\"", "").ToLower()).ToList();
                products.Add(product);
            }
            return products;
        }
    }
}