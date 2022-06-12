using System.Collections.Generic;

namespace ProductScanner.Core.Scanning.Commits
{
    public class ProductPriceChange
    {
        public int ProductId { get; set; }
        public List<VariantPriceChange> VariantPriceChanges { get; set; }
    }
}