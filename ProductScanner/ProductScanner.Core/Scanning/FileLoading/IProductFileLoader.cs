using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.FileLoading
{
    public interface IProductFileLoader<T> : IProductFileLoader where T : Vendor
    {
    }

    public interface IProductFileLoader
    {
        Task<List<ScanData>> LoadProductsAsync();
    }
}