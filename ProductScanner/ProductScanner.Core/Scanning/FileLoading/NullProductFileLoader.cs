using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.FileLoading
{
    public class NullProductFileLoader<T> : IProductFileLoader<T> where T : Vendor
    {
        public Task<List<ScanData>> LoadProductsAsync()
        {
            return Task.FromResult(new List<ScanData>());
        }
    }
}