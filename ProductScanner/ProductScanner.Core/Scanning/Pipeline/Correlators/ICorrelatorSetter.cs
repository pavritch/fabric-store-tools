using System.Collections.Generic;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Correlators
{
    public interface ICorrelatorSetter<T>
    {
        void SetCorrelators(List<ScanData> vendorProducts, List<StoreProduct> sqlProducts);
    }
}