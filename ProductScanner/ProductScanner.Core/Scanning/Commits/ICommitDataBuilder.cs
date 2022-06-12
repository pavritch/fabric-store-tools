using System.Collections.Generic;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Commits
{
    public interface ICommitDataBuilder<T> where T : Vendor
    {
        CommitData Build(List<VendorVariant> vendorProducts, List<StoreProduct> sqlProducts);
    }
}