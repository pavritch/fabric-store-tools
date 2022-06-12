using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.LoadTesting
{
    public interface IRandomRequestGenerator
    {
        Task<List<StockCheck>> GenerateAsync(int maxVendors, int maxProductsPerVendor);
        Task<List<StockCheck>> GenerateForVendors(List<Vendor> vendors, int maxProductsPerVendor);
    }
}